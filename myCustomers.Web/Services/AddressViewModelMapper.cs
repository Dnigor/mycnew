using myCustomers.Features;
using myCustomers.Web.Models;
using Quartet.Entities;

namespace myCustomers.Web.Services
{
    public class AddressViewModelMapper : IMappingService<Address, AddressViewModel>
    {
        IFeaturesConfigService _features;
        
        public AddressViewModelMapper(IFeaturesConfigService features)
        {
            _features = features;
        }
        
        public AddressViewModel Map(Address source)
        {
            if (source == null)
                return null;

            return new AddressViewModel
            {
                AddressKey  = source.AddressKey,
                Addressee   = source.Addressee,
                Street      = source.Street,
                UnitNumber  = source.UnitNumber,
                City        = source.City,
                RegionCode  = source.RegionCode,
                CountryCode = source.CountryCode,
                PostalCode  = source.PostalCode,
                IsPrimary   = source.IsPrimary,
                Telephone   = source.Telephone,
                Features    = _features.GetAddressViewModelFeatures()
            };
        }
    }
}