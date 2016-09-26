using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace YZ.Utility
{
    internal class LocalMemoryCache : ICache
    {
        //string CACHE_LOCKER_PREFIX = "CM_DL_";

        public T GetWithCache<T>(string cacheKey, Func<T> getter, int cacheTimeSecond, bool absoluteExpiration = true)
            where T : class
        {
            T rst = MemoryCache.Default.Get(cacheKey) as T;
            if (rst != null)
            {
                return rst;
            }
            //string locker = CACHE_LOCKER_PREFIX + cacheKey;
            //lock (string.Intern(locker))
            //{
                //rst = MemoryCache.Default.Get(cacheKey) as T;
                //if (rst != null)
                //{
                //    return rst;
                //}
                rst = getter();
                CacheItemPolicy cp = new CacheItemPolicy();
                if (absoluteExpiration)
                {
                    cp.AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddSeconds(cacheTimeSecond));
                }
                else
                {
                    cp.SlidingExpiration = TimeSpan.FromSeconds(cacheTimeSecond);
                }
                if ( rst != null )
                {
                    MemoryCache.Default.Set( cacheKey, rst, cp );
                }
                return rst;
            //}
        }

        public object GetWithCache(string cacheKey, Func<object> getter, int cacheTimeSecond, bool absoluteExpiration = true)
        {
            object rst = MemoryCache.Default.Get(cacheKey);
            if (rst != null)
            {
                return rst;
            }
            //string locker = CACHE_LOCKER_PREFIX + cacheKey;
            //lock (string.Intern(locker))
            //{
                //rst = MemoryCache.Default.Get(cacheKey);
                //if (rst != null)
                //{
                //    return rst;
                //}
                rst = getter();
                CacheItemPolicy cp = new CacheItemPolicy();
                if (absoluteExpiration)
                {
                    cp.AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddSeconds(cacheTimeSecond));
                }
                else
                {
                    cp.SlidingExpiration = TimeSpan.FromSeconds(cacheTimeSecond);
                }
                if ( rst != null )
                {
                    MemoryCache.Default.Set( cacheKey, rst, cp );
                }
                return rst;
            //}

        }
        public void Remove(string key)
        {
            MemoryCache.Default.Remove(key);
        }

        public void FlushAll()
        {
            MemoryCache.Default.Dispose();
        }

        public IEnumerable<KeyValuePair<string, object>> GetList()
        {
            return MemoryCache.Default;
        }
    }
}
