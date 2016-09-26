using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace YZ.Utility
{
    public static class StringHelper
    {
        public static string TrimNull(this object input)
        {
            return (input != null ? input.ToString().Trim() : string.Empty);
        }

        public static bool TrimEquals(this string a, string b, StringComparison comparisonType)
        {
            return a.TrimNull().Equals(b.TrimNull(), comparisonType);
        }

        /// <summary>
        /// 冒泡排序
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static string[] BubbleSort(this string[] r)
        {
            for (int i = 0; i < r.Length; i++)
            {
                bool flag = false;
                for (int j = r.Length - 2; j >= i; j--)
                {
                    if (string.CompareOrdinal(r[j + 1], r[j]) < 0)
                    {
                        string str = r[j + 1];
                        r[j + 1] = r[j];
                        r[j] = str;
                        flag = true;
                    }
                }
                if (!flag)
                {
                    return r;
                }
            }
            return r;
        }

        public static string[] ToStringArray<T>(this T[] sourceArray)
        {
            string[] destArray = new string[sourceArray.Length];
            for (int i = 0; i < sourceArray.Length; i++)
            {
                if (sourceArray[i] == null)
                {
                    destArray[i] = null;
                }
                else
                {
                    destArray[i] = sourceArray[i].ToString();
                }
            }
            return destArray;
        }

        public static T[] ToArray<T>(this string[] sourceArray, Func<string, T> convertFunc)
        {
            T[] destArray = new T[sourceArray.Length];
            for (int i = 0; i < sourceArray.Length; i++)
            {
                destArray[i] = convertFunc(sourceArray[i]);
            }
            return destArray;
        }

        /// <summary>
        /// 获得随机数，数字加字母
        /// </summary>
        /// <param name="stringLength"></param>
        /// <returns></returns>
        public static string GetRandomString(int stringLength)
        {
            string strArray = "1234567890abcdefghijkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ";
            Random random = new Random((int)DateTime.Now.Ticks);
            char[] array = new char[strArray.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = strArray[random.Next(strArray.Length)];
            }
            return new string(array);
        }

        /// <summary>
        /// 获得随机数，纯数字
        /// </summary>
        /// <param name="stringLength"></param>
        /// <returns></returns>
        public static string GetRandomNumString(int stringLength)
        {
            string strArray = "1234567890";
            Random random = new Random((int)DateTime.Now.Ticks);
            char[] array = new char[strArray.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = strArray[random.Next(strArray.Length)];
            }
            return new string(array);
        }


        public static string RemoveHtml(this string str)
        {
            //fixbug 
            if (string.IsNullOrWhiteSpace(str)) return str;
            Regex regex = new Regex(@"<\/*[^<>]*>");
            return regex.Replace(str, string.Empty);
        }

        #region 验证字符串格式合法性

        internal static bool RegexCheck(string input, string regex, RegexOptions regexOptions = RegexOptions.None)
        {
            if (input == null || regex == null)
            {
                return false;
            }
            return Regex.IsMatch(input, regex, regexOptions);
        }

        /// <summary>
        /// 验证字符串是否表示合法的手机号码
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns></returns>
        public static bool IsMobileNo(this string input)
        {
            return RegexCheck(input, @"^(1[3,4,5,7,8]\d{9}$)");
        }

        /// <summary>
        /// 验证字符串是否表示合法的电话号码
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsPhoneNo(this string input)
        {
            return RegexCheck(input, @"^\d{3,4}-\d{7,8}(-\d{3,4})?|1\d{10}$");
        }


        /// <summary>
        /// 验证字符串是否表示合法的日期时间
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns></returns>
        public static bool IsDateTime(this string input)
        {
            DateTime time;
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            return DateTime.TryParse(input, out time);
        }

        /// <summary>
        /// 验证字符串是否表示合法的浮点数
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns></returns>
        public static bool IsFloat(this string input)
        {
            return RegexCheck(input, @"^-?([1-9]\d*\.\d*|0\.\d*[1-9]\d*|0?\.0+|0)$");
        }

        /// <summary>
        /// 验证字符串是否表示合法的负整数
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns></returns>
        public static bool IsNegativeInteger(this string input)
        {
            return RegexCheck(input, @"^-[1-9]\d*$");
        }

        /// <summary>
        /// 验证字符串是否表示合法的非负整数
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns></returns>
        public static bool IsNonNegativeInteger(this string input)
        {
            return RegexCheck(input, @"^[1-9]\d*|0$");
        }

        /// <summary>
        /// 验证字符串是否是数字字符串
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsNumberString(this string input)
        {
            return RegexCheck(input, @"^\d*$");
        }

        /// <summary>
        /// 验证字符串是否表示合法的正整数
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns></returns>
        public static bool IsPositiveInteger(this string input)
        {
            return RegexCheck(input, @"^[1-9]\d*$");
        }

        /// <summary>
        /// 验证字符串是否表示合法的非正整数
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns></returns>
        public static bool IsNonPositiveInteger(this string input)
        {
            return RegexCheck(input, @"^-[1-9]\d*|0$");
        }

        /// <summary>
        /// 验证字符串是否表示合法的整数
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns></returns>
        public static bool IsInteger(this string input)
        {
            return RegexCheck(input, @"^-?[1-9]\d*$");
        }

        /// <summary>
        /// 验证字符串是否表示合法的中国邮政编码
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns></returns>
        public static bool IsZipcode(this string input)
        {
            return RegexCheck(input, @"^[1-9]\d{5}(?!\d)$");
        }

        /// <summary>
        /// 验证字符串是否表示合法的电子邮件地址
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns></returns>
        public static bool IsEmail(this string input)
        {
            return RegexCheck(input, @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
        }

        /// <summary>
        /// 验证字符串是否表示合法的传真
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns></returns>
        public static bool IsFax(this string input)
        {
            return RegexCheck(input, @"^(\d{3,4}-)?\d{7,8}$");
        }

        /// <summary>
        /// 验证字符串是否表示合法的IPv4地址
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns></returns>
        public static bool IsIPv4(this string input)
        {
            return RegexCheck(input, @"^((?:2[0-5]{2}|1\d{2}|[1-9]\d|[1-9])\.(?:(?:2[0-5]{2}|1\d{2}|[1-9]\d|\d)\.){2}(?:2[0-5]{2}|1\d{2}|[1-9]\d|\d)):(\d|[1-9]\d|[1-9]\d{2,3}|[1-5]\d{4}|6[0-4]\d{3}|654\d{2}|655[0-2]\d|6553[0-5])$");
        }

        /// <summary>
        /// 验证字符串是否表示合法的IPv6地址
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns></returns>
        public static bool IsIPv6(this string input)
        {
            return RegexCheck(input, "^([0-9A-Fa-f]{1,4}:){7}[0-9A-Fa-f]{1,4}$");
        }

        /// <summary>
        /// 验证字符串是否表示合法的货币金额
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns></returns>
        public static bool IsMoney(this string input)
        {
            return RegexCheck(input, "^([0-9]+|[0-9]{1,3}(,[0-9]{3})*)(.[0-9]{1,2})?$");
        }

        /// <summary>
        /// 验证字符串是否表示合法的Url地址
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns></returns>
        public static bool IsUrl(this string input)
        {
            return RegexCheck(input, @"(mailto\:|(news|(ht|f)tp(s?))\://).*");
        }

        /// <summary>
        /// 验证字符串是否表示有效的身份证号格式
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsIDCardNoFormat(this string input)
        {
            return RegexCheck(input, @"^(\d{18,18}|\d{15,15}|\d{17,17}x)$", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 验证字符串是否表示合法的身份证号
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns></returns>
        public static bool IsIDCardNo(this string input)
        {
            DateTime d;
            bool m;
            return IsIDCardNo(input, out d, out m);
        }

        /// <summary>
        /// 验证字符串是否表示合法的身份证号
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <param name="birthday">从合法的身份证号中获得的生日信息</param>
        /// <param name="isMale">从合法的身份证号中获得的性别信息，true为男性，false为女性</param>
        /// <returns></returns>
        public static bool IsIDCardNo(this string input, out DateTime birthday, out bool isMale)
        {
            birthday = default(DateTime);
            isMale = default(bool);
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }
            input = input.Trim().ToUpper();
            if (input.Length != 15 && input.Length != 18)
            {
                return false;
            }

            bool checkResult = CheckIDCard(input);
            if (!checkResult)
            {
                return false;
            }

            string yearStr;
            string monthStr;
            string dayStr;
            string sexStr;
            if (input.Length == 15) // 15位身份证
            {
                if (RegexCheck(input, @"^\d{15}$") == false)
                {
                    return false;
                }
                yearStr = "19" + input.Substring(6, 2);
                monthStr = input.Substring(8, 2);
                dayStr = input.Substring(10, 2);
                sexStr = input.Substring(14, 1);
            }
            else // 18位身份证
            {
                yearStr = input.Substring(6, 4);
                monthStr = input.Substring(10, 2);
                dayStr = input.Substring(12, 2);
                sexStr = input.Substring(16, 1);
            }

            int s;
            int.TryParse(sexStr, out s);
            isMale = (s % 2) != 0;
            DateTime.TryParse(yearStr + "-" + monthStr + "-" + dayStr, out birthday);

            return checkResult;
        }

        private static bool CheckIDCard(string Id)
        {
            Id = Id.Trim();
            if (Id.Length == 18)
            {
                bool check = CheckIDCard18(Id);
                return check;
            }
            else if (Id.Length == 15)
            {
                bool check = CheckIDCard15(Id);
                return check;
            }
            else
            {
                return false;
            }
        }

        private static bool CheckIDCard18(string Id)
        {
            long n = 0;
            if (long.TryParse(Id.Remove(17), out n) == false || n < Math.Pow(10, 16) || long.TryParse(Id.Replace('x', '0').Replace('X', '0'), out n) == false)
            {
                return false;//数字验证
            }
            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(Id.Remove(2)) == -1)
            {
                return false;//省份验证
            }
            string birth = Id.Substring(6, 8).Insert(6, "-").Insert(4, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证
            }

            string[] arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
            string[] Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
            char[] Ai = Id.Remove(17).ToCharArray();

            int sum = 0;

            for (int i = 0; i < 17; i++)
            {
                sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
            }

            int y = -1;
            Math.DivRem(sum, 11, out y);
            if (arrVarifyCode[y] != Id.Substring(17, 1).ToLower())
            {
                return false;//校验码验证
            }
            return true;//符合GB11643-1999标准
        }

        private static bool CheckIDCard15(string Id)
        {
            long n = 0;
            if (long.TryParse(Id, out n) == false || n < Math.Pow(10, 14))
            {
                return false;//数字验证
            }
            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(Id.Remove(2)) == -1)
            {
                return false;//省份验证
            }

            string birth = Id.Substring(6, 6).Insert(4, "-").Insert(2, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证
            }
            return true;//符合15位身份证标准
        }

        /// <summary>
        /// 验证字符串是否含有Html或Xml标签
        /// </summary>
        /// <param name="input">待验证的字符串</param>
        /// <returns></returns>
        public static bool IsHtml(this string input)
        {
            Regex regex = new Regex("<([^<>]*?)>");
            MatchCollection mc = regex.Matches(input);
            if (mc == null || mc.Count <= 0)
            {
                return false;
            }
            foreach (Match m in mc)
            {
                if (m == null || m.Success == false || m.Groups.Count < 2 || string.IsNullOrWhiteSpace(m.Groups[1].Value))
                {
                    continue;
                }
                string tag = m.Groups[1].Value.Trim();
                if (RegexCheck(input, @"<\s*\/\s*" + tag + @"\s*>", RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 字符串是否包含html标签(包括单标签类似于<br/>);
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsContainHtmlTag(this string str)
        {
            if (str == null)
                return false;

            Regex regex = new Regex(@"<[^>]+>");
            return regex.IsMatch(str);
        }

        #endregion

        #region Private Method

        private static readonly string[] ChineseNum = { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };

        private static string GetSmallMoney(string moneyValue)
        {
            var intMoney = Convert.ToInt32(moneyValue);
            if (intMoney == 0)
            {
                return "";
            }
            var strMoney = intMoney.ToString();
            int temp;
            var strBuf = new StringBuilder(10);
            if (strMoney.Length == 4)
            {
                temp = Convert.ToInt32(strMoney.Substring(0, 1));
                strMoney = strMoney.Substring(1, strMoney.Length - 1);
                strBuf.Append(ChineseNum[temp]);
                if (temp != 0)
                    strBuf.Append("仟");
            }
            if (strMoney.Length == 3)
            {
                temp = Convert.ToInt32(strMoney.Substring(0, 1));
                strMoney = strMoney.Substring(1, strMoney.Length - 1);
                strBuf.Append(ChineseNum[temp]);
                if (temp != 0)
                    strBuf.Append("佰");
            }
            if (strMoney.Length == 2)
            {
                temp = Convert.ToInt32(strMoney.Substring(0, 1));
                strMoney = strMoney.Substring(1, strMoney.Length - 1);
                strBuf.Append(ChineseNum[temp]);
                if (temp != 0)
                    strBuf.Append("拾");
            }
            if (strMoney.Length == 1)
            {
                temp = Convert.ToInt32(strMoney);
                strBuf.Append(ChineseNum[temp]);
            }
            return strBuf.ToString();
        }

        #endregion

        public static string ToChineseMoney(this decimal moneyValue)
        {
            var result = "";
            if (moneyValue == 0)
                return "零";

            if (moneyValue < 0)
            {
                moneyValue *= -1;
                result = "负";
            }
            var intMoney = Convert.ToInt32(moneyValue * 100);
            var strMoney = intMoney.ToString();
            var moneyLength = strMoney.Length;
            var strBuf = new StringBuilder(100);
            if (moneyLength > 14)
            {
                throw new Exception("Money Value Is Too Large");
            }

            //处理亿部分
            if (moneyLength > 10 && moneyLength <= 14)
            {
                strBuf.Append(GetSmallMoney(strMoney.Substring(0, strMoney.Length - 10)));
                strMoney = strMoney.Substring(strMoney.Length - 10, 10);
                strBuf.Append("亿");
            }

            //处理万部分
            if (moneyLength > 6)
            {
                strBuf.Append(GetSmallMoney(strMoney.Substring(0, strMoney.Length - 6)));
                strMoney = strMoney.Substring(strMoney.Length - 6, 6);
                strBuf.Append("万");
            }

            //处理元部分
            if (moneyLength > 2)
            {
                strBuf.Append(GetSmallMoney(strMoney.Substring(0, strMoney.Length - 2)));
                strMoney = strMoney.Substring(strMoney.Length - 2, 2);
                strBuf.Append("元");
            }

            //处理角、分处理分
            if (Convert.ToInt32(strMoney) == 0)
            {
                strBuf.Append("整");
            }
            else
            {
                if (moneyLength > 1)
                {
                    var intJiao = Convert.ToInt32(strMoney.Substring(0, 1));
                    strBuf.Append(ChineseNum[intJiao]);
                    if (intJiao != 0)
                    {
                        strBuf.Append("角");
                    }
                    strMoney = strMoney.Substring(1, 1);
                }

                var intFen = Convert.ToInt32(strMoney.Substring(0, 1));
                if (intFen != 0)
                {
                    strBuf.Append(ChineseNum[intFen]);
                    strBuf.Append("分");
                }
            }
            var temp = strBuf.ToString();
            while (temp.IndexOf("零零") != -1)
            {
                strBuf.Replace("零零", "零");
                temp = strBuf.ToString();
            }

            strBuf.Replace("零亿", "亿");
            strBuf.Replace("零万", "万");
            strBuf.Replace("亿万", "亿");

            return result + strBuf;
        }

        public static string ToChineseMoney(this decimal? moneyValue)
        {
            if (moneyValue == null)
            {
                return string.Empty;
            }
            return moneyValue.Value.ToChineseMoney();
        }
        public static string ToChineseMoneyFormat(this decimal? number)
        {
            if (number == null)
            {
                return string.Empty;
            }
            return number.Value.ToChineseMoneyFormat();
        }
        public static string ToChineseMoneyFormat(this decimal number)
        {
            string numberValue = number.ToString("F4"); // 数字金额
            var chineseValue = ""; // 转换后的汉字金额
            string CHNUM = "零壹贰叁肆伍陆柒捌玖"; // 汉字数字
            string UNI = "仟佰拾万仟佰拾亿仟佰拾万仟佰拾元"; // 对应单位
            var UNID = "角分";
            if (number == 0)
            {
                chineseValue = "零元整";
                return chineseValue;
            }
            //首尾的0去掉
            numberValue = numberValue.Trim('0');
            var arr = numberValue.Split('.');
            if (arr[0].Length > UNI.Length)
            {
                //alert("金额长度超长");
                return "";
            }

            //整数部分处理
            for (var i = 0; i < arr[0].Length; i++)
            {
                var unit = UNI[UNI.Length - arr[0].Length + i];
                var val = CHNUM[Convert.ToInt32(arr[0][i]) - 48];
                if (val == '零' && (unit == '万' || unit == '亿'))
                {
                    chineseValue = chineseValue + unit;
                }
                else if (val == '零')
                {
                    chineseValue = chineseValue + val;
                }
                else
                {
                    chineseValue = chineseValue + val + unit;
                }
            }

            //小数部分处理
            for (var i = 0; arr.Length > 1 && i < arr[1].Length; i++)
            {
                if (i > 1)
                    break;
                chineseValue += CHNUM[Convert.ToInt32(arr[1][i]) - 48] + UNID[i];
            }
            chineseValue = chineseValue.Trim('零');

            //去掉00
            while (chineseValue.IndexOf("零零") > 0)
            {
                chineseValue = chineseValue.Replace("零零", "零");
            }
            //去掉不规范的
            chineseValue = chineseValue.Replace("零亿", "亿");
            chineseValue = chineseValue.Replace("零万", "万");
            chineseValue = chineseValue.Replace("亿万", "亿");

            //只有整数
            if (arr.Length == 1 || arr[1].Length == 0)
            {
                if (!chineseValue.EndsWith("元"))
                    chineseValue += "元";
                chineseValue = chineseValue + "整";
            }
            return chineseValue;
        }

        #region 格式化为不带人民币符号的货币
        /// <summary>
        /// 格式化为人民币，不带人民币符号，精度为小数点后两位。
        /// </summary>
        /// <param name="price">需要格式化的值</param>
        /// <returns>格式化过的人民币。</returns>
        public static string FormatRMB(this decimal price)
        {
            return FormatRMB(price, 2);
        }

        /// <summary>
        /// 格式化为人民币，不带人民币符号，指定小数点后位数。
        /// </summary>
        /// <param name="price">需要格式化的值</param>
        /// <param name="bitCount">小数点后位数</param>
        /// <returns>格式化过的人民币。</returns>
        public static string FormatRMB(this decimal price, int bitCount)
        {
            string priceFormat = bitCount >= 0 ?
                    string.Format("f{0}", bitCount) : "f0";

            if (price < decimal.Zero)
            {
                return string.Format(@"-{0}", Math.Abs(price).ToString(priceFormat));
            }
            return price.ToString(priceFormat);
        }
        #endregion

        #region 格式化为带人民币符号的货币
        /// <summary>
        /// 格式化为人民币，带人民币符号，精度为小数点后两位。
        /// </summary>
        /// <param name="price">需要格式化的值</param>
        /// <returns>格式化过的人民币。</returns>
        public static string FormatRMBWithSign(this decimal price)
        {
            return FormatRMBWithSign(price, 2);
        }

        /// <summary>
        /// 格式化为人民币，带人民币符号，指定小数点后位数。
        /// </summary>
        /// <param name="price">需要格式化的值</param>
        /// <param name="bitCount">小数点后位数</param>
        /// <returns>格式化过的人民币。</returns>
        public static string FormatRMBWithSign(this decimal price, int bitCount)
        {
            string priceFormat = bitCount >= 0 ?
                    string.Format("f{0}", bitCount) : "f0";

            if (price < decimal.Zero)
            {
                return string.Format(@"&yen;-{0}", Math.Abs(price).ToString(priceFormat));
            }
            return string.Format(@"&yen;{0}", price.ToString(priceFormat));
        }
        #endregion

        #region 格式化为带人民币符号的货币，并且自定义人民币符号和价格的样式
        /// <summary>
        /// 格式化为人民币，自定义人民币符号和价格样式，精度为小数点后两位。
        /// </summary>
        /// <param name="price">需要格式化的值</param>
        /// <param name="html">
        /// 人民币符号和价格样式
        /// 样式格式：<![CDATA[<s class="ico_y">&yen;</s><span class="digi">{0}</span>]]>
        /// 需要将价格处设置为：{0}
        /// </param>
        /// <returns>格式化过的人民币。</returns>
        public static string FormatRMBWithSign(this decimal price, string html)
        {
            return FormatRMBWithSign(price, 2, html);
        }

        /// <summary>
        /// 格式化为人民币，自定义人民币符号和价格样式，指定小数点后位数
        /// </summary>
        /// <param name="price">需要格式化的值</param>
        /// <param name="bitCount">小数点后位数</param>
        /// <param name="html">
        /// 人民币符号和价格样式
        /// 样式格式：<![CDATA[<s class="ico_y">&yen;</s><span class="digi">{0}</span>]]>
        /// 需要将价格处设置为：{0}
        /// </param>
        /// <returns>格式化过的人民币。</returns>
        public static string FormatRMBWithSign(this decimal price, int bitCount, string html)
        {
            string priceFormat = bitCount >= 0 ?
                    string.Format("f{0}", bitCount) : "f0";

            if (price < decimal.Zero)
            {
                string result = string.Format(@"-{0}", Math.Abs(price).ToString(priceFormat));
                return string.Format(html, result);
            }
            return string.Format(html, price.ToString(priceFormat));
        }
        #endregion

        /// <summary>
        /// 截断字符串
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <param name="length">累计长度（中文，英文只占长度的1/2）</param>
        /// <param name="replaceChar">如果过长，替换字符</param>
        /// <returns></returns>
        public static string TruncateString(string input, int length, string replaceChar)
        {
            if (input.Length < length)
            {
                return input;
            }
            length = 2 * length;
            int strlen = System.Text.Encoding.Default.GetByteCount(input);
            int j = 0;//记录遍历的字节数
            int L = 0;//记录每次截取开始，遍历到开始的字节位，才开始记字节数
            int strW = 0;//字符宽度
            bool b = false;//当每次截取时，遍历到开始截取的位置才为true
            string restr = string.Empty;
            for (int i = 0; i < input.Length; i++)
            {
                char C = input[i];
                if ((int)C >= 0x4E00 && (int)C <= 0x9FA5)
                {
                    strW = 2;
                }
                else
                {
                    strW = 1;
                }
                if ((L == length - 1) && (L + strW > length))
                {
                    b = false;
                    break;
                }
                if (j >= 0)
                {
                    restr += C;
                    b = true;
                }

                j += strW;

                if (b)
                {
                    L += strW;
                    if (((L + 1) > length))
                    {
                        b = false;
                        break;
                    }
                }

            }
            return restr + replaceChar;
        }

    }
}
