using System.Data;
using System.Xml.Linq;
using Quartet.Entities;

namespace myCustomers.Data
{
    public class EmailAddressGenerator : BaseGenerator<EmailAddress>
    {
        public EmailAddressGenerator(XElement xeConfiguration, DataColumnCollection columns)
        {
            svgAddress   = new StringValueGenerator(columns, xeConfiguration, "Address");
            IsFunctional = svgAddress.IsFunctional;
        }

        public override EmailAddress GeneratedInstance(DataRow row)
        {
            var address = svgAddress.Value(row);
            if (!string.IsNullOrWhiteSpace(address))
                return new EmailAddress
                {
                    EmailAddressStatus = EmailAddressStatus.NewlyAdded,
                    Address            = address
                };

            return null;
        }

        protected readonly StringValueGenerator svgAddress;
    }
}
