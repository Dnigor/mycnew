using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aspose.Cells;
using myCustomers.Web.Models;

namespace myCustomers.Web.Services
{
    public enum InvoiceFormat
    {
        Xls,
        Pdf
    }

    public class InvoiceModel
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] Content { get; set; }
    }

	public interface IInvoicingService
	{
        InvoiceModel Export(CreateInvoiceViewModel createInvoiceViewModel);
    }
}