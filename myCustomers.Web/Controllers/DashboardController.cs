using System.Globalization;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MaryKay.Configuration;

namespace myCustomers.Web.Controllers
{
    [AuthorizeConsultant]
    public class DashboardController : Controller
    {
        readonly IAppSettings _appSettings;
        readonly ISubsidiaryAccessor _subsidiaryAccessor;

        public DashboardController(IAppSettings appSettings, ISubsidiaryAccessor subsidiaryAccessor)
        {
            _appSettings        = appSettings;
            _subsidiaryAccessor = subsidiaryAccessor;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Language(string id)
        {
            try
            {
                // validate by trying to construct a cultureinfo class
                var language = new CultureInfo(CultureMapper.MapCulture(id, _subsidiaryAccessor.GetSubsidiaryCode()));

                // use the unmapped culture value for the cookie for compatibility with InTouch
                SetCookie("uiculture", id);
                SetCookie("culture", id);
            }
            catch { }

            if (Request.UrlReferrer != null && !string.IsNullOrEmpty(Request.UrlReferrer.AbsolutePath))
                return Redirect(Request.UrlReferrer.ToString());
            else
                return RedirectToAction("Index");
        }

        string GetCookieDomain()
        {
            if (Request.Url.Host.Contains("localhost"))
                return string.Empty;

            return _appSettings.GetValue("CookieDomain");
        }

        void SetCookie(string name, string value)
        {
            if (Response.Cookies[name] != null)
                Response.Cookies.Remove(name);

            var cookie = new HttpCookie(name, value)
            {
                Path   = FormsAuthentication.FormsCookiePath,
                Domain = GetCookieDomain(),
            };

            Response.Cookies.Set(cookie);
        }
    }
}
