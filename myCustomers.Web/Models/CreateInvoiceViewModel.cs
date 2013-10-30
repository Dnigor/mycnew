using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using myCustomers.Web.Services;

namespace myCustomers.Web.Models
{
    public class CreateInvoiceViewModel
    {
        public Guid OrderId { get; set; }
        public InvoiceFormat SaveFormat { get; set; } 
        public string GiftMessage { get; set; }
        public string InvoiceMessage { get; set; }        
    }
}