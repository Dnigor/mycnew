using System;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Quartet.Services.Contracts;
using myCustomers.Web.Models;

namespace myCustomers.Web.Controllers.Api
{
    public class CustomerGroupsController : ApiController
    {
        IQuartetClientFactory _clientFactory;

        public CustomerGroupsController(IQuartetClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        // GET ~/api/customergroups/ - gets customers in the group
        [AcceptVerbs("GET")]
        public HttpResponseMessage GetConsultantsByGroup(CustomerGroupModel model)
        {
            if (ModelState.IsValid)
            {
                var criteria = new GroupIdQueryParameters
                {
                    GroupId  = model.g,
                    Page     = model.i,
                    PageSize = model.s,
                };

                var client    = _clientFactory.GetCustomersQueryServiceClient();
                var customers = client.GetCustomersByGroupId(criteria);
                var json      = JsonConvert.SerializeObject(customers.Data, new IsoDateTimeConverter());

                return ApiHelpers.JsonResponseMessage(json);
            }

            throw ApiHelpers.ServerError(ModelState);
        }


        // GET api/customergroups/{custId}
        [AcceptVerbs("GET")]
        public dynamic GetGroupsByConsultant(Guid custid)
        {
            if (ModelState.IsValid)
            {
                var client = _clientFactory.GetGlobalQueryServiceClient();
                var groups = client.GetGroupsByCustomer(custid);
                //var json = JsonConvert.SerializeObject(groups, new IsoDateTimeConverter());
                //return ApiHelpers.JsonResponseMessage(json);
                return groups;
            }

            throw ApiHelpers.ServerError(ModelState);
        }

        [AcceptVerbs("POST")]
        public void AddCustomerToGroup(Guid custid, Guid id)
        {
            var command = new Quartet.Entities.Commands.AddCustomersToGroup
            {
                CustomerIds = new Guid[1] { custid },
                GroupId = id
            };
            _clientFactory.GetCommandServiceClient().Execute(command);
        }

        [AcceptVerbs("POST")]
        public dynamic AddCustomerToNewGroup(Guid custid, Group group)
        {
            var command = new Quartet.Entities.Commands.AddGroup
            {
                GroupId = Guid.NewGuid(),
                Name = group.GroupName
            };
            _clientFactory.GetCommandServiceClient().Execute(command);

            AddCustomerToGroup(custid, command.GroupId);

            return new Quartet.Entities.Views.Group
            {
                GroupId = command.GroupId,
                Name = command.Name,
                Description = command.Description
            };
        }


        [AcceptVerbs("DELETE")]
        public void RemoveCustomerFromGroup(Guid custid, Guid id)
        {
            var command = new Quartet.Entities.Commands.RemoveCustomersFromGroup
            {
                CustomerIds = new Guid[1] { custid },
                GroupId = id
            };
            _clientFactory.GetCommandServiceClient().Execute(command);
        }

        [FromUri]
        public class CustomerGroupModel
        {
            public Guid g { get; set; }       // groupID
            // sort descending flag
            public int i { get; set; }          // page number
            public int s { get; set; }          // page size

            public CustomerGroupModel()
            {
                i = 1;
                s = 10;
            }
        }

    }
}
