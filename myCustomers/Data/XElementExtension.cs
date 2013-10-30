using System.Collections.Generic;
using System.Xml.Linq;

namespace myCustomers.Data
{
	public static class XElementExtension
	{
		public static string AttributeValue(this XElement xeInstance, string attributeName, bool isMandatory = false)
		{
			var xaInstance = xeInstance.Attribute(attributeName);
			
            if (xaInstance != null)
				return xaInstance.Value;

			if (isMandatory) 
                throw new KeyNotFoundException(string.Format("Mandatory attribute '{0}' missing from element '{1}'", attributeName, xeInstance.Name.LocalName));

			return null;
		}
	}
}
