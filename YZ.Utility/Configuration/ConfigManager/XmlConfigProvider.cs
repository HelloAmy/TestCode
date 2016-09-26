using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace YZ.Utility
{
    class XmlConfigProvider:ConfigProviderBase
    {
        public override T GetConfig<T>()
        {
            //在类型上查询ConfigFileAttribute,然后根据参数反序列配置文件
            Type configType = typeof(T);
            object[] attrs = configType.GetCustomAttributes(typeof(ConfigFileAttribute), true);
            if (attrs.Length == 0)
            {
                throw new Exception("Config class must have a ConfigFileAttribute.");
            }

            ConfigFileAttribute config = attrs[0] as ConfigFileAttribute;
            if (String.IsNullOrEmpty(config.RelativePath))
            {
                throw new Exception("Relative path cannot be null or empty.");
            }
            string relativePath = config.RelativePath;
            string baseDir = AppDomain.CurrentDomain.BaseDirectory + "Configuration\\";
            string fileName = Path.Combine(baseDir, relativePath);

            return SerializeHelper.LoadFromXml<T>(fileName);
        }
    }
}
