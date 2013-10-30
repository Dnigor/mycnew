using System.Collections.Generic;
using System.Web.Caching;

namespace myCustomers.Globalization
{
    public interface IResourceSet
    {
        IEnumerable<KeyValuePair<string, object>> GetAll();
        string GetString(string key);
        CacheDependency CacheDependency { get; }
    }

}
