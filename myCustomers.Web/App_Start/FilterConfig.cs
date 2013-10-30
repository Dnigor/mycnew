using System.Web.Mvc;

namespace myCustomers.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            // NOTE: we are using <customErrors /> seciont of web.config to
            // redirect to the Errors controller rather than the global filter.
            //filters.Add(new HandleErrorAttribute());

            filters.Add(new AcceptTermsFilter());
        }
    }
}