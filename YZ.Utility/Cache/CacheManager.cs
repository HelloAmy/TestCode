using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Caching;

namespace YZ.Utility
{
    public static class CacheManager
    {
        /// <summary>
        /// 构造缓存的Key
        /// </summary>
        /// <param name="baseKey"></param>
        /// <param name="paramlist"></param>
        /// <returns></returns>
        public static string GenerateKey(string baseKey, params string[] paramlist)
        {
            string key = baseKey;
            if (paramlist != null && paramlist.Length > 0)
            {
                foreach (string param in paramlist)
                {
                    key += "_" + param;
                }
            }
            return key;
        }

        /// <summary>
        /// 构造缓存的Key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="baseKey"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string GenerateKey<T>(string baseKey, T param) where T : class
        {
            string key = baseKey;
            if (param != null)
            {
                return key;
            }
            string strParam = SerializeHelper.BinarySerialize(param);
            key += "_" + strParam;
            return key;
        }

        #region 从文件获取

        /// <summary>
        /// 文件依赖性缓存读写，如果filePathList不为空，那么这些缓存将依赖其文件变化而清空
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="getter"></param>
        /// <param name="filePathList"></param>
        /// <returns></returns>
        public static T GetWithLocalCache<T>(string cacheKey, Func<T> getter, params string[] filePathList)
            where T : class
        {
            T rst = MemoryCache.Default.Get(cacheKey) as T;
            if (rst != null)
            {
                return rst;
            }
            string locker = "CM_F_" + cacheKey;
            lock (string.Intern(locker))
            {
                rst = MemoryCache.Default.Get(cacheKey) as T;
                if (rst != null)
                {
                    return rst;
                }
                rst = getter();
                List<string> list = new List<string>(filePathList.Length);
                foreach (var file in filePathList)
                {
                    if (File.Exists(file))
                    {
                        list.Add(file);
                    }
                }
                if (list.Count > 0)
                {
                    CacheItemPolicy cp = new CacheItemPolicy();
                    cp.ChangeMonitors.Add(new HostFileChangeMonitor(list));
                    MemoryCache.Default.Set(cacheKey, rst, cp);
                }
                else
                {
                    CacheItemPolicy cp = new CacheItemPolicy();
                    cp.AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddSeconds(CacheTime.Longest));
                    MemoryCache.Default.Set(cacheKey, rst, cp);
                }
                return rst;
            }
        }

        public static string ReadTextFileWithLocalCache(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }
            FileInfo f = new FileInfo(filePath);
            string key = "ReadTextFileWithCache_" + f.FullName.ToUpper().GetHashCode().ToString();
            return GetWithLocalCache<string>(key, () => LoadRawString(filePath), filePath);
        }

        public static T ReadXmlFileWithLocalCache<T>(string filePath)
            where T : class
        {
            if (!File.Exists(filePath))
            {
                return null;
            }
            FileInfo f = new FileInfo(filePath);
            string key = "ReadXmlFileWithCache_" + f.FullName.ToUpper().GetHashCode().ToString();
            return GetWithLocalCache<T>(key, () => SerializeHelper.LoadFromXml<T>(filePath), filePath);
        }

        public static T ReadJsonFileWithLocalCache<T>(string filePath)
            where T : class, new()
        {
            if (!File.Exists(filePath))
            {
                return null;
            }
            FileInfo f = new FileInfo(filePath);
            string key = "ReadJsonFileWithCache_" + f.FullName.ToUpper().GetHashCode().ToString();
            return GetWithLocalCache<T>(key, () => SerializeHelper.JsonDeserialize<T>(LoadRawString(filePath)), filePath);
        }

        private static string LoadRawString(string filePath)
        {
            using (StreamReader sr = new StreamReader(filePath, Encoding.GetEncoding("gb2312"), true))
            {
                return sr.ReadToEnd();
            }
        }

        #endregion

        public static T GetWithCache<T>(string cacheKey, Func<T> getter, int cacheTimeSecond, bool absoluteExpiration = true)
            where T : class
        {
            return GetInstance().GetWithCache<T>(cacheKey, getter, cacheTimeSecond, absoluteExpiration);
        }

        public static object GetWithCache(string cacheKey, Func<object> getter, int cacheTimeSecond, bool absoluteExpiration = true)
        {
            return GetInstance().GetWithCache(cacheKey, getter, cacheTimeSecond, absoluteExpiration);
        }

        public static void Remove(string key)
        {
            GetInstance().Remove(key);
        }

        public static void RemoveStartsWith(string prefix)
        {
            var cache = GetInstance();
            var list = cache.GetList().ToList();
            foreach (var item in list)
            {
                if (item.Key.StartsWith(prefix))
                {
                    GetInstance().Remove(item.Key);
                }
            }
        }

        public static void FlushAll()
        {
            GetInstance().FlushAll();
        }

        public static IEnumerable<KeyValuePair<string, object>> GetList()
        {
            return GetInstance().GetList();
        }

        private static ICache GetInstance()
        {
            string cacheType = "LOCAL";
            if (!string.IsNullOrWhiteSpace(AppSettingManager.GetSetting("System", "CacheType")))
            {
                cacheType = AppSettingManager.GetSetting("System", "CacheType").Trim().ToUpper();
            }
            switch (cacheType)
            {
                case "LOCAL":
                    return new LocalMemoryCache();

                default:
                    return new LocalMemoryCache();
            }
        }
    }
}
