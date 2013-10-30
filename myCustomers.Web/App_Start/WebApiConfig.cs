using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NLog;

namespace myCustomers.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var formatters = GlobalConfiguration.Configuration.Formatters;
            
            // XML is not supported at this time
            formatters.Remove(formatters.XmlFormatter); 

            // Always serialize UTC date/time and use ISO 8601 strict format
            formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            formatters.JsonFormatter.SerializerSettings.Converters.Add(new StrictIsoDateTimeConverter());

            // serialize enums as strings
            formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());

            // auto convert PascalCase properties to camelCase
            formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            config.Routes.MapHttpRoute(
                name: "WishListProductsApi",
                routeTemplate: "api/customers/{custid}/wishlist",
                defaults: new { id = RouteParameter.Optional, controller = "WishListProducts" }
            );

            config.Routes.MapHttpRoute(
                name: "RecentlyOrderedProductsApi",
                routeTemplate: "api/customers/{custid}/recentproducts",
                defaults: new { id = RouteParameter.Optional, controller = "RecentlyOrderedProducts" }
            );

            config.Routes.MapHttpRoute(
                name: "RecommendedProductsApi",
                routeTemplate: "api/customers/{custid}/recommendedproducts",
                defaults: new { id = RouteParameter.Optional, controller = "RecommendedProducts" }
            );

            config.Routes.MapHttpRoute(
                name: "CustomerNotesApi",
                routeTemplate: "api/customers/{custid}/notes/{id}",
                defaults: new { id = RouteParameter.Optional, controller = "CustomerNotes" }
            );

            config.Routes.MapHttpRoute(
               name: "GroupsApi",
               routeTemplate: "api/groups/{groupId}",
               defaults: new { groupId = RouteParameter.Optional, controller = "Groups" }
           );

            config.Routes.MapHttpRoute(
                name: "CustomerGroupsApi",
                routeTemplate: "api/customers/{custid}/groups/{id}",
                defaults: new { id = RouteParameter.Optional, controller = "CustomerGroups" }
            );

            config.Routes.MapHttpRoute(
                name: "QuestionnaireAnswersApi",
                routeTemplate: "api/customers/{custid}/questionnaireanswers",
                defaults: new { controller = "questionnaire", action = "questionnaireanswers" }
            );

            config.Routes.MapHttpRoute(
                name: "QuestionnaireApi",
                routeTemplate: "api/questionnaire",
                defaults: new { controller = "questionnaire", action = "questionnaire" }
            );

            config.Routes.MapHttpRoute(
                name: "CustomerTasksApi",
                routeTemplate: "api/customers/{custid}/reminders/{id}",
                defaults: new { id = RouteParameter.Optional, controller = "tasks" }
            );

            config.Routes.MapHttpRoute(
                name: "ValidationApi",
                routeTemplate: "api/validation/{action}",
                defaults: new { controller = "Validation" }
            );

            config.Routes.MapHttpRoute(
                name: "CommandsApi",
                routeTemplate: "api/commands/{action}",
                defaults: new { controller = "Commands" }
            );

            config.Routes.MapHttpRoute(
                name: "DashboardApi",
                routeTemplate: "api/dash/{action}",
                defaults: new { controller = "Dashboard" }
            );

            config.Routes.MapHttpRoute(
                name: "CCPaymentApi",
                routeTemplate: "api/orders/{orderId}/ccpayment/{action}",
                defaults: new { controller = "CCPayment" }
            );

            config.Routes.MapHttpRoute(
                name: "CDSApi",
                routeTemplate: "api/cds/{action}",
                defaults: new { controller = "CDS" }
            );

            config.Routes.MapHttpRoute(
                name: "eCardsApi",
                routeTemplate: "api/ecards/{action}",
                defaults: new { controller = "eCards" }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Filters.Add(new UnhandledExceptionFilter());
            config.MessageHandlers.Add(new VerbOverrideHandler());
        }
    }

    public class UnhandledExceptionFilter : ExceptionFilterAttribute
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public override void OnException(HttpActionExecutedContext context)
        {
            var errorCode = Guid.NewGuid().ToString().Substring(0,6).ToUpper();
            var message = string.Format("Error: {0}", errorCode);
            _logger.ErrorException(message, context.Exception);

            context.Exception = null;
            context.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                ReasonPhrase = string.Format("Error: {0}", errorCode),
                Content      = new StringContent("")
            };
        }
    }
}
