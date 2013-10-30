using System;
using Quartet.Entities;

namespace myCustomers.Web.Models
{
    public class Activity
    {
        public Guid? OrderId { get; set; }
        public string ConfirmationCode { get { return OrderId.HasValue ? OrderId.Value.ToString().Substring(0, 6).ToUpperInvariant() : string.Empty; } }
        public DateTime DisplayDateUtc { get; set; }
        public string ActivityType { get; set; }
        public bool IsComplete { get; set; }
        public Guid? SampleRequestId { get; set; }
        public string ShadeName { get; set; }
        public string Formula { get; set; }
        public string OrderStatus { get; set; }
        public int ItemCount { get; set; }
        public decimal EstimatedOrderAmount { get; set; }
        public string Content { get; set; }
        public string FollowUpType { get; set; }
		  public DateTime? DueDateUtc { get; set; }

        public bool IsOrderFollowUp
        {
            get
            {
                return ActivityType == "Task" &&
                    (
                        FollowUpType == TaskType.Order.ToString() ||
                        FollowUpType == TaskType.OrderDelivery.ToString() ||
                        FollowUpType == TaskType.OrderFollowUp.ToString()
                    );
            }
        }
    }

    public class ActivitySummary
    {
        public Activity[] Activities { get; set; }
        public int Count { get; set; }
    }
}