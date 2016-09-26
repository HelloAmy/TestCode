using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace YZ.Utility
{
    [Serializable]
    [XmlRoot("EmailTemplate")]
    public class EmailTemplate
    {
        [XmlElement("From")]
        public string From { get; set; }

        [XmlElement("To")]
        public string To { get; set; }

        [XmlElement("Cc")]
        public string Cc { get; set; }

        [XmlElement("IsBodyHtml")]
        public bool IsBodyHtml { get; set; }

        [XmlElement("Subject")]
        public string Subject { get; set; }

        [XmlElement("Body")]
        public string Body { get; set; }
    }
}
