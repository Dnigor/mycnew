using System;
using MaryKay.IBCDataServices.Entities;
using myCustomers.Features;
using Quartet.Entities;
using Quartet.Entities.Products;

namespace myCustomers.Web.Models
{
    public class EditOrderViewModel
    {
        public Consultant Consultant { get; set; }
        public bool IsNew { get { return !this.OrderId.HasValue; } }
        public bool IsArchived { get; set; }
        public bool IsEligibleForCDS { get; set; }
        public bool HasActiveProPayAccount { get; set; }
        public Guid? OrderId { get; set; }
        public string OrderSource { get; set; }
        public HCardViewModel Customer { get; set; }
        public AddressViewModel[] Addresses { get; set; }
        public OrderFeatures Features { get; set; }
        public GiftWithPurchaseConfig GiftWithPurchase { get; set; }

        public class GiftWithPurchaseConfig
        {
            public string PartId { get; set; }
            public string ProductId { get; set; }
            public string DisplayPartId { get; set; }
            public string CatalogName { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Shade { get; set; }
            public decimal MinOrderAmount { get; set; }
            public string ImageUrl { get; set; }
            public bool AvailableForCDS { get; set; }
        }
    }
}