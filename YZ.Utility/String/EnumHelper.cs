using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;

namespace YZ.Utility
{
    public static class EnumHelper
    {
        #region Private Member

        private static Dictionary<Type, Dictionary<object, string>> s_Cache = new Dictionary<Type, Dictionary<object, string>>();
        private static object s_SyncObj = new object();

        private static Dictionary<object, string> GetDescriptions(Type enumType)
        {
            if (!enumType.IsEnum && !IsGenericEnum(enumType)) // enumType既不是enum也不是enum?
            {
                throw new ApplicationException("The generic type 'TEnum' must be enum or Nullable<enum>.");
            }
            enumType = GetRealEnum(enumType);
            Dictionary<object, string> rst;
            if (s_Cache.TryGetValue(enumType, out rst))
            {
                return rst;
            }
            lock (s_SyncObj)
            {
                if (s_Cache.TryGetValue(enumType, out rst))
                {
                    return rst;
                }
                FieldInfo[] fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
                Dictionary<object, string> map = new Dictionary<object, string>(fields.Length * 2);
                foreach (FieldInfo field in fields)
                {
                    object[] displayAttr = field.GetCustomAttributes(typeof(DisplayAttribute), false);
                    if (displayAttr != null && displayAttr.Length > 0 && !(displayAttr[0] as DisplayAttribute).Display)
                    {
                        continue;
                    }
                    object[] objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    string name = string.Empty;
                    if (objs != null && objs.Length > 0)
                    {
                        DescriptionAttribute a = objs[0] as DescriptionAttribute;
                        if (a != null && a.Description != null)
                        {
                            name = a.Description;
                        }
                    }
                    object key = field.GetValue(null);
                    map.Add(key, name);
                }
                s_Cache.Add(enumType, map);
                return map;
            }
        }

        private static string GetDescription(object enumValue, Type enumType)
        {
            var dic = GetDescriptions(enumType);
            string tmp;
            if (dic.TryGetValue(enumValue, out tmp) && tmp != null)
            {
                return tmp;
            }
            return string.Empty;
        }

        private static bool IsGenericEnum(Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                    && type.GetGenericArguments() != null
                    && type.GetGenericArguments().Length == 1 && type.GetGenericArguments()[0].IsEnum);
        }

        private static Type GetRealEnum(Type type)
        {
            Type t = type;
            while (IsGenericEnum(t))
            {
                t = type.GetGenericArguments()[0];
            }
            return t;
        }

        #endregion


        /// <summary>
        /// 获取枚举值的描述
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum value)
        {
            return value == null ? string.Empty : GetDescription(value, value.GetType());
        }

        /// <summary>
        /// 获取枚举内容项，并以KeyValuePair列表的方式返回；其中，Key=枚举的值，Value=枚举的描述
        /// </summary>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        /// <param name="appendType">选择是否在列表项前插入一条选项的选项类型，比如是否插入所有|请选择等Key为Null的选项</param>
        /// <param name="customApplyDesc">如果要在列表项前插入一条选项，该选项的自定义描述</param>
        /// <returns></returns>
        public static List<KeyValuePair<Nullable<TEnum>, string>> GetKeyValuePairs<TEnum>(EnumAppendItemType appendType, params string[] customApplyDesc) where TEnum : struct
        {
            List<KeyValuePair<Nullable<TEnum>, string>> keyValuePairList = new List<KeyValuePair<Nullable<TEnum>, string>>();
            Type enumType = typeof(TEnum);
            if (enumType.IsEnum || IsGenericEnum(enumType))
            {
                Dictionary<TEnum, string> dic = EnumHelper.GetDescriptions<TEnum>();
                if (dic != null && dic.Count > 0)
                {
                    foreach (TEnum e in dic.Keys)
                    {
                        keyValuePairList.Add(new KeyValuePair<Nullable<TEnum>, string>(e, dic[e]));
                    }
                }
                if (appendType != EnumAppendItemType.None)
                {
                    if (customApplyDesc != null && customApplyDesc.Length > 0 && !string.IsNullOrEmpty(customApplyDesc[0]))
                    {
                        KeyValuePair<Nullable<TEnum>, string> kv = new KeyValuePair<Nullable<TEnum>, string>(null, customApplyDesc[0]);
                        keyValuePairList.Insert(0, kv);
                    }
                    else
                    {
                        string desc = GetDescription(appendType);
                        if (!string.IsNullOrEmpty(desc))
                        {
                            KeyValuePair<Nullable<TEnum>, string> kv = new KeyValuePair<Nullable<TEnum>, string>(null, desc);
                            keyValuePairList.Insert(0, kv);
                        }
                    }
                }
            }

            return keyValuePairList;
        }

        public static List<dynamic> GetKeyValuePairs2<TEnum>()
        {
            List<dynamic> result = new List<object>();
            Type enumType = typeof(TEnum);
            if (enumType.IsEnum || IsGenericEnum(enumType))
            {
                foreach (var value in Enum.GetValues(enumType))
                {
                    result.Add(new
                    {
                        value = (int)value,
                        text = GetDescription(value, enumType)
                    });
                }
            }
            return result;
        }

        /// <summary>
        /// 获取枚举类型下所有枚举值的描述集合
        /// </summary>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        /// <returns>枚举值与其描述的对应关系的集合，Key为枚举值，Value为其对应的描述</returns>
        public static Dictionary<TEnum, string> GetDescriptions<TEnum>() where TEnum : struct
        {
            Dictionary<object, string> dic = GetDescriptions(typeof(TEnum));
            Dictionary<TEnum, string> newDic = new Dictionary<TEnum, string>(dic.Count * 2);
            foreach (var entry in dic)
            {
                newDic.Add((TEnum)entry.Key, entry.Value);
            }
            return newDic;
        }
    }

    public enum EnumAppendItemType
    {
        None,
        /// <summary>
        /// 默认“所有”项
        /// </summary>
        [Description("--所有--")]
        All,
        /// <summary>
        /// 默认“请选择”项
        /// </summary>
        [Description("--请选择--")]
        Select
    }


    public class DisplayAttribute : Attribute
    {
        private bool display;

        public bool Display
        {
            get { return display; }
            set { display = value; }
        }

        public DisplayAttribute(bool display)
        {
            this.display = display;
        }
    }
}
