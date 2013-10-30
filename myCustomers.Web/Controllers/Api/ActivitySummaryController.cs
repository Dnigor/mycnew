using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using myCustomers.Contexts;
using myCustomers.Web.Models;
using myCustomers.Web.Services;
using Quartet.Entities;
using Quartet.Entities.Views;
using Quartet.Services.Contracts;

namespace myCustomers.Web.Controllers.Api
{
    public class ActivitySummaryController : ApiController
    {
        IQuartetClientFactory _quartetClientFactory;
        IConsultantContext _consultantContext;
        IMappingService<TaskListItem, Activity> _taskMapping;

        public ActivitySummaryController
        (
            IQuartetClientFactory quartetClientFactory,
            IConsultantContext consultantContext,
            IMappingService<TaskListItem, Activity> taskMapping
        )
        {
            _quartetClientFactory = quartetClientFactory;
            _consultantContext    = consultantContext;
            _taskMapping          = taskMapping;
        }

        [FromUri]
        public class ActivityCriteria
        {
            public Guid CustomerId { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
        }

        [AcceptVerbs("GET")]
        public ActivitySummary GetActivitySummary(ActivityCriteria criteria)
        {
            var activityList = new List<Activity>();
            var client = _quartetClientFactory.GetCustomersQueryServiceClient();
            var customer = client.GetCustomer(criteria.CustomerId);

            GetOrderActivities(activityList, criteria);
            GetNoteActivities(activityList, criteria, customer);
            GetTaskActivities(activityList, criteria);
            
            var activities = activityList
                .OrderByDescending(a => a.DisplayDateUtc)
                .Skip((criteria.Page * criteria.PageSize) - criteria.PageSize)
                .Take(criteria.PageSize)
                .ToArray();

            return new ActivitySummary { Activities = activities, Count = activityList.Count };
        }

        void GetTaskActivities(List<Activity> activityList, ActivityCriteria criteria)
        {
            var client = _quartetClientFactory.GetTaskQueryServiceClient();
            var results = client.QueryTaskListByCustomerId(new CustomerIdParameters{ CustomerId = criteria.CustomerId });

            if (results != null && results.Length > 0)
            {
                foreach (var result in results)
                    activityList.Add(_taskMapping.Map(result));
            }
        }

        void GetNoteActivities(List<Activity> activityList, ActivityCriteria criteria, Customer customer)
        {
            if (customer.Notes != null && customer.Notes.Count > 0)
            {
                foreach (var note in customer.Notes.Values)
                {
                    activityList.Add(new Activity
                    {
                        ActivityType = "Note",
                        Content = note.Content,
                        DisplayDateUtc = note.DateCreatedUtc
                    });
                }
            }
        }

        void GetOrderActivities(List<Activity> activityList, ActivityCriteria criteria)
        {
            var orderClient = _quartetClientFactory.GetCustomersQueryServiceClient();
            var orders = orderClient.GetCustomerOrders(criteria.CustomerId, false, false);
            if (orders != null && orders.Length > 0)
            {
                foreach (var order in orders)
                {
                    int itemCount = 0;
                    if (order.ProductsOrdered != null && order.ProductsOrdered.Length > 0)
                        foreach (var item in order.ProductsOrdered)
                            itemCount += item.Quantity;

                    if (order.OrderDateUtc.HasValue)
                        activityList.Add(new Activity
                        {
                            ActivityType         = "Order",
                            DisplayDateUtc       = order.OrderDateUtc.Value,
                            ItemCount            = itemCount,
                            OrderStatus          = order.Status,
                            OrderId              = order.OrderId,
                            EstimatedOrderAmount = order.EstimatedOrderAmount
                        });
                }
            }
        }
    }
}
