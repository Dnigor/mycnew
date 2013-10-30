using System;
using System.Collections.Generic;
using myCustomers.Features;

namespace myCustomers.Web.Models
{
    public class EditCustomerViewModel
    {
        public string[] Languages { get; set; }
        public IDictionary<string, string> TopicsOfInterest { get; set; }
        public IDictionary<string, string> ContactDays { get; set; }
        public IDictionary<string, string> ContactTimes { get; set; }
        public IDictionary<string, string> ContactFrequencies { get; set; }
        public IDictionary<string, string> ContactMethods { get; set; }
        public IDictionary<string, string> ShoppingMethods { get; set; }

        public string CustomerId { get; set; }
        public string CustomerProfilePictureUrl { get; set; }

        public bool IsNew { get { return string.IsNullOrEmpty(this.CustomerId); } }
        
        public EditCustomerFeatures Features { get; set; }

        public SocialNetworksConfig.SocialNetwork[] SocialNetworks { get; set; }

        public SubscriptionsConfig.Subscription[] Subscriptions { get; set; }
    }
}