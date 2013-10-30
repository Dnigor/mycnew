using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MaryKay.Configuration;
using myCustomers.Contexts;

namespace myCustomers.Web
{    
    public class AcceptTermsFilter : ActionFilterAttribute
    {
        public static string SessionKey
        {
            get { return HttpContext.Current.User.Identity.Name + "_Terms.Accepted"; }
        }

        bool IsDisclaimerAccepted(ActionExecutingContext filterContext) 
        {
            // check cached value first
            // REVIEW: perhaps we should use a cookie for this?
            var value = filterContext.HttpContext.Session[SessionKey] as bool?;

            if (!value.HasValue)
            {
                var appSettings       = DependencyResolver.Current.GetService<IAppSettings>();
                var consultantContext = DependencyResolver.Current.GetService<IConsultantContext>();
                var clientFactory     = DependencyResolver.Current.GetService<IConsultantDataServiceClientFactory>();
                var consultant        = consultantContext.Consultant;
                var consultantKey     = consultant.ConsultantKey ?? Guid.Empty;
                var disclaimerKey     = appSettings.GetValueOrThrow("Feature.AcceptTerms.DisclaimerKey");
                var disclaimerService = clientFactory.GetDisclaimerSeriviceClient();
                var disclaimer        = disclaimerService.SelectDisclaimer(consultantKey, disclaimerKey, consultant.SubsidiaryCode);

                value = disclaimer != null;

                // cache in session
                filterContext.HttpContext.Session[SessionKey] = value;
            }

            return value ?? false;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Does not apply to anonymous users
            if (!filterContext.HttpContext.Request.IsAuthenticated)
                return;

            // Does not apply for child actions (I'm looking at you omniture tagging)
            if (filterContext.ParentActionViewContext != null)
                return;

            // Does not apply to the Terms controller
            var controller = (filterContext.RequestContext.RouteData.Values["controller"] ?? string.Empty) as string;
            var isTermsController = "terms".Equals(controller, StringComparison.InvariantCultureIgnoreCase);
            var isAccountController = "account".Equals(controller, StringComparison.InvariantCultureIgnoreCase);
            if (isTermsController || isAccountController)
                return;

            var appSettings = DependencyResolver.Current.GetService<IAppSettings>();
            var enabled = appSettings.GetValue<bool>("Feature.AcceptTerms");

            if (enabled && !IsDisclaimerAccepted(filterContext))
                filterContext.Result = new RedirectToRouteResult("Default", new RouteValueDictionary(new { controller = "terms" }));
        }
    }
}
