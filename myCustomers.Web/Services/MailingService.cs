using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using GMF.Client;
using MaryKay.Configuration;
using myCustomers.Contexts;
using myCustomers.Globalization;
using myCustomers.Web.Controllers.Api;
using myCustomers.Web.Models;
using Quartet.Client.Customers;
using Quartet.Client.Products;

namespace myCustomers.Web.Services
{
    public class MailingService : IMailingService
    {
        readonly IAppSettings                 _appSettings;
        readonly IQuartetClientFactory        _quartetClientFactory;
        readonly IProductCatalogClientFactory _productCatalogClientFactory;
        readonly IConsultantContext           _consultantContext;
        readonly ISendMailService             _emailService;

        public MailingService
        (
            IAppSettings appSettings,
            IConsultantContext consultantContext,
            IQuartetClientFactory quartetClientFactory,
            IProductCatalogClientFactory productCatalogClientFactory,
            ISendMailService emailService
        )
        {
            _appSettings                 = appSettings;
            _quartetClientFactory        = quartetClientFactory;
            _productCatalogClientFactory = productCatalogClientFactory;
            _consultantContext           = consultantContext;
            _emailService                = emailService;
        }

        public void SendOptInMail(CustomerModel model)
        {
            if (!_appSettings.GetValue<bool>("Feature.ExactTarget.DoubleOptIn"))
                return;

            if (string.IsNullOrWhiteSpace(model.EmailAddress))
                return;

            var consultant = _consultantContext.Consultant;
            if (!consultant.Subscription.IsActive())
                return;

            var culture               = _appSettings.GetValue("GMF.EmailCulture");
            var emailCulture          = string.IsNullOrWhiteSpace(model.PreferredLanguage) ? culture : model.PreferredLanguage;
            var contentDomain         = string.Format(_appSettings.GetValue("GMF.EmailContentDomain"), consultant.SubsidiaryCode);
            var contentID             = string.Format(_appSettings.GetValue("GMF.OptInInviteContentId"), emailCulture);
            var subject               = string.Empty;
            var sender                = consultant.PrimaryEmailAddress;
            var consultantMoniker     = !string.IsNullOrWhiteSpace(consultant.PrimaryMoniker) ? "/" + consultant.PrimaryMoniker : string.Empty;
            var consultantPhoneNumber = (consultant.PrimaryPhoneNumber != null) ? consultant.PrimaryPhoneNumber.Number : "";
            var consultantLevel       = consultant.ConsultantLevel.HasValue ? consultant.ConsultantLevel.Value : 0;

            var recipient = new EmailRecipient
            {
                Recipient = model.EmailAddress,
                Attributes = new[]
				{
					new EmailAttribute { Name = "CustomerId",             Value = model.CustomerId.ToString("N") },
					new EmailAttribute { Name = "ConsultantMoniker",      Value = consultantMoniker },
					new EmailAttribute { Name = "ConsultantFirstName",    Value = consultant.FirstName },
					new EmailAttribute { Name = "ConsultantLastName",     Value = consultant.LastName },
					new EmailAttribute { Name = "ConsultantEmailAddress", Value = sender },
					new EmailAttribute { Name = "ConsultantPhoneNumber",  Value = consultantPhoneNumber },
					new EmailAttribute { Name = "ConsultantCareerLevel",  Value = consultantLevel.ToString() },
					new EmailAttribute { Name = "CustomerFirstName",      Value = model.FirstName },
					new EmailAttribute { Name = "CustomerLastName",       Value = model.LastName },
					new EmailAttribute { Name = "PWSDomain",              Value = _appSettings.GetValue("GMF.PWSDomain") },
					new EmailAttribute { Name = "InTouchDomain",          Value = _appSettings.GetValue("GMF.InTouchDomain") },
					new EmailAttribute { Name = "EmailAddress",           Value = model.EmailAddress }
				}
            };

            _emailService.SendPredefinedEmail(contentDomain, contentID, subject, sender, new[] { recipient });
        }

        public void SendInviteToRegister(CustomerModel model)
        {
            var consultant = _consultantContext.Consultant;
            if (!consultant.Subscription.IsActive())
                return;

            var culture               = _appSettings.GetValue("GMF.EmailCulture");
            var emailCulture          = string.IsNullOrWhiteSpace(model.PreferredLanguage) ? culture : model.PreferredLanguage;  
            var contentDomain         = string.Format(_appSettings.GetValue("GMF.EmailContentDomain"), consultant.SubsidiaryCode);
            var contentID             = string.Format(_appSettings.GetValue("GMF.InviteToRegisterContentId"), emailCulture);
            var subject               = Resources.GetString("INVITETOREGISTEREMAIL_SUBJECT");
            var sender                = consultant.PrimaryEmailAddress;
            var consultantMoniker     = !string.IsNullOrWhiteSpace(consultant.PrimaryMoniker) ? "/" + consultant.PrimaryMoniker : string.Empty;
            var consultantPhoneNumber = (consultant.PrimaryPhoneNumber != null) ? consultant.PrimaryPhoneNumber.Number : "";
            var consultantLevel       = consultant.ConsultantLevel.HasValue ? consultant.ConsultantLevel.Value : 0;

            var recipient = new EmailRecipient
            {
                Recipient = model.EmailAddress,
                Attributes = new[]
				{
					new EmailAttribute { Name = "CustomerId",             Value = model.CustomerId.ToString("N") },
					new EmailAttribute { Name = "ConsultantMoniker",      Value = consultantMoniker },
					new EmailAttribute { Name = "ConsultantFirstName",    Value = consultant.FirstName },
					new EmailAttribute { Name = "ConsultantLastName",     Value = consultant.LastName },
					new EmailAttribute { Name = "ConsultantEmailAddress", Value = sender },
					new EmailAttribute { Name = "ConsultantPhoneNumber",  Value = consultantPhoneNumber },
					new EmailAttribute { Name = "ConsultantCareerLevel",  Value = consultantLevel.ToString() },
					new EmailAttribute { Name = "CustomerFirstName",      Value = model.FirstName },
					new EmailAttribute { Name = "CustomerLastName",       Value = model.LastName },
					new EmailAttribute { Name = "PWSDomain",              Value = _appSettings.GetValue("GMF.PWSDomain") },
					new EmailAttribute { Name = "InTouchDomain",          Value = _appSettings.GetValue("GMF.InTouchDomain") },
					new EmailAttribute { Name = "EmailAddress",           Value = model.EmailAddress }
				}
            };

            _emailService.SendPredefinedEmail(contentDomain, contentID, subject, sender, new[] { recipient });
        }

        public void SendConfirmationEmail(ConfirmationEmailViewModel confirmationEmailViewModel)
        {
            var consultant = _consultantContext.Consultant;
            if (!consultant.Subscription.IsActive())
                return;

            if (!string.IsNullOrWhiteSpace(consultant.PrimaryEmailAddress))
            {
                var queryService = _quartetClientFactory.GetCustomersQueryServiceClient();
                var order = queryService.GetOrderById(confirmationEmailViewModel.OrderId);
                if (order != null)
                {
                    var customer = queryService.GetCustomer(order.CustomerId);
                    if (customer != null && customer.EmailAddress != null && !string.IsNullOrWhiteSpace(customer.EmailAddress.Address))
                    {
                        var webUiCulture = Thread.CurrentThread.CurrentUICulture;
                        var preferredLanguage = customer.ContactPreferences.PreferredLanguage;
                        var emailCulture = preferredLanguage != null ? new CultureInfo(preferredLanguage) : webUiCulture;

                        Thread.CurrentThread.CurrentUICulture = emailCulture;
                        try
                        {
                            var contentDomain         = string.Format(_appSettings.GetValue("GMF.EmailContentDomain"), consultant.SubsidiaryCode);
                            var contentID             = string.Format(_appSettings.GetValue("GMF.ConfirmationEmailContentId"), emailCulture.Name);
                            var customerEmailAddress  = customer.EmailAddress.Address;
                            var subject               = Resources.GetString("ORDERCONFIRMATIONEMAILSUBJECT");
                            var consultantMoniker     = !string.IsNullOrWhiteSpace(consultant.PrimaryMoniker) ? "/" + consultant.PrimaryMoniker : string.Empty;
                            var consultantPhoneNumber = (consultant.PrimaryPhoneNumber != null) ? consultant.PrimaryPhoneNumber.Number : null;
                            var consultantLevel       = consultant.ConsultantLevel.HasValue ? consultant.ConsultantLevel.Value : 0;
                            var customerId            = customer.LegacyContactId.HasValue ? customer.LegacyContactId.Value.ToString() : customer.CustomerId.ToString("N");
                            var hasPws                = consultant.Subscription.IsActive();
                            var freeShipping          = false;

                            if (hasPws)
                            {
                                var freeShippingAttributeKey = Guid.Parse(_appSettings.GetValue("PwsSubscription_AttributeKey_FreeShipping"));
                                var freeShippingAttribute    = consultant.Subscription.SubscriptionAttributes.Where(a => a.AttributeKey == freeShippingAttributeKey).FirstOrDefault();
                                freeShipping                 = freeShippingAttribute.IsSelected();
                            }

                            var statementsXml = StatementsXml(confirmationEmailViewModel);
                            var productsXml   = ProductsXml(confirmationEmailViewModel.OrderId);

                            var recipient = new EmailRecipient
                            {
                                Recipient = customerEmailAddress,
                                Attributes = new[]
                                {
                                    new EmailAttribute { Name = "CustomerId",             Value = customerId },
                                    new EmailAttribute { Name = "ConsultantMoniker",      Value = (consultantMoniker != null)? consultantMoniker: string.Empty },
                                    new EmailAttribute { Name = "ConsultantFirstName",    Value = consultant.FirstName },
                                    new EmailAttribute { Name = "ConsultantLastName",     Value = consultant.LastName },
                                    new EmailAttribute { Name = "ConsultantEmailAddress", Value = consultant.PrimaryEmailAddress },
                                    new EmailAttribute { Name = "ConsultantPhoneNumber",  Value = (consultantPhoneNumber != null)? consultantPhoneNumber: string.Empty },
                                    new EmailAttribute { Name = "ConsultantCareerLevel",  Value = consultantLevel.ToString() },
                                    new EmailAttribute { Name = "CustomerFirstName",      Value = customer.ContactInformation.FirstName },
                                    new EmailAttribute { Name = "CustomerLastName",       Value = customer.ContactInformation.LastName },
                                    new EmailAttribute { Name = "EmailAddress",           Value = customerEmailAddress },
                                    new EmailAttribute { Name = "FreeShipping",           Value = freeShipping ? "1" : "0" },
                                    new EmailAttribute { Name = "HasPws",                 Value = hasPws ? "1" : "0" },
                                    new EmailAttribute { Name = "Message",                Value = statementsXml },
                                    new EmailAttribute { Name = "OrderID",                Value = order.ConfirmationNumber },
                                    new EmailAttribute { Name = "PWSDomain",              Value = _appSettings.GetValue("GMF.PWSDomain") },
                                    new EmailAttribute { Name = "InTouchDomain",          Value = _appSettings.GetValue("GMF.InTouchDomain") },
                                    new EmailAttribute { Name = "ProductTable",           Value = productsXml }
                                }
                            };

                            _emailService.SendPredefinedEmail(contentDomain, contentID, subject, consultant.PrimaryEmailAddress, new[] { recipient });
                        }
                        finally
                        {
                            Thread.CurrentThread.CurrentUICulture = webUiCulture;
                        }
                    }
                }
            }
        }

        string ProductsXml(Guid OrderId)
        {
            var queryService        = _quartetClientFactory.GetCustomersQueryServiceClient();
            var productCatalog      = _productCatalogClientFactory.GetProductCatalogClient();
            var order               = queryService.GetOrderById(OrderId);
            var productIds          = order.Items.Select(p => p.ProductId).ToArray();
            var products            = productCatalog.GetByProductIds(productIds).Products;
            var productsById        = products.ToDictionary<Quartet.Entities.Products.Product, string>(p => p.ProductId);
            var productImageBaseUrl = _appSettings.GetValue("ProductImageBaseUrl");
            var productAmountFormat = _appSettings.GetValue("Globalization.CurrencyFormat");

            var xeProducts = new XElement
            (
                "products",
                new XAttribute("image-url-prefix", productImageBaseUrl),
                new XAttribute("total-price", string.Format(productAmountFormat, order.EstimatedOrderAmount)),//order.EstimatedOrderAmount.ToString(productAmountFormat)
                from item in order.Items
                let name      = item.Name ?? item.DisplayPartId ?? string.Empty
                let productId = item.ProductId
                let product   = productsById.ContainsKey(productId) ? productsById[productId] : null
                let imageUrl  = (product != null) ? product.HeroListImagePath ?? string.Empty : string.Empty
                let shadeName = item.ShadeName ?? string.Empty
                let quantity  = item.Quantity.ToString()
                let price     = string.Format(productAmountFormat, item.Price) //item.Price.ToString(productAmountFormat)
                select new XElement
                (
                    "product",
                    new XAttribute("name", name),
                    new XAttribute("image-url", imageUrl),
                    new XAttribute("shade", shadeName),
                    new XAttribute("quantity", quantity),
                    new XAttribute("price", price)
                )
            );

            var productsXml = xeProducts.ToString();

            return productsXml;
        }

        string StatementsXml(ConfirmationEmailViewModel confirmationEmailViewModel)
        {
            var xeStatements = new XElement("statements");

            string statement;
            if (confirmationEmailViewModel.OrderTotalSelected)
            {
                statement = string.Format(Resources.GetString("ORDERCONFIRMATIONEMAIL_ORDERTOTAL_STATEMENTFORMAT"), _appSettings.GetValue("Globalization.CurrencySymbol"), confirmationEmailViewModel.OrderTotalAmount);
                xeStatements.Add(new XElement("statement", statement));
            }

            if (confirmationEmailViewModel.OrderProcessedSelected)
            {
                statement = Resources.GetString("ORDERCONFIRMATIONEMAIL_ORDERPROCESSEDSTATEMENT");
                xeStatements.Add(new XElement("statement", statement));
            }

            if (confirmationEmailViewModel.OrderShippedSelected)
            {
                statement = Resources.GetString("ORDERCONFIRMATIONEMAIL_ORDERSHIPPEDSTATEMENT");
                xeStatements.Add(new XElement("statement", statement));
            }

            if (confirmationEmailViewModel.OrderIsGiftSelected)
            {
                statement = Resources.GetString("ORDERCONFIRMATIONEMAIL_ORDERISGIFTSTATEMENT");
                xeStatements.Add(new XElement("statement", statement));
            }

            if (confirmationEmailViewModel.ContactMeAboutPaymentSelected)
            {
                statement = Resources.GetString("ORDERCONFIRMATIONEMAIL_CONTACTMEABOUTPAYMENTSTATEMENT");
                xeStatements.Add(new XElement("statement", statement));
            }

            if (confirmationEmailViewModel.ContactMeAboutProcessingSelected)
            {
                statement = Resources.GetString("ORDERCONFIRMATIONEMAIL_CONTACTMEABOUTPROCESSINGSTATEMENT");
                xeStatements.Add(new XElement("statement", statement));
            }

            if (confirmationEmailViewModel.ContactMeAsDesiredSelected)
            {
                statement = Resources.GetString("ORDERCONFIRMATIONEMAIL_CONTACTMEASDESIREDSTATEMENT");
                xeStatements.Add(new XElement("statement", statement));
            }

            if (confirmationEmailViewModel.ContactMeAboutDeliverySelected)
            {
                statement = Resources.GetString("ORDERCONFIRMATIONEMAIL_CONTACTMEABOUTDELIVERYSTATEMENT");
                xeStatements.Add(new XElement("statement", statement));
            }

            if (confirmationEmailViewModel.ProposedDeliveryScheduleSelected)
            {
                statement = string.Format(Resources.GetString("ORDERCONFIRMATIONEMAIL_PROPOSEDDELIVERYSCHEDULESTATEMENTFORMAT"), confirmationEmailViewModel.ProposedDeliveryScheduleDate, confirmationEmailViewModel.ProposedDeliveryScheduleTime);
                xeStatements.Add(new XElement("statement", statement));
            }

            var statementsXml = xeStatements.ToString();

            return statementsXml;
        }
    }
}