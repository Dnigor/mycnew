using System.Web.Mvc;
using System.Web.Routing;

namespace myCustomers.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "CustomerImages",
                url: "customers/{customerId}/images/{action}",
                defaults: new { controller = "CustomerImages" }
            );

            routes.MapRoute(
                name: "Resources",
                url: "resources/{name}",
                defaults: new { controller = "Resources", action = "Index" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Dashboard", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}