using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using System.Threading;

namespace YZ.Utility
{
    /// <summary>
    /// 自定义AppSetting的配置管理
    /// </summary>
    public static class AppSettingManager
    {
        #region Private Memeber

        private const string NODE_NAME = "AppSettingsPath";
        private static readonly string s_BaseFolderPath = GetBaseFolderPath();

        private static string GetBaseFolderPath()
        {
            string path = ConfigurationManager.AppSettings[NODE_NAME];
            if (path == null || path.Trim().Length <= 0)
            {
                return Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Configuration/AppSettings");
            }
            string p = Path.GetPathRoot(path);
            if (p == null || p.Trim().Length <= 0) // 说明是相对路径
            {
                path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path);
            }
            return path;
        }

        private static Dictionary<string, string> LoadSettings(string filePath)
        {
            if (!File.Exists(filePath))
            {
                //throw new FileNotFoundException("Can't find the AppSettings config file: " + filePath + "!");
                return new Dictionary<string, string>(0);
            }
            Dictionary<string, string> rst = new Dictionary<string, string>();
            XElement doc = XElement.Load(filePath);
            foreach (var x in doc.Descendants("add"))
            {
                if (x.Attribute("key") == null || x.Attribute("key").Value == null || x.Attribute("key").Value.Trim().Length <= 0)
                {
                    throw new ApplicationException("There are some 'add' node without attribute 'key' in the AppSettings config file: " + filePath + ", please check it!");
                }
                string key = x.Attribute("key").Value.Trim().ToUpper();
                if (rst.ContainsKey(key))
                {
                    throw new ApplicationException("Duplicated value '" + x.Attribute("key").Value.Trim() + "' of attribute 'key' in 'add' node in the AppSettings config file: " + filePath + ", please check it (ex: ignore case)!");
                }
                string value = x.Attribute("value") == null ? null : (x.Attribute("value").Value == null ? null : x.Attribute("value").Value.Trim());
                value = value ?? x.Value;
                rst.Add(key, EnvironmentVariableManager.ReplaceVariable(value));
            }
            return rst;
        }

        #endregion

        /// <summary>
        /// 获取AppSetting配置文件所在的根目录（该目录路径默认为应用程序根目录下的Configuration/AppSettings，但可以由web.config或app.config的appsettings/add节点来配置：<add key="AppSettingsPath" value="" />）
        /// </summary>
        public static string BaseFolderPath
        {
            get { return s_BaseFolderPath; }
        }

        /// <summary>
        /// 获取所配置的数据值
        /// </summary>
        /// <param name="fileTitle">配置文件的名称（不包含后缀名部分，但需要包含相对于AppSetting根目录的路径，使用‘.’分隔目录，不区分大小写）</param>
        /// <param name="key">对应到配置文件中xml节点add的属性key的值</param>
        /// <returns>返回对应key的add节点的value属性值</returns>
        public static string GetSetting(string fileTitle, string key)
        {
            key = key.ToUpper();
            string fileName = fileTitle.Trim().Trim('.').Replace('.', Path.DirectorySeparatorChar).ToUpper();
            string filePath = Path.Combine(s_BaseFolderPath, fileName + ".config");
            Dictionary<string, string> data = CacheManager.GetWithLocalCache<Dictionary<string, string>>(fileName, () => LoadSettings(filePath), filePath);
            string rst;
            if (data.TryGetValue(key, out rst))
            {
                return rst;
            }
            return null;
        }

        /// <summary>
        /// Gets the conifg data.
        /// </summary>
        /// <param name="fileTitle">配置文件的名称（不包含后缀名部分，但需要包含相对于AppSetting根目录的路径，使用‘.’分隔目录，不区分大小写）</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetConifgData( string fileTitle )
        {
            string fileName = fileTitle.Trim( ).Trim( '.' ).Replace( '.', Path.DirectorySeparatorChar ).ToUpper( );
            string filePath = Path.Combine( s_BaseFolderPath, fileName + ".config" );
            
            return CacheManager.GetWithLocalCache<Dictionary<string, string>>( fileName, ( ) => LoadSettings( filePath ), filePath );

        }
    }
}   
 