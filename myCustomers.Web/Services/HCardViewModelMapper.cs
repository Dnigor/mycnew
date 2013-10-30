using System;
using System.Linq;
using myCustomers.Features;
using myCustomers.Web.Models;
using Quartet.Entities;

namespace myCustomers.Web.Services
{
    public class HCardViewModelMapper : IMappingService<Customer, HCardViewModel>
    {
        IMappingService<Address, AddressViewModel> _addressMapping;
        IMappingService<PhoneNumber, PhoneNumberViewModel> _phoneMapping;
        private readonly IFeaturesConfigService _features;

        public HCardViewModelMapper
        (
            IMappingService<Address, AddressViewModel> addressMapping,
            IMappingService<PhoneNumber, PhoneNumberViewModel> phoneMapping,
            IFeaturesConfigService features
        )
        {
            _addressMapping = addressMapping;
            _phoneMapping   = phoneMapping;
            _features = features;
        }

        public HCardViewModel Map(Customer customer)
        {
            if (customer == null)
                return null;

            var primaryAddress = 
                customer.Addresses != null ?
                (
                    from a in customer.Addresses.Values
                    where a.IsPrimary
                    select _addressMapping.Map(a)
                ).FirstOrDefault() :
                null;

            var primaryPhone =
                customer.PhoneNumbers != null ?
                (
                    from p in customer.PhoneNumbers.Values
                    where p.IsPrimary
                    select _phoneMapping.Map(p)
                ).FirstOrDefault() :
                null;
            
            return new HCardViewModel
            {
                PictureLastUpdatedDateUtc = customer.Pictures.Values.Count > 0 ? (DateTime?)customer.Pictures.Values.FirstOrDefault().LastUpdatedDateUtc : null,
                CustomerId                = customer.CustomerId,
                FirstName                 = customer.ContactInformation.FirstName,
                MiddleName                = customer.ContactInformation.MiddleName,
                LastName                  = customer.ContactInformation.LastName,
                PrimaryAddress            = primaryAddress,
                PrimaryPhone              = primaryPhone,
                Email                     = customer.EmailAddress != null ? customer.EmailAddress.Address : null,
                Features                  = _features.GetHCardViewModelFeatures()
            };
        }
    }
}