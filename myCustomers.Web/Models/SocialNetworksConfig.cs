using System.Xml.Serialization;

namespace myCustomers.Web.Models
{
    [XmlRoot("SocialNetworks")]
    public class SocialNetworksConfig
    {
        [XmlElement("SocialNetwork")]
        public SocialNetwork[] SocialNetworks { get; set; }

        public class SocialNetwork
        {
            [XmlAttribute]
            public string Type { get; set; }

            [XmlAttribute]
            public string Url { get; set; }

            [XmlAttribute]
            public string Icon { get; set; }

            [XmlAttribute]
            public string Tooltip { get; set; }

            [XmlAttribute]
            public string Placeholder { get; set; }

            [XmlAttribute]
            public string Validation { get; set; }
        }
    }
}