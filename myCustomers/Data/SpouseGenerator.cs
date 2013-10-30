using System.Data;
using System.Xml.Linq;
using QuartetEntities = Quartet.Entities;

namespace myCustomers.Data
{
	public class SpouseGenerator : BaseGenerator<QuartetEntities.Spouse>
	{
		public SpouseGenerator(XElement xeConfiguration, DataColumnCollection columns)
		{
			svgName         = new StringValueGenerator(columns, xeConfiguration, "Name");
			svgEmailAddress = new StringValueGenerator(columns, xeConfiguration, "EmailAddress");
			svgPhoneNumber  = new StringValueGenerator(columns, xeConfiguration, "PhoneNumber");
			svgExtension    = new StringValueGenerator(columns, xeConfiguration, "Extension");
			IsFunctional    = svgName.IsFunctional;
		}

        // REVIEW: The logic of this method seems a little suspect
		public override QuartetEntities.Spouse GeneratedInstance(DataRow row)
		{
			QuartetEntities.Spouse instance = null;

			string spouseName = svgName.Value(row);
			if (!string.IsNullOrWhiteSpace(spouseName))
			{
				instance = new QuartetEntities.Spouse();
				instance.SpouseName = spouseName;
				instance.EmailAddress = svgEmailAddress.Value(row);
				string phoneNumber = svgPhoneNumber.Value(row);
				if (!string.IsNullOrWhiteSpace(phoneNumber))
				{
					instance.PhoneNumber = phoneNumber;
					instance.Extension = svgExtension.Value(row);
				}
			}

			return instance;
		}

		protected readonly StringValueGenerator svgName;
		protected readonly StringValueGenerator svgEmailAddress;
		protected readonly StringValueGenerator svgPhoneNumber;
		protected readonly StringValueGenerator svgExtension;
	}
}
