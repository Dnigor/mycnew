using System;

namespace myCustomers.Web.Models
{
    public class PhoneNumberViewModel
    {
        public Guid PhoneNumberKey { get; set; }
        public string PhoneNumberType { get; set; }
        public string Number { get; set; }
        public string Extension { get; set; }
        public bool IsPrimary { get; set; }
    }
}