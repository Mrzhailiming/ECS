using Entity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Singleton
{
    public class TableAttributeHelper : Singleton<TableAttributeHelper>
    {
        /// <summary>
        /// 表名 2 表信息
        /// </summary>
        ConcurrentDictionary<string, TableInfo> name2tableinfo = new ConcurrentDictionary<string, TableInfo>();
        /// <summary>
        /// 表类型 2 表信息
        /// </summary>
        ConcurrentDictionary<Type, TableInfo> type2tableinfo = new ConcurrentDictionary<Type, TableInfo>();
        /// <summary>
        /// table类 2 (name 2 property) 基础类型 废弃
        /// </summary>
        ConcurrentDictionary<Type, ConcurrentDictionary<string, PropertyInfo>> itable2properties = new ConcurrentDictionary<Type, ConcurrentDictionary<string, PropertyInfo>>();
        ConcurrentDictionary<Type, TablePropetyInfos> itable2propertiesnew = new ConcurrentDictionary<Type, TablePropetyInfos>();

        public ConcurrentDictionary<string, PropertyInfo> GetProperties(Type type)
        {
            itable2properties.TryGetValue(type, out var properties);
            return properties;
        }
        public TablePropetyInfos GetPropertiesNew(Type tableType)
        {
            itable2propertiesnew.TryGetValue(tableType, out var properties);
            return properties;
        }
        public TableInfo GetTableInfo(Type type)
        {
            type2tableinfo.TryGetValue(type, out TableInfo tableInfo);
            return tableInfo;
        }

        public void Init()
        {
            List<Type> types = Global.Global.GetAllTypes();

            Initname2tableinfo(types);
            Inititable2properties(types);
            Check();
        }
        public void Check()
        {
            foreach (var pair in type2tableinfo)
            {
                Type type = pair.Key;
                TableInfo tableinfo = pair.Value;

                if (!itable2propertiesnew.TryGetValue(type, out var name2properties))
                {
                    LogAndThrowEx($"type:{type} not in itable2properties");
                }

                if (tableinfo.column2fieldinfo.Count != name2properties.column2fieldinfo.Count)
                {
                    LogAndThrowEx($"type:{type} column2fieldinfo name2properties count not match");
                }

                foreach (MyFieldInfo fieldinfo in tableinfo.column2fieldinfo.Values)
                {
                    if (!name2properties.column2fieldinfo.TryGetValue(fieldinfo.ColumnName, out TablePropertyInfo tablePropertyInfo))
                    {
                        LogAndThrowEx($"type:{type} not in itable2properties");
                    }

                    if (tablePropertyInfo.IsBaseType != fieldinfo.IsBaseType)
                    {
                        LogAndThrowEx($"type:{type} {fieldinfo.TableName} {fieldinfo.ColumnName} IsBaseType not match" +
                            $"{tablePropertyInfo.IsBaseType} != {fieldinfo.IsBaseType}");
                    }

                    if (!fieldinfo.IsBaseType)
                    {
                        CheckEx(tableinfo, fieldinfo.name2fieldinfo, tablePropertyInfo.column2propertiesinfo);
                    }
                }
            }
        }

        public void CheckEx(TableInfo tableinfo, ConcurrentDictionary<string, MyFieldInfo> name2fieldinfo, ConcurrentDictionary<string, TablePropertyInfo> name2properties)
        {

            if (name2fieldinfo.Count != name2properties.Count)
            {
                LogAndThrowEx($"TableName:{tableinfo.tableName} column2fieldinfo name2properties count not match");
            }

            foreach (MyFieldInfo fieldinfo in name2fieldinfo.Values)
            {
                if (!name2properties.TryGetValue(fieldinfo.ColumnName, out TablePropertyInfo tablePropertyInfo))
                {
                    LogAndThrowEx($"TableName:{fieldinfo.TableName} ColumnName:{fieldinfo.ColumnName} not in itable2properties");
                }

                if (tablePropertyInfo.IsBaseType != fieldinfo.IsBaseType)
                {
                    LogAndThrowEx($"{fieldinfo.TableName} {fieldinfo.ColumnName} IsBaseType not match" +
                        $"{tablePropertyInfo.IsBaseType} != {fieldinfo.IsBaseType}");
                }

                if (!fieldinfo.IsBaseType)
                {
                    CheckEx(tableinfo, fieldinfo.name2fieldinfo, tablePropertyInfo.column2propertiesinfo);
                }
            }
        }

        public void Inititable2properties(List<Type> types)
        {
            foreach (Type type in types)
            {
                Type interf = type.GetInterface("ITable");
                var tabattr = type.GetCustomAttribute<MyTableAttribute>();

                if (null == interf
                    || null == tabattr)
                {
                    continue;
                }

                // 获取 public 属性
                var properties = type.GetProperties();

                ConcurrentDictionary<string, PropertyInfo> tmpName2Property = new ConcurrentDictionary<string, PropertyInfo>();
                TablePropetyInfos tablePropetyInfos = new TablePropetyInfos();
                tablePropetyInfos.tableName = tabattr.TableName;

                foreach (var property in properties)
                {
                    var attr = property.GetCustomAttribute<DataAttribute>();
                    if (null == attr)
                    {
                        continue;
                    }

                    if (!tmpName2Property.TryAdd(property.Name, property))
                    {
                        string msg = $"tmpName2Property.TryAdd type:{type} property.Name:{property.Name} failed";
                        LoggerHelper.Instance().Log(LogType.Test, msg);
                        throw new Exception(msg);
                    }

                    TablePropertyInfo tablePropertyInfo = new TablePropertyInfo();

                    if (!attr.IsBaseType)
                    {
                        ExProperty(tablePropertyInfo.column2propertiesinfo, property, attr);
                        tablePropertyInfo.IsBaseType = false;
                    }
                    //else
                    {
                        tablePropertyInfo.propetyInfo = property;
                    }

                    if (!tablePropetyInfos.column2fieldinfo.TryAdd(property.Name, tablePropertyInfo))
                    {

                    }
                }

                if (tmpName2Property.Count < 1)
                {
                    string msg = $"tmpName2Property type:{type} no property";
                    LoggerHelper.Instance().Log(LogType.Test, msg);
                    throw new Exception(msg);
                }

                if (!itable2properties.TryAdd(type, tmpName2Property))
                {
                    LogAndThrowEx($"itable2properties.TryAdd type{type} failed");
                }

                if (!itable2propertiesnew.TryAdd(type, tablePropetyInfos))
                {
                    LogAndThrowEx($"itable2properties.TryAdd type{type} failed");
                }
            }

        }

        public void ExProperty(ConcurrentDictionary<string, TablePropertyInfo> column2fieldinfo, PropertyInfo propetyInfo, DataAttribute dataAttribute)
        {

            foreach (PropertyInfo proper in propetyInfo.PropertyType.GetProperties())
            {
                var attr = proper.GetCustomAttribute<DataAttribute>();
                if (null == attr)
                {
                    continue;
                }

                TablePropertyInfo tablePropertyInfo = new TablePropertyInfo();

                if (!attr.IsBaseType)
                {
                    // 递归处理
                    ExProperty(tablePropertyInfo.column2propertiesinfo, proper, attr);
                    tablePropertyInfo.IsBaseType = false;
                }
                //else
                {
                    tablePropertyInfo.propetyInfo = proper;
                }

                if (!column2fieldinfo.TryAdd(proper.Name, tablePropertyInfo))
                {
                    LogAndThrowEx($"ExProperty ColumnName:{attr.ColumnName} add failed");
                }
            }


        }
        public void Initname2tableinfo(List<Type> types)
        {
            foreach (Type type in types)
            {
                var classAtt = type.GetCustomAttribute(typeof(MyTableAttribute)) as MyTableAttribute;
                if (null == classAtt)
                {
                    continue;
                }
                TableInfo tableInfo = new TableInfo();
                tableInfo.tableName = classAtt.TableName;

                var properties = type.GetProperties();
                Ex(tableInfo, properties, tableInfo.column2fieldinfo);

                if (!name2tableinfo.TryAdd(tableInfo.tableName, tableInfo))
                {
                    string msg = $"name2tableinfo.TryAdd {tableInfo.tableName}";
                    LoggerHelper.Instance().Log(LogType.Test, msg);
                    throw new Exception(msg);
                }

                if (!type2tableinfo.TryAdd(type, tableInfo))
                {
                    string msg = $"type2tableinfo.TryAdd {tableInfo.tableName}";
                    LoggerHelper.Instance().Log(LogType.Test, msg);
                    throw new Exception(msg);
                }
            }
        }
        public void Ex(TableInfo tableInfo, PropertyInfo[] properties, ConcurrentDictionary<string, MyFieldInfo> column2fieldinfo)
        {
            foreach (PropertyInfo property in properties)
            {
                var propeAttr = property.GetCustomAttribute(typeof(DataAttribute)) as DataAttribute;

                if (null == propeAttr)
                {
                    continue;
                }

                MyFieldInfo fieldInfo = new MyFieldInfo();
                fieldInfo.ColumnName = propeAttr.ColumnName;
                fieldInfo.type = propeAttr.type;
                fieldInfo.IsPrimaryKey = propeAttr.IsPrimaryKey;
                fieldInfo.IsBaseType = propeAttr.IsBaseType;

                if (fieldInfo.IsPrimaryKey)
                {
                    tableInfo.TableKey = new TableKey() { key = fieldInfo };
                }

                if (!fieldInfo.IsBaseType)
                {
                    var pressp = property.PropertyType.GetProperties();
                    // 递归处理
                    Ex(tableInfo, pressp, fieldInfo.name2fieldinfo);
                }

                if (!column2fieldinfo.TryAdd(fieldInfo.ColumnName, fieldInfo))
                {
                    string msg = $"tableInfo.column2fieldinfo.TryAdd {tableInfo.tableName} propeAttr.ColumnName:{propeAttr.ColumnName}";
                    LoggerHelper.Instance().Log(LogType.Test, msg);
                    throw new Exception(msg);
                }
            }
        }

        public void LogAndThrowEx(string msg)
        {
            LoggerHelper.Instance().Log(LogType.Test, msg);
            throw new Exception(msg);
        }

    }
}
