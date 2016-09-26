using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YZ.Utility
{
    public static class Guard
    {
        public static void IsNotNull(object parameter, string parameterName)
        {
            if (parameter == null)
            {
                string msg = string.Format("参数 {0} 不能为null。", parameterName);
                throw new ArgumentNullException(parameterName, msg);
            }
        }

        public static void IsNotNullOrEmpty(string parameter, string parameterName)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                string msg = string.Format("参数 {0} 不能为null或空字符串。", parameterName);
                throw new ArgumentException(msg);
            }
        }
    }
}
