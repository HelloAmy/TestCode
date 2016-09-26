using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YZ.Utility
{
    /// <summary>
    /// 要想hashcode不重复，一个简单的做法就是扩大hashcode的取值范围，由原来4个字节的int型，变为8个字节的long型，这样算出来的hashcode重复率大大降低，低到几乎可以忽略不计。
    /// 参考： http://blog.csdn.net/scariii/article/details/7237042
    /// </summary>
    public static class LongHashCodeGenerator
    {
        static long[] byteTable = CreateLookupTable();
 
        static  long HSTART = unchecked((long)0xBB40E64DA205B064L);
    
        static long HMULT = 7664345821815920749L;

        private static long[] CreateLookupTable()
        {
            long[] byteTable = new long[256];
            long h = 0x544B2FBACAAF1684L;
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 31; j++)
                {
                    h = (h >> 7) ^ h;
                    h = (h << 11) ^ h;
                    h = (h >> 10) ^ h;
                }
                byteTable[i] = h;
            }
            return byteTable;
        }
        public static long GetHashCode(string cs)
        {
            long h = HSTART;
            long hmult = HMULT;
            long[] ht = byteTable;
            int len = cs.Length;
            for (int i = 0; i < len; i++)
            {
                char ch = cs[i];
                h = (h * hmult) ^ ht[ch & 0xff];
                h = (h * hmult) ^ ht[(ch >> 8) & 0xff];
            }
            return h;
        }
    }
}
