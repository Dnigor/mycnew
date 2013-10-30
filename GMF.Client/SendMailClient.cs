using System.Configuration;
using MaryKay.ServiceModel;
using MaryKay.ServiceModel.Config;

namespace GMF.Client
{
    public class SendMailClient : ISendMailService
    {
		static readonly ISyncClient<ISendMailService> _client;

        static SendMailClient()
        {
			var environment = ConfigurationManager.AppSettings["GMF.Environment"];
			if (string.IsNullOrEmpty(environment))
				environment = ConfigurationManager.AppSettings["Environment"];

            var region = ConfigurationManager.AppSettings["GMF.Region"];
			if (string.IsNullOrEmpty(region))
				region = ConfigurationManager.AppSettings["Region"];

			var endpoint = EndpointsConfig
                    .FromContractAssembly<ISendMailService>()
					.ForEnvironment(environment)
					.ForRegion(region)
					.WithParameters(key =>
					{
						if (key.Equals("Region"))
							return region;

						return ConfigurationManager.AppSettings[key];
					})
                    .ForContract<ISendMailService>()
                    .NamedEndpoint("SendMailService");

            _client = SyncClient<ISendMailService>.Create(endpoint);
        }

        SendSimpleEmailResponse ISendMailService.SendSimpleEmail(SendSimpleEmailRequest request)
        {
            return _client.Invoke(s => s.SendSimpleEmail(request));
        }

        SendTemplatedEmailResponse ISendMailService.SendTemplatedEmail(SendTemplatedEmailRequest request)
        {
            return _client.Invoke(s => s.SendTemplatedEmail(request));
        }

        SendPredefinedEmailResponse ISendMailService.SendPredefinedEmail(SendPredefinedEmailRequest request)
        {
            return _client.Invoke(s => s.SendPredefinedEmail(request));
        }

        SendMessageResponse ISendMailService.SendMessage(SendMessageRequest request)
        {
            return _client.Invoke(s => s.SendMessage(request));
        }
    }

    public static class ExtendISendMailService
    {
        public static void SendSimpleEmail(this ISendMailService svc, string senderEmail, string recipientEmail, string subject, string content, string domain, string recipientContactID)
        {
            var req = new SendSimpleEmailRequest
            {
                Body = new SendSimpleEmailRequestBody
                {
                    senderEmail        = senderEmail,
                    recipientEmail     = recipientEmail,
                    subject            = subject,
                    content            = content,
                    domain             = domain,
                    recipientContactID = recipientContactID,
                }
            };

            svc.SendSimpleEmail(req);
        }

        public static void SendTemplatedEmail(this ISendMailService svc, EmailMessage email)
        {
            var req = new SendTemplatedEmailRequest
            {
                Body = new SendTemplatedEmailRequestBody
                {
                    email = email
                }
            };

            svc.SendTemplatedEmail(req);
        }

        public static void SendPredefinedEmail(this ISendMailService svc, string contentDomain, string contentID, string subject, string sender, EmailRecipient[] recipients)
        {
            var req = new SendPredefinedEmailRequest
            {
                Body = new SendPredefinedEmailRequestBody
                {
                    contentDomain = contentDomain,
                    contentID     = contentID,
                    subject       = subject,
                    sender        = sender,
                    recipients    = recipients
                }
            };

            svc.SendPredefinedEmail(req);
        }

        public static long SendMessage(this ISendMailService svc, Message message)
        {
            var req = new SendMessageRequest
            {
                Body = new SendMessageRequestBody
                {
                    message = message
                }
            };

            var resp = svc.SendMessage(req);

            return resp.Body.SendMessageResult;
        }
    }
}
