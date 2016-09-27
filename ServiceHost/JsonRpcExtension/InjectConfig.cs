using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using YZ.Utility;

namespace YZ.Mall.ServiceHost
{
    [XmlRoot("inject")]
    public class InjectConfig
    {
        #region single instance methods
        private static InjectConfig currentConfig;
        private static object configLocker = new object();
        public static InjectConfig Current
        {
            get
            {
                lock (configLocker)
                {
                    return CacheManager.GetWithLocalCache("JSONRPC_InjectConfig", ReadConfig,
                        AppDomain.CurrentDomain.BaseDirectory + "\\jsonrpc-inject.config");
                }
            }
        }
        #endregion

        [XmlElement("group")]
        public List<InjectGroup> Groups { get; set; }
        
        private static InjectConfig ReadConfig()
        {
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\jsonrpc-Inject.config"))
            {
                string xmlContent = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\jsonrpc-inject.config");

                currentConfig = SerializeHelper.XmlDeserialize<InjectConfig>(xmlContent);

                if (currentConfig != null && currentConfig.Groups != null)
                {
                    foreach (InjectGroup group in currentConfig.Groups)
                    {
                        if (group.Profiles != null)
                        {
                            group.Profiles.ForEach(p =>
                            {
                                p.ParentGroup = group;
                                if (string.IsNullOrWhiteSpace(p.HandleMethod))
                                    p.HandleMethod = group.HandleMethod;
                            });
                            
                        }
                    }
                }

                return currentConfig;
            }
            
            return null;
        }
    }

    public class InjectGroup
    {
         [XmlElement("add")]
        public List<InjectProfile> Profiles { get; set; }

         [XmlAttribute("handleMethod")]
         public string HandleMethod { get; set; }
    }


    public class InjectProfile
    {
        [XmlAttribute("injectMethod")]
        public string InjectMethod { get; set; }

        [XmlAttribute("handleMethod")]
        public string HandleMethod { get; set; }

        [XmlAttribute("workerSysNoExpr")]
        public string WorkerSysNoExpr { get; set; }

        [XmlAttribute("idCardTypeExpr")]
        public string IDCardTypeExpr { get; set; }

        [XmlAttribute("idCardNumberExpr")]
        public string IDCardNumberExpr { get; set; }

        [XmlElement("param")]
        public Param[] ParamExprs { get; set; }

        [XmlIgnore]
        public InjectGroup ParentGroup { get; set; }
    }

    public class Param
    {
        [XmlAttribute("expr")]
        public string Expr { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
    }
}