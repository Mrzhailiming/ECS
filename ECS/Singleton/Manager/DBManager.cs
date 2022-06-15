using Entity;
using Entity.Interface;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Singleton.Manager
{
    public class DBManager : Singleton<DBManager>
    {

        ConcurrentQueue<MySqlConnection> connectionsQueue = new ConcurrentQueue<MySqlConnection>();
        ConcurrentDictionary<string, ConcurrentDictionary<TableKey, List<ITable>>> tablename2cache = new ConcurrentDictionary<string, ConcurrentDictionary<TableKey, List<ITable>>>();
        public void Init(string ip, int port, string uname, string pwd, string dbname, int conNum)
        {
            string constr = $"server={ip};port={port};user={uname};password={pwd};database={dbname};";

            for (int i = 0; i < conNum; ++i)
            {
                MySqlConnection connection = Connect(constr);
                if (null == connection)
                {
                    --i;
                    continue;
                }

                connectionsQueue.Enqueue(connection);
            }
        }
        public MySqlConnection Pop()
        {
            if (!connectionsQueue.TryDequeue(out MySqlConnection connection))
            {
                LoggerHelper.Instance().Log(LogType.Test, $"connectionsQueue.TryDequeue false");
                return null;
            }

            if (connection.State != ConnectionState.Open)
            {
                LoggerHelper.Instance().Log(LogType.Test, $"connectionsQueue.TryDequeue false State:{connection.State}");
                connection.CloseAsync();
                return null;
            }

            return connection;
        }

        public void Push(MySqlConnection connection)
        {
            if (null == connection)
            {
                return;
            }
            connectionsQueue.Enqueue(connection);
        }

        public MySqlConnection Connect(string constr)
        {
            try
            {
                MySqlConnection conn = new MySqlConnection(constr);
                conn.Open();

                if (conn.State != ConnectionState.Open)
                {
                    return null;
                }

                return conn;
            }
            catch (MySqlException sqlex)
            {
                switch (sqlex.Number)
                {
                    case 0:
                        LoggerHelper.Instance().Log(LogType.Test, $"Cannot connect to server. {sqlex.Message}");
                        break;
                    case 1045:
                        LoggerHelper.Instance().Log(LogType.Test, $"Invalid username/password, please try again {sqlex.Message}");
                        break;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance().Log(LogType.Test, ex.ToString());
            }

            return null;
        }

        public List<ITable> Select<T>(string str, TableInfo tableInfo, TableKey key) where T : ITable, new()
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            List<ITable> ret = null;

            var myConnnect = Pop();
            MySqlCommand myCmd = null;
            MySqlDataReader reader = null;
            try
            {
                myCmd = new MySqlCommand(str, myConnnect);
                reader = myCmd.ExecuteReader();

                ret = MakeTable<T>(reader, tableInfo, key);
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance().Log(LogType.Test, $"Select {ex}");
            }
            finally
            {
                myCmd?.Dispose();
                reader?.Dispose();
                Push(myConnnect);
            }

            return ret;
        }

        public List<ITable> Load<T>(IEntity entity) where T : ITable, new()
        {
            TableInfo tableInfo = TableAttributeHelper.Instance().GetTableInfo(typeof(T));
            TableKey key = new TableKey() { key = entity.ID };
            if(!tablename2cache.TryGetValue(tableInfo.tableName, out var valuePairs))
            {
                return Select<T>(MakeSelectStr(tableInfo, MakeLimitStr(entity)), tableInfo, key);
            }

            if (!valuePairs.TryGetValue(key, out var values))
            {
                return Select<T>(MakeSelectStr(tableInfo, MakeLimitStr(entity)), tableInfo, key);
            }

            return values;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="tableInfo"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<ITable> MakeTable<T>(MySqlDataReader reader, TableInfo tableInfo, TableKey key) where T : ITable, new()
        {
            List<ITable> ret = new List<ITable>();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    T table = new T();

                    // 缓存
                    var properties = TableAttributeHelper.Instance().GetPropertiesNew(typeof(T));

                    if (properties.column2fieldinfo.Count != tableInfo.column2fieldinfo.Count)
                    {
                        string msg = $"MakeTable type:{typeof(T)} tableInfo:{tableInfo.tableName} properties  column2fieldinfo count not match";
                        LoggerHelper.Instance().Log(LogType.Test, msg);
                        throw new Exception(msg);
                    }

                    foreach (MyFieldInfo fieldInfo in tableInfo.column2fieldinfo.Values)
                    {
                        string data = reader[fieldInfo.ColumnName].ToString();

                        // 如果没有, 抛异常
                        if (!properties.column2fieldinfo.TryGetValue(fieldInfo.ColumnName, out var prope))
                        {
                            string msg = $"MakeTable type:{typeof(T)} fieldInfo.ColumnName:{fieldInfo.ColumnName} property = null";
                            LoggerHelper.Instance().Log(LogType.Test, msg);
                            throw new Exception(msg);
                        }
                        object value = null;
                        if (!fieldInfo.IsBaseType)
                        {
                            value = Activator.CreateInstance(fieldInfo.type);
                            Ex(value, fieldInfo.name2fieldinfo, data, prope.column2propertiesinfo);
                        }
                        else
                        {
                            Func<string, object> func = Data2objectAttributeHelper.Instance().GetData2Method(fieldInfo.type);

                            // 出现异常怎么办呢, 抛异常, 由上层捕获, 并返回 null
                            value = func(data);
                        }

                        prope.propetyInfo.SetValue(table, value);
                    }

                    ret.Add(table);
                }

                if (!tablename2cache.TryGetValue(tableInfo.tableName, out var key2list))
                {
                    key2list = new ConcurrentDictionary<TableKey, List<ITable>>();
                    tablename2cache.TryAdd(tableInfo.tableName, key2list);
                }

                if (!key2list.TryGetValue(key, out var li))
                {
                    li = new List<ITable>();
                    key2list.TryAdd(key, li);
                }

                li.AddRange(ret);
            }

            return ret;
        }

        public void Ex(object o, ConcurrentDictionary<string, MyFieldInfo> name2fieldinfo, string data, ConcurrentDictionary<string, TablePropertyInfo> column2fieldinfo)
        {
            foreach (MyFieldInfo fieldInfo in name2fieldinfo.Values)
            {
                if (!column2fieldinfo.TryGetValue(fieldInfo.ColumnName, out TablePropertyInfo proinfo))
                {
                    LogAndThowEx($"TableName:{fieldInfo.TableName} ColumnName:{fieldInfo.ColumnName} has no TablePropertyInfo");
                }

                if (!fieldInfo.IsBaseType)
                {
                    object o2 = Activator.CreateInstance(fieldInfo.type);
                    // 递归
                    Ex(o2, fieldInfo.name2fieldinfo, data, proinfo.column2propertiesinfo);
                }

                Func<string, object> func = Data2objectAttributeHelper.Instance().GetData2Method(fieldInfo.type);

                // 出现异常怎么办呢, 抛异常, 由上层捕获, 并返回 null
                object value = func(data);

                proinfo.propetyInfo.SetValue(o, value);
            }
        }
        public string MakeLimitStr(IEntity entity)
        {
            return $"where `account`='{entity.mOwner}'";
        }
        public string MakeSelectStr(TableInfo tableInfo, string limit = "")
        {
            try
            {
                StringBuilder builder = new StringBuilder("select ");
                int i = 0;
                foreach (var fieldInfo in tableInfo.column2fieldinfo.Values)
                {
                    if (0 != i)
                    {
                        builder.Append(',');
                    }

                    builder.Append($"`{fieldInfo.ColumnName}` ");
                    ++i;
                }

                builder.Append($"from `{tableInfo.tableName}` {limit};");

                return builder.ToString();

            }
            catch (Exception ex)
            {
                LoggerHelper.Instance().Log(LogType.Test, $"MakeSelectStr {ex} tableInfo:{tableInfo.tableName}");
                return "";
            }

        }

        public void LogAndThowEx(string msg)
        {
            LoggerHelper.Instance().Log(LogType.Test, msg);
            throw new Exception(msg);
        }
    }
}
