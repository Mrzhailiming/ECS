using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Entity
{
    public class TableKey
    {
        public object key { get; set; }
    }
    public class PlayerInfo
    {
        [DataAttribute(ColumnName = "Id", type = typeof(int), IsPrimaryKey = false, IsBaseType = true)]
        public int Id { get; set; }
    }
    public class TablePropetyInfos
    {
        public string tableName;
        /// <summary>
        /// 属性名称 2 PropertyInfo
        /// </summary>
        public ConcurrentDictionary<string, TablePropertyInfo> column2fieldinfo = new ConcurrentDictionary<string, TablePropertyInfo>();
    }
    public class TablePropertyInfo
    {
        public bool IsBaseType { get; set; } = true;
        public PropertyInfo propetyInfo { get; set; }
        /// <summary>
        /// 属性名称 2 PropertyInfo 不是基类型 这个才有值, 否则用上面那个
        /// </summary>
        public ConcurrentDictionary<string, TablePropertyInfo> column2propertiesinfo = new ConcurrentDictionary<string, TablePropertyInfo>();
    }
    public class TableInfo
    {
        public string tableName { get; set; }
        public TableKey TableKey { get; set; }

        public ConcurrentDictionary<string, MyFieldInfo> column2fieldinfo = new ConcurrentDictionary<string, MyFieldInfo>();
    }

    public class MyFieldInfo
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public Type type { get; set; } = typeof(string);
        public bool IsPrimaryKey { get; set; } = false;
        public bool IsBaseType { get; set; } = false;
        /// <summary>
        /// 字段类型不是基本类型, 需要在增加
        /// </summary>
        public ConcurrentDictionary<string, MyFieldInfo> name2fieldinfo { get; set; } = new ConcurrentDictionary<string, MyFieldInfo>();

        new public string ToString()
        {
            return $"{TableName} {ColumnName} {type} {IsPrimaryKey}";
        }
    }

    public enum CmdHandlerType
    {
        Both,
        Client,
        Server,
    }
    public enum ThreadMode
    {
        LogIn,
        Normal,
        Battle,
        IO,
    }

    public enum ConnectType
    {
        Send,
        Recv,
    }

    public class Proto
    {
        /// <summary>
        /// 0-4 存放cmd指令
        /// </summary>
        public const int cmdIDOffset = 0; // 4
        /// <summary>
        /// 4-8 存放包长度
        /// </summary>
        public const int PacketLenOffset = 4; // 4
        public const int BodyOffset = 4; // 4
        public const int fileNameLengthOffset = 8; // 4
        public const int fileTotalLengthOffset = 12; // 4
        public const int fileKeyOffset = 16; // 8
        public const int fileNameOffset = 24; // 

        public const int protoHeadLen = 8;
        /// <summary>
        /// 文件名的偏移
        /// </summary>
        public const int sendOffset = fileNameOffset;
    }

    public enum TCPCMDS
    {
        LOGIN,
        UPLOAD,
        DOWNLOAD,
        TEST,
        HeartBeat,
        Move,
        Attack,
    }

}
