using System;

namespace myCustomers.Web.Models
{
    public class SampleRequestDetailViewModel
    {
        public HCardViewModel Customer { get; set; }
        public Guid SampleRequestId { get; set; }
    }
}