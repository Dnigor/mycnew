using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using MaryKay.Configuration;

namespace myCustomers.Web
{
    [AttributeUsageAttribute(AttributeTargets.Class|AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class EnvironmentAttribute : AuthorizeAttribute
    {
        string[] _environments;

        public EnvironmentAttribute(string environments)
        {
            _environments = environments.Split(',').Select(e => e.Trim()).ToArray();
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.HttpContext.Response.StatusCode = 404;
            filterContext.Result = new ContentResult
            {
                Content         = "",
                ContentEncoding = Encoding.UTF8,
                ContentType     = "text/html"
            };
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var environment = ConfigurationManager.AppSettings["Environment"];

            foreach (var allowedEnvironment in _environments)
                if (allowedEnvironment.Equals(environment, StringComparison.InvariantCultureIgnoreCase))
                    return true;

            return false;
        }
    }
}
