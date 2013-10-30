using System.Linq;
using Newtonsoft.Json;
using Quartet.Entities;

namespace myCustomers
{
    public static class ExtendCustomer
    {
        static StrictIsoDateTimeConverter _dateConverter = new StrictIsoDateTimeConverter();
       
        public static string ToJson(this CustomerNote[] notes)
        {
            var projection = notes.Select(n => new { content = n.Content, customerNoteKey = n.CustomerNoteKey, dateCreatedUtc = n.DateCreatedUtc })
                .OrderByDescending(a => a.dateCreatedUtc);

            var json = JsonConvert.SerializeObject(projection, Formatting.None, _dateConverter);

            return json.ToString();
        }

        public static string ToJson(this Subscription[] subscriptions)
        {
            var projection = subscriptions.Select(s => new { subscriptionType = s.SubscriptionType.ToString(), subscriptionStatus = s.SubscriptionStatus.ToString() })
                .OrderBy(s => s.subscriptionType);

            var json = JsonConvert.SerializeObject(projection, Formatting.None, _dateConverter);

            return json.ToString();
        }
    }
}
