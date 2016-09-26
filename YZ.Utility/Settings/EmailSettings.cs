using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace YZ.Utility
{
    [Serializable]
    [XmlRoot("EmailSettings")]
    [ConfigFile(@"Settings\Common\EmailSettings.xml")]
    public class EmailSettings
    {
        /// <summary>
        /// 是否正式上线
        /// </summary>
        public bool IsProduction { get; set; }

        /// <summary>
        /// 测试阶段接收地址
        /// </summary>
        public string TestingReceiverEmail { get; set; }

        /// <summary>
        /// 邮件服务器
        /// </summary>
        [XmlElement("Server")]
        public string Server { get; set; }

        /// <summary>
        /// 邮件服务器端口号
        /// </summary>
        [XmlElement("Port")]
        public int? Port { get; set; }

        /// <summary>
        /// 是否启用SSL加密
        /// </summary>
        [XmlElement("EnableSsl")]
        public bool? EnableSsl { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [XmlElement("UserName")]
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [XmlElement("Password")]
        public string Password { get; set; }

        /// <summary>
        /// 发送邮箱地址
        /// </summary>
        [XmlElement("EmailSender")]
        public string EmailSender { get; set; }
    }
}
