using System;
using Newtonsoft.Json;

namespace myCustomers
{
    public static class ExtendDateTime
    {
        const string ISODATEFORMAT = "yyyy-MM-ddTHH:mm:ss.fffK";

        public static string ToStrictIsoDate(this DateTime date)
        {
            switch (date.Kind)
            {
                case DateTimeKind.Unspecified:
                    date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
                    break;
                case DateTimeKind.Local:
                    date = date.ToUniversalTime();
                    break;
            }

            return date.ToString(ISODATEFORMAT);
        }

        public static string ToStrictIsoDate(this DateTime? date)
        {
            if (!date.HasValue) return null;
            
            switch (date.Value.Kind)
            {
                case DateTimeKind.Unspecified:
                    date = DateTime.SpecifyKind(date.Value, DateTimeKind.Utc);
                    break;
                case DateTimeKind.Local:
                    date = date.Value.ToUniversalTime();
                    break;
            }

            return date.Value.ToString(ISODATEFORMAT);
        }
    }
}
