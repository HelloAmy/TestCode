using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace YZ.Utility
{
    [Serializable]
    [XmlRoot("ApplicationSettings")]
    [ConfigFile(@"Settings\Common\ApplicationSettings.xml")]
    public class ApplicationSettings
    {
        /// <summary>
        /// 程序名称
        /// </summary>
        [XmlElement("AppName")]
        public string AppName { get; set; }

        /// <summary>
        /// 程序地址
        /// </summary>
        [XmlElement("AppUrl")]
        public string AppUrl { get; set; }

        /// <summary>
        /// 程序Logo地址
        /// </summary>
        [XmlElement("AppLogoUrl")]
        public string AppLogoUrl { get; set; }

        /// <summary>
        /// 公司名称
        /// </summary>
        [XmlElement("CompanyName")]
        public string CompanyName { get; set; }

        /// <summary>
        /// 公司地址
        /// </summary>
        [XmlElement("CompanyAddress")]
        public string CompanyAddress { get; set; }

        /// <summary>
        /// 程序支持团队
        /// </summary>
        [XmlElement("AppSupportTeam")]
        public string AppSupportTeam { get; set; }
    }
}
