using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using myCustomers.Contexts;
using Quartet.Entities.Search;

namespace myCustomers.Web.Controllers.Api
{
    [ApiAuthorizeConsultant]
    public class NotesController : ApiController
    {
        IQuartetClientFactory _clientFactory;
        IConsultantContext _consultantContext;
        
        public NotesController
        (
            IQuartetClientFactory clientFactory,
            IConsultantContext consultantContext
        )
        {
            _clientFactory     = clientFactory;
            _consultantContext = consultantContext;
        }

        // GET ~/api/customers/{custid}/notes - gets a customers notes
        [AcceptVerbs("GET")]
        public HttpResponseMessage Search
        (
            string q      = null,  // search terms
            Guid? cid     = null,  // customer id
            DateTime? dcs = null,  // min date created 
            DateTime? dce = null,  // max date created 
            int i         = 1,     // page number
            int s         = 5      // page size
        )
        {
            if (ModelState.IsValid)
            {
                var client = _clientFactory.GetCustomerNotesSearchClient();

                var criteria = new CustomerNoteCriteria
                {
                    QueryString       = q,
                    MinDateCreatedUtc = dcs,
                    MaxDateCreatedUtc = dce,
                    Page              = i - 1,
                    PageSize          = s
                };

                // if no query string is provided then set the sort to order date descending
                if (string.IsNullOrWhiteSpace(criteria.QueryString))
                    criteria.Sort = new[] { new Sort { Field = "DateCreatedUtc", Descending = true } };

                var json = client.Search(criteria);

                return ApiHelpers.JsonResponseMessage(json);
            }

            throw ApiHelpers.ServerError(ModelState);
        }
    }
}