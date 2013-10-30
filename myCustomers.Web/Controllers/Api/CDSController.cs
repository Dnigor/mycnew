using System;
using System.Web;
using System.Web.Http;
using MaryKay.Configuration;
using myCustomers.Contexts;
using myCustomers.Services.CDS;
using NLog;

namespace myCustomers.Web.Controllers.Api
{
    public class CDSController : ApiController
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        readonly IAppSettings _appSettings;
        readonly IConsultantContext _consultantContext;
        readonly IQuartetClientFactory _clientFactory;
        readonly ICDSIntegrationService _checkoutService;

        public CDSController
        (
            IAppSettings appSettings, 
            IConsultantContext consultantContext, 
            IQuartetClientFactory clientFactory,
            ICDSIntegrationService checkoutService
        )
        {
            _appSettings       = appSettings;
            _consultantContext = consultantContext;
            _clientFactory     = clientFactory;
            _checkoutService   = checkoutService;
        }

        [AcceptVerbs("POST")]
        public dynamic Submit(SubmitModel model)
        {
            var query = _clientFactory.GetCustomersQueryServiceClient();

            var order = query.GetOrderById(model.OrderId);
            if (order == null)
                throw new InvalidOperationException("Order not found");

            var customer = query.GetCustomer(order.CustomerId);
            if (customer == null) throw new InvalidOperationException("Customer not found");

            var response = _checkoutService.PostOrderData(_consultantContext.Consultant, customer, order);

            switch (response.Status)
            {
                case CDSStatusCode.Success:
                    if (response.IsPaymentApproved)
                    {
                        // payment was already approved so tell the client to just redirect to the cds complete
                        // url which will ensure the order is updated
                        var baseUrl = new Uri(_appSettings.GetValue("CDS.BaseReturnUrl"));
                        var returnUrl = new Uri(baseUrl, VirtualPathUtility.ToAbsolute(string.Format("~/cds/complete/{0}", model.OrderId)));
                        return new
                        {
                            response.Status,
                            response.IsPaymentApproved,
                            response.Exception,
                            redirectUrl = returnUrl
                        };
                    }
                    else
                    {
                        // payment has not yet been completed for this order
                        // so tell the client to redirect to Express Checkout
                        return new
                        {
                            response.Status,
                            response.IsPaymentApproved,
                            redirectUrl = GetExpressCheckoutUrl(model.OrderId)
                        };
                    }

                case CDSStatusCode.Error:
                    return new
                    {
                        response.OrderId,
                        response.Status,
                        response.IsPaymentApproved,
                        response.Details,
                        response.Exception
                    };

                // should never hit this case since the json parse will fail if the status code doesnt match the enum values
                default:
                    return ApiHelpers.ServerError("Unknown express checkout code.");
            }
        }

        string GetExpressCheckoutUrl(Guid orderId)
        {
            var baseUrl     = new Uri(_appSettings.GetValue("CDS.BaseReturnUrl"));
            var returnUrl   = new Uri(baseUrl, VirtualPathUtility.ToAbsolute(string.Format("~/orders/edit/{0}", orderId)));
            var completeUrl = new Uri(baseUrl, VirtualPathUtility.ToAbsolute(string.Format("~/cds/complete/{0}", orderId)));
            var reviewUrl   = new UriBuilder(new Uri(new Uri(_appSettings.GetValue("CDS.BaseCheckoutUrl")), _appSettings.GetValue("CDS.ReviewUrl")));

            reviewUrl.Query = string.Format("orderId={0}&confirmationUrl={1}&returnUrl={2}&cancelUrl={2}&invalidShippingUrl={2}", orderId.ToString("N"), HttpUtility.UrlEncode(completeUrl.ToString()), HttpUtility.UrlEncode(returnUrl.ToString()));

            return reviewUrl.Uri.AbsoluteUri;
        }

        public sealed class SubmitModel
        {
            public Guid OrderId { get; set; }
        }
    }
}
