using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using MaryKay.Configuration;
using myCustomers.Contexts;
using myCustomers.Features;
using myCustomers.Services;
using myCustomers.Web.Models;
using myCustomers.Web.Services;
using NLog;
using Quartet.Entities;
using Quartet.Entities.Commands;

namespace myCustomers.Web.Controllers.Api
{
    [ApiAuthorizeConsultant]
    public class CCPaymentController : ApiController
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        readonly IAppSettings                           _appSettings;
        readonly IProductCatalogClientFactory           _productCatalogClientFactory;
        readonly IQuartetClientFactory                  _clientFactory;
        readonly IMappingService<Order, OrderViewModel> _orderMapping;
        readonly IFeaturesConfigService                 _featuresConfigService;
        readonly ICreditCardService                     _creditCardService;
        readonly IConsultantContext                     _consultantContext;

        public CCPaymentController
        (
            IAppSettings                            appSettings,
            IProductCatalogClientFactory            productCatalogClientFactory,
            IQuartetClientFactory                   clientFactory, 
            IMappingService<Order, OrderViewModel>  orderMapping,
            IFeaturesConfigService                  featuresConfigService,
            ICreditCardService                      creditCardService,
            IConsultantContext                      consultantContext
        )
        {
            _appSettings                        = appSettings;
            _productCatalogClientFactory        = productCatalogClientFactory;
            _clientFactory                      = clientFactory;
            _orderMapping                       = orderMapping;
            _featuresConfigService              = featuresConfigService;
            _creditCardService                  = creditCardService;
            _consultantContext                  = consultantContext;
        }

        /// <summary>
        /// Creates a new cc payment in the pending state and saves it to the order in Quartet
        /// </summary>
        /// <param name="orderId">The id of the order to add the pending payment to</param>
        /// <param name="model">The payment info posted from the client used to create a payment token</param>
        /// <returns>The tokenized pending payment data</returns>
        // REVIEW: We should require https at least in production
        // POST ~/api/orders/{orderId}/ccpayments/add
        [AcceptVerbs("POST")]
        public dynamic Save([FromUri] Guid orderId, [FromBody] SaveModel model)
        {
            var queryService = _clientFactory.GetCustomersQueryServiceClient();

            var paymentId = model.PaymentId ?? Guid.NewGuid();
            var expDate   = new DateTime(model.ExpYear, model.ExpMonth, 1);
            var token     = _creditCardService.CreateToken(model.CreditCardNumber, expDate).ToString("N").ToUpper();

            var ba = new Address
            {
                AddressKey = Guid.NewGuid(),
                Addressee  = model.CardHolderName,
                Street     = model.BillingAddress,
                PostalCode = model.BillingZip
            };

            var cc = new CreditCard
            {
                CreditCardKey  = Guid.NewGuid(),
                CardHolderName = model.CardHolderName,
                Type           = model.CardType,
                Last4Digits    = model.CreditCardNumber.Length >= 4 ? model.CreditCardNumber.Substring(model.CreditCardNumber.Length - 4) : model.CreditCardNumber,
                ExpMonth       = model.ExpMonth,
                ExpYear        = model.ExpYear,
                Token          = token
            };

            var payment = new Payment
            {
                PaymentId      = paymentId,
                PaymentDateUtc = DateTime.UtcNow,
                InvoiceId      = orderId.ToString("N").ToUpper().Substring(0,6) + "-" + paymentId.ToString("N").ToUpper().Substring(0,6),
                PaymentStatus  = PaymentStatus.Pending,
                PaymentType    = PaymentType.CreditCard,
                AmountPaid     = model.Amount,
                CreditCard     = cc,
                BillingAddress = ba
            };

            var commandService = _clientFactory.GetCommandServiceClient();

            if (model.PaymentId.HasValue)
                commandService.Execute(new UpdateCustomerOrderPayment
                {
                    OrderId = orderId,
                    Payment = payment
                });
            else
                commandService.Execute(new AddCustomerOrderPayment
                {
                    OrderId = orderId,
                    Payment = payment
                });

            return new OrderViewModel.CreditCardPayment
            {
                PaymentId         = payment.PaymentId,
                Type              = payment.CreditCard.Type,
                Amount            = payment.AmountPaid.Value,
                CardHolderName    = payment.CreditCard.CardHolderName,
                BillingAddress    = payment.BillingAddress.Street,
                BillingZip        = payment.BillingAddress.PostalCode,
                Last4Digits       = payment.CreditCard.Last4Digits,
                ExpMonth          = payment.CreditCard.ExpMonth,
                ExpYear           = payment.CreditCard.ExpYear,
                PaymentDateUtc    = payment.PaymentDateUtc.Value,
                Status            = payment.PaymentStatus,
                ApprovalCode      = payment.ApprovalCode,
                AVSResponseCode   = payment.AVSResponseCode,
                TransactionId     = payment.TransactionId,
                TransactionStatus = payment.TransactionStatus,
                InvoiceId         = payment.InvoiceId,
                Token             = payment.CreditCard.Token,
                PostAuthAmount    = payment.PostAuthAmount,
                PostAuthDateUtc   = payment.PostAuthDateUtc
            };
        }

        // POST ~/api/orders/{orderId}/ccpayments/process - creates a new order
        [AcceptVerbs("POST")]
        public dynamic Process([FromUri] Guid orderId, [FromBody] ProcessModel model)
        {
            if (!ModelState.IsValid)
                throw ApiHelpers.ServerError(ModelState);

            var queryService = _clientFactory.GetCustomersQueryServiceClient();
            var order        = queryService.GetOrderById(orderId);

            if (order == null)
                throw ApiHelpers.ServerError("Order not found");

            if (order.Payments == null)
                order.Payments = new Payment[] {};

            var payment = order.Payments.FirstOrDefault(p => p.PaymentId == model.PaymentId);
            if (payment == null)
                throw ApiHelpers.ServerError("Payment not found");
            else if (payment.PaymentStatus == PaymentStatus.Approved)
                throw ApiHelpers.ServerError("Payment has already been processed");

            Guid token;
            try
            {
                token = Guid.Parse(payment.CreditCard.Token);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw ApiHelpers.ServerError("Invalid payment token.");
            }

            var consultant = _consultantContext.Consultant;
            if (consultant.ConsultantCCAccount == null || string.IsNullOrWhiteSpace(consultant.ConsultantCCAccount.AccountNumber))
                throw ApiHelpers.ServerError("Consultant does not have a ProPay account.");

            var expDate       = new DateTime(payment.CreditCard.ExpYear, payment.CreditCard.ExpMonth, 1);
            var accountNumber = long.Parse(consultant.ConsultantCCAccount.AccountNumber);

            Exception error = null;
            try
            {
                var paymentResult = _creditCardService.AuthorizePayment(accountNumber, payment.InvoiceId, payment.AmountPaid.Value, token, payment.BillingAddress.Street, payment.BillingAddress.PostalCode, expDate);

                payment.AVSResponseCode   = paymentResult.AVSResponseCode;
                payment.ApprovalCode      = paymentResult.AuthCode;
                payment.TransactionId     = paymentResult.TransactionNumber > 0 ? paymentResult.TransactionNumber.ToString() : null;
                payment.TransactionStatus = paymentResult.TransactionStatus.ToString();
                payment.PaymentDateUtc    = DateTime.UtcNow;

                // 00 - Success
                // 69 – Duplicate invoice number (Transaction succeeded in a prior attempt within the previous 24 hours.  This return code should be handled as a success.  Details about the original transaction are included whenever a 69 response is returned.  These details include a repeat of the authcode, the original AVS response, and the original CVV 
                if (paymentResult.TransactionStatus == 0 || paymentResult.TransactionStatus == 69) // REVIEW: Where did these magic numbers come from?
                {
                    payment.PaymentStatus   = PaymentStatus.Approved;
                    payment.PostAuthDateUtc = DateTime.UtcNow;
                }
                else
                {
                    payment.PaymentStatus = PaymentStatus.Declined;
                }

                var commandService = _clientFactory.GetCommandServiceClient();
                commandService.Execute(new UpdateCustomerOrderPayment
                {
                    OrderId = orderId,
                    Payment = payment
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                payment.PaymentStatus = PaymentStatus.Error;
                error = ex;
            }

            return new
            {
                Status  = payment.PaymentStatus,
                Error   = error != null ? error.ToString() : null,
                Payment = new OrderViewModel.CreditCardPayment
                {
                    PaymentId         = payment.PaymentId,
                    Type              = payment.CreditCard.Type,
                    Amount            = payment.AmountPaid.Value,
                    CardHolderName    = payment.CreditCard.CardHolderName,
                    BillingAddress    = payment.BillingAddress.Street,
                    BillingZip        = payment.BillingAddress.PostalCode,
                    Last4Digits       = payment.CreditCard.Last4Digits,
                    ExpMonth          = payment.CreditCard.ExpMonth,
                    ExpYear           = payment.CreditCard.ExpYear,
                    PaymentDateUtc    = payment.PaymentDateUtc.Value,
                    Status            = payment.PaymentStatus,
                    ApprovalCode      = payment.ApprovalCode,
                    AVSResponseCode   = payment.AVSResponseCode,
                    TransactionId     = payment.TransactionId,
                    TransactionStatus = payment.TransactionStatus,
                    ProPayLink        = string.Format(_appSettings.GetValue("ProPay.TransactionDetailsLink"), payment.TransactionId),
                    InvoiceId         = payment.InvoiceId,
                    Token             = payment.CreditCard.Token,
                    PostAuthAmount    = payment.PostAuthAmount,
                    PostAuthDateUtc   = payment.PostAuthDateUtc
                }
            };
        }

        public sealed class SaveModel
        {
            public Guid? PaymentId { get; set; }
            public CreditCardType CardType { get; set; }
            public string CardHolderName { get; set; }
            public string BillingAddress { get; set; }
            public string BillingZip { get; set; }
            public string CreditCardNumber { get; set; }
            public int ExpMonth { get; set; }
            public int ExpYear { get; set; }
            public decimal Amount { get; set; }
        }

        public sealed class ProcessModel
        {
            public Guid PaymentId { get; set; }
        }
    }
}
