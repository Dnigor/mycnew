using System;
using myCustomers.Features;

namespace myCustomers.Web.Models
{
    public class GroupDetailViewModel
    {     
        public Guid GroupId { get; set; }
        public string GroupName { get; set; }
        public GroupDetailFeatures Features { get; set; }
    }
}