using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YZ.Utility
{
    class XmlCachedConfigProvider : XmlConfigProvider
    {
        private static readonly Dictionary<string, object> _cache = new Dictionary<string, object>();
        public override T GetConfig<T>()
        {
            string typeName = typeof(T).AssemblyQualifiedName;
            if (_cache.ContainsKey(typeName))
            {
                return _cache[typeName] as T;
            }

            T config= base.GetConfig<T>();
            _cache.Add(typeName, config);
            return config;
        }
    }
}
