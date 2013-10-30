using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Caching;
using MaryKay.ServiceModel;
using MaryKay.ServiceModel.Config;
using NLog;

namespace myCustomers.Services.Inventory
{
    public sealed class MkUnavailablePartsService : IInventoryService
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        static readonly ISyncClient<IMkUnavailableParts> _client;

        static readonly HashSet<string> Empty = new HashSet<string>();

        static MkUnavailablePartsService()
        {
            var environment = ConfigurationManager.AppSettings["NetTax.Environment"];
            if (string.IsNullOrEmpty(environment))
                environment = ConfigurationManager.AppSettings["Environment"];

            var region = ConfigurationManager.AppSettings["NetTax.Region"];
            if (string.IsNullOrEmpty(region))
                region = ConfigurationManager.AppSettings["Region"];

            var endpoint = EndpointsConfig
                .FromContractAssembly<IMkUnavailableParts>()
                .ForEnvironment(environment)
                .ForRegion(region)
                .WithParameters(key =>
                {
                    if (key.Equals("Region"))
                        return region;

                    return ConfigurationManager.AppSettings[key];
                })
                .ForContract<IMkUnavailableParts>()
                .NamedEndpoint("MkUnavailableParts");

            _client = SyncClient<IMkUnavailableParts>.Create(endpoint);
        }

        static readonly string CACHE_KEY = "UNAVAILABLEPARTS";

        public HashSet<string> GetUnavailableParts(string subsidiaryCode)
        {
            if (string.IsNullOrWhiteSpace(subsidiaryCode))
                throw new ArgumentException("subsidiaryCode is required", "subsidiaryCode");

            var parts = HttpRuntime.Cache.Get(CACHE_KEY) as HashSet<string>;
            if (parts == null)
            {
                lock (_client)
                {
                    parts = HttpRuntime.Cache.Get(CACHE_KEY) as HashSet<string>;
                    if (parts == null)
                    {
                        var req = new GetUnavailableParts2
                        {
                            includeSubstitutionData = false,
                            subsidiaryCode = subsidiaryCode
                        };

                        var result = _client.Invoke(svc => svc.GetUnAvailableParts(req)).GetUnavailableParts2Result;
                        if (result != null)
                            parts = new HashSet<string>(result.Part.AsEnumerable().Select(p => p.Field<string>("PartID")).Distinct());
                        else
                            parts = Empty;

                        HttpRuntime.Cache.Insert(CACHE_KEY, parts, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration);
                    }
                }
            }

            return parts;
        }
    }
}
