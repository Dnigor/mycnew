using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Security;
using MaryKay.Configuration;
using myCustomers.Contexts;
using Newtonsoft.Json.Linq;
using NLog;

namespace myCustomers.Web.Controllers.Api
{
    public class eCardsController : ApiController
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        readonly IAppSettings _appSettings;
        readonly IConsultantContext _consultantContext;

        public eCardsController(IAppSettings appSettings, IConsultantContext consultantContext)
        {
            _appSettings       = appSettings;
            _consultantContext = consultantContext;
        }

        Cookie GetAuthCookie(string cookieDomain)
        {
            var authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            return new Cookie(FormsAuthentication.FormsCookieName, authCookie.Value, null, cookieDomain);
        }

        [AcceptVerbs("POST")]
        public dynamic Submit(JObject req)
        {
            var consultant = _consultantContext.Consultant;
            var baseAddress = new Uri(_appSettings.GetValue("eCards.BaseUrl"));
            var cookieContainer = new CookieContainer();
            cookieContainer.Add(baseAddress, GetAuthCookie(_appSettings.GetValue("eCards.CookieDomain")));

            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer, Proxy = WebRequest.DefaultWebProxy })
            using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var json = req.ToString();
                var content = new StringContent(json);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                _logger.Trace("eCards Proxy Request: {0}", json);

                var postTask = client.PostAsync(_appSettings.GetValue("eCards.PostUrl"), content, CancellationToken.None);
                postTask.Wait();

                var postResult = postTask.Result;

                var readTask = postResult.Content.ReadAsStringAsync();
                readTask.Wait();

                json = readTask.Result;

                postResult.EnsureSuccessStatusCode();

                return ApiHelpers.JsonResponseMessage(json);
            }
        }
    }
}
