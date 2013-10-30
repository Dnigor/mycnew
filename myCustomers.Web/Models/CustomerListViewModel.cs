using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using myCustomers.Features;

namespace myCustomers.Web.Models
{
    public class CustomerSelectionModel
    {
        public Guid[] Ids { get; set; }
    }

    public enum CustomerListMode
    {
        Search,
        Select
    }

	public class CustomerListViewModel
	{
        public Guid ConsultantKey { get; set; }
        public CustomerListMode Mode { get; set; }
        public string TargetUrlFormat { get; set; }
        public CustomerListFeatures Features { get; set; }
	}
}