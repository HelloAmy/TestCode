using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using YZ.Utility.ServiceClient;

namespace YZ.Utility.ESB
{
    public class ESBPublish
    {
        public static string ESBServiceUrl = ConfigurationManager.AppSettings["ESBServiceUrl"];
        public static string Publish(string topic, object message, HttpMethod method = HttpMethod.Post)
        {
            List<KeyValuePair<string, string>> paraList = new List<KeyValuePair<string, string>>();
            string messagetext = string.Empty;
            if (message != null)
            {
                SerializeHelper.JsonSerialize(message);
                messagetext = Uri.EscapeDataString(messagetext);
            }
            paraList.Add(new KeyValuePair<string, string>("topic", topic));
            paraList.Add(new KeyValuePair<string, string>("messagetext", messagetext));
            string methodtext = "POST";
            if (method == HttpMethod.Get)
            {
                methodtext = "GET";
            }
            return WebAPIClient.SendRequest(ESBServiceUrl, paraList, methodtext);
        }

        public enum HttpMethod
        {
            Get,
            Post,
            Put,
            Delete
        }
    }
}
