using Entity.DataStruct;
using System;

namespace Entity
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class InitComponentAttribute : Attribute
    {
        public InitComponentType mInitComponentType { get; set; }
    }
}
