using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using Aspose.Cells;
using MaryKay.Configuration;
using MaryKay.IBCDataServices.Entities;
using myCustomers.Contexts;
using myCustomers.Globalization;
using myCustomers.Pdf;
using myCustomers.Web.Models;
using Quartet.Client.Customers;
using Quartet.Entities;
using Quartet.Entities.Views;
using myCustomers.Data;

namespace myCustomers.Web.Services
{
    public class CustomerExportService : ExportServiceBase, ICustomerExportService
    {
        #region DataSet Formulas

        const string CUSTOMER_HOMEPHONETEXT_COLEXPR             = @"IIF(HomePhoneNumber is null, null, IIF(HomePhoneExtension is null, HomePhoneNumber, HomePhoneNumber + ' x' +HomePhoneExtension ))";
        const string CUSTOMER_ISPCPTEXT_COLEXPR                 = @"IIF(IsPcp, 'Yes', 'No')";
        const string CUSTOMER_MOBILEPHONETEXT_COLEXPR           = @"IIF(MobilePhoneNumber is null, null, IIF(MobilePhoneExtension is null, MobilePhoneNumber, MobilePhoneNumber + ' x' +MobilePhoneExtension ))";
        const string CUSTOMER_NAMETEXT_COLEXPR                  = @"IIF(FirstName is null, LastName, IIF(LastName is null, FirstName, FirstName + ' ' + LastName ))";
        const string CUSTOMER_PHONENUMBERSTEXT_COLEXPR          = @"IIF(HomePhoneText is null,
                                                                     IIF(MobilePhoneText is null,
                                                                      IIF(WorkPhoneText is null,
                                                                       null,
                                                                       'W: ' + WorkPhoneText),
                                                                      IIF(WorkPhoneText is null,
                                                                       'C: ' + MobilePhoneText,
                                                                       'C: ' + MobilePhoneText + NewLine + 'W: ' + WorkPhoneText)),
                                                                     IIF(MobilePhoneText is null,
                                                                      IIF(WorkPhoneText is null,
                                                                       'H: ' + HomePhoneText,
                                                                       'H: ' + HomePhoneText + NewLine + 'W: ' + WorkPhoneText),
                                                                      IIF(WorkPhoneText is null,
                                                                       'H: ' + HomePhoneText + NewLine + 'C: ' + MobilePhoneText,
                                                                       'H: ' + HomePhoneText + NewLine + 'C: ' + MobilePhoneText + NewLine + 'W: ' + WorkPhoneText)))";
        const string CUSTOMER_PREFERREDLANGUAGETEXT_COLEXPR     = @"IIF(PreferredLanguage is null, null, IIF(PreferredLanguage = 'en-US', 'English', IIF(PreferredLanguage = 'es-US', 'Spanish', PreferredLanguage )))";
        const string CUSTOMER_WORKPHONETEXT_COLEXPR             = @"IIF(WorkPhoneNumber is null, null, IIF(WorkPhoneExtension is null, WorkPhoneNumber, WorkPhoneNumber + ' x' +WorkPhoneExtension ))";

        #endregion

        IAppSettings          _appSettings;
        IQuartetClientFactory _clientFactory;
        IConfigService        _configService;
        IConsultantContext    _consultantContext;

        public CustomerExportService 
        (
            IConsultantContext consultantContext, 
            IQuartetClientFactory clientFactory, 
            IAppSettings appSettings, 
            IConfigService configService,
            IEnvironmentConfig environmentConfig,
            ISubsidiaryAccessor subsidiaryAccessor
        ) : base(appSettings, environmentConfig, subsidiaryAccessor)
        {
            _clientFactory     = clientFactory;
            _consultantContext = consultantContext;
            _appSettings       = appSettings;
            _configService     = configService;
        }

        public byte[] CreateLabels(CustomerSelectionModel selection)
        {
            var customerTable = GetCustomerTable(selection);
            var config = _configService.GetConfigXml("LabelsFormat");

            var nameFormat = _appSettings.GetValue("MailingLabelNameFormat");
            var streetUnitNumberFormat = _appSettings.GetValue("FormattedLocale_StreetUnitNumber");            
            var ds = MailingLabelFormatter.Format(customerTable, nameFormat, streetUnitNumberFormat);

            var fontSize = _appSettings.GetValue<int>("PrintLabelFontSize");
            PdfLabelGenerator generator;
            if (fontSize > 0)
                generator = new PdfLabelGenerator(config, ds, HttpContext.Current.Server.MapPath("~/content/fonts/arialuni.ttf"), fontSize);
            else
                generator = new PdfLabelGenerator(config, ds, HttpContext.Current.Server.MapPath("~/content/fonts/arialuni.ttf"));

            var stream = new MemoryStream();
            generator.SavePdf(stream);

            return stream.ToArray();
        }

        public byte[] Export(CustomerSelectionModel selection)
        {
            var consultant = _consultantContext.Consultant;
            using (var designerFileStream = GetLocalizedTemplate("Customer_ExportList_Designer.xls"))
            {
                if (designerFileStream != null)
                {
                    var consultantName = string.Format(_appSettings.GetValue("ConsultantNameFormat"), consultant.FirstName, consultant.LastName);
                    var now = DateTime.Now;

                    var data = new DataSet("ExportList");
                    var stringsTable = GetStringsTable();
                    data.Tables.Add(stringsTable);

                    var customerTable = GetCustomerTable(selection);
                    data.Tables.Add(customerTable);

                    var designer = new WorkbookDesigner();
                    designer.Workbook = new Workbook(designerFileStream, new Aspose.Cells.LoadOptions(LoadFormat.Excel97To2003));
                    SetWorkbookBuiltInProperties(designer.Workbook, consultantName);
                    SetWorksheetHeader(designer.Workbook, 0, consultantName, Resources.GetString("EXPORTLIST_DOCUMENTTITLE"), now.ToLongDateString());
                    SetWorksheetFooter(designer.Workbook, 0, Resources.GetString("EXPORTLIST_FOOTERLEFTSCRIPT").Trim(), Resources.GetString("EXPORTLIST_FOOTERCENTERSCRIPT").Trim(), Resources.GetString("EXPORTLIST_FOOTERRIGHTSCRIPT").Trim());
                    designer.SetDataSource(data);
                    designer.Process();

                    var stream = new MemoryStream();
                    designer.Workbook.Save(stream, new XlsSaveOptions(SaveFormat.Excel97To2003));

                    // position the stream at the beginnin so it will be ready for writing to the output
                    stream.Seek(0, SeekOrigin.Begin);

                    return stream.ToArray();
                }
            }

            return null;
        }

        public byte[] Print(CustomerSelectionModel selection)
        {
            var consultant = _consultantContext.Consultant;
            using (var designerFileStream = GetLocalizedTemplate("Customer_PrintList_Designer.xls"))
            {
                if (designerFileStream != null)
                {
                    var consultantName = string.Format(_appSettings.GetValue("ConsultantNameFormat"), consultant.FirstName, consultant.LastName);
                    var now = DateTime.Now;

                    var data = new DataSet("PrintList");
                    var stringsTable = GetStringsTable();
                    data.Tables.Add(stringsTable);

                    var customerTable = GetCustomerTable(selection);
                    data.Tables.Add(customerTable);

                    var designer = new WorkbookDesigner();
                    designer.Workbook = new Workbook(designerFileStream, new Aspose.Cells.LoadOptions(LoadFormat.Excel97To2003));
                    SetWorkbookBuiltInProperties(designer.Workbook, consultantName);
                    SetWorksheetHeader(designer.Workbook, 0, consultantName, Resources.GetString("PRINTLIST_DOCUMENTTITLE"), now.ToLongDateString());
                    SetWorksheetFooter(designer.Workbook, 0, Resources.GetString("PRINTLIST_FOOTERLEFTSCRIPT").Trim(), Resources.GetString("PRINTLIST_FOOTERCENTERSCRIPT").Trim(), Resources.GetString("PRINTLIST_FOOTERRIGHTSCRIPT").Trim());
                    designer.SetDataSource(data);
                    designer.Process();

                    var stream = new MemoryStream();
                    designer.Workbook.Save(stream, new XlsSaveOptions(SaveFormat.Pdf));

                    // position the stream at the beginnin so it will be ready for writing to the output
                    stream.Seek(0, SeekOrigin.Begin);

                    return stream.ToArray();
                }
            }

            return null;
        }

        DataTable GetStringsTable()
        {
            var table = new DataTable("Strings");
            table.Columns.Add("NameHeader", typeof(string));
            table.Columns.Add("PhoneNumberHeader", typeof(string));
            table.Columns.Add("AddressHeader", typeof(string));
            table.Columns.Add("EmailHeader", typeof(string));

            table.Columns.Add("FirstNameHeader", typeof(string));
            table.Columns.Add("LastNameHeader", typeof(string));
            table.Columns.Add("BirthMonthHeader", typeof(string));
            table.Columns.Add("BirthDayHeader", typeof(string));
            table.Columns.Add("StreetAddressHeader", typeof(string));
            table.Columns.Add("SuiteHeader", typeof(string));
            table.Columns.Add("CityHeader", typeof(string));
            table.Columns.Add("StateHeader", typeof(string));
            table.Columns.Add("ZipCodeHeader", typeof(string));
            table.Columns.Add("CellPhoneHeader", typeof(string));
            table.Columns.Add("HomePhoneHeader", typeof(string));
            table.Columns.Add("WorkPhoneHeader", typeof(string));

            var row = table.NewRow();

            row["NameHeader"]          = Resources.GetString("CUSTOMER_NAMEHEADER");
            row["PhoneNumberHeader"]   = Resources.GetString("CUSTOMER_PHONENUMBERHEADER");
            row["AddressHeader"]       = Resources.GetString("CUSTOMER_ADDRESSHEADER");
            row["EmailHeader"]         = Resources.GetString("CUSTOMER_EMAILHEADER");
            row["FirstNameHeader"]     = Resources.GetString("CUSTOMER_FIRSTNAMEHEADER");
            row["LastNameHeader"]      = Resources.GetString("CUSTOMER_LASTNAMEHEADER");
            row["BirthMonthHeader"]    = Resources.GetString("CUSTOMER_BIRTHMONTHHEADER");
            row["BirthDayHeader"]      = Resources.GetString("CUSTOMER_BIRTHDAYHEADER");
            row["StreetAddressHeader"] = Resources.GetString("CUSTOMER_STREETADDRESSHEADER");
            row["SuiteHeader"]         = Resources.GetString("CUSTOMER_SUITEHEADER");
            row["CityHeader"]          = Resources.GetString("CUSTOMER_CITYHEADER");
            row["StateHeader"]         = Resources.GetString("CUSTOMER_STATEHEADER");
            row["ZipCodeHeader"]       = Resources.GetString("CUSTOMER_ZIPCODEHEADER");
            row["CellPhoneHeader"]     = Resources.GetString("CUSTOMER_CELLPHONEHEADER");
            row["HomePhoneHeader"]     = Resources.GetString("CUSTOMER_HOMEPHONEHEADER");
            row["WorkPhoneHeader"]     = Resources.GetString("CUSTOMER_WORKPHONEHEADER");

            table.Rows.Add(row);

            return table;
        }

        List<CustomerListItem> GetCustomers(CustomerSelectionModel selection)
        {
            var queryService = _clientFactory.GetCustomersQueryServiceClient();

            // fetch the selected customers in batches of 200
            var customers = selection.Ids
                .Chunk(200)
                .Select(batch => queryService.QueryCustomerListByCustomerIds(batch.ToArray()))
                .SelectMany(batch => batch);

            var sortFormat = _appSettings.GetValue("CustomerNameSortFormat");
            customers = customers.OrderBy(c => string.Format(sortFormat, c.FirstName, c.MiddleName, c.LastName));

            return customers.ToList();
        }

        DataTable GetCustomerTable(CustomerSelectionModel selection)
        {
            var consultant = _consultantContext.Consultant;

            var customers = GetCustomers(selection);

            var table = new DataTable("Customer");
            table.Columns.Add("NewLine", typeof(string), "convert(13, 'System.Char') + convert(10, 'System.Char')");
            table.Columns.Add("CustomerId", typeof(Guid));
            table.Columns.Add("ConsultantKey", typeof(Guid));
            table.Columns.Add("FirstName", typeof(string));
            table.Columns.Add("MiddleName", typeof(string));
            table.Columns.Add("LastName", typeof(string));
            table.Columns.Add("NameText", typeof(string), CUSTOMER_NAMETEXT_COLEXPR);
            table.Columns.Add("EmailAddress", typeof(string));
            table.Columns.Add("HomePhoneNumber", typeof(string));
            table.Columns.Add("HomePhoneExtension", typeof(string));
            table.Columns.Add("HomePhoneIsPrimary", typeof(bool));
            table.Columns.Add("HomePhoneText", typeof(string), CUSTOMER_HOMEPHONETEXT_COLEXPR);
            table.Columns.Add("MobilePhoneNumber", typeof(string));
            table.Columns.Add("MobilePhoneExtension", typeof(string));
            table.Columns.Add("MobilePhoneIsPrimary", typeof(bool));
            table.Columns.Add("MobilePhoneText", typeof(string), CUSTOMER_MOBILEPHONETEXT_COLEXPR);
            table.Columns.Add("WorkPhoneNumber", typeof(string));
            table.Columns.Add("WorkPhoneExtension", typeof(string));
            table.Columns.Add("WorkPhoneIsPrimary", typeof(bool));
            table.Columns.Add("WorkPhoneText", typeof(string), CUSTOMER_WORKPHONETEXT_COLEXPR);
            table.Columns.Add("PhoneNumbersText", typeof(string), CUSTOMER_PHONENUMBERSTEXT_COLEXPR);
            table.Columns.Add("PrimaryAddressAddressee", typeof(string));
            table.Columns.Add("PrimaryAddressStreet", typeof(string));
            table.Columns.Add("PrimaryAddressUnitNumber", typeof(string));
            table.Columns.Add("PrimaryAddressCity", typeof(string));
            table.Columns.Add("PrimaryAddressRegionCode", typeof(string));
            table.Columns.Add("PrimaryAddressRegionName", typeof(string));
            table.Columns.Add("PrimaryAddressCountry", typeof(string));
            table.Columns.Add("PrimaryAddressPostalCode", typeof(string));            
            table.Columns.Add("PrimaryAddressText", typeof(string));
            table.Columns.Add("PreferredLanguage", typeof(string));
            table.Columns.Add("PreferredLanguageText", typeof(string), CUSTOMER_PREFERREDLANGUAGETEXT_COLEXPR);
            table.Columns.Add("IsPcp", typeof(bool));
            table.Columns.Add("IsPcpText", typeof(string), CUSTOMER_ISPCPTEXT_COLEXPR);
            table.Columns.Add("BirthMonth", typeof(Int16));
            table.Columns.Add("BirthDay", typeof(Int16));
            table.Columns.Add("LastOrderDateUtc", typeof(DateTime));
            table.Columns.Add("DateAddedUtc", typeof(DateTime));
            //table.Columns.Add("LastViewedDateUtc", typeof(DateTime));

            foreach (var customer in customers)
            {
                var row = table.NewRow();

                row["CustomerId"]    = customer.CustomerId;
                row["ConsultantKey"] = customer.ConsultantKey;
                row["FirstName"]     = TrimAndNullify(customer.FirstName);
                row["MiddleName"]    = TrimAndNullify(customer.MiddleName);
                row["LastName"]      = TrimAndNullify(customer.LastName);
                row["EmailAddress"]  = TrimAndNullify(customer.EmailAddress);

                var homePhone = customer.PhoneNumbers.FirstOrDefault(p => p.PhoneNumberType == PhoneNumberType.Home);
                if (homePhone != null)
                {
                    row["HomePhoneNumber"] = PhoneFormatter.FormatLocal(homePhone.Number, consultant.SubsidiaryCode); 
                    row["HomePhoneExtension"] = TrimAndNullify(homePhone.Extension);
                    row["HomePhoneIsPrimary"] = homePhone.IsPrimary;
                }

                var mobilePhone = customer.PhoneNumbers.FirstOrDefault(p => p.PhoneNumberType == PhoneNumberType.Mobile);
                if (mobilePhone != null)
                {
                    row["MobilePhoneNumber"] = PhoneFormatter.FormatLocal(mobilePhone.Number, consultant.SubsidiaryCode); 
                    row["MobilePhoneExtension"] = TrimAndNullify(mobilePhone.Extension);
                    row["MobilePhoneIsPrimary"] = mobilePhone.IsPrimary;
                }

                var workPhone = customer.PhoneNumbers.FirstOrDefault(p => p.PhoneNumberType == PhoneNumberType.Work);
                if (workPhone != null)
                {
                    row["WorkPhoneNumber"] = PhoneFormatter.FormatLocal(workPhone.Number, consultant.SubsidiaryCode); 
                    row["WorkPhoneExtension"] = TrimAndNullify(workPhone.Extension);
                    row["WorkPhoneIsPrimary"] = workPhone.IsPrimary;
                }

                row["PrimaryAddressAddressee"]  = TrimAndNullify(customer.Addressee);
                row["PrimaryAddressStreet"]     = TrimAndNullify(customer.Street);
                row["PrimaryAddressUnitNumber"] = TrimAndNullify(customer.UnitNumber);
                row["PrimaryAddressCity"]       = TrimAndNullify(customer.City);
                row["PrimaryAddressRegionCode"] = TrimAndNullify(customer.RegionCode);
                row["PrimaryAddressPostalCode"] = TrimAndNullify(customer.PostalCode);
                row["PrimaryAddressCountry"]    = TrimAndNullify(customer.CountryCode);
                
                var streetUnitNumber = string.Format(_appSettings.GetValue("FormattedLocale_StreetUnitNumber"), customer.Street, customer.UnitNumber).Trim();
                var address = AddressFormatter.Format(customer.City, customer.RegionCode, customer.PostalCode);
                var customerAddressComponents = new List<string>();
                if (!string.IsNullOrEmpty(streetUnitNumber)) customerAddressComponents.Add(streetUnitNumber);
                if (!string.IsNullOrEmpty(address)) customerAddressComponents.Add(address);
                if (!string.IsNullOrEmpty(customer.CountryCode)) customerAddressComponents.Add(customer.CountryCode);
                var customerAddressBlock = string.Join("\n", customerAddressComponents);
                row["PrimaryAddressText"] = TrimAndNullify(customerAddressBlock);

                row["PreferredLanguage"]   = TrimAndNullify(customer.PreferredLanguage);
                row["IsPcp"]               = customer.IsPcp;
                row["BirthMonth"]          = customer.BirthMonth.HasValue ? (object)customer.BirthMonth.Value : DBNull.Value;
                row["BirthDay"]            = customer.BirthDay.HasValue ? (object)customer.BirthDay.Value : DBNull.Value;
                row["LastOrderDateUtc"]    = customer.LastOrderDateUtc.HasValue ? (object)customer.LastOrderDateUtc.Value : DBNull.Value;
                row["DateAddedUtc"]        = customer.DateAddedUtc;
                //row["LastViewedDateUtc"] = customer.LastViewedDateUtc.HasValue ? (object)customer.LastViewedDateUtc.Value : DBNull.Value;

                table.Rows.Add(row);
            }

            return table;
        }

        static void SetWorkbookBuiltInProperties(Workbook workbook, string consultantName)
        {
            workbook.BuiltInDocumentProperties.Author      = consultantName;
            workbook.BuiltInDocumentProperties.Company     = string.Empty;
            workbook.BuiltInDocumentProperties.LastSavedBy = consultantName;
        }

        static void SetWorksheetFooter(Workbook workbook, int worksheetIndex, string leftScript, string centerScript, string rightScript)
        {
            workbook.Worksheets[worksheetIndex].PageSetup.SetFooter(0, leftScript);
            workbook.Worksheets[worksheetIndex].PageSetup.SetFooter(1, centerScript);
            workbook.Worksheets[worksheetIndex].PageSetup.SetFooter(2, rightScript);
        }

        static void SetWorksheetHeader(Workbook workbook, int worksheetIndex, string leftScript, string centerScript, string rightScript)
        {
            workbook.Worksheets[worksheetIndex].PageSetup.SetHeader(0, leftScript);
            workbook.Worksheets[worksheetIndex].PageSetup.SetHeader(1, centerScript);
            workbook.Worksheets[worksheetIndex].PageSetup.SetHeader(2, rightScript);
        }

        static object TrimAndNullify(string input)
        {
            var output = string.IsNullOrWhiteSpace(input) ? (object)DBNull.Value : input.Trim();
            return output;
        }
    }
}