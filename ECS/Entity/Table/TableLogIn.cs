using Entity.Interface;
using System.Collections.Generic;

namespace Entity.Table
{
    [MyTableAttribute(TableName = "login")]
    public class TableLogIn : ITable
    {
        [DataAttribute(ColumnName = "account", type = typeof(string), IsPrimaryKey = true)]
        public string account { get; set; }

        [DataAttribute(ColumnName = "pwd", type = typeof(int), IsPrimaryKey = false)]
        public int pwd { get; set; }
        [DataAttribute(ColumnName = "li", type = typeof(List<int>), IsPrimaryKey = false)]
        public List<int> li { get; set; }
        [DataAttribute(ColumnName = "playerInfo", type = typeof(PlayerInfo), IsPrimaryKey = false, IsBaseType = false)]
        public PlayerInfo playerInfo { get; set; }
        
    }
}
