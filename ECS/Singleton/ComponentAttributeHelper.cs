using Entity;
using Entity.DataStruct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;

namespace Singleton
{
    public class ComponentAttributeHelper : Singleton<ComponentAttributeHelper>
    {
        public Dictionary<Type, ComponentAttribute> mType2Attr = new Dictionary<Type, ComponentAttribute>();
        public Dictionary<InitComponentType, Action<object[]>> mType2InitMethod = new Dictionary<InitComponentType, Action<object[]>>();

        public ComponentAttributeHelper()
        {
        }

        public void Init()
        {
            string path = Directory.GetCurrentDirectory();

            ComponentTypes();
            InitComponent(path);
            Check();
        }

        /// <summary>
        /// 为实体增加必备的组件,添加 base 和 componentType 对应的组价
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="componentType"></param>
        /// <param name="sendArg"></param>
        /// <param name="recvArg"></param>
        public void AddAllComponents(IEntity entity, ComponentType componentType, SocketAsyncEventArgs sendArg, SocketAsyncEventArgs recvArg)
        {
            foreach (var pair in mType2Attr)
            {
                Type cmpType = pair.Key;
                ComponentAttribute cmpAttr = pair.Value;

                if (cmpAttr.mComponentType != ComponentType.Base
                    && cmpAttr.mComponentType != componentType)
                {
                    continue;
                }

                if (!mType2InitMethod.TryGetValue(cmpAttr.mInitComponentType, out Action<object[]> initMethod))
                {
                    LoggerHelper.Instance().Log(LogType.Fatal, $"type:{cmpType} initMethod null");
                    continue;
                }

                object[] objects = new object[] { entity, sendArg, recvArg };

                initMethod.Invoke(objects);
            }
        }



        /// <summary>
        /// 两个特性  要匹配
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void Check()
        {
            foreach (var pair in mType2Attr)
            {
                Type cmpType = pair.Key;
                ComponentAttribute cmpAttr = pair.Value;

                if (!mType2InitMethod.TryGetValue(cmpAttr.mInitComponentType, out Action<object[]> initMethod))
                {
                    LoggerHelper.Instance().Log(LogType.Fatal, $"type:{cmpType} initMethod null");
                    throw new Exception($"type:{cmpType} initMethod cant be null");
                }
            }

        }

        /// <summary>
        /// ComponentAttribute
        /// </summary>
        public void ComponentTypes()
        {
            // 获取所有程序集
            var ass = AppDomain.CurrentDomain.GetAssemblies();

            if (null == ass)
            {
                return;
            }

            // 获取所有 ComponentAttribute 的类
            foreach (var assembly in ass)
            {
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    var attr = type.GetCustomAttribute(typeof(ComponentAttribute)) as ComponentAttribute;

                    if (null == attr)
                    {
                        continue;
                    }

                    mType2Attr.Add(type, attr);
                }
            }
        }

        /// <summary>
        /// InitComponentAttribute
        /// </summary>
        public void InitComponent(string path)
        {
            // 获取所有程序集
            var ass = LoadAllAssembly(path);

            if (null == ass)
            {
                return;
            }

            // 获取所有 ComponentAttribute 的类
            foreach (var assembly in ass)
            {
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                    foreach (var method in methods)
                    {
                        var att = method.GetCustomAttribute(typeof(InitComponentAttribute)) as InitComponentAttribute;

                        if (null == att)
                        {
                            continue;
                        }

                        var action = Delegate.CreateDelegate(typeof(Action<object[]>), method) as Action<object[]>;

                        mType2InitMethod.Add(att.mInitComponentType, action);
                    }
                }
            }
        }

        public List<Assembly> LoadAllAssembly(string path)
        {
            List<Assembly> ret = new List<Assembly>();

            DirectoryInfo root = new DirectoryInfo(path);
            FileInfo[] files = root.GetFiles();

            foreach (FileInfo file in files)
            {
                if (!file.Name.Contains(".dll"))
                {
                    continue;
                }

                ret.Add(Assembly.LoadFile(file.FullName));
            }

            return ret;
        }

    }
}
