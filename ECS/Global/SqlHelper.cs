using MySql.Data.MySqlClient;
using System;

namespace Global
{
    public class SqlHelper
    {
        public static void Test()
        {
            string constructorString = "server=localhost;port=3306;User Id=root;password=123456;";
            MySqlConnection myConnnect = new MySqlConnection(constructorString);
            myConnnect.Open();
            MySqlCommand myCmd = new MySqlCommand("insert into t_dept(name,year) values('jjj',22）", myConnnect);
            Console.WriteLine(myCmd.CommandText);
            if (myCmd.ExecuteNonQuery() > 0)
            {
                Console.WriteLine("数据插入成功！");
            }
            myCmd.CommandText = "insert into t_dept(name,year) values('jjj4',22）";
            Console.WriteLine(myCmd.CommandText);
            if (myCmd.ExecuteNonQuery() > 0)
            {
                Console.WriteLine("数据插入成功！");
            }
            myCmd.CommandText = "delete from t_dept";
            Console.WriteLine(myCmd.CommandText);
            if (myCmd.ExecuteNonQuery() > 0)
            {
                Console.WriteLine("user表类型数据全部删除成功！");
            }
            myCmd.Dispose();
            myConnnect.Close();
        }
    }
}
