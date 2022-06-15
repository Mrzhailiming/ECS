using System;
using System.Collections.Generic;

namespace Entity
{
    public class TestEntity : IEntity
    {
        public Dictionary<Type, IComponent> mComponents { get; set; } = new Dictionary<Type, IComponent>();
        public string ID { get; set; }

        public string mOwner { get; set; }

        public void AddComponent(Type type, IComponent component)
        {
            if (type != component.GetType())
            {
                Console.WriteLine($"SocketEntity AddComponent mOwner:{mOwner} type:{type} != {component.GetType()}");
                return;
            }

            bool res = mComponents.TryAdd(type, component);
            Console.WriteLine($"ConnectionEntity AddComponent mOwner:{mOwner} type:{type} result:{res}");

        }

        public void AddComponent(IComponent component)
        {
            Type type = component.GetType().BaseType;

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
