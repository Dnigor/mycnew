using myCustomers.Web.Models;
using Quartet.Entities.Views;

namespace myCustomers.Web.Services
{
    public class CustomerTaskContentMapper : IMappingService<TaskListItem, CustomerTaskContent>
    {
        public CustomerTaskContent Map(TaskListItem source)
        {
            return new CustomerTaskContent 
            { 
                CustomerId  = source.CustomerId,
                Description = source.Description,
                DueDateUtc  = source.DueDateUtc,
                FirstName   = source.CustomerFirstName,
                LastName    = source.CustomerLastName,
                TaskId      = source.TaskId,
                Title       = source.MapTaskTitle(),
                TaskType    = source.TaskType
            };
        }
    }
}