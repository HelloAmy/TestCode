using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YZ.Utility
{
    /// <summary>
    /// 缓存时间枚举
    /// </summary>
    public struct CacheTime
    {
        /// <summary>
        /// 非常短的时间，60秒，1分钟
        /// </summary>
        public static int Shortest = 60;

        /// <summary>
        /// 短时间，300秒，5分钟
        /// </summary>
        public static int Short = 300;


        /// <summary>
        /// 中等时间，600秒，10分钟
        /// </summary>
        public static int Middle = 600;

        /// <summary>
        /// 长时间，900秒，15分钟
        /// </summary>
        public static int Long = 900;

        /// <summary>
        /// 更长时间，1800秒，半小时
        /// </summary>
        public static int Longer = 1800;

        /// <summary>
        /// 最长时间，3600秒，1小时
        /// </summary>
        public static int Longest = 3600;
    }
}
