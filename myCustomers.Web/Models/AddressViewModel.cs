using System;
using System.Runtime.Serialization;
using myCustomers.Features;

namespace myCustomers.Web.Models
{
    public class AddressViewModel
    {
        public Guid AddressKey { get; set; }
        public string Addressee { get; set; }
        public string Street { get; set; }
        public string UnitNumber { get; set; }
        public string City { get; set; }
        public string RegionCode { get; set; }
        public string CountryCode { get; set; }
        public string PostalCode { get; set; }
        public string Telephone { get; set; }
        public bool IsPrimary { get; set; }
        public AddressViewModelFeatures Features { get; set; }

        //Calculated
        [IgnoreDataMember]
        public bool HasTelephone { get { return !string.IsNullOrWhiteSpace(Telephone); } }

        [IgnoreDataMember]
        public bool HasAddressee { get { return !string.IsNullOrWhiteSpace(Addressee); } }
    }
}