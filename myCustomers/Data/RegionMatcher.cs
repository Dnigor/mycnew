using System;
using System.Collections.Generic;
using MaryKay.Configuration;
using Microsoft.Practices.ServiceLocation;

namespace myCustomers.Data
{
    public class RegionMatcher
    {
        public RegionMatcher()
        {
            _codeToName = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            _nameToCode = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            var configName    = "RegionListItems";
            var regionElement = "ListItem";
            var codeAttribute = "Value";
            var nameAttribute = "Text";
           
            var configService = ServiceLocator.Current.GetInstance<IConfigService>();
            var xeRegions = configService.GetConfigXml(configName);
            if (xeRegions != null)
            {
                foreach (var xeRegion in xeRegions.Elements(regionElement))
                {
                    var code = xeRegion.AttributeValue(codeAttribute);
                    var name = xeRegion.AttributeValue(nameAttribute);
                    if (!string.IsNullOrWhiteSpace(code) && !string.IsNullOrWhiteSpace(name))
                    {
                        code = code.Trim();
                        name = name.Trim();
                        _codeToName[code] = name;
                        _nameToCode[name] = code;
                    }
                }
            }
            
        }

        public void GetMatch(string source, out string regionCode, out string regionName)
        {
            regionCode = null;
            regionName = null;

            if (!string.IsNullOrWhiteSpace(source))
            {
                source = source.Trim();
                if (CodeToName.ContainsKey(source))
                {
                    regionCode = source;
                    regionName = CodeToName[source];
                }
                else if (NameToCode.ContainsKey(source))
                {
                    regionCode = NameToCode[source];
                    regionName = CodeToName[regionCode];
                }
                else
                {
                    regionName = source;
                }
            }
        }

        protected Dictionary<string, string> CodeToName { get { return _codeToName; } }

        protected Dictionary<string, string> NameToCode { get { return _nameToCode; } }

        private readonly Dictionary<string, string> _codeToName;
        private readonly Dictionary<string, string> _nameToCode;
    }
}
