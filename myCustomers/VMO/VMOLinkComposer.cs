using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using MaryKay.Configuration;
using MaryKay.IBCDataServices.Entities;
using myCustomers.Contexts;
using Quartet.Entities;

namespace myCustomers.VMO
{
    public interface IVMOLinkComposer
    {
        Uri GetCustomerVmoLink(Customer customer, Uri requestUri);
    }

    public class VMOLinkComposer : IVMOLinkComposer
    {
        IAppSettings _appSettings;
        IConsultantContext _consultantContext;
        IQuartetClientFactory _quartetClientFactory;

        public VMOLinkComposer(IAppSettings appSettings, IConsultantContext consultantContext, IQuartetClientFactory quartetClientFactory)
        {
            _appSettings          = appSettings;
            _consultantContext    = consultantContext;
            _quartetClientFactory = quartetClientFactory;
        }

        public Uri GetCustomerVmoLink(Customer customer, Uri requestUri)
        {
            var primaryMoniker = !string.IsNullOrWhiteSpace(_consultantContext.Consultant.PrimaryMoniker) ? _consultantContext.Consultant.PrimaryMoniker : "0";
            
            var projectKey = _appSettings.GetValue("CustomerVMOProjectKey");
            if (string.IsNullOrWhiteSpace(projectKey)) throw new ConfigurationErrorsException("CustomerVMOProjectKey is a required appSetting used by VMOLinkComposer");

            var vmoMappings = _quartetClientFactory.GetCustomersQueryServiceClient().MapVMOCodes(customer.QuestionnaireItems);

            var vmoRecId = "0";
            var vmoCatId = string.Empty;
            if (vmoMappings.Length > 0)
            {
                var recResult = vmoMappings.FirstOrDefault(v => v.QuestionCode == "MakeupLook");
                if (recResult != null)
                {
                    vmoRecId = recResult.VMOID;
                }

                var catResult = vmoMappings.FirstOrDefault(c => c.QuestionCode == "SkinTone");
                if (catResult != null)
                {
                    vmoCatId = catResult.VMOID;
                }
            }

            var template = "{0}={1}";

            var queryStringVariables = new List<string>();

            queryStringVariables.Add(string.Format(template, "d", _appSettings.GetValue("VMOPWSRoot")));
            queryStringVariables.Add(string.Format(template, "m", primaryMoniker));
            queryStringVariables.Add(string.Format(template, "pk", projectKey));
            queryStringVariables.Add(string.Format(template, "e", (string.IsNullOrWhiteSpace(_consultantContext.Consultant.PrimaryMoniker)) ? "0" : "1"));
            queryStringVariables.Add(string.Format(template, "b", "1"));
            queryStringVariables.Add(string.Format(template, "sknID", "1"));
            queryStringVariables.Add(string.Format(template, "it", "1"));
            queryStringVariables.Add(string.Format(template, "enc", EncryptVMOUserNameString(_consultantContext.Consultant, customer, projectKey)));
            queryStringVariables.Add(string.Format(template, "recLook", vmoRecId));
            if (!String.IsNullOrEmpty(vmoCatId)) queryStringVariables.Add(string.Format(template, "catID", vmoCatId));

            var value = String.Format("{0}?{1}", _appSettings.GetValue("VMORoot"), String.Join("&", queryStringVariables.ToArray()));
            return new Uri(requestUri, value);
        }

        string EncryptVMOUserNameString(Consultant consultant, Customer customer, string projectKey = "3")
        {
            string vmoTag = @"<vmo username=""{0};{1};{2}"" project=""{3}"" userlevel=""{4}"" />";

            string vmoTagFormatted = String.Format(vmoTag,
                                                    consultant.ConsultantID,
                                                    customer.LegacyContactId.HasValue ? customer.LegacyContactId.Value.ToString() : customer.CustomerId.ToString(),
                                                    consultant.SubsidiaryCode,
                                                    projectKey,
                                                    "3");

            string encryptedUserName = VMOEncryption.Encrypt(vmoTagFormatted);
            return encryptedUserName;
        }
    }

    public class VMOEncryption
    {
        public static string Encrypt(string text)
        {
            byte[] encrypted = EncryptStringToBytes_AES(text);
            return BytesToHexString(encrypted);
        }

        static byte[] EncryptStringToBytes_AES(string plainText)
        {
            if (plainText == null || plainText.Length <= 0) throw new ArgumentNullException("plainText");

            var aesAlg = new RijndaelManaged();
            aesAlg.Key = ToByteArray("193a75e14f12b17c");
            aesAlg.IV = ToByteArray("18a8fbc6ef7da739");

            using (var msEncrypt = new MemoryStream())
            {
                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        swEncrypt.Write(plainText);

                return msEncrypt.ToArray();
            }
        }

        static string BytesToHexString(byte[] bytes)
        {
            var builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2").ToLower());
            }

            return builder.ToString();
        }

        static byte[] ToByteArray(string str)
        {
            var encoding = new ASCIIEncoding();
            return encoding.GetBytes(str);
        }
    }
}
