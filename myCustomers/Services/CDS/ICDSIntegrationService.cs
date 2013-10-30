using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Security;
using MaryKay.Configuration;
using MaryKay.IBCDataServices.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;
using Quartet.Entities;

namespace myCustomers.Services.CDS
{
    public interface ICDSIntegrationService
    {
        CDSResponse PostOrderData(Consultant consultant, Customer customer, Order order);
    }

    public sealed class CDSIntegrationService : ICDSIntegrationService
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        IAppSettings _appSettings;

        public CDSIntegrationService(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public CDSResponse PostOrderData(Consultant consultant, Customer customer, Order order)
        {
            var baseAddress = new Uri(_appSettings.GetValue("CDS.BaseCheckoutUrl"));
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(baseAddress, GetAuthCookie(_appSettings.GetValue("CDS.CookieDomain")));

            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer, Proxy = WebRequest.DefaultWebProxy })
            using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var formData = GetFormData(consultant, customer, order);
                var content = new FormUrlEncodedContent(formData);
                _logger.Trace("CDS Handshake Request: {0}", content);

                var postTask = client.PostAsync(_appSettings.GetValue("CDS.HandshakeUrl"), content);
                postTask.Wait();

                var postResult = postTask.Result;
                postResult.EnsureSuccessStatusCode();

                var readTask = postResult.Content.ReadAsStringAsync();
                readTask.Wait();

                var json = readTask.Result;
                _logger.Trace("CDS Handshake Response: {0}", json);

                try
                {
                    var result = JsonConvert.DeserializeObject<CDSResult>(json, new IsoDateTimeConverter());
                    return result.Response;
                }
                catch (JsonReaderException)
                {
                    _logger.Error("Failed to parse CDS response:\r\n{0}", json);
                    throw;
                }
            }
        }

        IEnumerable<KeyValuePair<string, string>> GetFormData(Consultant consultant, Customer customer, Order order)
        {
            if (consultant == null) throw new ArgumentNullException("consultant");
            if (customer == null) throw new ArgumentNullException("customer");
            if (order == null) throw new ArgumentNullException("order");
            if (order.Items == null || order.Items.Length == 0) throw new InvalidOperationException("Order contains no items");
            if (order.ShippingAddress == null) throw new InvalidOperationException("Order.ShippingAddress not specified.");

            var customerName = string.Format("{0} {1}", customer.ContactInformation.FirstName, customer.ContactInformation.LastName);
            var shippingAddress = order.ShippingAddress;

            var formData = new Dictionary<string, string>
            {
                { "order_id",              order.OrderId.ToString()                                                                         },
                { "order_type",            "salesticket"                                                                                    },
                { "purchaser_id",          consultant.ConsultantID                                                                          },
                { "purchaser_firstname",   consultant.FirstName                                                                             },
                { "purchaser_lastname",    consultant.LastName                                                                              },
                { "customer_id",           customer.LegacyContactId.HasValue ? customer.LegacyContactId.Value.ToString() : string.Empty     },
                { "customer_key",          customer.CustomerId.ToString()                                                                   },
                { "customer_firstName",    customer.ContactInformation.FirstName                                                            },
                { "customer_lastName",     customer.ContactInformation.LastName                                                             },
                { "customer_emailAddress", customer.EmailAddress != null ? customer.EmailAddress.Address : string.Empty                     },
                { "customer_language",     customer.ContactPreferences.PreferredLanguage ?? string.Empty                                    },
                { "shipto_name",           !string.IsNullOrWhiteSpace(shippingAddress.Addressee) ? shippingAddress.Addressee : customerName },
                { "shipto_streetAddress",  shippingAddress.Street                                                                           },
                { "shipto_addressLine2",   shippingAddress.UnitNumber                                                                       },
                { "shipto_city",           shippingAddress.City                                                                             },
                { "shipto_state",          shippingAddress.RegionCode                                                                       },
                { "shipto_zip",            shippingAddress.PostalCode                                                                       },
                { "shipto_phone",          shippingAddress.Telephone                                                                        },
                { "billto_name",           customerName                                                                                     },
                { "billto_address",        shippingAddress.Street                                                                           },
                { "billto_zip",            shippingAddress.PostalCode                                                                       },
            };

            if (order.GiftMessage != null && order.GiftMessage.IsGift)
                formData.Add("gift_message", order.GiftMessage.Message);

            for (var i = 0; i < order.Items.Length; i += 1)
            {
                var orderItem = order.Items[i];
                if (string.IsNullOrWhiteSpace(orderItem.PartId))
                    throw new InvalidOperationException("Order.Items[" + i.ToString() + "].PartId not specified.");

                if (i == 0)
                {
                    formData.Add("item_sku", orderItem.PartId);
                    formData.Add("item_quantity", orderItem.Quantity.ToString());
                }
                else
                {
                    formData.Add(string.Format("item_sku{0}", i + 1), orderItem.PartId);
                    formData.Add(string.Format("item_quantity{0}", i + 1), orderItem.Quantity.ToString());
                }
            }

            if (order.Payments != null)
            {
                var ccPayments = order.Payments.Where(p => p.PaymentType.HasValue && p.PaymentType.Value == PaymentType.CreditCard).ToArray();
                if (ccPayments.Length > 1) throw new InvalidOperationException("Orders with more than on credit card payment cannot be sent to CDS.");
                if (ccPayments.Length != 0)
                {
                    var payment = ccPayments[0];
                    if (payment.PaymentStatus == PaymentStatus.Approved || payment.PaymentStatus == PaymentStatus.Declined || payment.PaymentStatus == PaymentStatus.Error)
                        throw new InvalidOperationException("Existing credit card payment has an invalid status for CDS");

                    var creditCard = payment.CreditCard;
                    if (creditCard != null)
                    {
                        var expirationDate = new DateTime(Convert.ToInt32(creditCard.ExpYear), Convert.ToInt32(creditCard.ExpMonth), 1).AddMonths(1).AddDays(-1).ToString("M/d/yyyy");
                        formData.Add("billto_ccBrand", ((int)creditCard.Type).ToString());
                        formData.Add("billto_ccDisplayNumber", creditCard.Last4Digits);
                        formData.Add("billto_ccToken", creditCard.Token);
                        formData.Add("billto_expirationDate", expirationDate);
                    }
                }
            }

            return formData;
        }

        Cookie GetAuthCookie(string cookieDomain)
        {
            var authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            return new Cookie(FormsAuthentication.FormsCookieName, authCookie.Value, null, cookieDomain);
        }
    }

    internal sealed class CDSResult
    {
        public CDSResponse Response { get; set; }
    }

    public sealed class CDSResponse
    {
        public CDSStatusCode Status { get; set; }
        public string Message { get; set; }
        public Guid OrderId { get; set; }
        public bool IsPaymentApproved { get; set; }
        public CDSDetails Details { get; set; }
        public CDSException Exception { get; set; }
    }

    public sealed class CDSDetails
    {
        public Guid OrderId { get; set; }
        public CDSPayment Payment { get; set; }
    }

    public sealed class CDSPayment
    {
        public decimal AmountPaid { get; set; }
        public string ApprovalCode { get; set; }
        public string AVSResponseCode { get; set; }
        public string InvoiceId { get; set; }
        public CDSPaymentType PaymentType { get; set; }
        public DateTime? PreAuthDateUtc { get; set; }
        public DateTime? PostAuthDateUtc { get; set; }
        public string TransactionId { get; set; }
        public string TransactionStatus { get; set; }
        public CDSAddress BillingAddress { get; set; }
        public CDSCreditCard CreditCard { get; set; }

        public Payment ToQuartetPayment()
        {
            if (this.PaymentType != CDSPaymentType.CreditCard || this.BillingAddress == null || this.CreditCard == null)
                return null;

            var payment = new Payment
            {
                PaymentId         = Guid.NewGuid(),
                AmountPaid        = this.AmountPaid,
                PaymentDateUtc    = DateTime.UtcNow,
                PaymentStatus     = PaymentStatus.Approved,
                PaymentType       = Quartet.Entities.PaymentType.CreditCard,
                InvoiceId         = this.InvoiceId,
                ApprovalCode      = this.ApprovalCode,
                AVSResponseCode   = this.AVSResponseCode,
                TransactionId     = this.TransactionId,
                TransactionStatus = this.TransactionStatus,
                PreAuthDateUtc    = this.PreAuthDateUtc,
                PostAuthDateUtc   = this.PostAuthDateUtc,
                BillingAddress    = new Quartet.Entities.Address
                {
                    AddressKey = Guid.NewGuid(),
                    Addressee  = this.BillingAddress.Addressee,
                    Street     = this.BillingAddress.Street,
                    PostalCode = this.BillingAddress.PostalCode
                },
                CreditCard = new CreditCard
                {
                    CreditCardKey  = Guid.NewGuid(),
                    Token          = this.CreditCard.Token,
                    Type           = (CreditCardType)this.CreditCard.Type,
                    CardHolderName = this.CreditCard.CardHolderName,
                    Last4Digits    = this.CreditCard.Last4Digits,
                    ExpMonth       = this.CreditCard.ExpMonth,
                    ExpYear        = this.CreditCard.ExpYear
                }
            };

            return payment;
        }
    }

    public sealed class CDSAddress
    {
        public string Addressee { get; set; }
        public string Street { get; set; }
        public string PostalCode { get; set; }
    }

    public sealed class CDSCreditCard
    {
        public string CardHolderName { get; set; }
        public string Last4Digits { get; set; }
        public int ExpMonth { get; set; }
        public int ExpYear { get; set; }
        public string Token { get; set; }
        public short Type { get; set; }
    }

    public sealed class CDSException
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string InnerException { get; set; }
        public CDSExceptionDetails Details { get; set; }
    }

    public sealed class CDSExceptionDetails
    {
        public string Message { get; set; }
        public string[] Skus { get; set; }
        public IDictionary<string,string> PostedInput { get; set; }
    }

    public enum CDSStatusCode
    {
        Success = 100,
        Error = 200
    }

    public enum CDSPaymentType
    {
        CreditCard = 1,
        ProPayFunds = 3
    }
}
