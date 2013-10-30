using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Caching;
using MaryKay.Configuration;
using Microsoft.Practices.ServiceLocation;
using NLog;

namespace myCustomers.Globalization
{
    public static class Resources
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();
       
        public static IDictionary<string, string> GetUISiteTermElements(string siteTermId)
        {
            var cultureCode    = Thread.CurrentThread.CurrentUICulture.ToString();
            var subsidiaryCode = ServiceLocator.Current.GetInstance<ISubsidiaryAccessor>().GetSubsidiaryCode();
            var cacheKey       = string.Format("{0}_{1}_{2}", subsidiaryCode, cultureCode, siteTermId);

            var elements = HttpRuntime.Cache.Get(cacheKey) as IDictionary<string, string>;

            if (elements == null)
            {
                var quartetClientFactory = ServiceLocator.Current.GetInstance<IQuartetClientFactory>();
                var customerQueryClient  = quartetClientFactory.GetCustomersQueryServiceClient();
                var siteTerm             = customerQueryClient.GetSiteTermById(siteTermId, cultureCode);

                if (siteTerm == null)
                {
                    _logger.Log(LogLevel.Info, string.Format("Siteterm {0} is null for {1} {2}", siteTermId, cultureCode, subsidiaryCode));
                    elements = new Dictionary<string, string>();
                    //throw new ConfigurationErrorsException(string.Format("Siteterm {0} is null for {1} {2}", siteTermId, cultureCode, subsidiaryCode));
                }
                else
                {
                    elements = siteTerm.Elements.OrderBy(e => e.SortOrder).ToDictionary(e => e.Id, e => e.DisplayName, StringComparer.InvariantCultureIgnoreCase);
                }
                HttpRuntime.Cache.Insert(cacheKey, elements, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration);
            }

            return elements;
        }

        // {0} - file name
        // {1} - subsidiary
        // {2} - 5 char lang (en-US)
        // {3} - 2 char lang (en)
        static string[] _ContentPathTemplates = new[]
        {
            "~/Resources/Content/{1}/{0}.{2}.html",
            "~/Resources/Content/{1}/{0}.{3}.html",
            "~/Resources/Content/{0}.{2}.html",
            "~/Resources/Content/{0}.{3}.html"
        };

        public static string GetLocalizedContent(string fileName)
        {
            var subsidiaryAccessor = ServiceLocator.Current.GetInstance<ISubsidiaryAccessor>();
            if (subsidiaryAccessor == null)
                throw new Exception("Could not get instance of ISubsidiaryContext");

            var sub = subsidiaryAccessor.GetSubsidiaryCode();
            var lang = CultureInfo.CurrentUICulture.ToString();
            var lang2 = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            var paths = new List<string>();
            foreach (var template in _ContentPathTemplates)
            {
                var vpath = string.Format(template, fileName, sub, lang, lang2);
                var path = HttpContext.Current.Request.MapPath(vpath);
                paths.Add(vpath);
                if (File.Exists(path))
                    return File.ReadAllText(path);
            }

            return string.Format("<pre>Missing resource content file '{0}'\r\n{1}</pre>", fileName, string.Join("\r\n", paths.ToArray()));
        }

        public static string GetSiteTermString(string key, string siteTermId)
        {
            var elements = GetUISiteTermElements(siteTermId);
            if (!elements.ContainsKey(key))
                throw new ConfigurationErrorsException(string.Format("SiteTermElement {0} for SiteTerm {1} is null", key, siteTermId));

            var value = elements[key];
            if (string.IsNullOrEmpty(value))
                throw new ConfigurationErrorsException(string.Format("SitetermElement {0} for SiteTerm {1} has a null or empty display name", key, siteTermId));

            return value;
        }

        public static IResourceSet GetResourceSet(string resourceSetName = "Strings", string subsidiaryCode = null, CultureInfo culture = null)
        {
            var provider = ServiceLocator.Current.GetInstance<IResourceProvider>();

            if (string.IsNullOrEmpty(subsidiaryCode))
                subsidiaryCode = ServiceLocator.Current.GetInstance<ISubsidiaryAccessor>().GetSubsidiaryCode();

            return provider.GetCachedResourceSet(subsidiaryCode, resourceSetName, culture);
        }

        public static string GetString(string resourceKey, params object[] keyValues)
        {
            var resourceSet = HttpContext.Current.Items["StringResourceSet"] as IResourceSet;
            if (resourceSet == null)
            {
                var subsidiaryCode = ServiceLocator.Current.GetInstance<ISubsidiaryAccessor>().GetSubsidiaryCode();
                var provider = ServiceLocator.Current.GetInstance<IResourceProvider>();
                resourceSet = provider.GetCachedResourceSet(subsidiaryCode, "Strings");
                HttpContext.Current.Items.Add("StringResourceSet", resourceSet);
            }

            var value = resourceSet.GetString(resourceKey, keyValues);

            return value;
        }

        public static string GetString(this IResourceSet resourceSet, string resourceKey, params object[] keyValues)
        {
            if (keyValues != null && keyValues.Length > 0)
                resourceKey = string.Format(resourceKey, keyValues);
            
            var value = resourceSet.GetString(resourceKey);

            return value;
        }
    }
}
