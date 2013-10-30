using System.Threading;
using System.Web.Mvc;
using MaryKay.Configuration;
using myCustomers.Contexts;
using myCustomers.Web.Models;

namespace myCustomers.Web.Controllers
{
    [AuthorizeConsultant]
    public class OmnitureController : Controller
    {
        private readonly IConsultantContext _context;
        private readonly IAppSettings _setting;

        public OmnitureController(IConsultantContext context, IAppSettings setting)
        {
            _context = context;
            _setting = setting;
        }

        public ActionResult Index()
        {
            var consultant = _context.Consultant;
            if (consultant != null)
            {
				string[] page = Request.Path.ToLower().Split('/');
				string pageName = Request.Path.ToLower();
				if (page.Length > 4)
				{
					pageName = string.Format("{0}/{1}/{2}/{3}", page[0], page[1], page[2], page[3]);
				}

                var model = new SiteCatalystTagVM
                {
                    ConsultantID       = string.Concat(consultant.ConsultantID, consultant.SubsidiaryCode).ToLower(),
                    ActivityStatusCode = consultant.ConsultantStatus,
                    CareerLevelCode    = consultant.CareerLevelCode,
                    Unit               = consultant.Unit,
                    StartDate          = consultant.StartDate.Value.ToShortDateString(),
                    Birthday           = (consultant.BirthDate.HasValue) ? consultant.BirthDate.Value.ToShortDateString() : null,
                    PageHierarchy      = Request.AppRelativeCurrentExecutionFilePath.Substring(2).Replace("/", ","),
                    Channel            = _setting.GetValue("Omniture.Channel"),
                    EnvironmentDomain  = _setting.GetValue("Omniture.EnvironmentDomain"),
                    PageName           = pageName,
                    UICulture          = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToLower(),
                    WebServer          = Server.MachineName,
                    CurrencyCode       = _setting.GetValue("Omniture.CurrencyCode")
                };

                return PartialView("_omniture", model);
            }

            return Content(string.Empty);
        }
    }
}
