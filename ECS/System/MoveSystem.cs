using Entity;
using Entity.Component;
using Singleton;
using Singleton.Manager;
using SystemShare;

namespace SystemServer
{
    public class MoveSystem : ISystem
    {
        public void Tick(long tick, IEntity entity)
        {
            MoveComponent moveComponent = entity.GetComponent<MoveComponent>() as MoveComponent;

            if (null == moveComponent)
            {
                LoggerHelper.Instance().Log(LogType.Info, $"entity:{entity.mOwner} null == moveComponent");
            }

            if (moveComponent.IsMoving)
            {
                LoggerHelper.Instance().Log(LogType.Test, $"server tick move result:X{++moveComponent.X} Y{++moveComponent.Y}");
            }
        }

        /// <summary>
        /// 服务器收到客户端移动指令,进行移动
        /// </summary>
        /// <param name="packet"></param>
        [CmdHandlerAttribute(mThreadMode = ThreadMode.Battle, mTCPCMDS = TCPCMDS.Move, mCmdHandlerType = CmdHandlerType.Server)]
        public static void OnMove(TCPPacket packet)
        {
            IEntity entity = packet.mOwner as IEntity;

            if (null == entity)
            {
                LoggerHelper.Instance().Log(LogType.Console, $"MoveSystem entity null");
                return;
            }

            SocketComponent socketComponent = entity.GetComponent<SocketComponent>() as SocketComponent;
            if (null == socketComponent)
            {
                LoggerHelper.Instance().Log(LogType.Console, $"MoveSystem SocketComponent null");
                return;
            }

            MoveComponent moveComponent = entity.GetComponent<MoveComponent>() as MoveComponent;

            if (null == moveComponent)
            {
                LoggerHelper.Instance().Log(LogType.Info, $"entity:{entity.mOwner} null == moveComponent");
            }

            // 移动
            moveComponent.IsMoving = !moveComponent.IsMoving; ;

            // 向客户端返回移动结果
            string msg = $"X:{moveComponent.X} Y:{moveComponent.Y}";
            Global.Global.SendAsync(socketComponent, TCPCMDS.Move, msg);
            LoggerHelper.Instance().Log(LogType.Test, $"server recv OnMove result:{msg}");
        }

    }
}
