using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace myCustomers
{
    /// <summary>
    /// Override to always serialize UTC date/time and use ISO 8601 strict format
    /// </summary>
    public class StrictIsoDateTimeConverter : IsoDateTimeConverter
    {
        public StrictIsoDateTimeConverter()
        {
            DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffK";
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var date = (DateTime)value;

            switch (date.Kind)
            {
                case DateTimeKind.Unspecified:
                    date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
                    break;
                case DateTimeKind.Local:
                    date = date.ToUniversalTime();
                    break;
            }

            base.WriteJson(writer, date, serializer);
        }
    }
}
