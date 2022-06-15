using System;
using System.Collections.Generic;
using System.Text;

namespace Entity
{
    public class Data2objectAttribute : Attribute
    {
        public Type TarType { get; set; } = typeof(string);
    }
}
