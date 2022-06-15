using System;
using System.Collections.Generic;

namespace Entity
{
    /// <summary>
    /// 当做服务器与客户端连接的实体
    /// </summary>
    public class SocketEntity : IEntity
    {
        public Dictionary<Type, IComponent> mComponents { get; set; } = new Dictionary<Type, IComponent>();

        public string mOwner { get; set; }
        public string ID { get; set; }

        public void AddComponent(Type type, IComponent component)
        {
            if (type != component.GetType())
            {
                Console.WriteLine($"SocketEntity AddComponent mOwner:{mOwner} type:{type} != {component.GetType()}");
                return;
            }

            bool res = mComponents.TryAdd(type, component);
            Console.WriteLine($"SocketEntity AddComponent mOwner:{mOwner} type:{type} result:{res}");
        }

        public void AddComponent(IComponent component)
        {
            Type type = component.GetType();

            bool res = mComponents.TryAdd(type, component);
            Console.WriteLine($"SocketEntity AddComponent mOwner:{mOwner} type:{type} result:{res}");
        }

        public IComponent GetComponent<T>()
        {
            Type type = typeof(T);
            mComponents.TryGetValue(type, out IComponent component);

            return component;
        }
    }
}
