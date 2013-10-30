using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using NLog;

namespace myCustomers.Web
{
    public static class ApiHelpers
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public static HttpResponseException ServerError(ModelStateDictionary modelState)
        {
            var errors =
                from k in modelState.Keys
                where modelState[k].Errors.Any()
                from e in modelState[k].Errors
                select string.Format("{0} - {1}", e.ErrorMessage, e.Exception);

            return new HttpResponseException
            (
                new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    ReasonPhrase = "Validation failed.",
                    Content = new StringContent(string.Join("\r\n", errors.ToArray()))
                }
            );
        }

        public static HttpResponseException ServerError(string message, string content = "")
        {
            _logger.Error("{0}\r\n{1}", message, content);

            return new HttpResponseException
            (
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    ReasonPhrase = message,
                    Content = new StringContent(content)
                }
            );
        }

        public static HttpResponseMessage JsonResponseMessage(string json)
        {
            var resp = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };

            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return resp;
        }
    }
}
