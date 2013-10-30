using System.Web.Mvc;
using MaryKay.Configuration;
using MaryKay.IBCDataServices.Entities;
using myCustomers.Contexts;

namespace myCustomers.Web.Controllers
{
    public class TermsController : Controller
    {
        IConsultantDataServiceClientFactory _clientFactory;
        Consultant _consultant;
        IAppSettings _appSettings;
       
        public TermsController(IConsultantDataServiceClientFactory clientFactory, IConsultantContext consultantContext, IAppSettings appSettings)
        {
            _clientFactory = clientFactory;
            _consultant    = consultantContext.Consultant;
            _appSettings   = appSettings;
        }

        [HttpGet]
        public ActionResult Index()
        {           
            return View();
        }

        [HttpPost]
        public ActionResult Index(bool accepted)
        {
            Session.Remove(AcceptTermsFilter.SessionKey);
            var consultantKey = _consultant.ConsultantKey;
            var client = _clientFactory.GetDisclaimerSeriviceClient();
            client.SaveDisclaimer(consultantKey.Value, _appSettings.GetValueOrThrow("Feature.AcceptTerms.DisclaimerKey"), true, _consultant.SubsidiaryCode);
            Session[AcceptTermsFilter.SessionKey] = accepted;
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
