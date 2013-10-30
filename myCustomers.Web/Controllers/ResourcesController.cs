using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using myCustomers.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace myCustomers.Web.Controllers
{
    [AuthorizeConsultant]
    public class ResourcesController : Controller
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        [ETag]
        public ActionResult Index(string name, string sub, string lang)
        {
            try
            {
                if (!Regex.IsMatch(name, @"^[\w_]+$"))
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "invalid resource name");

                if (string.IsNullOrWhiteSpace(sub) || sub.Length != 2)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "invalid subsidiary name");

                if (string.IsNullOrWhiteSpace(lang) || sub.Length != 2)
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "invalid language");

                var cacheKey = string.Format("myCust.resources.{0}.{1}.{2}", name, sub, lang);
                var script = HttpRuntime.Cache.Get(cacheKey) as string;
                if (null == script)
                {
                    try
                    {
                        var culture = new CultureInfo(lang);
                        var resourceSet = Resources.GetResourceSet(name, sub, culture);
                        var resources = resourceSet.GetAll();

                        var messages = new JObject();
                        foreach (var resource in resources)
                            messages.Add(new JProperty((string)resource.Key, resource.Value));

                        var json = JsonConvert.SerializeObject(new { messages });

                        script = string.Format("Globalize.addCultureInfo('{0}', {1});", culture.TwoLetterISOLanguageName, json);

                        HttpRuntime.Cache.Add(cacheKey, script, resourceSet.CacheDependency, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
                    }
                    catch (FileNotFoundException)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.NotFound, "resource file not found");
                    }
                }

                return JavaScript(script);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
