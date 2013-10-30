using System;
using System.Configuration;
using MaryKay.ServiceModel;
using MaryKay.ServiceModel.Config;
using NLog;

namespace myCustomers.Services.NetTax
{
    public sealed class Code1AddressVerificationService : IAddressVerificationService
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        static readonly ISyncClient<ICode1Service> _client;

        static Code1AddressVerificationService()
        {
            var environment = ConfigurationManager.AppSettings["NetTax.Environment"];
            if (string.IsNullOrEmpty(environment))
                environment = ConfigurationManager.AppSettings["Environment"];

            var region = ConfigurationManager.AppSettings["NetTax.Region"];
            if (string.IsNullOrEmpty(region))
                region = ConfigurationManager.AppSettings["Region"];

            var endpoint = EndpointsConfig
                .FromContractAssembly<ICode1Service>()
                .ForEnvironment(environment)
                .ForRegion(region)
                .WithParameters(key =>
                {
                    if (key.Equals("Region"))
                        return region;

                    return ConfigurationManager.AppSettings[key];
                })
                .ForContract<ICode1Service>()
                .NamedEndpoint("Code1Service");

            _client = SyncClient<ICode1Service>.Create(endpoint);
        }

        public AddressVerificationResult VerifyAddress(string street, string city, string regionCode, string postalCode)
        {
            if (street == null) street = string.Empty;
            if (city       == null) city       = string.Empty;
            if (regionCode == null) regionCode = string.Empty;
            if (postalCode == null) postalCode = string.Empty;

            var request = new ScrubAddressForOrderEntryRequest
            {
                Body = new ScrubAddressForOrderEntryRequestBody
                {
                    Address = street.Trim(),
                    City    = city.Trim(),
                    State   = regionCode.Trim(),
                    Zip     = postalCode.Trim()
                }
            };

            try
            {
                var response = _client.Invoke(c => c.ScrubAddressForOrderEntry(request));

                if (response == null || response.Body == null || response.Body.ScrubAddressForOrderEntryResult == null || !string.IsNullOrEmpty(response.Body.ScrubAddressForOrderEntryResult.Message))
                    return new AddressVerificationResult { VerificationState = AddressVerificationState.NotFound };

                var result = response.Body.ScrubAddressForOrderEntryResult;
                var recommendedAddress = new AddressVerificationResult.Address
                {
                    Street = result.Address,
                    City = result.City,
                    RegionCode = result.State,
                    PostalCode = result.Zip + "-" + result.ZipPlus4.Trim()
                };

                if (recommendedAddress.Street == street && recommendedAddress.City == city && recommendedAddress.RegionCode == regionCode && recommendedAddress.PostalCode == postalCode)
                    return new AddressVerificationResult { VerificationState = AddressVerificationState.ExactMatch };

                return new AddressVerificationResult
                {
                    VerificationState = AddressVerificationState.RecommendChange,
                    RecommendedAddress = recommendedAddress
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return new AddressVerificationResult { VerificationState = AddressVerificationState.NotFound };
            }
        }
    }
}
