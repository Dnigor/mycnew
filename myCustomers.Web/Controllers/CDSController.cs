using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MaryKay.Configuration;
using MaryKay.IBCDataServices.Entities;
using myCustomers.Contexts;
using myCustomers.Services.CDS;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;
using Quartet.Entities;
using Quartet.Entities.Commands;

namespace myCustomers.Web.Controllers
{
    public class CDSController : Controller
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        readonly IAppSettings _appSettings;
        readonly IConsultantContext _consultantContext;
        readonly IQuartetClientFactory _clientFactory;
        readonly ICDSIntegrationService _cdsIntegrationService;

        public CDSController
        (
            IAppSettings appSettings,
            IConsultantContext consultantContext,
            IQuartetClientFactory clientFactory,
            ICDSIntegrationService cdsIntegrationService
        )
        {
            _appSettings           = appSettings;
            _consultantContext     = consultantContext;
            _clientFactory         = clientFactory;
            _cdsIntegrationService = cdsIntegrationService;
        }

        // NOTE: Orders are submitted to CDS through the WebAPI controller

        [HttpGet]
        public ActionResult Complete(Guid id)
        {
            var query = _clientFactory.GetCustomersQueryServiceClient();

            var order = query.GetOrderById(id);
            if (order == null)
                throw new InvalidOperationException(string.Format("Cannot complete CDS order for order id '{0}' because the order was not found.", id));

            if (!order.ShipCDS.HasValue || !order.ShipCDS.Value)
                throw new InvalidOperationException(string.Format("Cannot complete CDS order for order id '{0}' because 'ShipCDS' is false", id));

            if (order.OrderStatus == OrderStatus.Processed || order.OrderStatus == OrderStatus.ShippedDelivered)
                throw new InvalidOperationException(string.Format("Cannot complete CDS order for order id '{0}' because it is already processed.", id));

            var customer = query.GetCustomer(order.CustomerId);
            if (customer == null)
                throw new InvalidOperationException(string.Format("Cannot complete CDS order for order id '{0}' because customer {1} was not found.", id, order.CustomerId));

            var response = _cdsIntegrationService.PostOrderData(_consultantContext.Consultant, customer, order);
            if (response.Status != CDSStatusCode.Success)
                throw new InvalidOperationException(string.Format("Cannot complete CDS order for order id '{0}' because express checkout returned an error.", id));

            if (!response.IsPaymentApproved)
                throw new InvalidOperationException(string.Format("Cannot complete CDS order for order id '{0}' because payment was not approved.", id));

            var invoiceId    = response.Details.Payment.InvoiceId;
            var cdsPayment   = response.Details.Payment.ToQuartetPayment();
            var orderPayment = order.Payments.FirstOrDefault(p => p.PaymentType == PaymentType.CreditCard);

            var command = _clientFactory.GetCommandServiceClient();
            command.Execute(new UpdateCustomerCDSOrderId
            {
                OrderId    = response.OrderId,
                CDSOrderId = response.Details.OrderId
            });

            if (cdsPayment != null)
            {
                if (orderPayment == null)
                {
                    // create a new payment
                    command.Execute(new AddCustomerOrderPayment
                    {
                        OrderId = response.OrderId,
                        Payment = cdsPayment
                    });
                }
                else
                {
                    orderPayment.InvoiceId         = invoiceId;
                    orderPayment.AmountPaid        = cdsPayment.AmountPaid;
                    orderPayment.BillingAddress    = cdsPayment.BillingAddress;
                    orderPayment.CreditCard        = cdsPayment.CreditCard;
                    orderPayment.PaymentDateUtc    = cdsPayment.PaymentDateUtc;
                    orderPayment.PaymentStatus     = cdsPayment.PaymentStatus;
                    orderPayment.PaymentType       = cdsPayment.PaymentType;
                    orderPayment.ApprovalCode      = cdsPayment.ApprovalCode;
                    orderPayment.AVSResponseCode   = cdsPayment.AVSResponseCode;
                    orderPayment.TransactionId     = cdsPayment.TransactionId;
                    orderPayment.TransactionStatus = cdsPayment.TransactionStatus;
                    orderPayment.PreAuthDateUtc    = cdsPayment.PreAuthDateUtc;
                    orderPayment.PostAuthDateUtc   = cdsPayment.PostAuthDateUtc;

                    command.Execute(new UpdateCustomerOrderPayment
                    {
                        OrderId = response.OrderId,
                        Payment = orderPayment
                    });
                }
            }

            // update order statuses
            command.Execute(new UpdateCustomerOrderPaymentStatus
            {
                OrderId = response.OrderId,
                OrderPaymentStatus = OrderPaymentStatus.Completed
            });

            command.Execute(new UpdateCustomerOrderStatus
            {
                OrderId = response.OrderId,
                OrderStatus = OrderStatus.ShippedDelivered
            });

            return RedirectToAction("detail", "orders", new { id = id });
        }

        public ActionResult InvalidShipping(Guid orderId)
        {
            return RedirectToAction("edit", "orders", new { id = orderId });
        }
    }
}
