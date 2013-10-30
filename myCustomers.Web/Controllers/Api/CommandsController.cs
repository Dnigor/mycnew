using System;
using System.Linq;
using System.Web.Http;
using myCustomers.Contexts;
using myCustomers.Web.Models;
using NLog;
using Quartet.Entities;
using Quartet.Entities.Commands;

namespace myCustomers.Web.Controllers.Api
{
    [ApiAuthorizeConsultant]
    public class CommandsController : ApiController
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        IQuartetClientFactory _clientFactory;
        IConsultantContext _consultantContext;

        public CommandsController
        (
            IQuartetClientFactory clientFactory,
            IConsultantContext consultantContext
        )
        {
            _clientFactory     = clientFactory;
            _consultantContext = consultantContext;
        }

        [AcceptVerbs("POST")]
        public void DeleteCustomer(DeleteUndeleteRequest req)
        {
            if (req.Id == Guid.Empty)
                throw ApiHelpers.ServerError("Request contains an invalid customer id");

            _clientFactory.GetCommandServiceClient().Execute(new DeleteCustomer { CustomerId = req.Id });
        }

        [AcceptVerbs("POST")]
        public void UndeleteCustomer(DeleteUndeleteRequest req)
        {
            if (req.Id == Guid.Empty)
                throw ApiHelpers.ServerError("Request contains an invalid customer id");

            _clientFactory.GetCommandServiceClient().Execute(new UndeleteCustomer { CustomerId = req.Id });
        }

        [AcceptVerbs("POST")]
        public void DeleteCustomers(DeleteUndeleteManyRequest req)
        {
            if (req.Ids == null || req.Ids.Any(id => id == Guid.Empty))
                throw ApiHelpers.ServerError("Request contains an invalid customer id");

            var client = _clientFactory.GetCommandServiceClient();
            foreach (var id in req.Ids)
                client.Execute(new DeleteCustomer { CustomerId = id });
        }

        [AcceptVerbs("POST")]
        public void UndeleteCustomers(DeleteUndeleteManyRequest req)
        {
            if (req.Ids == null || req.Ids.Any(id => id == Guid.Empty))
                throw ApiHelpers.ServerError("Request contains an invalid customer id");

            var client = _clientFactory.GetCommandServiceClient();
            foreach (var id in req.Ids)
                client.Execute(new UndeleteCustomer { CustomerId = id });
        }

        [AcceptVerbs("POST")]
        public void PurgeCustomers(DeleteUndeleteManyRequest req)
        {
            if (req.Ids == null || req.Ids.Any(id => id == Guid.Empty))
                throw ApiHelpers.ServerError("Request contains an invalid customer id");

            var client = _clientFactory.GetCommandServiceClient();
            client.Execute(new PurgeCustomers { CustomerIds = req.Ids });
        }

        [AcceptVerbs("POST")]
        public void DeleteOrder(DeleteUndeleteRequest req)
        {
            if (req.Id == Guid.Empty)
                throw ApiHelpers.ServerError("Request contains an invalid order id");

            _clientFactory.GetCommandServiceClient().Execute(new DeleteCustomerOrder { OrderId = req.Id });
        }

        [AcceptVerbs("POST")]
        public void UndeleteOrder(DeleteUndeleteRequest req)
        {
            if (req.Id == Guid.Empty)
                throw ApiHelpers.ServerError("Request contains an invalid order id");

            _clientFactory.GetCommandServiceClient().Execute(new UndeleteCustomerOrder { OrderId = req.Id });
        }

        [AcceptVerbs("POST")]
        public void DeleteOrders(DeleteUndeleteManyRequest req)
        {
            if (req.Ids == null || req.Ids.Any(id => id == Guid.Empty))
                throw ApiHelpers.ServerError("Request contains an invalid order id");

            var client = _clientFactory.GetCommandServiceClient();
            foreach (var id in req.Ids)
                client.Execute(new DeleteCustomerOrder { OrderId = id });
        }

        [AcceptVerbs("POST")]
        public void UndeleteOrders(DeleteUndeleteManyRequest req)
        {
            if (req.Ids == null || req.Ids.Any(id => id == Guid.Empty))
                throw ApiHelpers.ServerError("Request contains an invalid order id");

            var client = _clientFactory.GetCommandServiceClient();
            foreach (var id in req.Ids)
                client.Execute(new UndeleteCustomerOrder { OrderId = id });
        }

        [AcceptVerbs("POST")]
        public void PurgeOrders(DeleteUndeleteManyRequest req)
        {
            if (req.Ids == null || req.Ids.Any(id => id == Guid.Empty))
                throw ApiHelpers.ServerError("Request contains an invalid order id");

            var client = _clientFactory.GetCommandServiceClient();
            client.Execute(new PurgeOrders { OrderIds = req.Ids });
        }

        [AcceptVerbs("POST")]
        public void MarkTaskComplete(DeleteUndeleteRequest req)
        {
            if (req.Id == Guid.Empty)
                throw ApiHelpers.ServerError("Request contains an invalid task id");

            _clientFactory.GetCommandServiceClient().Execute(new MarkTaskComplete { TaskId = req.Id });
        }

        [AcceptVerbs("POST")]
        public void MarkTaskNotComplete(DeleteUndeleteRequest req)
        {
            if (req.Id == Guid.Empty)
                throw ApiHelpers.ServerError("Request contains an invalid task id");

            _clientFactory.GetCommandServiceClient().Execute(new MarkTaskIncomplete { TaskId = req.Id });
        }

        [AcceptVerbs("POST")]
        public void UpdateTaskDescriptionTitleDueDateUtc(UpdateTaskDescriptionTitleAndDueDateUtc req)
        {
            if (req.TaskId == Guid.Empty)
                throw ApiHelpers.ServerError("Request contains an invalid task id");

            var taskClient = _clientFactory.GetTaskQueryServiceClient();
            var task = taskClient.GetTaskById(req.TaskId);

            if (task == null) throw ApiHelpers.ServerError("Task for which an update was requested does not exist");

            if (task.TaskType != TaskType.Consultant && task.TaskType != TaskType.Customer && !String.IsNullOrEmpty(task.Title))
            {
                req.Title = task.Title;
            }

            var command = new UpdateTask
            {
                CustomerFirstName = task.CustomerFirstName,
                CustomerId        = task.CustomerId,
                CustomerLastName  = task.CustomerLastName,
                Description       = task.Description,
                DueDateUtc        = task.DueDateUtc,
                Metadata          = task.Metadata,
                OrderId           = task.OrderId,
                ProductId         = task.ProductId,
                SampleRequestId   = task.SampleRequestId,
                TaskId            = task.TaskId,
                TaskType          = task.TaskType,
                Title             = req.Title
            };

            if (!string.IsNullOrWhiteSpace(req.Description))
                command.Description = req.Description;

            if (req.DueDateUtc.HasValue)
                command.DueDateUtc = req.DueDateUtc.Value.ToUniversalTime();

            _clientFactory.GetCommandServiceClient().Execute(command);
        }

        [AcceptVerbs("POST")]
        public void UpdatePaymentStatus(PaymentStatusViewModel paymentStatusViewModel)
        {
            var client = _clientFactory.GetCommandServiceClient();
            foreach (Guid id in paymentStatusViewModel.OrderIds)
            {
                var cmdUpdate = new UpdateCustomerOrderPaymentStatus()
                {
                    OrderId = id,
                    OrderPaymentStatus = paymentStatusViewModel.PaymentStatus
                };
                client.Execute(cmdUpdate);
            }           
        }

        [AcceptVerbs("POST")]
        public void ArchiveOrders(DeleteUndeleteManyRequest req)
        {
            var client = _clientFactory.GetCommandServiceClient();
            foreach (Guid id in req.Ids)
            {
                client.Execute(new ArchiveCustomerOrder() { OrderId = id });
            }
        }

        [AcceptVerbs("POST")]
        public void UnarchiveOrders(DeleteUndeleteManyRequest req)
        {
            var client = _clientFactory.GetCommandServiceClient();
            foreach (Guid id in req.Ids)
            {
                client.Execute(new UnarchiveCustomerOrder() { OrderId = id });
            }
        }

        [AcceptVerbs("POST")]
        public void ArchiveOrder(DeleteUndeleteRequest req)
        {
            if (req.Id == Guid.Empty)
                throw ApiHelpers.ServerError("Request contains an invalid order id");

            _clientFactory.GetCommandServiceClient().Execute(new ArchiveCustomerOrder { OrderId = req.Id });
        }

        [AcceptVerbs("POST")]
        public void UpdateOrderStatus(UpdateOrderStatusReq req)
        {
            if (req.Id == Guid.Empty)
                throw ApiHelpers.ServerError("Request contains an invalid order id");

            _clientFactory.GetCommandServiceClient().Execute(new UpdateCustomerOrderStatus { OrderId = req.Id, OrderStatus = req.OrderStatus});
        }

        [AcceptVerbs("POST")]
        public void UnarchiveOrder(DeleteUndeleteRequest req)
        {
            if (req.Id == Guid.Empty)
                throw ApiHelpers.ServerError("Request contains an invalid order id");

            _clientFactory.GetCommandServiceClient().Execute(new UnarchiveCustomerOrder { OrderId = req.Id });
        }

        [AcceptVerbs("POST")]
        public void ArchiveCustomers(DeleteUndeleteManyRequest req)
        {
            var client = _clientFactory.GetCommandServiceClient();
            foreach (Guid id in req.Ids)
            {
                client.Execute(new ArchiveCustomer() { CustomerId = id });
            }
        }

        [AcceptVerbs("POST")]
        public void UnarchiveCustomers(DeleteUndeleteManyRequest req)
        {
            var client = _clientFactory.GetCommandServiceClient();
            foreach (Guid id in req.Ids)
            {
                client.Execute(new UnarchiveCustomer() { CustomerId = id });
            }
        }

        [AcceptVerbs("POST")]
        public void ArchiveCustomer(DeleteUndeleteRequest req)
        {
            if (req.Id == Guid.Empty)
                throw ApiHelpers.ServerError("Request contains an invalid customer id");

            _clientFactory.GetCommandServiceClient().Execute(new ArchiveCustomer { CustomerId = req.Id });
        }

        [AcceptVerbs("POST")]
        public void UnarchiveCustomer(DeleteUndeleteRequest req)
        {
            if (req.Id == Guid.Empty)
                throw ApiHelpers.ServerError("Request contains an invalid customer id");

            _clientFactory.GetCommandServiceClient().Execute(new UnarchiveCustomer { CustomerId = req.Id });
        }

 		[AcceptVerbs("POST")]
        public void DeleteGroups(DeleteUndeleteManyRequest req)
        {
            var client = _clientFactory.GetCommandServiceClient();
            if(req.Ids == null)
                throw ApiHelpers.ServerError("Request contains an invalid group id");
            
            foreach (Guid id in req.Ids)
            {
                client.Execute(new DeleteGroup { GroupId = id });
            }
        }
		
        [AcceptVerbs("POST")]
        public void AddToGroup(AddToGroupRequest req)
        {
            var customersQueryService = _clientFactory.GetCustomersQueryServiceClient();
            var customersAlreadyInGroup = customersQueryService.GetCustomersByGroupId(req.GroupId);

            var command = new AddCustomersToGroup
            {
                GroupId = req.GroupId,
                CustomerIds = req.Ids.Except(customersAlreadyInGroup.Select(c => c.CustomerId)).ToArray(),
            };

            if (command.CustomerIds.Any())
            {
                var commandService = _clientFactory.GetCommandServiceClient();
                commandService.Execute(command);
            }
        }
		
        [AcceptVerbs("POST")]
        public void DeleteFromGroup(DeleteFromGroupRequest req)
        {
            var client = _clientFactory.GetCommandServiceClient();
            Guid[] customersToDelete = null;

            if (req.AllSelected)
            {
                var queryService = _clientFactory.GetCustomersQueryServiceClient();
                var customers = queryService.GetCustomersByGroupId(req.GroupId).Select(c => c.CustomerId);
                customersToDelete = customers.Except(req.Ids).ToArray();
            }
            else customersToDelete = req.Ids;
           
            if (customersToDelete != null)
                client.Execute(new RemoveCustomersFromGroup() {  CustomerIds = customersToDelete, GroupId = req.GroupId });
        }

        public class DeleteUndeleteRequest
        {
            public Guid Id { get; set; }
        }

        public class DeleteUndeleteManyRequest
        {
            public Guid[] Ids { get; set; }
        }

        public class UpdateTaskDescriptionTitleAndDueDateUtc
        {
            public Guid TaskId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public DateTime? DueDateUtc { get; set; }
        }

        public class UpdateOrderStatusReq
        {
            public Guid Id { get; set; }
            public OrderStatus OrderStatus { get; set; }
        }

        public class AddToGroupRequest
        {
            public Guid GroupId { get; set; }
            public Guid[] Ids { get; set; }
        }

        public class DeleteFromGroupRequest
        {
            public Guid GroupId { get; set; }
            public Guid[] Ids { get; set; }
            public bool AllSelected { get; set; }
        }
    }
}
