using System;
using System.Data;
using System.Xml.Linq;
using QuartetEntities = Quartet.Entities;

namespace myCustomers.Data
{
    public class AddressGenerator : BaseGenerator<QuartetEntities.Address>
    {
        public AddressGenerator(XElement xeConfiguration, DataColumnCollection columns)
        {
            _regionMatcher = new RegionMatcher();
            AddressType = (QuartetEntities.AddressType)Enum.Parse(typeof(QuartetEntities.AddressType), xeConfiguration.AttributeValue("AddressType", true));
            svgAddresseeFirstName = new StringValueGenerator(columns, xeConfiguration, "AddresseeFirstName");
            svgAddresseeLastName = new StringValueGenerator(columns, xeConfiguration, "AddresseeLastName");
            svgStreet = new StringValueGenerator(columns, xeConfiguration, "Street");
            svgUnitNumber = new StringValueGenerator(columns, xeConfiguration, "UnitNumber");
            svgCity = new StringValueGenerator(columns, xeConfiguration, "City");
            svgRegion = new StringValueGenerator(columns, xeConfiguration, "Region");
            svgCountry = new StringValueGenerator(columns, xeConfiguration, "Country");
            svgPostalCode = new StringValueGenerator(columns, xeConfiguration, "PostalCode");
            svgPhoneNumber = new StringValueGenerator(columns, xeConfiguration, "PhoneNumber");
            IsFunctional = svgAddresseeFirstName.IsFunctional && svgAddresseeLastName.IsFunctional
                && (svgStreet.IsFunctional || svgCity.IsFunctional || svgRegion.IsFunctional || svgCountry.IsFunctional || svgPostalCode.IsFunctional);
        }

        public QuartetEntities.AddressType AddressType { get; protected set; }

        public override QuartetEntities.Address GeneratedInstance(DataRow row)
        {
            QuartetEntities.Address instance = null;

            string street = svgStreet.Value(row);
            string unitNumber = svgUnitNumber.Value(row);
            string city = svgCity.Value(row);
            string region = svgRegion.Value(row);
            string country = svgCountry.Value(row);
            string postalCode = svgPostalCode.Value(row);
            string phoneNumber = svgPhoneNumber.Value(row);
            if (!string.IsNullOrWhiteSpace(street) || !string.IsNullOrWhiteSpace(unitNumber)
             || !string.IsNullOrWhiteSpace(city) || !string.IsNullOrWhiteSpace(region)
             || !string.IsNullOrWhiteSpace(country) || !string.IsNullOrWhiteSpace(postalCode))
            {
                string addresseeFirstName = svgAddresseeFirstName.Value(row);
                string addresseeLastName = svgAddresseeLastName.Value(row);
                string addressee;
                if (!string.IsNullOrWhiteSpace(addresseeFirstName))
                {
                    addressee = !string.IsNullOrWhiteSpace(addresseeLastName) ? addresseeFirstName + " " + addresseeLastName : addresseeFirstName;
                }
                else
                {
                    addressee = !string.IsNullOrWhiteSpace(addresseeLastName) ? addresseeLastName : null;
                }
                string regionCode;
                string regionName;
                RegionMatcher.GetMatch(region, out regionCode, out regionName);
                instance = new QuartetEntities.Address();
                instance.AddressKey = Guid.NewGuid();
                instance.Addressee = addressee;
                instance.AddressType = AddressType;
                instance.IsPrimary = false;
                instance.Street = street;
                instance.UnitNumber = unitNumber;
                instance.City = city;
                instance.RegionCode = regionCode;
                instance.CountryCode = country;
                instance.PostalCode = postalCode;
                instance.Telephone = phoneNumber;
            }

            return instance;
        }

        protected RegionMatcher RegionMatcher { get { return _regionMatcher; } }

        private readonly RegionMatcher _regionMatcher;
        protected readonly StringValueGenerator svgAddresseeFirstName;
        protected readonly StringValueGenerator svgAddresseeLastName;
        protected readonly StringValueGenerator svgCity;
        protected readonly StringValueGenerator svgCountry;
        protected readonly StringValueGenerator svgPhoneNumber;
        protected readonly StringValueGenerator svgPostalCode;
        protected readonly StringValueGenerator svgRegion;
        protected readonly StringValueGenerator svgStreet;
        protected readonly StringValueGenerator svgUnitNumber;
    }
}
