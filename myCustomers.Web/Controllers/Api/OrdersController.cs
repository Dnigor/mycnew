using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using MaryKay.Configuration;
using myCustomers.Features;
using myCustomers.Web.Models;
using myCustomers.Web.Services;
using NLog;
using Quartet.Client.Products;
using Quartet.Entities;
using Quartet.Entities.Commands;
using Quartet.Entities.Search;

namespace myCustomers.Web.Controllers.Api
{
    [ApiAuthorizeConsultant]
    public class OrdersController : ApiController
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        readonly IAppSettings                           _appSettings;
        readonly IProductCatalogClientFactory           _productCatalogClientFactory;
        readonly IQuartetClientFactory                  _clientFactory;
        readonly IMappingService<Order, OrderViewModel> _orderMapping;
        readonly IFeaturesConfigService                 _featuresConfigService;

        public OrdersController
        (
            IAppSettings                            appSettings,
            IProductCatalogClientFactory            productCatalogClientFactory,
            IQuartetClientFactory                   clientFactory, 
            IMappingService<Order, OrderViewModel>  orderMapping,
            IFeaturesConfigService                  featuresConfigService
        )
        {
            _appSettings                 = appSettings;
            _productCatalogClientFactory = productCatalogClientFactory;
            _clientFactory               = clientFactory;
            _orderMapping                = orderMapping;
            _featuresConfigService       = featuresConfigService;
        }

        // GET ~/api/orders/{guid} - loads an existing order
        [AcceptVerbs("GET")]
        public OrderViewModel GetById(Guid id)
        {
            var queryService = _clientFactory.GetCustomersQueryServiceClient();

            var order = queryService.GetOrderById(id, false);
            if (order == null)
                throw new HttpResponseException(HttpStatusCode.NotFound); 

            var model = _orderMapping.Map(order);
            return model;
        }

        // GET ~/api/orders/{guid}?fields - loads an existing order
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetById(Guid id, string fields)
        {
            if (null == fields) fields = string.Empty;
            var orderSearch = _clientFactory.GetOrderSearchClient();
            var json = orderSearch.GetById(id, false, fields.Split(','));
            return ApiHelpers.JsonResponseMessage(json);
        }

        // GET ~/api/orders?q=&p=&ps= etc for search
        [AcceptVerbs("GET")]
        public HttpResponseMessage Search(SearchModel model)
        {
            if (ModelState.IsValid)
            {
                string[] productIds = null;
                if (!string.IsNullOrWhiteSpace(model.p))
                {
                    var productCatalog = _productCatalogClientFactory.GetProductCatalogClient();
                    var productSearchResult = productCatalog.Search(new ProductQuery
                    {
                        SearchField = ProductQuery.SearchFields.LimitedFields,
                        SearchTerms = model.p,
                        PageSize    = 50,
                    });

                    if (productSearchResult == null || productSearchResult.TotalCount == 0)
                        return ApiHelpers.JsonResponseMessage(@"{ ""hits"": { ""total"": 0, ""hits"": [] } }");

                    productIds = productSearchResult.Products.Select(r => r.ProductId).ToArray();
                }

                var criteria = new OrderCriteria
                {
                    QueryString     = model.q,
                    CustomerId      = model.cid,
                    OrderSource     = model.src,
                    OrderStatus     = model.sts,
                    PaymentStatus   = model.ps,
                    ProductIds      = productIds,
                    MinOrderDateUtc = model.ods.HasValue ? model.ods.Value.ToUniversalTime() : (DateTime?)null,
                    MaxOrderDateUtc = model.ode.HasValue ? model.ode.Value.ToUniversalTime() : (DateTime?)null,
                    IsDeleted       = model.d,
                    QueryFields     = model.qf,
                    ResultFields    = model.rf,
                    Page            = model.i - 1,
                    PageSize        = model.s,
                    IsArchived      = model.a
                };

                if (!string.IsNullOrWhiteSpace(model.sf))
                    criteria.Sort = new[] { new Sort { Field = model.sf, Descending = model.sd ?? false } };

                var orderSearch = _clientFactory.GetOrderSearchClient();

                var json = orderSearch.Search(criteria);

                return ApiHelpers.JsonResponseMessage(json);
            }

            throw ApiHelpers.ServerError(ModelState);
        }

        // POST ~/api/orders/ - creates a new order
        [AcceptVerbs("POST")]
        public dynamic CreateSalesTicket(CreateSalesTicketModel model)
        {
            if (!ModelState.IsValid)
                throw ApiHelpers.ServerError(ModelState);

            if (model.Items == null)
                model.Items = new CreateSalesTicketModel.LineItem[] { };

            if (model.CashPayments == null)
                model.CashPayments = new CreateSalesTicketModel.CashPayment[] { };

            if (model.CheckPayments == null)
                model.CheckPayments = new CreateSalesTicketModel.CheckPayment[] { };

            if (model.Followups == null)
                model.Followups = new FollowUpItem[] { };

            var cashPayments = model.CashPayments.Select(p => new Payment
            {
                PaymentId      = Guid.NewGuid(),
                PaymentType    = PaymentType.Cash,
                AmountPaid     = p.Amount,
                PaymentDateUtc = p.PaymentDateUtc
            });

            var checkPayments = model.CheckPayments.Select(p => new Payment
            {
                PaymentId      = Guid.NewGuid(),
                PaymentType    = PaymentType.PersonalCheck,
                AmountPaid     = p.Amount,
                CheckNumber    = p.CheckNumber,
                PaymentDateUtc = p.PaymentDateUtc
            });

            var command = new CreateSalesTicket
            {
                CommandId                    = Guid.NewGuid(),
                CustomerId                   = model.CustomerId,
                OrderId                      = Guid.NewGuid(),
                OrderDateUtc                 = model.OrderDateUtc,
                OrderStatus                  = model.OrderStatus,
                PaymentStatus                = model.PaymentStatus,
                ShipCDS                      = model.ShipCDS,
                CatalogName                  = _appSettings.GetValue("ProductCatalog.CatalogName"),
                IsGift                       = model.IsGift,
                GiftMessage                  = model.GiftMessage,
                Notes                        = model.Notes,
                DeliveryDateUtc              = model.DeliveryDateUtc,
                DeliveryAddress              = model.DeliveryAddress,
                Payments                     = cashPayments.Concat(checkPayments).ToArray(),
                Followups                    = model.Followups.Distinct().ToArray(),
                AddDeliveryAddressToCustomer = model.AddDeliveryAddressToCustomer,

                Items = model.Items.Select(i => new CreateSalesTicket.LineItem
                {
                    ProductId              = i.Id,
                    Price                  = i.Price,
                    Qty                    = i.Qty,
                    UseupRateInDays        = i.UseupRate
                }).ToArray()
            };

            if (command.DeliveryAddress != null && (command.DeliveryAddress.AddressKey == Guid.Empty || model.AddDeliveryAddressToCustomer))
            {
                command.DeliveryAddress.IsPrimary = false;
                command.DeliveryAddress.AddressKey = Guid.NewGuid();
            }

            var commandService = _clientFactory.GetCommandServiceClient();
            var queryService = _clientFactory.GetCustomersQueryServiceClient();

            try
            {
                commandService.Execute(command);

                return new
                {
                    id = command.OrderId,
                    links = new
                    {
                        self = new { href = Url.Route("DefaultApi", new { id = command.OrderId }) },
                        detail = new { href = Url.Route("Default", new { controller = "orders", action = "detail", id = command.OrderId }) }
                    }
                };
            }
            catch (CommandException ex)
            {
                throw ApiHelpers.ServerError(ex.Message, string.Join("\r\n", ex.Errors));
            }
        }

        // POST ~/api/orders/{guid} - updates an existing order
        [AcceptVerbs("POST")]
        public dynamic Save(Guid id, SaveOrderModel model)
        {
            var commandService = _clientFactory.GetCommandServiceClient();
            var queryService   = _clientFactory.GetCustomersQueryServiceClient();
            var order          = queryService.GetOrderById(id);

            if (!ModelState.IsValid)
                throw ApiHelpers.ServerError(ModelState);

            if (model.Items == null)
                model.Items = new SaveOrderModel.LineItem[] { };

            if (model.CashPayments == null)
                model.CashPayments = new SaveOrderModel.CashPayment[] { };

            if (model.CheckPayments == null)
                model.CheckPayments = new SaveOrderModel.CheckPayment[] { };

            if (model.DeletedCCPaymentIds == null)
                model.DeletedCCPaymentIds = new HashSet<Guid>();

            if (model.Followups == null)
                model.Followups = new FollowUpItem[] { };

            var cashPayments = model.CashPayments.Select(p => new Payment
            {
                PaymentId      = p.PaymentId == Guid.Empty ? Guid.NewGuid() : p.PaymentId,
                PaymentType    = PaymentType.Cash,
                AmountPaid     = p.Amount,
                PaymentDateUtc = p.PaymentDateUtc
            });

            var checkPayments = model.CheckPayments.Select(p => new Payment
            {
                PaymentId      = p.PaymentId == Guid.Empty ? Guid.NewGuid() : p.PaymentId,
                PaymentType    = PaymentType.PersonalCheck,
                AmountPaid     = p.Amount,
                CheckNumber    = p.CheckNumber,
                PaymentDateUtc = p.PaymentDateUtc
            });

            // Pass through CC payments since they cannot be changed by saving an order. They must be changed through the ccpayment web api
            // CC Payments that are not approved can be removed though so exclude those.
            var ccPayments = order.Payments.Where(p => p.PaymentType == PaymentType.CreditCard && !model.DeletedCCPaymentIds.Contains(p.PaymentId));
            
            var command = new SaveOrder
            {
                CommandId                    = Guid.NewGuid(),
                OrderId                      = id,
                OrderDateUtc                 = model.OrderDateUtc,
                OrderStatus                  = model.OrderStatus,
                PaymentStatus                = model.PaymentStatus,
                ShipCDS                      = model.ShipCDS,
                CatalogName                  = _appSettings.GetValue("ProductCatalog.CatalogName"),
                IsGift                       = model.IsGift,
                GiftMessage                  = model.GiftMessage,
                Notes                        = model.Notes,
                DeliveryDateUtc              = model.DeliveryDateUtc,
                DeliveryAddress              = model.DeliveryAddress,
                Payments                     = cashPayments.Concat(checkPayments).Concat(ccPayments).ToArray(),
                Followups                    = model.Followups.Distinct().ToArray(),
                AddDeliveryAddressToCustomer = model.AddDeliveryAddressToCustomer,

                Items = model.Items.Select(i => new SaveOrder.LineItem
                {
                    ProductId              = i.Id,
                    Price                  = i.Price,
                    Qty                    = i.Qty,
                    UseupRateInDays        = i.UseupRate
                }).ToArray()
            };

            if (command.DeliveryAddress != null && (command.DeliveryAddress.AddressKey == Guid.Empty || model.AddDeliveryAddressToCustomer))
            {
                command.DeliveryAddress.IsPrimary = false;
                command.DeliveryAddress.AddressKey = Guid.NewGuid();
            }

            try
            {
                commandService.Execute(command);

                return new
                {
                    id = command.OrderId,
                    links = new
                    {
                        self = new { href = Url.Route("DefaultApi", new { id = command.OrderId }) },
                        detail = new { href = Url.Route("Default", new { controller = "orders", action = "detail", id = command.OrderId }) }
                    }
                };
            }
            catch (CommandException ex)
            {
                throw ApiHelpers.ServerError(ex.Message, string.Join("\r\n", ex.Errors));
            }
        }
        
        [FromUri]
        public class SearchModel
        {
            public string q { get; set; }       // search terms
            public string src { get; set; }     // order source
            public string sts { get; set; }     // order status
            public string ps { get; set; }      // payment status
            public Guid? cid { get; set; }      // customer id
            public bool? d { get; set; }        // deleted
            public bool? a { get; set; }        // archived
            public DateTime? ods { get; set; }  // min order date
            public DateTime? ode { get; set; }  // max order date
            public string p { get; set; }       // product name/id
            public string[] qf { get; set; }    // the fields to search in (null uses default _all field)
            public string[] rf { get; set; }    // the fields to include in the result (null returns all available fields)
            public string sf { get; set; }      // the field to sort on
            public bool? sd { get; set; }       // sort descending flag
            public int i { get; set; }          // page number
            public int s { get; set; }          // page size

            public SearchModel()
            {
                i = 1;
                s = 10;
            }
        }
    }
}
