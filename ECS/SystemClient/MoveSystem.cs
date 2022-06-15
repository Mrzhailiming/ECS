using Entity;
using Entity.Component;
using Singleton;
using Singleton.Manager;
using System;
using System.Text;
using SystemShare;

namespace SystemClient
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
                LoggerHelper.Instance().Log(LogType.Test, $"client tick move X-{++moveComponent.X} Y-{++moveComponent.Y}");
            }

            SocketComponent socketComponent = entity.GetComponent<SocketComponent>() as SocketComponent;

            if (null == socketComponent)
            {
                LoggerHelper.Instance().Log(LogType.Info, $"entity:{entity.mOwner} null == socketComponent");
            }

        }

        /// <summary>
        /// 客户端接收服务器移动结果
        /// </summary>
        /// <param name="packet"></param>
        [CmdHandlerAttribute(mThreadMode = ThreadMode.Battle, mTCPCMDS = TCPCMDS.Move, mCmdHandlerType = CmdHandlerType.Client)]
        public static void ClientRecvOnMove(TCPPacket packet)
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

            byte[] recv = new byte[packet.mTotalLen - Proto.protoHeadLen];
            Buffer.BlockCopy(packet.mBuff, Proto.protoHeadLen, recv, 0, packet.mTotalLen - Proto.protoHeadLen);
            string recvstr = Encoding.Default.GetString(recv);

            LoggerHelper.Instance().Log(LogType.Test, $"client recv OnMove back:{(TCPCMDS)packet.mCmd} ServerPos:{recvstr} client curpos:X-{moveComponent.X} Y-{moveComponent.Y}");
        }

    }
}
