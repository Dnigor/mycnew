using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Aspose.Cells;
using MaryKay.Configuration;
using MaryKay.IBCDataServices.Entities;
using myCustomers.Contexts;
using myCustomers.Globalization;
using Quartet.Client.Customers;
using Quartet.Entities;
using myCustomers.Web.Models;
using System.Globalization;
using myCustomers.Data;

namespace myCustomers.Web.Services
{
	public class InvoicingService : ExportServiceBase, IInvoicingService
	{
		readonly IAppSettings          _appSettings;
		readonly IQuartetClientFactory _quartetClientFactory;
        readonly IConsultantContext    _consultantContext;
        CultureInfo _numberFormatCulture;
        decimal orderSumTotal;

		public InvoicingService
                    (
                                IConsultantContext consultantContext,
                                IQuartetClientFactory clientFactory,
                                IAppSettings appSettings,
                                IEnvironmentConfig environmentConfig,
                                ISubsidiaryAccessor subsidiaryAccessor
                            ) : base(appSettings, environmentConfig, subsidiaryAccessor)
		{
			_quartetClientFactory = clientFactory;
            _consultantContext    = consultantContext;
			_appSettings          = appSettings;
            _numberFormatCulture  = CultureInfo.CreateSpecificCulture(_appSettings.GetValue("Order_InvoiceReceipt_NumberFormatCulture"));
		}

		public InvoiceModel Export(CreateInvoiceViewModel model)
		{
            var queryService = _quartetClientFactory.GetCustomersQueryServiceClient();
            var order = queryService.GetOrderById(model.OrderId, true);
			if (order != null)
			{				
				var confirmationNumber = order.ConfirmationNumber;
                var designer = GetWorkbookDesigner("Order_InvoiceReceipt_Designer.xls", LoadFormat.Excel97To2003);
                var data = GetData(order, model);
                ConfigureWorkbook(designer.Workbook);
                designer.SetDataSource(data);
                designer.Process();

                var stream = new MemoryStream();
                designer.Workbook.CalculateFormula();
                SetWorksheetFooter(designer.Workbook, 0, Resources.GetString("INVOICE_FOOTERLEFTSCRIPT").Trim(), Resources.GetString("INVOICE_FOOTERCENTERSCRIPT").Trim(), Resources.GetString("INVOICE_FOOTERRIGHTSCRIPT").Trim());
                designer.Workbook.Save(stream, new XlsSaveOptions(model.SaveFormat == InvoiceFormat.Xls ? SaveFormat.Excel97To2003 : SaveFormat.Pdf));

                return new InvoiceModel
                {
                    FileName    = "Order_" + confirmationNumber + (model.SaveFormat == InvoiceFormat.Xls ? ".xls" : ".pdf"),
                    ContentType = model.SaveFormat == InvoiceFormat.Xls ? "application/vnd.ms-excel" : "application/pdf",
                    Content     = stream.ToArray()
                };
			}

            return null;
		}

		void ConfigureWorkbook(Workbook workbook)
		{
            workbook.Worksheets[0].PageSetup.FirstPageNumber = 1;
		}

		DataSet GetData(Order order, CreateInvoiceViewModel model)
		{
			var data = new DataSet("InvoiceReceipt");
			
			var tblOrderItem = GetOrderItemTable(order);
			data.Tables.Add(tblOrderItem);

            var tblOrder = GetOrderTable(order, model);
            data.Tables.Add(tblOrder);

			return data;
		}

		DataTable GetOrderItemTable(Order order)
		{            
			var tblOrderItem = new DataTable("OrderItem");
			tblOrderItem.Columns.Add("NameBlock", typeof(string));
			tblOrderItem.Columns.Add("Quantity", typeof(int));
			tblOrderItem.Columns.Add("Price", typeof(string));
			tblOrderItem.Columns.Add("CurrencySymbol", typeof(string));
            tblOrderItem.Columns.Add("Total", typeof(string));
            orderSumTotal = 0;
			if (order.Items != null)
			{
				foreach (var orderItem in order.Items)
				{
					var row                    = tblOrderItem.NewRow();
					var name                   = !string.IsNullOrWhiteSpace(orderItem.Name) ? orderItem.Name.Trim() : null;
					var formula                = !string.IsNullOrWhiteSpace(orderItem.Formula) ? orderItem.Formula.Trim() : null;
					var shade                  = !string.IsNullOrWhiteSpace(orderItem.ShadeName) ? orderItem.ShadeName.Trim() : null;
					var formulaShadeComponents = new List<string>(2);

					if (formula != null) formulaShadeComponents.Add(formula);
					if (shade != null) formulaShadeComponents.Add(shade);

					var formulaShade = string.Join(_appSettings.GetValue("Order_InvoiceReceipt_FormulaShadeComponentSeparator"), formulaShadeComponents);

					if (!string.IsNullOrWhiteSpace(formulaShade))
						row["NameBlock"] = TrimAndNullify(name + "\n" + formulaShade);
					else
						row["NameBlock"] = TrimAndNullify(name);

					row["Quantity"]       = orderItem.Quantity;                    
                    row["Price"]          = orderItem.Price.ToString("N", _numberFormatCulture.NumberFormat);
                    var total             = orderItem.Price * orderItem.Quantity;
                    row["Total"]          = total.ToString("N", _numberFormatCulture.NumberFormat);
					row["CurrencySymbol"] = TrimAndNullify(_appSettings.GetValue("InvoiceCurrencySymbol"));
					tblOrderItem.Rows.Add(row);
                    orderSumTotal += total;
				}
			}

			return tblOrderItem;
		}

		DataTable GetOrderTable(Order order, CreateInvoiceViewModel model)
		{
            var consultant = _consultantContext.Consultant;
            
			var tblOrder = new DataTable("Order");

			// Page_1
			tblOrder.Columns.Add("ConfirmationNumber", typeof(string));
			tblOrder.Columns.Add("OrderDate", typeof(string));
			tblOrder.Columns.Add("ConsultantName", typeof(string));
			tblOrder.Columns.Add("ConsultantAddressInline", typeof(string));
			tblOrder.Columns.Add("ConsultantNameAddressBlock", typeof(string));
			tblOrder.Columns.Add("CustomerName", typeof(string));
			tblOrder.Columns.Add("CustomerNameAddressBlock", typeof(string));
			tblOrder.Columns.Add("ConsultantComment", typeof(string));
			tblOrder.Columns.Add("CurrencySymbol", typeof(string));
			tblOrder.Columns.Add("ConsultantGiftMessage", typeof(string));

			tblOrder.Columns.Add("SoldByLabel", typeof(string));
			tblOrder.Columns.Add("SoldToLabel", typeof(string));
			tblOrder.Columns.Add("OrderNumberLabel", typeof(string));
			tblOrder.Columns.Add("DateHeader", typeof(string));
			tblOrder.Columns.Add("QtyHeader", typeof(string));
			tblOrder.Columns.Add("ProductNameHeader", typeof(string));
			tblOrder.Columns.Add("PriceHeader", typeof(string));
			tblOrder.Columns.Add("AmountHeader", typeof(string));
			tblOrder.Columns.Add("PriceDisclaimer", typeof(string));
			tblOrder.Columns.Add("EstimatedValueLabel", typeof(string));
			tblOrder.Columns.Add("AdjustmentsLabel", typeof(string));
			tblOrder.Columns.Add("SalesTaxLabel", typeof(string));
			tblOrder.Columns.Add("TotalLabel", typeof(string));
			tblOrder.Columns.Add("NoteText", typeof(string));
            tblOrder.Columns.Add("SumTotal", typeof(string));

			// Page_2
			tblOrder.Columns.Add("SatisfactionGuaranteeHeader", typeof(string));
			tblOrder.Columns.Add("Paragraph1", typeof(string));
			tblOrder.Columns.Add("Paragraph2", typeof(string));
			tblOrder.Columns.Add("Paragraph3", typeof(string));
			tblOrder.Columns.Add("MaryKayContactText", typeof(string));
			tblOrder.Columns.Add("NoticeOfCancellationText", typeof(string));
			tblOrder.Columns.Add("DateLabel", typeof(string));
			tblOrder.Columns.Add("Paragraph4", typeof(string));
			tblOrder.Columns.Add("Paragraph5", typeof(string));
			tblOrder.Columns.Add("Paragraph6", typeof(string));
			tblOrder.Columns.Add("Paragraph7", typeof(string));
			tblOrder.Columns.Add("Paragraph8", typeof(string));
			tblOrder.Columns.Add("CancelTransactionText", typeof(string));
			tblOrder.Columns.Add("NameOfSellerLabel", typeof(string));
			tblOrder.Columns.Add("AddressOfSeller", typeof(string));
			tblOrder.Columns.Add("BuyersSignatureLabel", typeof(string));
			tblOrder.Columns.Add("TradeMarkText", typeof(string));

			DataRow row = tblOrder.NewRow();

			row["ConfirmationNumber"] = TrimAndNullify(order.ConfirmationNumber);

			var localTimeZoneId   = _appSettings.GetValue("SystemTimeZoneId");
			var localTimeZone     = TimeZoneInfo.FindSystemTimeZoneById(localTimeZoneId);
            var invoiceDateFormat = _appSettings.GetValue("InvoiceDateFormat");
            row["OrderDate"]      = order.OrderDateUtc.HasValue ? (object)TimeZoneInfo.ConvertTimeFromUtc(order.OrderDateUtc.Value, localTimeZone).ToString(invoiceDateFormat) : DBNull.Value;
			
			var consultantName = string.Format(_appSettings.GetValue("NameDisplayFormat"), consultant.FirstName, consultant.MiddleName, consultant.LastName);
			row["ConsultantName"] = TrimAndNullify(consultantName);
			MaryKay.IBCDataServices.Entities.Address consultantPrimaryAddress = consultant.Addresses.Where(a => a.IsPrimary.HasValue && a.IsPrimary.Value).FirstOrDefault();
			var consultantPrimaryAddressLine1 = (consultantPrimaryAddress != null) && !string.IsNullOrWhiteSpace(consultantPrimaryAddress.Address1) ? consultantPrimaryAddress.Address1.Trim() : null;
			var consultantPrimaryAddressLine2 = (consultantPrimaryAddress != null) && !string.IsNullOrWhiteSpace(consultantPrimaryAddress.Address2) ? consultantPrimaryAddress.Address2.Trim() : null;
			var consultantPrimaryAddressLine3 = (consultantPrimaryAddress != null) ? AddressFormatter.Format(consultantPrimaryAddress.Address3, consultantPrimaryAddress.Address4, consultantPrimaryAddress.Address5) : null;
			var consultantAddressComponents = new List<string>(3);
			if (consultantPrimaryAddressLine1 != null) consultantAddressComponents.Add(consultantPrimaryAddressLine1);
			if (consultantPrimaryAddressLine2 != null) consultantAddressComponents.Add(consultantPrimaryAddressLine2);
			if (consultantPrimaryAddressLine3 != null) consultantAddressComponents.Add(consultantPrimaryAddressLine3);
			var consultantAddressInline = string.Join(_appSettings.GetValue("Order_InvoiceReceipt_InlineAddressComponentSeparator"), consultantAddressComponents);
			row["ConsultantAddressInline"] = TrimAndNullify(consultantAddressInline);
			var consultantNameAddressComponents = new List<string>(consultantAddressComponents.Count + 1);
			consultantNameAddressComponents.Add(consultantName);
			consultantNameAddressComponents.AddRange(consultantAddressComponents);
			var consultantNameAddressBlock = string.Join("\n", consultantNameAddressComponents);
			row["ConsultantNameAddressBlock"] = TrimAndNullify(consultantNameAddressBlock);

			var customerName = string.Format(_appSettings.GetValue("NameDisplayFormat"), order.FirstName, order.MiddleName, order.LastName);
			row["CustomerName"] = TrimAndNullify(customerName);

            string shippingAddressLine1 = null;            
            if (order.ShippingAddress != null){
                shippingAddressLine1 = string.Format(_appSettings.GetValue("FormattedLocale_StreetUnitNumber"), order.ShippingAddress.Street, order.ShippingAddress.UnitNumber).Trim(); 
            }
            var shippingAddressLine2 = (order.ShippingAddress != null) ? AddressFormatter.Format(order.ShippingAddress.City, order.ShippingAddress.RegionCode, order.ShippingAddress.PostalCode) : null;            
			var customerNameAddressComponents = new List<string>();
			customerNameAddressComponents.Add(customerName);
			if (!string.IsNullOrEmpty(shippingAddressLine1)) customerNameAddressComponents.Add(shippingAddressLine1);
            if (!string.IsNullOrEmpty(shippingAddressLine2)) customerNameAddressComponents.Add(shippingAddressLine2);
            if (!string.IsNullOrEmpty(order.ShippingAddress.CountryCode)) customerNameAddressComponents.Add(order.ShippingAddress.CountryCode);
			var customerNameAddressBlock = string.Join("\n", customerNameAddressComponents);
			row["CustomerNameAddressBlock"] = TrimAndNullify(customerNameAddressBlock);

            row["CurrencySymbol"] = TrimAndNullify(_appSettings.GetValue("InvoiceCurrencySymbol"));

            row["ConsultantComment"] = TrimAndNullify(model.InvoiceMessage);

            row["ConsultantGiftMessage"] = TrimAndNullify(model.GiftMessage);

            row["SoldByLabel"]         = Resources.GetString("ORDER_INVOICERECEIPT_SOLDBYLABEL");
            row["SoldToLabel"]         = Resources.GetString("ORDER_INVOICERECEIPT_SOLDTOLABEL");
            row["OrderNumberLabel"]    = Resources.GetString("ORDER_INVOICERECEIPT_ORDERNUMBERLABEL");
            row["DateHeader"]          = Resources.GetString("ORDER_INVOICERECEIPT_DATEHEADER");
			row["QtyHeader"]           = Resources.GetString("ORDER_INVOICERECEIPT_QTYHEADER");
            row["ProductNameHeader"]   = Resources.GetString("ORDER_INVOICERECEIPT_PRODUCTNAMEHEADER");
            row["PriceHeader"]         = Resources.GetString("ORDER_INVOICERECEIPT_PRICEHEADER");
            row["AmountHeader"]        = Resources.GetString("ORDER_INVOICERECEIPT_AMOUNTHEADER");
            row["PriceDisclaimer"]     = Resources.GetString("ORDER_INVOICERECEIPT_PRICEDISCLAIMER");
            row["EstimatedValueLabel"] = Resources.GetString("ORDER_INVOICERECEIPT_ESTIMATEDVALUELABEL");
            row["AdjustmentsLabel"]    = Resources.GetString("ORDER_INVOICERECEIPT_ADJUSTMENTSLABEL");
            row["SalesTaxLabel"]       = Resources.GetString("ORDER_INVOICERECEIPT_SALESTAXLABEL");
            row["TotalLabel"]          = Resources.GetString("ORDER_INVOICERECEIPT_TOTALLABEL");
            row["NoteText"]            = Resources.GetString("ORDER_INVOICERECEIPT_NOTETEXT");
            row["SumTotal"]            = orderSumTotal.ToString("N", _numberFormatCulture.NumberFormat);            

			row["SatisfactionGuaranteeHeader"] = Resources.GetString("ORDER_INVOICERECEIPT_SATISFACTIONGUARANTEEHEADER");
            row["Paragraph1"]                  = Resources.GetString("ORDER_INVOICERECEIPT_PARAGRAPH1");
            row["Paragraph2"]                  = Resources.GetString("ORDER_INVOICERECEIPT_PARAGRAPH2");
            row["Paragraph3"]                  = Resources.GetString("ORDER_INVOICERECEIPT_PARAGRAPH3");
            row["MaryKayContactText"]          = Resources.GetString("ORDER_INVOICERECEIPT_MARYKAYCONTACTTEXT");
            row["NoticeOfCancellationText"]    = Resources.GetString("ORDER_INVOICERECEIPT_NOTICEOFCANCELLATIONTEXT");
            row["DateLabel"]                   = Resources.GetString("ORDER_INVOICERECEIPT_DATELABEL");
            row["Paragraph4"]                  = Resources.GetString("ORDER_INVOICERECEIPT_PARAGRAPH4");
            row["Paragraph5"]                  = Resources.GetString("ORDER_INVOICERECEIPT_PARAGRAPH5");
            row["Paragraph6"]                  = Resources.GetString("ORDER_INVOICERECEIPT_PARAGRAPH6");
            row["Paragraph7"]                  = Resources.GetString("ORDER_INVOICERECEIPT_PARAGRAPH7");
            row["Paragraph8"]                  = Resources.GetString("ORDER_INVOICERECEIPT_PARAGRAPH8");
            row["CancelTransactionText"]       = Resources.GetString("ORDER_INVOICERECEIPT_CANCELTRANSACTIONTEXT");
            row["NameOfSellerLabel"]           = Resources.GetString("ORDER_INVOICERECEIPT_NAMEOFSELLERLABEL");
            row["AddressOfSeller"]             = Resources.GetString("ORDER_INVOICERECEIPT_ADDRESSOFSELLER");
            row["BuyersSignatureLabel"]        = Resources.GetString("ORDER_INVOICERECEIPT_BUYERSSIGNATURELABEL");
            row["TradeMarkText"]               = Resources.GetString("ORDER_INVOICERECEIPT_TRADEMARKTEXT");

			tblOrder.Rows.Add(row);

			return tblOrder;
		}

        WorkbookDesigner GetWorkbookDesigner(string fileName, LoadFormat format)
        {
            using (var designerFileStream = GetLocalizedTemplate(fileName))
                if (designerFileStream != null)
                    return new WorkbookDesigner
                    {
                        Workbook = new Workbook(designerFileStream, new LoadOptions(format))
                    };

            return null;
        }

        static void SetWorksheetFooter(Workbook workbook, int worksheetIndex, string leftScript, string centerScript, string rightScript)
        {
            SetWorksheetFooterSection(workbook, worksheetIndex, 0, leftScript);
            SetWorksheetFooterSection(workbook, worksheetIndex, 1, centerScript);
            SetWorksheetFooterSection(workbook, worksheetIndex, 2, rightScript);
        }

        static void SetWorksheetFooterSection(Workbook workbook, int worksheetIndex, int sectionIndex, string sectionScript)
        {
            workbook.Worksheets[worksheetIndex].PageSetup.SetFooter(sectionIndex, sectionScript);
        }

        static void SetWorksheetHeader(Workbook workbook, int worksheetIndex, string leftScript, string centerScript, string rightScript)
        {
            SetWorksheetHeaderSection(workbook, worksheetIndex, 0, leftScript);
            SetWorksheetHeaderSection(workbook, worksheetIndex, 1, centerScript);
            SetWorksheetHeaderSection(workbook, worksheetIndex, 2, rightScript);
        }

        static void SetWorksheetHeaderSection(Workbook workbook, int worksheetIndex, int sectionIndex, string sectionScript)
        {
            workbook.Worksheets[worksheetIndex].PageSetup.SetHeader(sectionIndex, sectionScript);
        }

        static object TrimAndNullify(string input)
        {
            var output = string.IsNullOrWhiteSpace(input) ? (object)DBNull.Value : input.Trim();
            return output;
        }
    }
}