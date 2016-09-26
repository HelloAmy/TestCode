using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.International.Converters.PinYinConverter;

namespace YZ.Utility
{
    public static  class PinYinHelper
    {
        /// <summary> 
        /// 汉字转化为拼音
        /// </summary> 
        /// <param name="str">汉字</param> 
        /// <param name="str">转化的拼音是否需要用空格隔开</param> 
        /// <returns>全拼</returns> 
        public static string GetPinYin(string str, bool withBlank)
        {
            if (str == null)
            {
                return null;
            }
            if (str.Trim().Length <= 0)
            {
                return string.Empty;
            }

            string[] list = new string[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                char obj = str[i];
                if (ChineseChar.IsValidChar(obj))
                {
                    ChineseChar chineseChar = new ChineseChar(obj);
                    string t = chineseChar.Pinyins[0];
                    list[i] = t.Substring(0, t.Length - 1);
                }
                else
                {
                    list[i] = obj.ToString();
                }
            }
            return string.Join(withBlank ? " " : "", list);
        }

        /// <summary> 
        /// 汉字转化为拼音首字母
        /// </summary> 
        /// <param name="str">汉字</param> 
        /// <returns>首字母</returns> 
        public static string GetFirstPinYin(string str)
        {
            if (str == null)
            {
                return null;
            }
            if (str.Trim().Length <= 0)
            {
                return string.Empty;
            }
            char[] list = new char[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                char obj = str[i];
                if (ChineseChar.IsValidChar(obj))
                {
                    ChineseChar chineseChar = new ChineseChar(obj);
                    list[i] = chineseChar.Pinyins[0][0];
                }
                else
                {
                    list[i] = obj;
                }
            }
            return new string(list);
        }

    }
}