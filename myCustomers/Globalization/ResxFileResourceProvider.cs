using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Web;
using System.Web.Caching;
using NLog;

namespace myCustomers.Globalization
{
    public sealed class ResxFileResourceProvider : IResourceProvider
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public IResourceSet GetResourceSet(string subsidiaryCode, string resourceSetName, CultureInfo culture = null)
        {
            culture = culture ?? CultureInfo.CurrentUICulture;

            var filePath = GetResourceSetFilePath(subsidiaryCode, resourceSetName, culture);
            if (filePath == null)
                return new NullResourceSet();

            var resourceSet = new ResxFileResourceSet(subsidiaryCode, filePath, new ResourceSet(new ResXResourceReader(filePath)));
            return resourceSet;
        }

        public IResourceSet GetCachedResourceSet(string subsidiaryCode, string resourceSetName, CultureInfo culture = null)
        {
            culture = culture ?? CultureInfo.CurrentUICulture;
            var cacheKey = string.Format("ResourceSet:{0}:{1}:{2}", subsidiaryCode, resourceSetName, culture.Name);

            var resourceSet = HttpContext.Current.Cache[cacheKey] as IResourceSet;
            if (resourceSet == null)
            {
                resourceSet = this.GetResourceSet(subsidiaryCode, resourceSetName, culture);
                HttpRuntime.Cache.Insert(cacheKey, resourceSet, resourceSet.CacheDependency);
            }

            return resourceSet;
        }

        // {0} - file name
        // {1} - subsidiary
        // {2} - 5 char lang (en-US)
        // {3} - 2 char lang (en)
        static string[] _filePathTemplates = new[]
        {
            "~/Resources/{1}/{0}.{2}.resx",
            "~/Resources/{1}/{0}.{3}.resx",
            "~/Resources/{0}.{2}.resx",
            "~/Resources/{0}.{3}.resx"
        };

        string GetResourceSetFilePath(string subsidiaryCode, string resourceSetName, CultureInfo culture)
        {
            var lang        = culture.Name;
            var lang2       = culture.TwoLetterISOLanguageName;
            var paths       = new List<string>();

            string filePath = null;
            foreach (var template in _filePathTemplates)
            {
                var vpath = string.Format(template, resourceSetName, subsidiaryCode, lang, lang2);
                filePath = HttpContext.Current.Request.MapPath(vpath);
                paths.Add(vpath);

                if (File.Exists(filePath))
                    break;
                else
                    filePath = null;
            }

            if (null == filePath)
                _logger.Warn("Missing resource file '{0}'\r\n{1}\r\n", resourceSetName, string.Join("\r\n\t", paths.ToArray()));

            return filePath;
        }

        class NullResourceSet : IResourceSet
        {
            public IEnumerable<KeyValuePair<string, object>> GetAll()
            {
                return Enumerable.Empty<KeyValuePair<string, object>>();
            }

            public string GetString(string key)
            {
                return string.Format("[[{0}]]", key);
            }

            public CacheDependency CacheDependency
            {
                get { return null; }
            }
        }

        class ResxFileResourceSet : IResourceSet
        {
            string _subsidiaryCode;
            string _filePath;
            ResourceSet _resourceSet;

            public ResxFileResourceSet(string subsidiaryCode, string filePath, ResourceSet resourceSet)
            {
                _subsidiaryCode = subsidiaryCode;
                _filePath       = filePath;
                _resourceSet    = resourceSet;
            }

            public IEnumerable<KeyValuePair<string,object>> GetAll()
            {
                foreach (DictionaryEntry item in _resourceSet)
                    yield return new KeyValuePair<string, object>((string)item.Key, item.Value);
            }

            public string GetString(string key)
            {
                var value = _resourceSet.GetString(key, true);

                if (string.IsNullOrEmpty(value))
                    return string.Format("[[{0}]]", key);

                return value;
            }

            public CacheDependency CacheDependency
            {
                get { return new CacheDependency(_filePath); }
            }
        }
    }
}