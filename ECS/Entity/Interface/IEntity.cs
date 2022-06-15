using System;
using System.Collections.Generic;

namespace Entity
{
    /// <summary>
    /// 每个实体(Entity)有多个组件(Component)
    /// </summary>
    public interface IEntity
    {
        string ID { get; set; }
        string mOwner { get; set; }
        Dictionary<Type, IComponent> mComponents { get; }
        void AddComponent(Type type, IComponent component);
        void AddComponent(IComponent component);
        IComponent GetComponent<T>();
    }
}
