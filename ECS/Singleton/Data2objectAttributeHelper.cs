using Entity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Singleton
{
    public class Data2objectAttributeHelper : Singleton<Data2objectAttributeHelper>
    {
        /// <summary>
        /// 数据类型 2 转换函数
        /// </summary>
        ConcurrentDictionary<Type, Func<string, object>> type2method = new ConcurrentDictionary<Type, Func<string, object>>();
        
        public string DefaultMethod(string str)
        {
            return str;
        }

        /// <summary>
        ///  Func<string, object> func = Delegate.CreateDelegate(typeof(Func<string, object>), method) as Func<string, object>;
        ///  必须是静态的
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        [Data2objectAttribute(TarType = typeof(int))]
        public static object string2intMethod(string str)
        {
            if (!int.TryParse(str, out int ret))
            {
                return -1;
            }

            return ret;
        }

        [Data2objectAttribute(TarType = typeof(List<int>))]
        public static object string2Listint(string str)
        {
            return Global.Global.Parse2Listint(str);
        }
        [Data2objectAttribute(TarType = typeof(PlayerInfo))]
        public static object string2PlayerInfo(string str)
        {
            return Global.Global.Parse2playerinfo(str);
        }

        /// <summary>
        /// 获取将string转化为对应数据类型的方法
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Func<string, object> GetData2Method(Type type)
        {
            if (!type2method.TryGetValue(type, out Func<string, object> func))
            {
                return DefaultMethod;
            }
            return func;
        }

        public void Init()
        {
            List<Type> types = Global.Global.GetAllTypes();

            #region 获取将string转化为对应数据类型的方法

            foreach (Type type in types)
            {
                var classAtts = type.GetMethods();

                foreach (var method in classAtts)
                {
                    var attr = method.GetCustomAttribute<Data2objectAttribute>();

                    if (null == attr)
                    {
                        continue;
                    }

                    method.CreateDelegate(typeof(Func<string, object>));
                    Func<string, object> func = Delegate.CreateDelegate(typeof(Func<string, object>), method) as Func<string, object>;

                    if (null == func)
                    {
                        LoggerHelper.Instance().Log(LogType.Test, $"type:{type} method:{method} CreateDelegate failed");
                        continue;
                    }

                    if (!type2method.TryAdd(attr.TarType, func))
                    {
                        string msg = $"type2tableinfo.TryAdd type:{type} method:{method} CreateDelegate failed";
                        LoggerHelper.Instance().Log(LogType.Test, msg);
                        throw new Exception(msg);
                    }
                }
            }

            #endregion 获取将string转化为对应数据类型的方法

            

        }
    }
}
