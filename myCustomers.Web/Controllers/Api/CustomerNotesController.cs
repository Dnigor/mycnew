using System;
using System.Net;
using System.Web.Http;
using myCustomers.Contexts;
using myCustomers.Web.Models;
using myCustomers.Web.Services;
using Quartet.Entities;
using Quartet.Entities.Commands;
using System.Linq;

namespace myCustomers.Web.Controllers.Api
{
    [ApiAuthorizeConsultant]
    public class CustomerNotesController : ApiController
    {
        IQuartetClientFactory _clientFactory;
        IConsultantContext _consultantContext;
        IMappingService<Customer, CustomerDetailViewModel> _customerMappingService;
        IImportCustomerService _importCustomer;
        
        public CustomerNotesController
        (
            IQuartetClientFactory clientFactory,
            IConsultantContext consultantContext,
            IMappingService<Customer, CustomerDetailViewModel> customerMappingService,
            IImportCustomerService importCustomer
        )
        {
            _clientFactory          = clientFactory;
            _consultantContext      = consultantContext;
            _customerMappingService = customerMappingService;
            _importCustomer         = importCustomer;
        }

        // POST ~/api/customers/{custid}/notes/ - creates a new note
        [AcceptVerbs("POST")]
        public NoteContent CreateNote(Guid custid, NoteContent noteContent)
        {
            var consultantKey = _consultantContext.Consultant.ConsultantKey.Value;
            var command = new AddCustomerNote
            {
                CustomerId = custid,
                CustomerNote = new CustomerNote 
                {
                    ConsultantKey   = consultantKey,
                    Content         = noteContent.Content,
                    CustomerId      = custid,
                    CustomerNoteKey = Guid.NewGuid(),
                    DateCreatedUtc  = DateTime.UtcNow
                }
            };

            _clientFactory.GetCommandServiceClient().Execute(command);

            return new NoteContent
            {
                Content         = command.CustomerNote.Content,
                DateCreatedUtc  = command.CustomerNote.DateCreatedUtc,
                CustomerNoteKey = command.CustomerNote.CustomerNoteKey
            };
        }

        // POST ~/api/customers/{custid}/notes/{id} - updates a note
        [AcceptVerbs("POST")]
        public HttpStatusCode UpdateNote(Guid custid, Guid id, NoteContent noteContent)
        {
            var consultantKey = _consultantContext.Consultant.ConsultantKey.Value;
            var command = new Quartet.Entities.Commands.UpdateCustomerNote
            {
                CustomerId = custid,
                CustomerNote = new CustomerNote
                {
                    ConsultantKey   = consultantKey,
                    Content         = noteContent.Content,
                    CustomerId      = custid,
                    CustomerNoteKey = noteContent.CustomerNoteKey,
                    DateCreatedUtc  = noteContent.DateCreatedUtc.ToUniversalTime()
                }
            };

            _clientFactory.GetCommandServiceClient().Execute(command);
            return HttpStatusCode.OK;
        }

        // DELETE ~/api/customers/{custid}/notes/{id} - deletes a note
        [AcceptVerbs("DELETE")]
        public HttpStatusCode DeleteNote(Guid custid, Guid id, NoteContent noteContent)
        {
            var client = _clientFactory.GetCommandServiceClient();

            var command = new DeleteCustomerNote
            {
                CustomerId = custid,
                CustomerNoteKey = id
            };

            client.Execute(command);
            return HttpStatusCode.OK;
        }

        // GET ~/api/customers/{custid}/notes - gets a customers notes
        [AcceptVerbs("GET")]
        public CustomerNote[] GetNotes(Guid custid)
        {
            var client = _clientFactory.GetCustomersQueryServiceClient();
            var customer = client.GetCustomer(custid);

            
            if (customer.Notes != null && customer.Notes.Count > 0)
            {
                var model = _customerMappingService.Map(customer);
                return model.Notes;
            }
            return new CustomerNote[0];
        }
    }
}