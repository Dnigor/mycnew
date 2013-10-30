using System;
using System.Data;
using System.Xml.Linq;
using QuartetEntities = Quartet.Entities;

namespace myCustomers.Data
{
    public class SpecialOccasionGenerator : BaseGenerator<QuartetEntities.SpecialOccasion>
    {
        public SpecialOccasionGenerator(XElement xeConfiguration, DataColumnCollection columns)
        {
            SpecialOccasionType = (QuartetEntities.SpecialOccasionType)Enum.Parse(typeof(QuartetEntities.SpecialOccasionType), xeConfiguration.AttributeValue("SpecialOccasionType", true));
            dtvgDate            = new DateTimeValueGenerator(columns, xeConfiguration, "Date");
            IsFunctional        = dtvgDate.IsFunctional;
        }

        public QuartetEntities.SpecialOccasionType SpecialOccasionType { get; protected set; }

        public override QuartetEntities.SpecialOccasion GeneratedInstance(DataRow row)
        {
            DateTime? date = dtvgDate.Value(row);
            if ((date.HasValue) && (date.Value > DateTime.MinValue) && (date.Value < DateTime.MaxValue))
                return new QuartetEntities.SpecialOccasion
                {
                    SpecialOccasionKey  = Guid.NewGuid(),
                    SpecialOccasionType = SpecialOccasionType,
                    Day                 = Convert.ToByte(date.Value.Day),
                    Month               = Convert.ToByte(date.Value.Month),
                    Year                = Convert.ToInt16(date.Value.Year)
                };

            return null;
        }

        protected readonly DateTimeValueGenerator dtvgDate;
    }
}
