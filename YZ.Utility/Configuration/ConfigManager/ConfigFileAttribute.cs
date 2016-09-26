using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YZ.Utility
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ConfigFileAttribute : Attribute
    {
        private string _relatedPath;
        public ConfigFileAttribute(string relatedPath)
        {
            _relatedPath = relatedPath;
        }

        public string RelativePath
        {
            get
            {
                return _relatedPath;
            }
        }
    }
}
