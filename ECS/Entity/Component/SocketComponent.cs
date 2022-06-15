using Entity.DataStruct;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Entity.Component
{
    [ComponentAttribute(mComponentType = ComponentType.Base, mInitComponentType = InitComponentType.SocketComponent)]
    public  class SocketComponent : IComponent
    {
        public bool IsConnected { get; set; } = true;
        public bool IsLoginSuccess { get; set; } = false;
        public Socket mSocket { get; set; }
        /// <summary>
        /// 异步事件
        /// </summary>
        public SocketAsyncEventArgs mRecvSocketArg { get; set; }
        public bool mIsCompleteSend { get; set; } = true;
        /// <summary>
        /// 异步事件
        /// </summary>
        public SocketAsyncEventArgs mSendSocketArg { get; set; }
        /// <summary>
        /// 发送缓冲区偏移
        /// </summary>
        public int mSendBufOffset { get; set; }
        /// <summary>
        /// 发送缓冲区长度
        /// </summary>
        public int mSendBufLength { get; set; }
        /// <summary>
        /// 接收缓冲区偏移
        /// </summary>
        public int mRecvBufOffset { get; set; }
        /// <summary>
        /// 接收缓冲区长度
        /// </summary>
        public int mRecvBufLength { get; set; }
        public IEntity mOwner { get; set; }

        public bool mSocketInvild { get; set; } = false;

        public int cmdID = -1;
        public byte[] recvBuff = null; // 存储一次异步返回的数据, 可能不够 一个包,或者有多个包
        public int hadRecvNum = 0;
        public int needRecvNum = 0;

        public byte[] sendBuf = null;

        public long mHeartBeatTicks;
    }
}
