using System.Xml.Serialization;

namespace myCustomers.Web.Models
{
    [XmlRoot("TopicsOfInterest")]
    public class TopicOfInterestsConfig
    {
        [XmlElement("TopicOfInterest")]
        public TopicOfInterest[] Topics { get; set; }

        public class TopicOfInterest
        {
            [XmlAttribute]
            public string TopicDescriptionResourceKey { get; set; }
            [XmlAttribute]
            public string Value { get; set; }
            [XmlAttribute]
            public string Description { get; set; }
        }
    }
}