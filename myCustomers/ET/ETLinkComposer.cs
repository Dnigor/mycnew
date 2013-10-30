using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using MaryKay.Configuration;
using myCustomers.Contexts;
using myCustomers.Features;
using NLog;
using Quartet.Entities;

namespace myCustomers.ET
{
    public interface IETLinkComposer
    {
        UriBuilder GetCustomerETLink(Customer customer, Uri requestUri);
    }

    public class ETLinkComposer : IETLinkComposer
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();
        
        const string _myCustomersETMode = "c";

        IAppSettings _appSettings;
        IConsultantContext _consultantContext;
        ISubsidiaryAccessor _subsidiaryAccessor;
        IFeaturesConfigService _featuresConfigService;

        public ETLinkComposer
        (
            IAppSettings appSettings,
            IConsultantContext consultantContext,
            ISubsidiaryAccessor subsidiaryAccessor,
            IFeaturesConfigService featuresConfigService
        )
        {
            _appSettings = appSettings;
            _consultantContext = consultantContext;
            _subsidiaryAccessor = subsidiaryAccessor;
            _featuresConfigService = featuresConfigService;
        }

        public UriBuilder GetCustomerETLink(Customer customer, Uri requestUri)
        {
            if (!_featuresConfigService.GetCustomerDetailFeatures().ExactTargetSuscriptionManagement)
                return null;

            var url = new StringBuilder(string.Format("{0}:{1}", requestUri.Scheme, _appSettings.GetValue("ExactTarget.SubscriptionManagementRootUrl")));
            if (string.IsNullOrWhiteSpace(url.ToString()))
                throw new ConfigurationErrorsException("AppSettings for ExactTarget.SubscriptionManagementRootUrl is required when ET feature is enabled.");

            var subscriberKeyEnvironment = _appSettings.GetValue("ExactTarget.SubscriberKeyEnvironment");
            if (string.IsNullOrWhiteSpace(subscriberKeyEnvironment))
                throw new ConfigurationErrorsException("AppSettings for ExactTarget.SubscriberKeyEnvironment is required when ET feature is enabled.");

            var customerPassPhrase = _appSettings.GetValue("ExactTarget.CustomerPassPhrase");
            if (string.IsNullOrWhiteSpace(customerPassPhrase))
                throw new ConfigurationErrorsException("AppSettings for ExactTarget.CustomerPassPhrase is required when ET feature is enabled.");

            if (customer.EmailAddress == null || string.IsNullOrWhiteSpace(customer.EmailAddress.Address)) return null;

            var queryStringToEncrypt = new StringBuilder();
            queryStringToEncrypt.AppendFormat("S={0}", GetSubscriberKey(customer, subscriberKeyEnvironment));
            queryStringToEncrypt.AppendFormat("&CU={0}", Thread.CurrentThread.CurrentUICulture.ToString());
            queryStringToEncrypt.AppendFormat("&C={0}", _consultantContext.Consultant.ConsultantID);
            queryStringToEncrypt.AppendFormat("&M={0}", _myCustomersETMode);
            

            url.AppendFormat("?q={0}", HttpUtility.UrlEncode(Encrypt(queryStringToEncrypt.ToString(), customerPassPhrase)));
            return new UriBuilder(url.ToString());
        }

        string GetSubscriberKey(Customer customer, string subscriberKeyEnvironment)
        {
            return string.Format("{0:N}-{1}-{2}-{3}",
                customer.CustomerId,
                customer.EmailAddress.Address.Trim(),
                _subsidiaryAccessor.GetSubsidiaryCode(),
                subscriberKeyEnvironment);
        }

        string Encrypt(string originalString, string passPhrase)
        {
            var key = UTF8Encoding.UTF8.GetBytes(passPhrase);
            try
            {
                if (String.IsNullOrWhiteSpace(originalString))
                {
                    throw new ArgumentNullException ("The string which needs to be encrypted can not be null or WhiteSpace.");
                }
                var cryptoProvider = new DESCryptoServiceProvider();
                cryptoProvider.Mode = CipherMode.ECB;
                cryptoProvider.Padding = PaddingMode.Zeros;
                using (var memoryStream = new MemoryStream())
                using (var cryptoStream = new CryptoStream(memoryStream, cryptoProvider.CreateEncryptor(key, key), CryptoStreamMode.Write))
                using (var writer = new StreamWriter(cryptoStream))
                {
                    writer.Write(originalString);
                    writer.Flush();
                    cryptoStream.FlushFinalBlock();
                    writer.Flush();
                    var encryptedValue = Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
                    return encryptedValue;
                }
            }
            catch (Exception e)
            {
                _logger.Warn(string.Format("Error encyrpting ET query string.  {0}", e));
                return null;
            }
        }
    }
}
