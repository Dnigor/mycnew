using System;
using Quartet.Entities;

namespace myCustomers.Web.Models
{
    public class HyperLinkViewModel
    {
        public Guid HyperLinkKey { get; set; }
        public HyperLinkType HyperLinkType { get; set; }
        public string Url { get; set; }
    }
}