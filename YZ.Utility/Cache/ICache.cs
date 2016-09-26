using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace YZ.Utility
{
    internal interface ICache
    {
        T GetWithCache<T>(string cacheKey, Func<T> getter, int cacheTimeSecond, bool absoluteExpiration = true) 
            where T : class;

        object GetWithCache(string cacheKey, Func<object> getter, int cacheTimeSecond, bool absoluteExpiration = true);

        void Remove(string key);

        void FlushAll();

        IEnumerable<KeyValuePair<string, object>> GetList();
    }
}
