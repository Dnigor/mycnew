using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MaryKay.Configuration;
using Microsoft.Practices.ServiceLocation;
using myCustomers.Contexts;

namespace myCustomers.Web
{
    [AttributeUsageAttribute(AttributeTargets.Class|AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class AuthorizeConsultantAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!base.AuthorizeCore(httpContext))
                return false;

            var consultantContext = ServiceLocator.Current.GetInstance<IConsultantContext>();
            var consultant = consultantContext.Consultant;
            if (consultant == null)
                return false;

            var subsidiaryAccessor = ServiceLocator.Current.GetInstance<ISubsidiaryAccessor>();
            var subsidiaryCode = subsidiaryAccessor.GetSubsidiaryCode();
            if (consultant.SubsidiaryCode != subsidiaryCode)
                return false;

            return true;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var consultantContext = ServiceLocator.Current.GetInstance<IConsultantContext>();
            consultantContext.Clear();

            FormsAuthentication.RedirectToLoginPage();

            base.HandleUnauthorizedRequest(filterContext);
        }
    }
}
