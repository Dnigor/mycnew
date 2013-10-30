using System.Linq;
using Quartet.Entities.Views;
using myCustomers.Web.Models;

namespace myCustomers.Web.Services
{
    public class DashboardViewModelMapper : IMappingService<NotificationCounts, DashboardViewModel>
    {
       
        public DashboardViewModel Map(NotificationCounts counts)
        {
            if (counts == null)
                return null;

            return new DashboardViewModel
            {
                CustomersCount = counts.CustomerNotificationCount,
                OrdersCount    = counts.NewOrderNotificationCount,
                TasksCount     = counts.TaskTypeCounts.Sum(t => t.Count)
            };
        }
    }
}