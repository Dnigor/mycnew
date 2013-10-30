using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using myCustomers.Data;

namespace myCustomers.Pdf
{
	public class MailingLabelFormatter
	{
		private const string FirstName = "FirstName";
		private const string LastName = "LastName";
		private const string MiddleName = "MiddleName";
		private const string AddressStreet = "PrimaryAddressStreet";
		private const string AddressUnitNumber = "PrimaryAddressUnitNumber";
		private const string AddressCity = "PrimaryAddressCity";
		private const string AddressState = "PrimaryAddressRegionCode";
		private const string AddressZipCode = "PrimaryAddressPostalCode";
        private const string AddressCountry = "PrimaryAddressCountry";

		public static MailingLabelDS Format(DataTable customerTable, string nameFormat, string streetUnitNumberFormat)
		{
			var ds = new MailingLabelDS();
			var dsHelper = new MailingLableDSHelper(ds);
			
			//Create the list of selected users and build the labels.
			//DateTime now = DateTime.Now;

			var data = new DataSet("PrintList");
			data.Tables.Add(customerTable);

			foreach (DataRowView row in data.Tables[0].DefaultView)
			{
				var name = string.Format(nameFormat, row[FirstName], row[LastName]);
                var streetUnitNumber = string.Format(streetUnitNumberFormat, row[AddressStreet], row[AddressUnitNumber]).Trim();				
				var city = GetValue(AddressCity, row);
				var state = GetValue(AddressState, row);
				var zip = GetValue(AddressZipCode, row);
                var country = GetValue(AddressCountry, row);

			    var address = AddressFormatter.Format(city, state, zip);
               
				//Add the address to the dataset
                dsHelper.AddAddress(name, streetUnitNumber, address, country);

			}

			return ds;
		}

		public static string GetValue(string columnName, DataRowView row)
		{
			return row[columnName] == null ? string.Empty : row[columnName].ToString();
		}
	}
}
