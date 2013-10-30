using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;


namespace myCustomers.Web.Controllers.Api
{
    [ApiAuthorizeConsultant]
    public class GroupsController : ApiController
    {
        IQuartetClientFactory _clientFactory;
        
        public GroupsController(IQuartetClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        // GET ~/api/customers/groups - gets a customers groups
        [AcceptVerbs("GET")]
        public HttpResponseMessage Search()
        {
            if (ModelState.IsValid)
            {
                var client = _clientFactory.GetGlobalQueryServiceClient();

                var groups = client.GetGroupsByConsultant();

                var json = JsonConvert.SerializeObject(groups, new IsoDateTimeConverter());

                return ApiHelpers.JsonResponseMessage(json);
            }

            throw ApiHelpers.ServerError(ModelState);
        }

      
    }
}