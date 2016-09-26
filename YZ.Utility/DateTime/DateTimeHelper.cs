using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YZ.Utility
{
    public class DateTimeHelper
    {
        /// <summary>
        /// 获取当前年份
        /// </summary>
        /// <returns></returns>
        public static string GetYearString()
        {
            return DateTime.Now.ToString("yyyy");
        }
    }
}
