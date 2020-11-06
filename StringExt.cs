using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HttpProxy
{
    public static class StringExt
    {
        /// <summary>
        /// 如果是空字符串则用对应的字符代替
        /// </summary>
        /// <param name="str"></param>
        /// <param name="insted"></param>
        /// <returns></returns>
        public static string EmptyInsted(this string str, string insted = "-")
        {
            if (string.IsNullOrEmpty(str))
            {
                return insted;
            }

            return str;
        }

        public static bool IsNullOrEmpty(this String str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsNullOrWhitespace(this String str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static string Formater(this string str, params object[] args)
        {
            return string.Format(str, args);
        }

        public static DateTime? ToDate(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            if (DateTime.TryParse(str, out DateTime date))
            {
                return date;
            }

            return null;
        }

        public static string ToJson(this object obj)
        {
            if (obj == null)
            {
                return null;
            }
            return JsonConvert.SerializeObject(obj);
        }

        public static string RemoveBlankCharFromOracleClob(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            var arr = source.ToArray();
            var newstr = new string(arr.Where(s => s != '\0').ToArray());

            return newstr;
        }
    }
}