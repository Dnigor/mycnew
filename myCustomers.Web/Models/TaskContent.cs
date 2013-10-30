using System;

namespace myCustomers.Web.Models
{
    public class CustomerTaskContent
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public Guid TaskId { get; set; }
        public DateTime? DueDateUtc { get; set; }
        public Guid? CustomerId { get; set; }
        public string TaskType { get; set; }
    }

    public class CustomerTaskResult
    {
        public CustomerTaskContent[] Tasks { get; set; }
        public int Count { get; set; }
    }
}