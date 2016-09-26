using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace YZ.Utility
{
    public static class TextResourceManager
    {
        #region Private Member

        private const string NODE_NAME = "TextResourcesPath";
        private static readonly string s_BaseFolderPath = GetBaseFolderPath();

        private static string GetBaseFolderPath()
        {
            string path = ConfigurationManager.AppSettings[NODE_NAME];
            if (path == null || path.Trim().Length <= 0)
            {
                return Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, "Configuration/TextResources");
            }
            string p = Path.GetPathRoot(path);
            if (p == null || p.Trim().Length <= 0) // 说明是相对路径
            {
                path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, path);
            }
            return path;
        }

        private static Dictionary<string, string> LoadTextResource(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new Dictionary<string, string>(0);
                //throw new FileNotFoundException("Can't find the CodeNamePair config file: " + getFullConfigPath + "!");
            }
            Dictionary<string, string> rst = new Dictionary<string, string>();
            XElement doc = XElement.Load(filePath);
            foreach (var x in doc.Descendants("Message"))
            {
                if (x.Attribute("name") == null || x.Attribute("name").Value == null || x.Attribute("name").Value.Trim().Length <= 0)
                {
                    throw new ApplicationException("There are some 'Message' node without attribute 'name' in the TextResource config file: " + filePath + ", please check it!");
                }
                string name = x.Attribute("name").Value.Trim().ToUpper();
                if (rst.ContainsKey(name))
                {
                    throw new ApplicationException("Duplicated value '" + x.Attribute("name").Value.Trim() + "' of attribute 'name' in 'Message' node in the TextResource config file: " + filePath + ", please check it (ex: ignore case)!");
                }
                rst.Add(name, EnvironmentVariableManager.ReplaceVariable(x.Value));
            }
            return rst;
        }

        /// <summary>
        /// 获取Message的描述信息
        /// </summary>
        /// <param name="resouceFileTitle">资源文件的名称（不包含语言和后缀名部分，但需要包含相对于Resources根目录的路径，使用‘.’分隔目录，不区分大小写）</param>
        /// <param name="keyName">Message的键值</param>
        /// <param name="inputLanguageCode">需要调用的Message语言版本</param>
        /// <returns>Message 描述</returns>
        private static string GetText(string resouceFileTitle, string keyName, string inputLanguageCode, bool fileNameNeedLanguageCode)
        {
            string fileName = resouceFileTitle.Trim().Trim('.').Replace('.', Path.DirectorySeparatorChar);
            if (fileNameNeedLanguageCode)
            {
                fileName = string.Format("{0}.{1}.config", fileName.Trim(), inputLanguageCode.Trim());
            }
            else
            {
                fileName = string.Format("{0}.config", fileName.Trim());
            }
            string filePath = Path.Combine(s_BaseFolderPath, fileName);
            string key = fileName.ToUpper();
            Dictionary<string, string> resDic = CacheManager.GetWithLocalCache<Dictionary<string, string>>(key, () => LoadTextResource(filePath), filePath);
            string txt;
            if (resDic != null && resDic.TryGetValue(keyName.ToUpper(), out txt))
            {
                return txt;
            }
            return null;
        }

        #endregion

        /// <summary>
        /// 获取TextResource配置文件所在的根目录（该目录路径默认为应用程序根目录下的Configuration/TextResources，但可以由web.config或app.config的appsettings/add节点来配置：<add key="TextResourcesPath" value="" />）
        /// </summary>
        public static string BaseFolderPath
        {
            get { return s_BaseFolderPath; }
        }

        /// <summary>
        /// 获取TextResource中的描述信息，本方法将自动使用当前线程上下文的语言信息
        /// </summary>
        /// <param name="resouceFileTitle">资源文件的名称（不包含语言和后缀名部分，但需要包含相对于TextResource根目录的路径，使用‘.’分隔目录，不区分大小写）</param>
        /// <param name="keyName">对应到配置文件中Message节点的键值</param>
        /// <returns>TextResource的Message描述</returns>
        public static string GetText(string resouceFileTitle, string keyName)
        {
            return GetText(resouceFileTitle, keyName, true);
        }

        public static string GetText(string resouceFileTitle, string keyName, bool fileNameNeedLanguageCode)
        {
            string inputLanguageCode = Thread.CurrentThread.CurrentCulture.Name;
            return GetText(resouceFileTitle, keyName, inputLanguageCode, fileNameNeedLanguageCode);
        }
    }

    public static class LangHelper
    {
        /// <summary>
        /// 获取文字性内容
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetText(string key)
        {
            return key;
        }

        /// <summary>
        /// 获取文字性内容
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetText(string key, string fileOnlyName, string languageCode)
        {
            string text = TextResourceManager.GetText(languageCode + "." + fileOnlyName, key, false);
            if (string.IsNullOrEmpty(text))
            {
                return key;
            }
            return text;
        }
    }
}
