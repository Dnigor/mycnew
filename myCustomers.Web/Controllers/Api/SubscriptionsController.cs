using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using myCustomers.Web.Models;
using myCustomers.Web.Services;

namespace myCustomers.Web.Controllers.Api
{
    [ApiAuthorizeConsultant]
    public class SubscriptionsController : ApiController
    {       
        IImportCustomerService _importCustomer;

        public SubscriptionsController
        (
            IImportCustomerService importCustomer
        )
        {
            _importCustomer = importCustomer;
        }

        // POST ~/api/subscriptions/submitsubscriptions - submit imported customers' subscriptions
        [AcceptVerbs("POST")]
        public HttpStatusCode SubmitSubscriptions(ImportedCustomer[] importedCustomers)
        {
            _importCustomer.SubmitSubscriptions(importedCustomers);
            return HttpStatusCode.OK;
        }
    }
}
