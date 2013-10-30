using MaryKay.Configuration;
using myCustomers.Web.Models;
using Quartet.Entities.Views;

namespace myCustomers.Web.Services
{
    public class CustomerTaskActivityMapper : IMappingService<TaskListItem, Activity>
    {
        IAppSettings _appSettings;

        public CustomerTaskActivityMapper(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }
        
        public Activity Map(TaskListItem source)
        {
            var result = new Activity 
            {
                ActivityType   = "Task",
                DisplayDateUtc = source.CreatedDateUtc,
                Content        = source.MapTaskTitle(),
                FollowUpType   = source.TaskType,
                IsComplete     = source.IsComplete,
                OrderId        = source.OrderId,
					 DueDateUtc     = source.DueDateUtc
            };
            return result;
        }

        
    }
}