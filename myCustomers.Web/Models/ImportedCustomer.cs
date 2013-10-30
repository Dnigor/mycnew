using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using myCustomers.Data;
using Quartet.Entities;

namespace myCustomers.Web.Models
{
    public class ImportedCustomer
    {
        public Guid CustomerId { get; set; }
        public Guid ExistingCustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImportAction { get; set; }
        public string IsFaulted { get; set; }
        public Subscription[] Subscriptions { get; set; }      
    }
}