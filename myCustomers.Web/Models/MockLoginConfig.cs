using System.Xml.Serialization;

namespace myCustomers.Web.Models
{
    [XmlRoot("MockLogin")]
    public class MockLoginConfig
    {
        [XmlElement("Subsidiary")]
        public Subsidiary[] Subsidiaries { get; set; }

        public class Subsidiary
        {
            [XmlAttribute]
            public string Region { get; set; }

            [XmlAttribute]
            public string SubsidiaryCode { get; set; }

            [XmlAttribute]
            public string Culture { get; set; }

            [XmlAttribute]
            public string DefaultUICulture { get; set; }

            [XmlAttribute]
            public string UICultures { get; set; }

            [XmlAttribute]
            public string TestAccounts { get; set; }
        }
    }
}