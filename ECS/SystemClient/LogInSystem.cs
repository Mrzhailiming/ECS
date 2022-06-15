using Entity;
using Entity.Component;
using Entity.DataStruct;
using Singleton;
using Singleton.Manager;
using SystemShare;

namespace SystemClient
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

        [CmdHandlerAttribute(mThreadMode = ThreadMode.LogIn, mTCPCMDS = TCPCMDS.LOGIN, mCmdHandlerType = CmdHandlerType.Client)]
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
            LoggerHelper.Instance().Log(LogType.Test, $"client OnLogin ret:{str}");

            if (str == "1")
            {
                socketComponent.IsLoginSuccess = true;
            }
            else
            {
                socketComponent.IsLoginSuccess = false;
            }
        }
    }
}
