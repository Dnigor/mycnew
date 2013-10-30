using System;
using System.Configuration;
using MaryKay.ServiceModel;
using MaryKay.ServiceModel.Config;
using NLog;

namespace myCustomers.Services.NetCC
{
    public sealed class ProPayCreditCardService : ICreditCardService
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        static readonly ISyncClient<ICCProcessing> _client;

        static ProPayCreditCardService()
        {
            var environment = ConfigurationManager.AppSettings["NetTax.Environment"];
            if (string.IsNullOrEmpty(environment))
                environment = ConfigurationManager.AppSettings["Environment"];

            var region = ConfigurationManager.AppSettings["NetTax.Region"];
            if (string.IsNullOrEmpty(region))
                region = ConfigurationManager.AppSettings["Region"];

            var endpoint = EndpointsConfig
                .FromContractAssembly<ICCProcessing>()
                .ForEnvironment(environment)
                .ForRegion(region)
                .WithParameters(key =>
                {
                    if (key.Equals("Region"))
                        return region;

                    return ConfigurationManager.AppSettings[key];
                })
                .ForContract<ICCProcessing>()
                .NamedEndpoint("CCProcessing");

            _client = SyncClient<ICCProcessing>.Create(endpoint);
        }

        public Guid CreateToken(string ccNumber, DateTime ccExpirationDate)
        {
            var request = new EncryptCCNumberUnAssociated
            {
                ccnumber       = ccNumber,
                expirationdate = ccExpirationDate
            };

            var response = _client.Invoke(svc => svc.EncryptCCNumber(request));

            return response.EncryptCCNumberUnAssociatedResult;
        }

        public AuthorizePaymentResult AuthorizePayment(long account, string invoiceNumber, decimal amount, Guid ccToken, string ccAddress, string ccZip, DateTime ccExpirationDate)
        {
            var request = new AuthorizeProPayPaymentRequest
            {
                Body = new AuthorizeProPayPaymentRequestBody
                {
                    AccountNumber    = account,
                    InvoiceNumber    = invoiceNumber,
                    PaymentAmount    = Convert.ToInt64(amount * 100),
                    CCToken          = ccToken,
                    Address          = ccAddress,
                    ZipCode          = ccZip,
                    CCExpirationDate = ccExpirationDate.Month.ToString("D2") + ccExpirationDate.Year.ToString().Substring(2)
                }
            };

            var response = _client.Invoke(svc => svc.AuthorizeProPayPayment(request)).Body;

            return new AuthorizePaymentResult(response.AuthCode, response.TransactionNumber, response.TransactionStatus, response.AVSResponseCode);
        }
    }
}
