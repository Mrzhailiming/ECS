using Entity;
using Entity.Component;
using Entity.DataStruct;
using Entity.Table;
using Global;
using Singleton;
using Singleton.Manager;
using SystemShare;

namespace SystemServer
{
    public class LogInSystem : ISystem
    {
        public void Tick(long tick, IEntity entity)
        {
            LoginComponent loginComponent = entity.GetComponent<LoginComponent>() as LoginComponent;

            if (null == loginComponent)
            {
                LoggerHelper.Instance().Log(LogType.Console, "LogInSystem Tick LoginComponent null");
                return;
            }

            if (loginComponent.mLoginState == LoginState.Complete)
            {
                LoggerHelper.Instance().Log(LogType.Console, $"LogInSystem Tick entity:{entity.mOwner} login success");
            }
        }

        [CmdHandlerAttribute(mThreadMode = ThreadMode.LogIn, mTCPCMDS = TCPCMDS.LOGIN, mCmdHandlerType = CmdHandlerType.Server)]
        public static void OnLogin(TCPPacket packet)
        {
            IEntity entity = packet.mOwner as IEntity;
            if (null == entity)
            {
                LoggerHelper.Instance().Log(LogType.Console, $"LogInSystem OnLogin entity null");
            }

            SocketComponent socketComponent = entity.GetComponent<SocketComponent>() as SocketComponent;
            if (null == socketComponent)
            {
                LoggerHelper.Instance().Log(LogType.Test, $"LogInSystem SocketComponent null");
                return;
            }

            string str = packet.GetString();
            string[] strings = str.Split(' ');
            entity.mOwner = strings[0];
            entity.ID = strings[0];

            TableLogIn dbinfo = DBManager.Instance().Load<TableLogIn>(entity)?[0] as TableLogIn;

            if (null == dbinfo)
            {
                LoggerHelper.Instance().Log(LogType.Test, $"LogInSystem OnLogin account:{entity.mOwner} TableLogIn null");
                return;
            }

            if (dbinfo.pwd.ToString() != strings[1])
            {
                string msg = "-1";
                Global.Global.SendAsync(socketComponent, TCPCMDS.LOGIN, msg);
                LoggerHelper.Instance().Log(LogType.Test, $"LogInSystem OnLogin account:{entity.mOwner} pwd error");
            }
            else
            {
                // 登录成功才调度
                socketComponent.IsLoginSuccess = true;
                EntityManager.Instance().AddEntity(entity, socketComponent.mSocket.RemoteEndPoint.GetHashCode());
                string msg = "1";
                Global.Global.SendAsync(socketComponent, TCPCMDS.LOGIN, msg);
                LoggerHelper.Instance().Log(LogType.Test, $"LogInSystem OnLogin account:{entity.mOwner} pwd success");

            }
        }
    }
}
