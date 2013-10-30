using System.Data;
using System.Xml.Linq;
using QuartetEntities = Quartet.Entities;

namespace myCustomers.Data
{
	public class ContactInformationGenerator : BaseGenerator<QuartetEntities.ContactInformation>
	{
		public ContactInformationGenerator(XElement xeConfiguration, DataColumnCollection columns)
		{
			svgFirstName  = new StringValueGenerator(columns, xeConfiguration, "FirstName");
			svgMiddleName = new StringValueGenerator(columns, xeConfiguration, "MiddleName");
			svgLastName   = new StringValueGenerator(columns, xeConfiguration, "LastName");
			svgEmployer   = new StringValueGenerator(columns, xeConfiguration, "Employer");
			svgOccupation = new StringValueGenerator(columns, xeConfiguration, "Occupation");
			IsFunctional  = svgFirstName.IsFunctional && svgLastName.IsFunctional;
		}

		public override QuartetEntities.ContactInformation GeneratedInstance(DataRow row)
		{
			QuartetEntities.ContactInformation instance = null;

			var firstName = svgFirstName.Value(row);
			var lastName  = svgLastName.Value(row);

			if (!string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName))
			{
				instance            = new QuartetEntities.ContactInformation();
				instance.FirstName  = firstName;
				instance.MiddleName = svgMiddleName.Value(row);
				instance.LastName   = lastName;
				instance.Employer   = svgEmployer.Value(row);
				instance.Occupation = svgOccupation.Value(row);
			}

			return instance;
		}
		
		protected readonly StringValueGenerator svgEmployer;
		protected readonly StringValueGenerator svgFirstName;
		protected readonly StringValueGenerator svgLastName;
		protected readonly StringValueGenerator svgMiddleName;
		protected readonly StringValueGenerator svgOccupation;
	}
}
