using System.IO;
using myCustomers.Web.Models;

namespace myCustomers.Web.Services
{
	public interface ICustomerExportService
	{
        byte[] CreateLabels(CustomerSelectionModel selection);
        byte[] Export(CustomerSelectionModel selection);
        byte[] Print(CustomerSelectionModel selection);
	}
}