using System;
using myCustomers.Features;

namespace myCustomers.Web.Models
{
    public class HCardViewModel
    {
        public Guid CustomerId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public AddressViewModel PrimaryAddress { get; set; }
        public PhoneNumberViewModel PrimaryPhone { get; set; }
        public DateTime? PictureLastUpdatedDateUtc { get; set; }
        public HCardViewModelFeatures Features { get; set; }
    }
}