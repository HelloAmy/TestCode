using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace YZ.Utility
{
    public static class ConfigManager
    {
        private static readonly XmlConfigProvider _xmlConfigProvider;
        private static readonly XmlCachedConfigProvider _xmlCachedConfigProvider;

        static ConfigManager()
        {
            _xmlConfigProvider = new XmlConfigProvider();
            _xmlCachedConfigProvider = new XmlCachedConfigProvider();
        }

        public static T GetXmlConfig<T>() where T:class,new()
        {
            return _xmlConfigProvider.GetConfig<T>();
        }

        public static T GetXmlCachedConfig<T>() where T : class,new()
        {
            return _xmlCachedConfigProvider.GetConfig<T>();
        }

        private static Dictionary<string, List<KeyValuePair<string, string>>> _kvCache;

        public static List<KeyValuePair<string, string>> GetKeyValuePair(string keyValuePairName, AppendType appendType)
        {
            if (appendType == AppendType.Select)
            {
                return GetKeyValuePair(keyValuePairName, "", ResFramework.SelectValue);
            }

            return GetKeyValuePair(keyValuePairName, "", ResFramework.AllValue);
        }

        /// <summary>
        /// 从配置文件Configuration\KeyValuePair*.xml读取KeyValuePair,并在起始位置增加键值
        /// </summary>
        /// <param name="keyValuePairName">配置名称</param>
        /// <param name="firstKey">默认在起始位置增加键值</param>
        /// <param name="firstValue">默认在起始位置增加键值对应的值</param>
        /// <returns></returns>
        public static List<KeyValuePair<string, string>> GetKeyValuePair(string keyValuePairName, string firstKey, string firstValue)
        {
            var list = GetKeyValuePair(keyValuePairName);
            var firstKeyValue = new KeyValuePair<string,string>(firstKey,firstValue);
            list.Insert(0, firstKeyValue);
            return list;
        }

        /// <summary>
        /// 从配置文件Configuration\KeyValuePair*.xml读取KeyValuePair,
        /// 为了防止在源代码管理器中多人编辑冲突，可以按模块或其它方式分多个文件，
        /// 比如KeyValuePair.xml,KeyValuePair_SysMgmt.xml等，文件名必须以KeyValuePair开头，以.xml结尾
        /// </summary>
        /// <param name="keyValuePairName">配置名称</param>
        /// <returns></returns>
        public static List<KeyValuePair<string, string>> GetKeyValuePair(string keyValuePairName)
        {
            if (_kvCache == null)
            {
                _kvCache = new Dictionary<string, List<KeyValuePair<string, string>>>();
                var files = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory + "Configuration"
                    , "KeyValuePair*.xml"
                    , SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(file);
                    var nodes = xmlDoc.SelectNodes("/Root/KeyValuePair");
                    KeyValuePair<string, string> kv;
                    foreach (XmlNode n in nodes)
                    {
                        var items = n.SelectNodes("item");
                        string key = n.Attributes["name"].Value;
                        if (!_kvCache.ContainsKey(key))
                        {
                            List<KeyValuePair<string, string>> kvList = new List<KeyValuePair<string, string>>();
                            foreach (XmlNode i in items)
                            {
                                kv = new KeyValuePair<string, string>(i.Attributes["key"].Value
                                    , i.Attributes["value"].Value);
                                kvList.Add(kv);
                            }
                            _kvCache.Add(key, kvList);
                        }
                    }
                }
            }

            if (_kvCache.ContainsKey(keyValuePairName))
            {
                return _kvCache[keyValuePairName];
            }

            return new List<KeyValuePair<string, string>>();
        }
    }
}
