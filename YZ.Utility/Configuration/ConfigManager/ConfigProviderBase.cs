using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YZ.Utility
{
    abstract class ConfigProviderBase
    {
        public abstract T GetConfig<T>() where T : class,new();
    }
}
