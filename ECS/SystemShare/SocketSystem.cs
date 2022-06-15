using Entity;
using Entity.Component;
using Entity.DataStruct;
using Singleton;
using Singleton.Manager;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SystemShare
{
    public partial class SocketSystem : ISystem
    {
        /// <summary>
        /// 3s
        /// </summary>
        public const long HeartBeatTimeInterval = 3 * 1000 * 10000;

        [CmdHandlerAttribute(mThreadMode = ThreadMode.Normal, mTCPCMDS = TCPCMDS.HeartBeat, mCmdHandlerType = CmdHandlerType.Both)]
        public static void OnHeartBeat(TCPPacket packet)
        {
            IEntity entity = packet.mOwner as IEntity;

            if (null == entity)
            {
                LoggerHelper.Instance().Log(LogType.HeartBeat, $"server SocketSystem OnHeartBeat entity null");
                return;
            }

            SocketComponent socketComponent = entity.GetComponent<SocketComponent>() as SocketComponent;
            if (null == socketComponent)
            {
                LoggerHelper.Instance().Log(LogType.HeartBeat, $"server SocketSystem OnHeartBeat SocketComponent null");
                return;
            }

            socketComponent.mHeartBeatTicks = Global.Global.Now().Ticks;

            LoggerHelper.Instance().Log(LogType.HeartBeat, $"server SocketSystem OnHeartBeat ProcessCmdID:{(TCPCMDS)packet.mCmd} body:{packet.GetString()}");
        }

        public void Tick(long tick, IEntity entity)
        {
            // 比如定时发送心跳
            SocketComponent socketComponent = entity.GetComponent<SocketComponent>() as SocketComponent;
            LoginComponent loginComponent = entity.GetComponent<LoginComponent>() as LoginComponent;

            if (null == socketComponent || null == loginComponent)
            {
                LoggerHelper.Instance().Log(LogType.Console, $"SocketSystem Tick IEntity:{entity.mOwner} SocketComponent || loginComponent null");
                return;
            }

            string str = "heartbeat";

            if (socketComponent.mIsCompleteSend)
            {
                socketComponent.mIsCompleteSend = false;
                SendAsync(socketComponent, TCPCMDS.HeartBeat, str);
            }

            if (Global.Global.Now().Ticks - socketComponent.mHeartBeatTicks > HeartBeatTimeInterval)
            {
                loginComponent.mLoginState = LoginState.TimeOut;
                LoggerHelper.Instance().Log(LogType.Test, $"SocketSystem Tick IEntity:{entity.mOwner} HeartBeatTimeTimeOut");
            }

        }

        public void Init()
        {
            // 初始化缓冲区
            for (int index = 0; index < BufferManager.MaxConnect; ++index)
            {
                BufferManager.Instance().SendBufUseMap.Add(index, false);
                BufferManager.Instance().RecvBufUseMap.Add(index, false);
            }
        }
        public void RunServer(IEntity entity, IPEndPoint iPEndPoint, SocketType socketType, ProtocolType protocolType
            , string ip, int port, int backlog)
        {
            SocketEntity socketEntity = entity as SocketEntity;
            if (null == socketEntity)
            {
                LoggerHelper.Instance().Log(LogType.Console, $"SocketSystem Init SocketEntity null");
                return;
            }

            SocketComponent socketComponent = entity.GetComponent<SocketComponent>() as SocketComponent;
            if (null == socketComponent)
            {
                LoggerHelper.Instance().Log(LogType.Console, $"SocketSystem Init SocketComponent null");
                return;
            }


            CreateSocket(socketComponent, iPEndPoint, socketType, protocolType);

            Bind(socketComponent, ip, port);

            Listen(socketComponent, backlog);

            AcceptAsync(socketComponent);

            LoggerHelper.Instance().Log(LogType.Console, $"RunServer 开始监听 IP:{ip} Port:{port}");
        }


        public void RunClient(IEntity entity, IPEndPoint iPEndPoint, SocketType socketType, ProtocolType protocolType
            , string ip, int port)
        {
            SocketEntity socketEntity = entity as SocketEntity;
            if (null == socketEntity)
            {
                LoggerHelper.Instance().Log(LogType.Console, $"SocketSystem Init SocketEntity null");
                return;
            }

            //SocketComponent socketComponent = entity.GetComponent<SocketComponent>() as SocketComponent;
            //if (null == socketComponent)
            //{
            //    LoggerHelper.Instance().Log(LogType.Console, $"SocketSystem Init SocketComponent null");
            //    return;
            //}

            LoggerHelper.Instance().Log(LogType.Console, $"连接server IP:{ip} Port:{port}");

            //CreateSocket(socketComponent, iPEndPoint, socketType, protocolType);

            Connect(entity, iPEndPoint, socketType, protocolType);
        }


        public void CreateSocket(SocketComponent socketComponent, IPEndPoint iPEndPoint, SocketType socketType, ProtocolType protocolType)
        {
            // 创建socket
            socketComponent.mSocket = new Socket(iPEndPoint.AddressFamily, socketType, protocolType);
        }

        public void Connect(IEntity entity, IPEndPoint iPEndPoint, SocketType socketType, ProtocolType protocolType)
        {
            SocketAsyncEventArgs socketAsyncEventArgs = new SocketAsyncEventArgs();
            socketAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(Connect_Completed);
            socketAsyncEventArgs.RemoteEndPoint = iPEndPoint; // 设置需要连接的ip
            socketAsyncEventArgs.UserToken = entity;

            var mSocket = new Socket(iPEndPoint.AddressFamily, socketType, protocolType);

            bool isAsync = mSocket.ConnectAsync(socketAsyncEventArgs);
            if (!isAsync)
            {
                ProcessConnect(socketAsyncEventArgs);
            }
        }

        public void Bind(SocketComponent socketComponent, string ip, int port)
        {
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            socketComponent.mSocket.Bind(iPEndPoint);
        }

        public void Listen(SocketComponent socketComponent, int backlog)
        {
            socketComponent.mSocket.Listen(backlog);
        }

        public void AcceptAsync(SocketComponent socketComponent)
        {
            socketComponent.mRecvSocketArg = new SocketAsyncEventArgs();
            socketComponent.mRecvSocketArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptCompleted);
            //socketComponent.mSocketArg.SetBuffer(new byte[1024 * 1024], 0, 1024 * 1024);

            socketComponent.mRecvSocketArg.UserToken = socketComponent;
            //socketComponent.mSocketInvild = true;
            if (!socketComponent.mSocket.AcceptAsync(socketComponent.mRecvSocketArg))
            {
                ProcessAccept(socketComponent.mRecvSocketArg);
            }
        }
        public void Accept(SocketAsyncEventArgs e)
        {
            SocketComponent socketComponent = e.UserToken as SocketComponent;
            if (null == socketComponent)
            {
                LoggerHelper.Instance().Log(LogType.Console, $"Accept SocketComponent null");
            }

            socketComponent.mSocket.AcceptAsync(socketComponent.mRecvSocketArg);
        }

        public void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            LoggerHelper.Instance().Log(LogType.Console, $"AcceptCompleted");
            ProcessAccept(e);
        }

        public void ProcessAccept(SocketAsyncEventArgs e)
        {
            if (null == e.AcceptSocket)
            {
                LoggerHelper.Instance().Log(LogType.Console, $"ProcessAccept e.AcceptSocket null");
                return;
            }

            LoggerHelper.Instance().Log(LogType.Console, $"ProcessAccept e.AcceptSocket != null");

            //SocketComponent socketComponent = new SocketComponent();

            // 接收事件
            SocketAsyncEventArgs recvArg = new SocketAsyncEventArgs();
            //socketComponent.mRecvSocketArg = recvArg;
            //socketComponent.mConnectType = ConnectType.Recv;
            //recvArg.UserToken = socketComponent;
            recvArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            recvArg.AcceptSocket = e.AcceptSocket;

            // 发送事件
            SocketAsyncEventArgs sendArg = new SocketAsyncEventArgs();
            //socketComponent.mSendSocketArg = sendArg;
            //socketComponent.mConnectType = ConnectType.Send;
            //sendArg.UserToken = socketComponent;
            sendArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            sendArg.AcceptSocket = e.AcceptSocket;

            SocketEntity entity = new SocketEntity();
            //socketComponent.mOwner = entity;

            string userName = e.AcceptSocket.RemoteEndPoint.ToString();
            entity.mOwner = userName;

            ComponentAttributeHelper.Instance().AddAllComponents(entity, ComponentType.Server, sendArg, recvArg);

            //socketComponent.mSocket = e.AcceptSocket;
            //socketComponent.mSocketInvild = true;
            //socketComponent.mHeartBeatTicks = Global.Global.Now().Ticks;

            #region 添加相关组件


            // 添加 SocketComponent
            //entity.AddComponent(typeof(SocketComponent), socketComponent);

            //// 添加 TestComponent (测试用)
            //TestComponent testComponent = new TestComponent();
            //testComponent.mName = userName;
            //testComponent.mOwner = entity;
            //testComponent.mSocket = e.AcceptSocket;
            //entity.AddComponent(typeof(TestComponent), testComponent);

            //// 添加login组件
            //LoginComponent loginComponent = new LoginComponent();
            //loginComponent.mOwner = entity;
            //entity.AddComponent(typeof(LoginComponent), loginComponent);

            //// 添加Move组件
            //MoveComponent moveComponent = new MoveComponent();
            //moveComponent.mOwner = entity;
            //entity.AddComponent(typeof(MoveComponent), moveComponent);


            #endregion

            SocketComponent socketComponent = entity.GetComponent<SocketComponent>() as SocketComponent;

            // 被动连接 设置 recvbuf
            if (!PopBuf(socketComponent))
            {
                LoggerHelper.Instance().Log(LogType.Console, $"SocketSystem ProcessAccept PopBuf false");
                return;
            }

            // 添加到实体管理队列
            EntityManager.Instance().AddEntity(entity, socketComponent.mSocket.RemoteEndPoint.GetHashCode());

            bool isAsync = socketComponent.mSocket.ReceiveAsync(socketComponent.mRecvSocketArg);
            if (!isAsync)
            {
                ProcessReceive(socketComponent.mRecvSocketArg);
            }

            // 继续接收下一个连接
            e.AcceptSocket = null;
            Accept(e);
        }
        public void Connect_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    ProcessConnect(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a Connect");
            }
        }
        public void ProcessConnect(SocketAsyncEventArgs e)
        {
            IEntity entity = e.UserToken as IEntity;

            if (null == e.ConnectSocket)
            {
                LoggerHelper.Instance().Log(LogType.Test, $"client ProcessConnect failed");
                return;
            }

            if (null == entity)
            {
                LoggerHelper.Instance().Log(LogType.Console, $"client ProcessConnect IEntity null");
                return;
            }
            else
            {
                LoggerHelper.Instance().Log(LogType.Console, $"client ProcessConnect IEntity != null");
            }

            // 接收事件
            SocketAsyncEventArgs recvArg = new SocketAsyncEventArgs();
            recvArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            recvArg.AcceptSocket = e.ConnectSocket;

            // 发送事件
            SocketAsyncEventArgs sendArg = new SocketAsyncEventArgs();
            sendArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
            sendArg.AcceptSocket = e.ConnectSocket;

            ComponentAttributeHelper.Instance().AddAllComponents(entity, ComponentType.Client, sendArg, recvArg);

            SocketComponent socketComponent = entity.GetComponent<SocketComponent>() as SocketComponent;
            if (null == entity)
            {
                LoggerHelper.Instance().Log(LogType.Console, $"client ProcessConnect SocketComponent add failed");
                return;
            }

            // 设置缓冲区
            if (!PopBuf(socketComponent))
            {
                LoggerHelper.Instance().Log(LogType.Console, $"SocketSystem Connect PopBuf false");
                return;
            }

            EntityManager.Instance().AddEntity(entity, socketComponent.mSocket.RemoteEndPoint.GetHashCode());

            //socketComponent.mSocket = e.ConnectSocket;
            //socketComponent.mHeartBeatTicks = Global.Global.Now().Ticks;
            //socketComponent.mSocketInvild = true;

            //SocketEntity entity = new SocketEntity();
            //socketComponent.mOwner = entity;

            #region 添加组件

            //// 添加 SocketComponent
            //entity.AddComponent(typeof(SocketComponent), socketComponent);

            //LoginComponent loginComponent = new LoginComponent();
            //loginComponent.mOwner = entity;
            //loginComponent.mLoginState = LoginState.Complete;
            //entity.AddComponent(typeof(LoginComponent), loginComponent);

            //// 添加Move组件
            //MoveComponent moveComponent = new MoveComponent();
            //moveComponent.mOwner = entity;
            //entity.AddComponent(typeof(MoveComponent), moveComponent);

            #endregion


            // 开始接收
            if (!socketComponent.mSocket.ReceiveAsync(socketComponent.mRecvSocketArg))
            {
                ProcessReceive(socketComponent.mRecvSocketArg);
            }
        }

        #region IO

        public void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        public void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                SocketComponent socketComponent = e.UserToken as SocketComponent;

                if (null == socketComponent)
                {
                    LoggerHelper.Instance().Log(LogType.Console, $"ProcessReceive SocketComponent null");
                    return;
                }

                LoggerHelper.Instance().Log(LogType.Console, $"ProcessReceive recv:{e.BytesTransferred}");

                // copy 接收到的数据到 socketComponent
                //if (BuffCopy(socketComponent))
                //{
                //    TCPPacket packet = new TCPPacket(socketComponent.mOwner, socketComponent.cmdID, socketComponent.recvBuff, socketComponent.needRecvNum);

                //    // 投递


                //    CMDDispatcher.Instance().Dispatcher(packet);

                //    ResetComponentCmdBuf(socketComponent);
                //}

                BuffCopyRecursion(socketComponent);

                // 继续异步接收
                // 重置接收缓冲区
                socketComponent.mRecvSocketArg.SetBuffer(socketComponent.mRecvSocketArg.Offset, BufferManager.PerBufLen);
                bool isAsync = socketComponent.mSocket.ReceiveAsync(socketComponent.mRecvSocketArg);
                if (!isAsync)
                {
                    ProcessReceive(e);
                }
            }
            else
            {
                if (e.SocketError == SocketError.SocketError)
                {
                    LoggerHelper.Instance().Log(LogType.Console, "ProcessReceive() ReceiveAsync failed SocketError");
                }
                else
                {
                    LoggerHelper.Instance().Log(LogType.Console, $"ProcessReceive ReceiveAsync  failed SocketError:{e.SocketError}");
                }

                Close(e);
            }
        }
        public void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                LoggerHelper.Instance().Log(LogType.Console, "ProcessSend()");
                SocketComponent socketComponent = e.UserToken as SocketComponent;
                if (null == socketComponent)
                {
                    Close(e);
                    LoggerHelper.Instance().Log(LogType.Console, "ProcessSend() null == socketComponent close!");
                    return;
                }

                // 恢复缓冲区的大小
                socketComponent.mSendSocketArg.SetBuffer(e.Offset, BufferManager.PerBufLen);
                socketComponent.mIsCompleteSend = true;
            }
            else
            {
                Close(e);
                LoggerHelper.Instance().Log(LogType.Console, $"ProcessSend() SocketError {e.SocketError}");
            }
        }

        #endregion

        public void Close(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            try
            {
                SocketComponent socketComponent = socketAsyncEventArgs.UserToken as SocketComponent;
                if (null == socketComponent)
                {
                    LoggerHelper.Instance().Log(LogType.Console, "SocketSystem Close PushBuf SocketComponent null");
                    return;
                }

                PushBuf(socketComponent);

                socketComponent.mSocket?.Shutdown(SocketShutdown.Send);
                socketComponent.mSocket?.Close();
                socketComponent.mSocket = null;
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance().Log(LogType.Exception, ex.ToString());
            }
        }

        #region 管理缓冲区
        public void PushBuf(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            SocketComponent socketComponent = socketAsyncEventArgs.UserToken as SocketComponent;
            if (null == socketComponent)
            {
                LoggerHelper.Instance().Log(LogType.Console, "SocketSystem Close PushBuf SocketComponent null");
                return;
            }

            PushBuf(socketComponent);
        }
        public void PushBuf(SocketComponent socketComponent)
        {
            PushBuf(socketComponent, ConnectType.Send);
            PushBuf(socketComponent, ConnectType.Recv);
        }

        public void PushBuf(SocketComponent socketComponent, ConnectType connectType)
        {
            try
            {
                Dictionary<int, bool> map;
                if (connectType == ConnectType.Send)
                {
                    map = BufferManager.Instance().SendBufUseMap;
                }
                else if (connectType == ConnectType.Recv)
                {
                    map = BufferManager.Instance().RecvBufUseMap;
                }
                else
                {
                    LoggerHelper.Instance().Log(LogType.Console, "SocketSystem PushBuf ConnectType undefined");
                    return;
                }

                // socketComponent.mSendBufLength <= 0 没有分配缓冲区
                if (connectType == ConnectType.Send && socketComponent.mSendBufLength <= 0)
                {
                    LoggerHelper.Instance().Log(LogType.Console, $"SocketSystem PushBuf ConnectType connectType{connectType} no buf");
                    return;
                }
                if (connectType == ConnectType.Send && socketComponent.mRecvBufLength <= 0)
                {
                    LoggerHelper.Instance().Log(LogType.Console, $"SocketSystem PushBuf ConnectType connectType{connectType} no buf");
                    return;
                }


                int freeIndex;
                if (connectType == ConnectType.Send)
                {
                    freeIndex = socketComponent.mSendBufOffset / BufferManager.PerBufLen;
                }
                else
                {
                    freeIndex = socketComponent.mRecvBufOffset / BufferManager.PerBufLen;
                }

                map[freeIndex] = false;

                LoggerHelper.Instance().Log(LogType.Console, $"SocketSystem PushBuf connectType:{connectType} freeIndex:{freeIndex}");
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance().Log(LogType.Exception, $"SocketSystem PushBuf ex:{ex}");
            }
        }

        /// <summary>
        /// 新 PopBuf
        /// </summary>
        /// <param name="socketComponent"></param>
        /// <returns></returns>
        public bool PopBuf(SocketComponent socketComponent)
        {
            bool ret = PopBuf(socketComponent, socketComponent.mRecvSocketArg, ConnectType.Recv)
                && PopBuf(socketComponent, socketComponent.mSendSocketArg, ConnectType.Send);

            if (!ret)
            {
                PushBuf(socketComponent);
            }

            return ret;
        }

        /// <summary>
        /// 新 PopBuf
        /// </summary>
        public bool PopBuf(SocketComponent socketComponent, SocketAsyncEventArgs Args, ConnectType connectType)
        {
            try
            {
                Dictionary<int, bool> map;
                byte[] buf;
                int freeIndex;
                // 设置发送缓冲区
                if (connectType == ConnectType.Send)
                {
                    map = BufferManager.Instance().SendBufUseMap;
                    buf = BufferManager.Instance().SendTotalBuffer;
                }
                // 设置接收缓冲区
                else if (connectType == ConnectType.Recv)
                {
                    map = BufferManager.Instance().RecvBufUseMap;
                    buf = BufferManager.Instance().RecvTotalBuffer;
                }
                else
                {
                    LoggerHelper.Instance().Log(LogType.Console, "SocketSystem PopBuf ConnectType undefined");
                    return false;
                }

                freeIndex = GetFreeIndex(map);
                if (freeIndex < 0)
                {
                    LoggerHelper.Instance().Log(LogType.Console, "SocketSystem PopBuf buf is full");
                    return false;
                }

                // 标记已经分配
                map[freeIndex] = true;

                int offset = freeIndex * BufferManager.PerBufLen;

                Args.SetBuffer(buf, offset, BufferManager.PerBufLen);

                // 设置发送缓冲区
                if (connectType == ConnectType.Send)
                {
                    socketComponent.mSendBufOffset = offset;
                    socketComponent.mSendBufLength = BufferManager.PerBufLen;
                }
                // 设置接收缓冲区
                else
                {
                    socketComponent.mRecvBufOffset = offset;
                    socketComponent.mRecvBufLength = BufferManager.PerBufLen;
                }

                LoggerHelper.Instance().Log(LogType.Console, $"SocketSystem PopBuf freeIndex:{freeIndex}");
                return true;
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance().Log(LogType.Exception, $"SocketSystem PopBuf ex:{ex}");
                return false;
            }
        }
        public int GetFreeIndex(Dictionary<int, bool> map)
        {
            foreach (var pair in map)
            {
                if (pair.Value)
                {
                    continue;
                }

                return pair.Key;
            }

            return -1;
        }

        #endregion
    }

    public delegate void Asyncdelegate(object sender, SocketAsyncEventArgs e);
}
