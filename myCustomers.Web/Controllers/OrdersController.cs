using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MaryKay.Configuration;
using IBCDS = MaryKay.IBCDataServices.Entities;
using myCustomers.Contexts;
using myCustomers.Features;
using myCustomers.Web.Models;
using myCustomers.Web.Services;
using Quartet.Entities;
using myCustomers.Services;
using Quartet.Client.Products;
using Quartet.Entities.Products;

namespace myCustomers.Web.Controllers
{
    [AuthorizeConsultant]
    public class OrdersController : Controller
    {
        readonly IQuartetClientFactory                      _clientFactory;
        readonly IMappingService<Order, OrderViewModel>     _orderMapping;
        readonly IMappingService<Customer, HCardViewModel>  _hcardMapping;
        readonly IMappingService<Address, AddressViewModel> _addressMapping;
        readonly IMailingService                            _mailingService;
        readonly IInvoicingService                          _invoicingService;
        readonly IFeaturesConfigService                     _featuresConfigService;
        readonly IAppSettings                               _appSettings;
        readonly IConsultantContext                         _consultantContext;
        readonly IConsultantDataServiceClientFactory        _consultantDataServiceClientFactory;
        readonly IPromotionService                          _promotionService;
        readonly IProductCatalogClientFactory               _productCatalogClientFactory;
        readonly IInventoryService                          _inventoryService;
        readonly ISubsidiaryAccessor                        _subsidiaryAccessor;

        public OrdersController
        (
            IQuartetClientFactory                       clientFactory,
            IMappingService<Order,    OrderViewModel>   orderMapping,
            IMappingService<Customer, HCardViewModel>   hcardMapping,
            IMappingService<Address,  AddressViewModel> addressMapping,
            IMailingService                             mailingService,
            IInvoicingService                           invoicingService,
            IFeaturesConfigService                      featuresConfigService,
            IAppSettings                                appSettings,
            IConsultantContext                          consultantContext,
            IConsultantDataServiceClientFactory         consultantDataServiceClientFactory,
            IPromotionService                           promotionService,
            IProductCatalogClientFactory                productCatalogClientFactory,
            IInventoryService                           inventoryService,
            ISubsidiaryAccessor                         subsidiaryAccessor
        )
        {
            _clientFactory                      = clientFactory;
            _orderMapping                       = orderMapping;
            _hcardMapping                       = hcardMapping;
            _addressMapping                     = addressMapping;
            _mailingService                     = mailingService;
            _invoicingService                   = invoicingService;
            _featuresConfigService              = featuresConfigService;
            _appSettings                        = appSettings;
            _consultantContext                  = consultantContext;
            _consultantDataServiceClientFactory = consultantDataServiceClientFactory;
            _promotionService                   = promotionService;
            _productCatalogClientFactory        = productCatalogClientFactory;
            _inventoryService                   = inventoryService;
            _subsidiaryAccessor                 = subsidiaryAccessor;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Detail(Guid id)
        {
            var queryService = _clientFactory.GetCustomersQueryServiceClient();

            var order = queryService.GetOrderById(id);
            if (order == null)
                return HttpNotFound();

            var customer = queryService.GetCustomer(order.CustomerId);
            if (customer == null)
                return HttpNotFound();

            var model = new OrderDetailViewModel
            {
                Customer        = _hcardMapping.Map(customer),
                Order           = _orderMapping.Map(order),
                Features        = _featuresConfigService.GetOrderFeatures(),
                CurrencyFormat  = _appSettings.GetValue("Globalization.CurrencyFormat")
            };

            return View(model);
        }

        bool GetIsEligibleForCDS(IBCDS.Consultant consultant)
        {
            var consultantDataService = _consultantDataServiceClientFactory.GetConsultantServiceClient();
            return consultantDataService.IsConsultantEligibleForCDS(consultant.ConsultantKey.Value, consultant.SubsidiaryCode);
        }

        [HttpGet]
        public ActionResult Add(Guid customerId)
        {
            var queryService = _clientFactory.GetCustomersQueryServiceClient();

            var customer = queryService.GetCustomer(customerId);
            if (customer == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var addresses = customer.Addresses != null ? customer.Addresses.Values.Select(a => _addressMapping.Map(a)).ToArray() : new AddressViewModel[] { };

            var consultant = _consultantContext.Consultant;
            var features = _featuresConfigService.GetOrderFeatures();

            var model = new EditOrderViewModel
            {
                Consultant             = consultant,
                Customer               = _hcardMapping.Map(customer),
                Addresses              = addresses,
                Features               = features,
                IsEligibleForCDS       = features.CDS && GetIsEligibleForCDS(consultant),
                HasActiveProPayAccount = consultant.ConsultantCCAccount != null && !string.IsNullOrWhiteSpace(consultant.ConsultantCCAccount.AccountNumber),
                GiftWithPurchase       = features.GWP ? GetGWPConfig() : null
            };

            return View("Edit", model);
        }

        [HttpGet]
        public ActionResult Edit(Guid id)
        {
            var queryService = _clientFactory.GetCustomersQueryServiceClient();

            var order = queryService.GetOrderById(id, false);
            if (order == null)
                return HttpNotFound();

            var customer = queryService.GetCustomer(order.CustomerId);
            if (customer == null)
                return HttpNotFound();

            var addresses = customer.Addresses != null ? customer.Addresses.Values.Select(a => _addressMapping.Map(a)).ToArray() : new AddressViewModel[] { };

            var consultant = _consultantContext.Consultant;
            var features = _featuresConfigService.GetOrderFeatures();

            var model = new EditOrderViewModel
            {
                Consultant             = consultant,
                OrderId                = order.OrderId,
                Customer               = _hcardMapping.Map(customer),
                Addresses              = addresses,
                Features               = features,
                IsArchived             = order.IsArchived,
                IsEligibleForCDS       = features.CDS && GetIsEligibleForCDS(consultant),
                HasActiveProPayAccount = consultant.ConsultantCCAccount != null && !string.IsNullOrWhiteSpace(consultant.ConsultantCCAccount.AccountNumber),
                GiftWithPurchase       = features.GWP ? GetGWPConfig() : null
            };

            return View("Edit", model);        
        }

        [HttpPost]
        public JsonResult SendConfirmationEmail(ConfirmationEmailViewModel confirmationEmailViewModel)
        {
            _mailingService.SendConfirmationEmail(confirmationEmailViewModel);
            return Json(new { result = true });
        }

        [HttpPost]
        public FileContentResult CreateInvoice(CreateInvoiceViewModel createInvoiceViewModel)
        {
            var model = _invoicingService.Export(createInvoiceViewModel);

            Response.Headers.Remove("Content-Disposition");
            Response.Headers.Add("Content-Disposition", string.Format("attachment; filename={0}", model.FileName));
            Response.SetCookie(new HttpCookie("fileDownload", "true") { Path = "/" });

            return File(model.Content, model.ContentType);
        }

        EditOrderViewModel.GiftWithPurchaseConfig GetGWPConfig()
        {
            var promotions = _promotionService.GetActivePromotions();

            if (promotions != null)
            {
                var promotion = promotions.FirstOrDefault(p => p.PromotionCode == "GWP");
                if (promotion != null)
                {
                    var productQuery = _productCatalogClientFactory.GetProductCatalogClient();
                    var products = productQuery.GetByProductIds(promotion.ProductId).Products;
                    var product = products != null ? products.FirstOrDefault() : null;
                    if (product != null)
                    {
                        var today            = DateTime.Now.Date;
                        var subsidiaryCode   = _subsidiaryAccessor.GetSubsidiaryCode();
                        var unavailableParts = _inventoryService.GetUnavailableParts(subsidiaryCode);
                        var available        = !unavailableParts.Contains(product.PartId);
                        var expired          = product.ExpirationDate.HasValue && product.ExpirationDate.Value <= today;

                        return new EditOrderViewModel.GiftWithPurchaseConfig
                        {
                            CatalogName         = _appSettings.GetValue("ProductCatalog.CatalogName"),
                            ProductId           = product.ProductId,
                            PartId              = product.PartId,
                            DisplayPartId       = product.DisplayPartId,
                            Name                = product.DisplayName,
                            Description         = product.Description,
                            Shade               = product.ShadeName,
                            MinOrderAmount      = promotion.EstimatedAmountThreshold,
                            ImageUrl            = MapImageUrl(product.HeroListImagePath),
                            AvailableForCDS     = available && !expired
                        };
                    }
                }
            }

            return null;
        }

        string MapImageUrl(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return VirtualPathUtility.ToAbsolute(_appSettings.GetValue("ProductCatalog.DefaultImageUrl"));

            try
            {
                var baseUrl = new Uri(Request.Url, _appSettings.GetValue("ProductCatalog.BaseImageUrl"));
                var url = new Uri(baseUrl, imageUrl);
                return url.ToString();
            }
            catch
            {
                return null;
            }
        }
    }
}
