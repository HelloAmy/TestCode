using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace YZ.Utility
{
    public static class XElementExtentions
    {
        public static string ToStringFixed(this XElement element, MatchEvaluator replacer = null)
        {
            FixXmlValue(element, replacer);

            return element.ToString();
        }

        public static string ToStringFixed(this string value, MatchEvaluator replacer = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
            return ToXmlString(value, replacer);
        }

        #region private methods

        static Lazy<Regex> RegexControlChars = new Lazy<Regex>(() => new Regex("[\x00-\x1f]", RegexOptions.Compiled));

        public static string ReplaceControlChars(Match match)
        {
            if ((match.Value.Equals("\t")) || (match.Value.Equals("\n")) || (match.Value.Equals("\r")))
                return match.Value;

            return string.Empty;
        }

        public static string EncodeControlChars(Match match)
        {
            if ((match.Value.Equals("\t")) || (match.Value.Equals("\n")) || (match.Value.Equals("\r")))
                return match.Value;

            return "&#" + ((int)match.Value[0]).ToString("X4") + ";";
        }

        private static string ToXmlString(string data, MatchEvaluator replacer = null)
        {
            if (data == null) return null;
            string fixedData;
            if (replacer != null)
            {
                fixedData = RegexControlChars.Value.Replace(data.ToString(), replacer);
            }
            else
            {
                fixedData = RegexControlChars.Value.Replace(data.ToString(), ReplaceControlChars);
            }
            return fixedData;
        }

        private static void FixXmlValue(XElement element, MatchEvaluator replacer = null)
        {
            if (element.HasElements)
            {
                foreach (var child in element.Elements())
                {
                    FixXmlValue(child);
                }
            }
            else
            {
                element.Value = ToXmlString(element.Value, replacer);
            }
        }

        #endregion
    }
}
