using System;

namespace myCustomers.Web.Models
{
    public class SampleRequestViewModel
    {
        public Guid SampleRequestId { get; set; }
        public Guid ConsultantKey { get; set; }
        public Guid? CustomerId { get; set; }
        public DateTime RequestDateUtc { get; set; }
        public DateTime? DeliveryDateUtc { get; set; }
        public bool IsCompleted { get; set; }
        public string ProductId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string StateProvince { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public Guid? FollowUpTaskId { get; set; }
        public string PartId { get; set; }
        public string DisplayPartId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string ProuctImageUrl { get; set; }
        public string ShadeName { get; set; }
        public string Formula { get; set; }

        public DateTime? FollowUpDateUtc { get; set; }
    }
}