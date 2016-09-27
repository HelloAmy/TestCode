using System;
using System.Collections.Generic;
//using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Xml;
using YZ.Utility;

namespace YZ.Mall.ServiceHost
{
    public class OptLogConfig
    {
        public Dictionary<string, JsonRpcOptLogConfigMethod> Methods { get; set; }
        public Dictionary<string, string> DictBizObjectTypes { get; set; }
        public Dictionary<string, string> DictBizOperationTypes { get; set; }

        private static OptLogConfig current;
        private static readonly object loadLocker = new object();

        public static OptLogConfig Current
        {
            get
            {
                lock (loadLocker)
                {
                    return CacheManager.GetWithLocalCache("JSONRPC_OptLogConfig", ReadConfig,
                        AppDomain.CurrentDomain.BaseDirectory + "\\jsonrpc-optlog.config");
                }
            }
        }

        private static OptLogConfig ReadConfig()
        {
            var tempConfig = new OptLogConfig();
            tempConfig.DictBizObjectTypes = new Dictionary<string, string>();
            tempConfig.DictBizOperationTypes = new Dictionary<string, string>();
            tempConfig.Methods = new Dictionary<string, JsonRpcOptLogConfigMethod>();
            var jsonrpcConfigPath = AppDomain.CurrentDomain.BaseDirectory + "\\jsonrpc-optlog.config";

            XmlDocument configXml = new XmlDocument();
            configXml.Load(jsonrpcConfigPath);
            foreach (XmlNode node in configXml["jsonrpc-optlog"]["dictBizObjectTypes"].ChildNodes)
            {
                if (!"add".Equals(node.Name, StringComparison.InvariantCultureIgnoreCase))
                    continue;
                tempConfig.DictBizObjectTypes.Add(node.Attributes["key"].Value, node.Attributes["name"].Value);
            }

            foreach (XmlNode node in configXml["jsonrpc-optlog"]["dictBizOperationTypes"].ChildNodes)
            {
                if (!"add".Equals(node.Name, StringComparison.InvariantCultureIgnoreCase))
                    continue;
                tempConfig.DictBizOperationTypes.Add(node.Attributes["key"].Value, node.Attributes["name"].Value);
            }

            foreach (XmlNode node in configXml["jsonrpc-optlog"]["methods"].ChildNodes)
            {
                if (!"add".Equals(node.Name, StringComparison.InvariantCultureIgnoreCase))
                    continue;
                try
                {
                    tempConfig.Methods.Add(
                        node.Attributes["method"].Value,
                        new JsonRpcOptLogConfigMethod()
                        {
                            method = node.Attributes["method"].Value,
                            bizObjectType = node.Attributes["bizObjectType"].Value,
                            bizOperationType = node.Attributes["bizOperationType"].Value,
                            bizObjectIdExpression = node.Attributes["bizObjectIdExpression"].Value,
                            operationDesc = node.Attributes["operationDesc"].Value
                        });
                }
                catch
                {
                }
            }
            return tempConfig;
        }
    }

    public class JsonRpcOptLogConfigMethod
    {
        public string method { get; set; }
        public string bizObjectType { get; set; }
        public string bizOperationType { get; set; }
        public string bizObjectIdExpression { get; set; }
        public string operationDesc { get; set; }
    }
}