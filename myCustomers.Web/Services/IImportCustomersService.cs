using System.Collections.Generic;
using System.Web;
using myCustomers.Data;
using myCustomers.Web.Models;

namespace myCustomers.Web.Services
{
	public interface IImportCustomerService
	{
		void GetContactsFromCSVFile(HttpPostedFileBase file);
		List<UploadedCustomer> UploadedCustomers { get; }
        string FacebookUrl { get; }
        void GetFacebookContacts(string authToken);
        ImportedCustomer[] ImportUploadedCustomers();
        void SubmitSubscriptions(ImportedCustomer[] importedCustomers);
	}
}