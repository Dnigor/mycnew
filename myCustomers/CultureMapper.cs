using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myCustomers
{
    public static class CultureMapper
    {
        static readonly IDictionary<string, IDictionary<string, string>> _cultureMappings = new Dictionary<string, IDictionary<string, string>>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "US", new Dictionary<string,string>(StringComparer.InvariantCultureIgnoreCase) { { "es-MX", "es-US" } } },
            { "KZ", new Dictionary<string,string>(StringComparer.InvariantCultureIgnoreCase) { { "ru-KZ", "ru-RU" } } }
        };

        public static string MapCulture(string culture, string subsidiary)
        {
            if (_cultureMappings.ContainsKey(subsidiary))
            {
                var map = _cultureMappings[subsidiary];
                if (map.ContainsKey(culture))
                    culture = map[culture];
            }

            return culture;
        }
    }
}
