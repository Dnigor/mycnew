using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using MaryKay.Configuration;
using Microsoft.Practices.ServiceLocation;
using NLog;

namespace myCustomers.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : HttpApplication
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();

        protected void Application_Start()
        {
            ContainerConfig.ConfigureContainer();
            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
			LicenseConfig.RegisterLicenses();
        }

        protected void Application_Error()
        {
            var ctx = HttpContext.Current;
            var error = ctx.Server.GetLastError();
            _logger.ErrorException("Unhandled Exception", error);
        }

        // TODO: move to http module
        public void Application_AuthenticateRequest()
        {
            if (Request.IsAuthenticated)
            {
                if (Request.Cookies[FormsAuthentication.FormsCookieName] != null && Request.Cookies[FormsAuthentication.FormsCookieName].Value.Length > 0)
                {
                    FormsAuthenticationTicket ticket;
                    try
                    {
                        ticket = FormsAuthentication.Decrypt(Request.Cookies[FormsAuthentication.FormsCookieName].Value);
                        var roles = ticket.UserData.Split(new[] { ';' });

                        Context.User = new GenericPrincipal(Context.User.Identity, roles);
                    }
                    catch (CryptographicException) {}
                }
            }
        }

        public void Application_BeginRequest()
        {
            if (Request.Cookies["uiculture"] != null && !string.IsNullOrEmpty(Request.Cookies["uiculture"].Value))
            {
                var subsidiary = ServiceLocator.Current.GetInstance<ISubsidiaryAccessor>().GetSubsidiaryCode();
                var culture = CultureMapper.MapCulture(Request.Cookies["uiculture"].Value, subsidiary);
                Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
            }
        }
    }
}