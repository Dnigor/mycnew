using System;
using System.Data;
using System.Xml.Linq;
using QuartetEntities = Quartet.Entities;

namespace myCustomers.Data
{
    public class PhoneNumberGenerator : BaseGenerator<QuartetEntities.PhoneNumber>
    {
        public PhoneNumberGenerator(XElement xeConfiguration, DataColumnCollection columns)
        {
            PhoneNumberType = (QuartetEntities.PhoneNumberType)Enum.Parse(typeof(QuartetEntities.PhoneNumberType), xeConfiguration.AttributeValue("PhoneNumberType", true));
            svgNumber       = new StringValueGenerator(columns, xeConfiguration, "Number");
            svgExtension    = new StringValueGenerator(columns, xeConfiguration, "Extension");
            IsFunctional    = svgNumber.IsFunctional;
        }

        public QuartetEntities.PhoneNumberType PhoneNumberType { get; protected set; }

        public override QuartetEntities.PhoneNumber GeneratedInstance(DataRow row)
        {
            var number = svgNumber.Value(row);
            if (!string.IsNullOrWhiteSpace(number))
                return new QuartetEntities.PhoneNumber
                {
                    PhoneNumberKey  = Guid.NewGuid(),
                    PhoneNumberType = PhoneNumberType,
                    IsPrimary       = false,
                    Number          = number,
                    Extension       = svgExtension.Value(row)
                };

            return null;
        }

        protected readonly StringValueGenerator svgExtension;
        protected readonly StringValueGenerator svgNumber;
    }
}
