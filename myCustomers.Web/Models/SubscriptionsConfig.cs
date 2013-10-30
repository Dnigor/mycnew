using System.Xml.Serialization;

namespace myCustomers.Web.Models
{
    [XmlRoot("Subscriptions")]
    public class SubscriptionsConfig
    {
        [XmlElement("Subscription")]
        public Subscription[] Subscriptions { get; set; }

        public class Subscription
        {
            [XmlAttribute]
            public string SubscriptionType { get; set; }

            [XmlAttribute]
            public string Name { get; set; }
        }
    }
}