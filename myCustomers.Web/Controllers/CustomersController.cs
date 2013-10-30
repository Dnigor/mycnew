using System;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MaryKay.Configuration;
using myCustomers.Contexts;
using myCustomers.ET;
using myCustomers.Features;
using myCustomers.Globalization;
using myCustomers.VMO;
using myCustomers.Web.Models;
using myCustomers.Web.Services;
using Newtonsoft.Json;
using Quartet.Entities;
using System.Threading;
using System.Collections.Generic;

namespace myCustomers.Web.Controllers
{
    [AuthorizeConsultant]
    public class CustomersController : Controller
    {
        readonly IMappingService<Customer, CustomerDetailViewModel> _customerDetailMappingService;

        readonly IAppSettings           _appSettings;
        readonly IQuartetClientFactory  _clientFactory;
        readonly IConsultantContext     _consultantContext;
        readonly ICustomerExportService _customerListService;
        readonly IImportCustomerService _importCustomer;
        readonly IConfigService         _configService;
        readonly IVMOLinkComposer       _vmoLinkComposer;
        readonly IETLinkComposer        _etLinkComposer;
        readonly IFeaturesConfigService _featuresConfigService;

        public CustomersController
        (
            IMappingService<Customer, CustomerDetailViewModel> customerDetailMappingService,
            IAppSettings appSettings,
            IQuartetClientFactory clientFactory,
            IConsultantContext consultantContext,
            ICustomerExportService customerListService,
            IImportCustomerService importCustomer,
            IConfigService configService,
            IVMOLinkComposer vmoLinkComposer,
            IFeaturesConfigService featuresConfigService,
            IETLinkComposer etLinkComposer
        )
        {
            _appSettings                  = appSettings;
            _clientFactory                = clientFactory;
            _consultantContext            = consultantContext;
            _customerDetailMappingService = customerDetailMappingService;
            _customerListService          = customerListService;
            _importCustomer               = importCustomer;
            _configService                = configService;
            _vmoLinkComposer              = vmoLinkComposer;
            _featuresConfigService        = featuresConfigService;
            _etLinkComposer               = etLinkComposer;
        }

        public ActionResult Index()
        {
            var model = new CustomerListViewModel
            {
                ConsultantKey   = _consultantContext.Consultant.ConsultantKey.Value,
                Mode            = CustomerListMode.Search,
                TargetUrlFormat = Url.Action("detail", "customers", new { id = "_id_" }),
                Features        = _featuresConfigService.GetCustomerListFeatures()
            };

            return View(model);
        }

        public ActionResult Select(string r)
        {
            var model = new CustomerListViewModel
            {
                ConsultantKey   = _consultantContext.Consultant.ConsultantKey.Value,
                Mode            = CustomerListMode.Select,
                TargetUrlFormat = r,
                Features        = _featuresConfigService.GetCustomerListFeatures()
            };

            return View("index", model);
        }

        public ActionResult Detail(Guid id)
        {
            var customersQueryClient = _clientFactory.GetCustomersQueryServiceClient();
            var customer             = customersQueryClient.GetCustomer(id);

            if (customer == null)
                return new HttpNotFoundResult();

            var model = _customerDetailMappingService.Map(customer);
            model.Features = _featuresConfigService.GetCustomerDetailFeatures();
            
            ViewBag.VmoUrl = _vmoLinkComposer.GetCustomerVmoLink(customer, Request.Url);
            var etUri = _etLinkComposer.GetCustomerETLink(customer, Request.Url);

            // REVIEW: why ViewBag and not the model?
            ViewBag.ExternalETUrl = etUri != null ? etUri.ToString() : string.Empty;

            return View(model);
        }

        [HttpPost]
        public ActionResult Export([Bind(Prefix = "id[]")] Guid[] ids)
        {
            var model = new CustomerSelectionModel { Ids = ids };
            var fileName = string.Format(Resources.GetString("EXPORTCUSTOMERS_EXCEL_FILENAME"), DateTime.Now);
            Response.Headers.Remove("Content-Disposition");
            Response.Headers.Add("Content-Disposition", string.Format("attachment; filename={0}", fileName));
            Response.SetCookie(new HttpCookie("fileDownload", "true") { Path = "/" });

            var data = _customerListService.Export(model);
            if (data != null)
                return File(data, "application/vnd.ms-excel");
            else
                return new EmptyResult();            
        }

        [HttpPost]
        public ActionResult Print([Bind(Prefix = "id[]")] Guid[] ids)
        {
            var model = new CustomerSelectionModel { Ids = ids };
            var fileName = string.Format(Resources.GetString("EXPORTCUSTOMERS_PRINT_FILENAME"), DateTime.Now);
            Response.Headers.Remove("Content-Disposition");
            Response.Headers.Add("Content-Disposition", string.Format("attachment; filename={0}", fileName));
            Response.SetCookie(new HttpCookie("fileDownload", "true") { Path = "/" });

            var data = _customerListService.Print(model);
            if (data != null) 
                return File(data, "application/pdf");
            else 
                return new EmptyResult();
        }

        [HttpPost]
        public ActionResult Labels([Bind(Prefix = "id[]")] Guid[] ids)
        {
            var model = new CustomerSelectionModel { Ids = ids };
            var fileName = string.Format(Resources.GetString("EXPORTCUSTOMERS_LABELS_FILENAME"), DateTime.Now);
            Response.Headers.Remove("Content-Disposition");
            Response.Headers.Add("Content-Disposition", string.Format("attachment; filename={0}", fileName));
            Response.SetCookie(new HttpCookie("fileDownload", "true") { Path = "/" });

            var data = _customerListService.CreateLabels(model);
            if (data != null)
                return File(data, "application/pdf");
            else
                return new EmptyResult();
        }

        [HttpGet]
        public ActionResult Import()
        {            
            if (Request["code"] != null)
            {
                _importCustomer.GetFacebookContacts(Request["code"]);
                return RedirectToAction("ImportList");
            }
            else
                return RedirectToAction("Index");            
        }

		[HttpPost]
        public JsonResult UploadCSVFile()
		{
            var files = Request.Files;
            HttpPostedFileBase file = files[0];

            // TODO: refactor to return HTTP 200 for success and HTTP 500 for failure. 
            // Javascript should use AJAX error handler for failed state
            try
            {
                _importCustomer.GetContactsFromCSVFile(file);
                return Json(new { success = true }, "text/html", JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false, message = Resources.GetString("ERROR_CSV_FILE_PARSE") }, "text/html", JsonRequestBehavior.AllowGet);
            }
		}

        [HttpPost]
        public JsonResult ImportCustomers(PendingImportCustomerDTO[] pendingCustomers)
        {
            foreach (var pendingCustomer in pendingCustomers)
            {
                var matchedCustomers = _importCustomer
                    .UploadedCustomers
                    .Where(c => c.CustomerId == pendingCustomer.CustomerId)
                    .ToList();

                matchedCustomers.ForEach(c => c.Action = pendingCustomer.ImportAction);
            }

            var importedCustomers = _importCustomer.ImportUploadedCustomers();
            var json = Json(importedCustomers);

            return json;
        }

        public ActionResult ImportList() 
        {
            var model = _importCustomer.UploadedCustomers.Select(c => c.ToPendingImportCustomerDTO()).OrderBy(c => c.FirstName);
            return View("ImportList", model);
        }

		public void ImportFacebookFriends() 
		{
            Response.Redirect(_importCustomer.FacebookUrl);
		}

        public ActionResult Add()
        {
            var model = CreateEditCustomerViewModel();
            return View("Edit", model);
        }

        public ActionResult Edit(Guid id)
        {
            var customersQueryClient = _clientFactory.GetCustomersQueryServiceClient();
            var customer = customersQueryClient.GetCustomer(id);

            if (customer == null)
                return new HttpNotFoundResult();

            var customerDetails = _customerDetailMappingService.Map(customer);

            var model = CreateEditCustomerViewModel();
            
            model.CustomerId = customer.CustomerId.ToString();

            if (customerDetails.PictureLastUpdatedDateUtc.HasValue)
            {
                var parameters = new RouteValueDictionary
                {
                    { "customerId", id },
                    //{"h", height},
                    { "ts", customerDetails.PictureLastUpdatedDateUtc.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") }
                };

                model.CustomerProfilePictureUrl = UrlHelper.GenerateUrl("CustomerImages", "profile", "CustomerImages", parameters, RouteTable.Routes, ControllerContext.RequestContext, true);
            }

            return View("Edit", model);
        }

        [HttpPost]
        public JsonResult FilterCustomersForeCard(Guid[] customerIds)
        {
            List<string> result = new List<string>();
            var customers = _clientFactory.GetCustomersQueryServiceClient().QueryCustomerListByCustomerIds(customerIds);
            return Json(customers.Where(c => c.MkeCardsSubscriptionStatus == SubscriptionStatus.OptedIn)
                    .Select(c => new { Id = c.CustomerId })
                    .ToList());
        }

        EditCustomerViewModel CreateEditCustomerViewModel()
        {
            var preferredLanguages = _appSettings
                .GetValue("CustomerProfile.PreferredLanguages")
                .Split(',');

            var learnMoreAbout              = Resources.GetUISiteTermElements("InterestedToLearnMoreAbout");
            var contactDays                 = Resources.GetUISiteTermElements("ContactDaysPreference");
            var contactTimePreferences      = Resources.GetUISiteTermElements("ContactTimePreference");
            var contactFrequencyPreferences = Resources.GetUISiteTermElements("ContactFrequencyPreference");
            var contactMethodPreferences    = Resources.GetUISiteTermElements("ContactMethod");
            var shoppingLocationPreference  = Resources.GetUISiteTermElements("ShoppingLocationPreference");
            var occasion                    = Resources.GetUISiteTermElements("Occasion");

            var socialNetworks = _configService
                .GetConfig<SocialNetworksConfig>("SocialNetworks")
                .SocialNetworks
                .Select
                (
                    sn => 
                    {
                        sn.Icon = Url.Content(sn.Icon);
                        return sn;
                    }
                )
                .ToArray();

            var subscriptions = _configService
                .GetConfig<SubscriptionsConfig>("Subscriptions")
                .Subscriptions.ToArray();
               

            var model = new EditCustomerViewModel
            {
                Languages          = preferredLanguages,
                TopicsOfInterest   = learnMoreAbout,
                ContactDays        = contactDays,
                ContactTimes       = contactTimePreferences,
                ContactFrequencies = contactFrequencyPreferences,
                ContactMethods     = contactMethodPreferences,
                ShoppingMethods    = shoppingLocationPreference,
                Features           = _featuresConfigService.GetEditCustomerFeatures(),
                SocialNetworks     = socialNetworks,
                Subscriptions      = subscriptions 
            };

            return model;
        }
    }
}

