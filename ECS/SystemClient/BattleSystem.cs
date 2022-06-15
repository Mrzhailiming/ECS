using Entity;
using Entity.Component;
using Singleton;
using Singleton.Manager;
using System;
using System.Collections.Generic;
using System.Text;
using SystemShare;

namespace SystemClient
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
        [CmdHandlerAttribute(mThreadMode = ThreadMode.Battle, mTCPCMDS = TCPCMDS.Attack, mCmdHandlerType = CmdHandlerType.Client)]
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

            string msg = $"mylife:{packet.GetString()}";
            LoggerHelper.Instance().Log(LogType.Test, $"client OnAttack result:{msg}");
        }
    }
}
