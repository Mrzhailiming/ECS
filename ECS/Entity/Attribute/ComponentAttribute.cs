using Entity.DataStruct;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entity
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ComponentAttribute : Attribute
    {
        public ComponentType mComponentType { get; set; }
        public InitComponentType mInitComponentType { get; set; }
    }
}
