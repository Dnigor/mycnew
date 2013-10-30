using System;

namespace myCustomers.Web.Models
{
    public class Group
    {
        public Guid GroupId { get; set; }
        public string GroupName {get; set;}
        public int CustomerCount { get; set; }
    }
}