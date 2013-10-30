using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Quartet.Entities;
using Quartet.Entities.Views;
using myCustomers.Contexts;
using Quartet.Entities.Search;
using Quartet.Client.Customers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using myCustomers.Web.Models;
using myCustomers.Web.Services;

namespace myCustomers.Web.Controllers.Api
{
    [ApiAuthorizeConsultant]
    public class SampleRequestController : ApiController
    {
        readonly IQuartetClientFactory _clientFactory;
        readonly IConsultantContext _consultantContext;
        private readonly IMappingService<SampleRequest, SampleRequestViewModel> _sampleRequestMappingService;

        public SampleRequestController
        (
            IQuartetClientFactory clientFactory,
            IConsultantContext consultantContext,
            IMappingService<SampleRequest, SampleRequestViewModel> sampleRequestMappingService
        )
        {
            _clientFactory = clientFactory;
            _consultantContext = consultantContext;
            _sampleRequestMappingService = sampleRequestMappingService;
        }

        [AcceptVerbs("GET")]
        public HttpResponseMessage GetById(Guid id)
        {
            if (id == null)
                throw ApiHelpers.ServerError("Sample Request id is null");

            var customersQuery = _clientFactory.GetCustomersQueryServiceClient();

            var sampleRequest = customersQuery.GetSampleRequest(id);
            var mappedRequest = _sampleRequestMappingService.Map(sampleRequest);

            if (sampleRequest.FollowUpTaskId.HasValue)
            {
                var followUpTask = _clientFactory.GetTaskQueryServiceClient().GetTaskById(sampleRequest.FollowUpTaskId.Value);
                if (followUpTask.DueDateUtc.HasValue)
                    mappedRequest.FollowUpDateUtc = DateTime.SpecifyKind(followUpTask.DueDateUtc.Value, DateTimeKind.Utc);
            }

            var serializedSampleRequest = JsonConvert.SerializeObject(mappedRequest, new IsoDateTimeConverter());

            return ApiHelpers.JsonResponseMessage(serializedSampleRequest);
        }

        [AcceptVerbs("GET")]
        public HttpResponseMessage Search
        (
            string q = null,  // search terms
            Guid? cid = null,  // customer id
            DateTime? dcs = null,  // min date created 
            DateTime? dce = null,  // max date created 
            int i = 1,     // page number
            int s = 5      // page size
        )
        {
            if (ModelState.IsValid)
            {

                // no search client is implemented for sample requests in quartet. Until then return all sample requests.

                var client = _clientFactory.GetCustomersQueryServiceClient();
                var sampleRequests = client.QuerySampleRequestsByStatus(false, true).OrderByDescending(smpl => smpl.RequestDateUtc);                    

                var json = JsonConvert.SerializeObject(sampleRequests, new IsoDateTimeConverter());

                return ApiHelpers.JsonResponseMessage(json);
            }

            throw ApiHelpers.ServerError(ModelState);
        }
    }
}
