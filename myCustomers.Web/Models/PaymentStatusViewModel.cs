using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartet.Entities;

namespace myCustomers.Web.Models
{
    public class PaymentStatusViewModel
    {
        public Guid[] OrderIds { get; set; }
        public OrderPaymentStatus PaymentStatus { get; set; }
    }
}