using System;
using System.Web.Mvc;
using myCustomers.Web.Models;
using myCustomers.Web.Services;
using Quartet.Client.Customers;
using Quartet.Entities;
using Quartet.Entities.Commands;

namespace myCustomers.Web.Controllers
{
    [AuthorizeConsultant]
    public class SampleRequestsController : Controller
    {
        readonly IQuartetClientFactory _clientFactory;
        readonly IMappingService<Customer, HCardViewModel> _hcardMapping;
        readonly ICustomersQueryServiceClient _customersQuery;

        public SampleRequestsController(IQuartetClientFactory clientFactory, IMappingService<Customer, HCardViewModel> hcardMapping)
        {
            _clientFactory = clientFactory;
            _hcardMapping = hcardMapping;
            _customersQuery = _clientFactory.GetCustomersQueryServiceClient();
        }

        public ActionResult Detail(Guid? id)
        {
            if (!id.HasValue)
                throw new InvalidOperationException("Sample Request id is null");

            var sampleRequest = _customersQuery.GetSampleRequest(id.Value);

            if (!sampleRequest.CustomerId.HasValue)
                throw new InvalidOperationException("Sample Request CustomerId is null");

            var customer = _customersQuery.GetCustomer(sampleRequest.CustomerId.Value);

            var model = new SampleRequestDetailViewModel
            {
                Customer = _hcardMapping.Map(customer),
                SampleRequestId = id.Value
            };

            return View(model);
        }

        public ActionResult Save(SampleRequestSaveViewModel model)
        {
            if (model.SampleRequestId == null)
                throw new InvalidOperationException("SampleRequstid is null");

            if (model.FollowUpDate == DateTime.MinValue)
                throw new InvalidOperationException("FollowUpDate is incorrect");

            if (model.DeliveryDate == DateTime.MinValue)
                throw new InvalidOperationException("DeliveryDate is incorrect");


            var sampleRequest = _customersQuery.GetSampleRequest(model.SampleRequestId);

            var completeSrCommand = new CompleteSampleRequest
            {
                SampleRequestId = sampleRequest.SampleRequestId,
                DeliveryDateUtc = model.DeliveryDate.ToUniversalTime()
            };

            _clientFactory.GetCommandServiceClient().Execute(completeSrCommand);

            var followUpTask = new AddTask
            {
                TaskId            = Guid.NewGuid(),
                SampleRequestId   = sampleRequest.SampleRequestId,
                TaskType          = TaskType.ProductSampleRequestFollowUp,
                CustomerFirstName = sampleRequest.FirstName,
                CustomerLastName  = sampleRequest.LastName,
                CustomerId        = sampleRequest.CustomerId,
                DueDateUtc        = model.FollowUpDate.ToUniversalTime(),
                Title             = "[NL] SampleRequestFollow-Up"
            };

            sampleRequest.FollowUpTaskId = followUpTask.TaskId;

            _clientFactory.GetCommandServiceClient().Execute(followUpTask);

            var deliveryTask = new AddTask
            {
                TaskId            = Guid.NewGuid(),
                SampleRequestId   = sampleRequest.SampleRequestId,
                TaskType          = TaskType.ProductSampleDelivery,
                CustomerFirstName = sampleRequest.FirstName,
                CustomerLastName  = sampleRequest.LastName,
                CustomerId        = sampleRequest.CustomerId,
                DueDateUtc        = model.DeliveryDate.ToUniversalTime(),
                Title             = "[NL] Product Sample Delivery"
            };

            _clientFactory.GetCommandServiceClient().Execute(deliveryTask);

            return Json("Tasks were created");
        }
    }

    public class SampleRequestSaveViewModel
    {
        public Guid SampleRequestId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public DateTime FollowUpDate { get; set; }
    }
}