using System;
using System.Web.Http;
using myCustomers.Contexts;
using myCustomers.Services;
using NLog;
using Quartet.Entities;

namespace myCustomers.Web.Controllers.Api
{
    [ApiAuthorizeConsultant]
    public class ValidationController : ApiController
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        IConsultantContext          _consultantContext;
        IQuartetClientFactory       _clientFactory;
        IAddressVerificationService _addressVerificationService;

        public ValidationController
        (
            IConsultantContext          consultantContext,
            IQuartetClientFactory       clientFactory,
            IAddressVerificationService addressVerificationService
        )
        {
            _consultantContext          = consultantContext;
            _clientFactory              = clientFactory;
            _addressVerificationService = addressVerificationService;
        }

        [AcceptVerbs("GET")]
        public dynamic Email(string value, Guid? customerId = null)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new { IsValid = true };

            var client = _clientFactory.GetCustomersQueryServiceClient();
            var id = client.GetCustomerIdForEmail(value);

            return new { IsValid = !id.HasValue || id == customerId, Message = "VALIDATION_DUPEMAIL" };
        }

        [AcceptVerbs("POST")]
        public AddressVerificationResult VerifyAddress(Address address)
        {
            var result = _addressVerificationService.VerifyAddress(address.Street, address.City, address.RegionCode, address.PostalCode);
            return result;
        }
    }
}
