using System.Data;
using System.Xml.Linq;

namespace myCustomers.Data
{
	public class TextGenerator : BaseGenerator<string>
	{
		public TextGenerator(XElement xeConfiguration, DataColumnCollection columns)
		{
			svgText      = new StringValueGenerator(columns, xeConfiguration, "Text") as StringValueGenerator;
			IsFunctional = svgText.IsFunctional;
		}

		public override string GeneratedInstance(DataRow row)
		{
			return svgText.Value(row);
		}
		
		protected readonly StringValueGenerator svgText;
	
	}
}
