using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuTaoCore.Reflect
{
    public static class ReflectGetClass
    {
        /// <summary>
        /// 反射获取类的属性和值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetClassNameValues<T>(T t) where T : class
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            foreach (System.Reflection.PropertyInfo info in t.GetType().GetProperties())
            {
                object[] o = info.GetCustomAttributes(typeof(NoReflecAttribute), true);
                if (o.Length > 0) continue;
                keyValuePairs.Add(info.Name, t.GetType().GetProperty(info.Name).GetValue(t, null).ToString());
            }
            return keyValuePairs;
        }

        /// <summary>
        /// 通过反射给对象的属性赋值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="keyValuePairs"></param>
        public static void SetClassValues<T>(T t, Dictionary<string, string> keyValuePairs) where T : class
        {
            Type type = t.GetType(); //获取类型
            string[] strings = keyValuePairs.Keys.ToArray();
            foreach (var item in strings)
            {
                System.Reflection.PropertyInfo propertyInfo = type.GetProperty(item); //获取指定名称的属性
                string str = propertyInfo.PropertyType.Name;
                propertyInfo.SetValue(t, Type_Conversion(str, keyValuePairs[item])); //给对应属性赋值
            }
        }

        /// <summary>
        /// 类型转化
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        static object Type_Conversion(string type, string value)
        {
            try
            {
                switch (type)
                {
                    case "Int32":
                        return Convert.ToInt32(value);
                    case "String":
                        return value;
                    case "Double":
                        return Convert.ToDouble(value);
                    default: return value;
                }
            }
            catch (Exception)
            {
                return value;
            }
        }
    }
    internal class NoReflecAttribute : Attribute
    {
        public bool Att;
        public NoReflecAttribute(bool b)
        {
            this.Att = b;
        }
    }
}
