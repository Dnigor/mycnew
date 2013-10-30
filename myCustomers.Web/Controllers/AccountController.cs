using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MaryKay.Configuration;
using MaryKay.IBCDataServices.Entities;
using myCustomers.Contexts;
using myCustomers.Web.Models;

namespace myCustomers.Web.Controllers
{
    public class AccountController : Controller
    {
        const string UI_CULTURE_COOKIE = "uiculture";

        IConsultantContext                  _consultantContext;
        IConsultantDataServiceClientFactory _consultantServiceFactory;
        IConfigService                      _configService;
        IEnvironmentConfig                  _environmentConfig;
        IAppSettings                        _appSettings;

        public AccountController(IConsultantContext consultantContext, IConsultantDataServiceClientFactory consultantServiceFactory, IConfigService configService, IEnvironmentConfig environmentConfig, IAppSettings appSettings)
        {
            _consultantContext        = consultantContext;
            _consultantServiceFactory = consultantServiceFactory;
            _configService            = configService;
            _environmentConfig        = environmentConfig;
            _appSettings              = appSettings;
        }

        [HttpGet]
        public ActionResult Config()
        {
            var enabled = _appSettings.GetValue<bool>("Feature.MockLogin");
            if (!enabled)
                return new HttpNotFoundResult();

            try
            {
                var region = _environmentConfig.GetRegion();
                var config = _configService.GetConfig<MockLoginConfig>("MockLogin");

                var json = new
                {
                    subsidiaries = config.Subsidiaries
                        .Where(s => s.Region == region)
                        .GroupBy(s => s.SubsidiaryCode)
                        .ToDictionary
                        (
                            s => s.Key,
                            s => s.Select
                            (
                                g => new
                                {
                                    culture          = g.Culture,
                                    defaultUICulture = g.DefaultUICulture,
                                    uiCultures       = g.UICultures.Split(',').Select(c => c.Trim()).ToArray(),
                                    testAccounts     = g.TestAccounts != null ? g.TestAccounts.Split(',').Select(ta => ta.Trim()).ToArray() : null
                                }
                            ).Single()
                        )
                };

                return Json(json, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult Login()
        {
            var enabled = _appSettings.GetValue<bool>("Feature.MockLogin");
            if (enabled)
                return View();
            else
                return Redirect(_appSettings.GetValue("InTouch.LoginUrl"));
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            var enabled = _appSettings.GetValue<bool>("Feature.MockLogin");
            if (!enabled)
                return new HttpUnauthorizedResult();

            if (ModelState.IsValid)
            {
                var consultantServiceClient = _consultantServiceFactory.GetConsultantServiceClient();
                var consultant = consultantServiceClient.GetConsultant(model.ConsultantID, model.SubsidiaryCode);

                if (consultant != null)
                {
                    _consultantContext.Clear();

                    SetCookies(consultant, model.UICulture, model.UICulture);

                    if (returnUrl == null)
                        return RedirectToAction("index", "dashboard");

                    return Redirect(returnUrl);
                }
                else
                    ModelState.AddModelError("", "Consultant not found");
            }

            return View();
        }

        [HttpPost]
        public ActionResult Logout()
        {
            _consultantContext.Clear();
            Session.RemoveAll();

            // remove all cookies
            foreach (var cookie in Request.Cookies.AllKeys)
                DeleteCookie(cookie);

			return RedirectToAction("login");
        }

		[HttpPost]
		public ActionResult InTouchLogout()
		{
            _consultantContext.Clear();
            Session.RemoveAll();

            // remove all cookies
            foreach (var cookie in Request.Cookies.AllKeys)
                DeleteCookie(cookie);
           
            var inTouchBaseUrl = new Uri(Request.Url, _appSettings.GetValue("InTouch.BaseUrl"));
			var logoutUrl = new Uri(inTouchBaseUrl, _appSettings.GetValue("InTouch.Logout"));

			return Redirect(logoutUrl.ToString());
		}

        string GetCookieDomain()
        {
            if (Request.Url.Host.Contains("localhost"))
                return string.Empty;

            return _appSettings.GetValue("CookieDomain");
        }

        void SetCookies(Consultant consultant, string culture, string uiCulture)
		{
			var roles = new List<string>();
			if (consultant.Roles != null && consultant.Roles.Count > 0)
				roles = (from s in consultant.Roles select s.RoleName).ToList();

            roles.Add("Corporate");

            var now = DateTime.Now;
            var ticket = new FormsAuthenticationTicket(1, consultant.ID.ToString(), now, now.AddDays(1), false, string.Join(";", roles.ToArray()));

            SetCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
            SetCookie("culture", culture.ToString());
            SetCookie("uiculture", uiCulture.ToString());
            SetCookie("subsidiary", consultant.SubsidiaryCode);
		}

        void SetCookie(string name, string value, DateTime? expires = null)
        {
            if (Response.Cookies[name] != null)
                Response.Cookies.Remove(name);

            var cookie = new HttpCookie(name, value)
            {
                Path   = FormsAuthentication.FormsCookiePath,
                Domain = GetCookieDomain(),
            };

            if (expires.HasValue)
                cookie.Expires = expires.Value;

            Response.Cookies.Add(cookie);
        }

        void DeleteCookie(string name)
        {
            if (Response.Cookies[name] != null)
                Response.Cookies.Remove(name);

            var cookie = new HttpCookie(name)
            {
                Path    = FormsAuthentication.FormsCookiePath,
                Domain  = GetCookieDomain(),
                Expires = DateTime.Now.AddDays(-1)
            };

            Response.Cookies.Add(cookie);
        }

        static string Encrypt(string plaintextValue)
        {
            var plaintextBytes = Encoding.UTF8.GetBytes(plaintextValue);
            return MachineKey.Encode(plaintextBytes, MachineKeyProtection.All);
        }
    }
}
