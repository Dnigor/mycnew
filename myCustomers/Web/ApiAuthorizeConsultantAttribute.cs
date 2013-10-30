using System;
using System.Net;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Security;
using MaryKay.Configuration;
using Microsoft.Practices.ServiceLocation;
using myCustomers.Contexts;

namespace myCustomers.Web
{
    [AttributeUsageAttribute(AttributeTargets.Class|AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class ApiAuthorizeConsultantAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (!base.IsAuthorized(actionContext))
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

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            var consultantContext = ServiceLocator.Current.GetInstance<IConsultantContext>();
            consultantContext.Clear();

            throw new HttpResponseException(HttpStatusCode.Unauthorized);
        }
    }
}
