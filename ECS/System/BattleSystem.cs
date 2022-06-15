using Entity;
using Entity.Component;
using Singleton;
using Singleton.Manager;
using System;
using SystemShare;

namespace SystemServer
{
    public class BattleSystem : ISystem
    {
        public void Tick(long tick, IEntity entity)
        {
            MoveComponent moveComponent = entity.GetComponent<MoveComponent>() as MoveComponent;

            if (null == moveComponent)
            {
                LoggerHelper.Instance().Log(LogType.Test, $"BattleSystem Tick entity:{entity.mOwner} null == moveComponent");
            }


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        [CmdHandlerAttribute(mThreadMode = ThreadMode.Battle, mTCPCMDS = TCPCMDS.Attack, mCmdHandlerType = CmdHandlerType.Server)]
        public static void OnAttack(TCPPacket packet)
        {
            IEntity entity = packet.mOwner as IEntity;

            if (null == entity)
            {
                LoggerHelper.Instance().Log(LogType.Test, $"BattleSystem entity null");
                return;
            }

            Action action = () =>
            {
                Attack(entity, packet);
            };

            CMDDispatcher.DispatcherAction(ThreadMode.Battle, action);
        }

        public static void Attack(IEntity entity, TCPPacket packet)
        {
            if (null == entity)
            {
                LoggerHelper.Instance().Log(LogType.Test, $"BattleSystem entity null");
                return;
            }

            SocketComponent socketComponent = entity.GetComponent<SocketComponent>() as SocketComponent;
            if (null == socketComponent)
            {
                LoggerHelper.Instance().Log(LogType.Test, $"BattleSystem SocketComponent null");
                return;
            }

            MoveComponent moveComponent = entity.GetComponent<MoveComponent>() as MoveComponent;

            if (null == moveComponent)
            {
                LoggerHelper.Instance().Log(LogType.Info, $"BattleSystem entity:{entity.mOwner} null == moveComponent");
            }

            // 向客户端返回结果
            string msg = $"life:{--moveComponent.Life}";
            Global.Global.SendAsync(socketComponent, TCPCMDS.Attack, msg);
            LoggerHelper.Instance().Log(LogType.Test, $"server OnAttack recv:{packet.GetString()}");
            LoggerHelper.Instance().Log(LogType.Test, $"server OnAttack send:{msg}");
        }
    }
}
