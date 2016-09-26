using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace YZ.Utility.ServiceClient
{
    public class WebAPIClient
    {
        /// <summary>
        /// 请求WEBAPI并返回结果
        /// </summary>
        /// <param name="url">URL地址，http/https格式，不能含?和参数</param>
        /// <param name="paraList">参数，以keyvalue列表的方式，key表示参数名，value为参数值，value不要urlencode编码</param>
        /// <param name="method">POST/GET</param>
        /// <returns>返回结果，如果发生异常则直接向外抛出</returns>
        public static string SendRequest(string url, List<KeyValuePair<string, string>> paraList, string method)
        {
            string strResult = string.Empty;
            if (url == null || url == "")
            {
                return null;
            }
            if (string.IsNullOrWhiteSpace(method))
            {
                method = "GET";
            }

            // GET方式
            if (method.ToUpper() == "GET")
            {

                string fullUrl = url.TrimEnd('?', '&') + (url.Contains("?") ? "&" : "?");

                if (paraList != null)
                {
                    foreach (KeyValuePair<string, string> kv in paraList)
                    {
                        //Jin：Uri.EscapeDataString是最完美URLENCODE方案
                        fullUrl += kv.Key + "=" + EscapeDataString(kv.Value) + "&";
                    }
                    fullUrl = fullUrl.TrimEnd('?', '&');
                }

                System.Net.WebRequest wrq = System.Net.WebRequest.Create(fullUrl);
                wrq.Method = "GET";
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                System.Net.WebResponse response = wrq.GetResponse();
                System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream(), System.Text.Encoding.GetEncoding("utf-8"));

                strResult = sr.ReadToEnd();
            }

            // POST方式
            if (method.ToUpper() == "POST")
            {
                url = url.TrimEnd('?');
                WebRequest wrq = WebRequest.Create(url);
                wrq.Method = "POST";
                wrq.ContentType = "application/x-www-form-urlencoded";
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;

                StringBuilder sbPara = new StringBuilder();
                string paraString = string.Empty;
                if (paraList != null)
                {
                    foreach (KeyValuePair<string, string> kv in paraList)
                    {
                        //Jin：Uri.EscapeDataString是最完美URLENCODE方案
                        sbPara.Append(kv.Key + "=" + EscapeDataString(kv.Value) + "&");
                    }
                    paraString = sbPara.ToString().TrimEnd('&');

                    byte[] buf = System.Text.Encoding.GetEncoding("utf-8").GetBytes(paraString);
                    System.IO.Stream requestStream = wrq.GetRequestStream();
                    requestStream.Write(buf, 0, buf.Length);
                    requestStream.Close();
                }
                else
                {
                    wrq.ContentLength = 0;
                }

                WebResponse response = wrq.GetResponse();
                Stream receiveStream = response.GetResponseStream();

                //Byte[] read = new Byte[512];
                //int bytes = receiveStream.Read(read, 0, 512);
                //while (bytes > 0)
                //{
                //    Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                //    strResult += encode.GetString(read, 0, bytes);
                //    bytes = receiveStream.Read(read, 0, 512);
                //}

                Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                //Pipe the stream to a higher level stream reader with the required encoding format. 
                StreamReader readStream = new StreamReader(receiveStream, encode);
                Char[] read = new Char[256];
                // Read 256 charcters at a time.    
                int count = readStream.Read(read, 0, 256);
                while (count > 0)
                {
                   // Dump the 256 characters on a string and display the string onto the console.
                    String str = new String(read, 0, count);
                    strResult += str;
                    count = readStream.Read(read, 0, 256);
                }
                readStream.Close();
                response.Close();
            }

            return strResult;

        }

        private static string EscapeDataString(string value)
        {
            int maxStringSize = 3000;

            if (string.IsNullOrEmpty(value))
                return value;

            if (value.Length <= maxStringSize)
                return Uri.EscapeDataString(value);

            char[] valueArray = value.ToCharArray();
            StringBuilder sb = new StringBuilder();
            int count = value.Length % maxStringSize == 0 ? value.Length / maxStringSize : value.Length / maxStringSize + 1;
            for (int i = 0; i < count; i++)
            {
                int bufferSize = (i == count - 1 ? value.Length - i * maxStringSize : maxStringSize);

                string tempEscapedString = new string(valueArray, i * maxStringSize, bufferSize);

                sb.Append(Uri.EscapeDataString(tempEscapedString));
            }

            return sb.ToString();
        }
    }
}
