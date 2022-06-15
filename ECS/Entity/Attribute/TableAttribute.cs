using System;
using System.Collections.Generic;
using System.Text;

namespace Entity
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class MyTableAttribute : Attribute
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public Type type { get; set; } = typeof(string);
        public bool IsPrimaryKey { get; set; } = false;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class DataAttribute : Attribute
    {
        public string ColumnName { get; set; }
        public Type type { get; set; } = typeof(string);
        public bool IsPrimaryKey { get; set; } = false;
        public bool IsBaseType { get; set; } = true;
    }

}
