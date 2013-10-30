using System;


namespace myCustomers.Web.Models
{
	public class TaskViewModel
	{
		public Guid? CustomerId { get; set; }
		public string Description { get; set; }
		public DateTime? DueDateUtc { get; set; }
        public string EndText { get; set; }
		public bool IsComplete { get; set; }
		public string Name { get; set; }
		public string ProfileUrl { get; set; }
	    public string PrefixText { get; set; }
        public Guid? OrderId { get; set; }
	    public string TargetUrl { get; set; }
		public Guid TaskId { get; set; }
		public string TaskType { get; set; }
		public string Title { get; set; }
        public Guid? SampleRequestId { get; set; }
        public string UrlText { get; set; }
        


	}
}