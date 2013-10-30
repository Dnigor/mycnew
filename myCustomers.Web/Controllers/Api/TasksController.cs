using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using myCustomers.Contexts;
using myCustomers.Web.Models;
using myCustomers.Web.Services;
using NLog;
using Quartet.Entities;
using Quartet.Entities.Commands;
using Quartet.Entities.Views;
using Quartet.Services.Contracts;

namespace myCustomers.Web.Controllers.Api
{
    [ApiAuthorizeConsultant]
    public class TasksController : ApiController
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        
        IQuartetClientFactory _clientFactory;
        IMappingService<TaskListItem, CustomerTaskContent> _taskMapping;
        IConsultantContext _consultantContext;

        public TasksController
		(
			IQuartetClientFactory clientFactory,
            IMappingService<TaskListItem, CustomerTaskContent> taskMapping,
            IConsultantContext consultantContext
		)
		{
			_clientFactory = clientFactory;
			_taskMapping = taskMapping;
            _consultantContext = consultantContext;
		}

        [FromUri]
        public class TaskCriteria
        {
            public DateTime? MinDueDateUtc { get; set; }
            public DateTime? MaxDueDateUtc { get; set; }
            public int PageSize { get; set; }
            public int Page { get; set; }
            public bool? IsComplete { get; set; }

            public TaskCriteria()
            {
                    Page     = 1;
                    PageSize = 5;
            }
        }

        [AcceptVerbs("GET")]
        public CustomerTaskResult GetCustomerReminders(Guid custId, TaskCriteria criteria)
        {
            if (criteria == null)
                criteria = new TaskCriteria 
                { 
                    Page     = 1,
                    PageSize = 10
                };
            
            var taskQuery = _clientFactory.GetTaskQueryServiceClient();
            var results = taskQuery.QueryTaskListByCustomerId(new CustomerIdParameters
            {
                CustomerId = custId,
                Page = criteria.Page,
                PageSize = 500, //REVIEW: Pulling them all down until quartet can be updated with additional filters or elastic search is adopted
                SortOptions = new SortOption[] { new SortOption { Direction = SortDirection.Descending, Name = "CreatedDateUtc" } }
            });

            if (criteria.IsComplete.HasValue)
            {
                results = results.Where(t => t.IsComplete == criteria.IsComplete.Value).ToArray();
            }

            //REVIEW: Pulling them all down until quartet can be updated with additional filters or elastic search is adopted
            var count = results.Length;
            results = results.Take(criteria.PageSize).ToArray();

            var mappedResults = new List<CustomerTaskContent>();
            foreach (var result in results)
                mappedResults.Add(_taskMapping.Map(result));

            return new CustomerTaskResult
            {
                Tasks = mappedResults.ToArray(),
                Count = count
            };
        }

        [AcceptVerbs("POST")]
        public CustomerTaskContent CreateCustomerReminder(Guid custId, CustomerTaskContent content)
        {
            content.TaskId = Guid.NewGuid();

            var commandService = _clientFactory.GetCommandServiceClient();
            var command = new AddTask
            {
                CustomerFirstName = content.FirstName,
                CustomerId        = custId,
                CustomerLastName  = content.LastName,
                Description       = content.Description,
                DueDateUtc        = content.DueDateUtc.HasValue ? (DateTime?)content.DueDateUtc.Value.ToUniversalTime() : null,
                TaskId            = content.TaskId,
                TaskType          = TaskType.Customer,
                Title             = content.Title
            };
            
            try
            {
                commandService.Execute(command);
                content.TaskType = TaskType.Customer.ToString();
                return content;
            }
            catch (CommandException ex)
            {
                throw ApiHelpers.ServerError(ex.Message, string.Join("\r\n", ex.Errors));
            }
        }
    }
}
