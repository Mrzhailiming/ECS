using Entity;
using Entity.Component;
using Singleton;
using System;
using SystemShare;

namespace SystemShare
{
    /// <summary>
    /// 连接系统
    /// 1.打印 ConnectionComponent 的 name
    /// </summary>
    public class TestSysytem : ISystem
    {
        public void Send(IEntity entity)
        {
            TestComponent cp = entity.GetComponent<TestComponent>() as TestComponent;

            if (null == cp)
            {
                LoggerHelper.Instance().Log(LogType.Console, $"ConnectSysytem Send GetComponent null");
                return;
            }

            byte[] buf = new byte[100];
            cp?.mSocket.Send(buf);
        }

        public void Tick(long tick, IEntity entity)
        {
            TestComponent connection = entity.GetComponent<TestComponent>() as TestComponent;

            if (null == connection)
            {
                LoggerHelper.Instance().Log(LogType.Console, $"TestSysytem Tick GetComponent null");
                return;
            }

            //PrintName(connection);
            LoggerHelper.Instance().Log(LogType.Console, $"TestSysytem tick");
        }


        public void PrintName(TestComponent connection)
        {
            LoggerHelper.Instance().Log(LogType.Console, $"TestSysytem PrintName entityOwner:{(connection.mOwner)?.mOwner} ComponentOwner:{connection?.mOwner} {connection?.mName}");
        }
    }
}
