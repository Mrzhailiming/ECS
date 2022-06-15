using Entity;
using Entity.Component;
using Global;
using Singleton;
using Singleton.Manager;
using System;
using System.Net.Sockets;
using System.Text;

namespace SystemShare
{
    public partial class SocketSystem
    {

        public static void SendAsync(SocketComponent socketComponent, TCPCMDS cmd, string str)
        {
            if (null == socketComponent || null == socketComponent.mSocket)
            {
                LoggerHelper.Instance().Log(LogType.Console, $"SocketSystem SendAsync {null == socketComponent} || {null == socketComponent.mSocket}");
                return;
            }

            SendAsync(socketComponent, GenBuf(cmd, str));
        }

        public static void SendAsync(SocketComponent socketComponent, byte[] buf)
        {
            if(null == socketComponent || null == socketComponent.mSocket)
            {
                LoggerHelper.Instance().Log(LogType.Console, $"SocketSystem SendAsync {null == socketComponent} || {null == socketComponent.mSocket}");
                return;
            }

            SetBuffer(socketComponent.mSendSocketArg, buf);

            if (!socketComponent.mSocket.SendAsync(socketComponent.mSendSocketArg))
            {
                socketComponent.mIsCompleteSend = true;
                LoggerHelper.Instance().Log(LogType.Console, $"send sync");
            }
        }

        public static void SetBuffer(SocketAsyncEventArgs AsyncEventArgs, byte[] buf)
        {
            Buffer.BlockCopy(buf, 0, AsyncEventArgs.Buffer, AsyncEventArgs.Offset, buf.Length);

            AsyncEventArgs.SetBuffer(AsyncEventArgs.Offset, buf.Length);
        }

        public static byte[] GenBuf(TCPCMDS cmd, string str)
        {
            byte[] bodyBuf = Encoding.Default.GetBytes(str);
            int packetLen = Proto.protoHeadLen + bodyBuf.Length;

            byte[] buf = new byte[packetLen];

            // cmd
            Array.Copy(BitConverter.GetBytes((int)cmd), 0, buf, Proto.cmdIDOffset, 4);
            // paklen
            Array.Copy(BitConverter.GetBytes(packetLen), 0, buf, Proto.PacketLenOffset, 4);
            // body
            Array.Copy(bodyBuf, 0, buf, Proto.protoHeadLen, bodyBuf.Length);

            return buf;
        }

        public static bool BuffCopy(SocketComponent socketComponent)
        {
            try
            {
                SocketAsyncEventArgs AsyncEventArgs = socketComponent.mRecvSocketArg;
                // 接收命令头 4位cmdID 4位包长度
                if (socketComponent.needRecvNum <= 0)
                {
                    socketComponent.recvBuff = new byte[AsyncEventArgs.BytesTransferred]; // 可能把下一个包的数据也接受了

                    Buffer.BlockCopy(AsyncEventArgs.Buffer, AsyncEventArgs.Offset, socketComponent.recvBuff, 0, AsyncEventArgs.BytesTransferred);
                    socketComponent.hadRecvNum += AsyncEventArgs.BytesTransferred;
                    if (socketComponent.hadRecvNum >= 8)
                    {
                        //socketComponent.cmdID = Global.Global.Byte2Int(socketComponent.headerBuff, Proto.cmdIDOffset);//获取 cmdid
                        //socketComponent.needRecvNum = Global.Global.Byte2Int(socketComponent.headerBuff, Proto.PacketLenOffset);//获取命令包长度
                        //socketComponent.recvBuff = new byte[socketComponent.needRecvNum]; // 搞个内存池
                        //Buffer.BlockCopy(socketComponent.headerBuff, 0, socketComponent.recvBuff, 0, socketComponent.headerBuff.Length);

                        DePacket(socketComponent, 0);
                    }
                }

                // 接收包体
                else if (socketComponent.hadRecvNum < socketComponent.needRecvNum)
                {
                    //AsyncEventArgs.BytesTransferred 大于剩余需要拷贝的字节数，说明下一条数据来了, 咋处理
                    //int realCopy = AsyncEventArgs.BytesTransferred <= (socketComponent.needRecvNum - socketComponent.hadRecvNum) ? AsyncEventArgs.BytesTransferred : (socketComponent.needRecvNum - socketComponent.hadRecvNum);
                    //Buffer.BlockCopy(AsyncEventArgs.Buffer, AsyncEventArgs.Offset, socketComponent.recvBuff, socketComponent.hadRecvNum, realCopy);

                    //socketComponent.hadRecvNum += realCopy;

                    DePacket(socketComponent, 0);
                }

                if (socketComponent.hadRecvNum < socketComponent.needRecvNum)
                {
                    return false;
                }

                return true;// 接收完毕
            }
            catch (Exception ex)
            {
                Console.WriteLine("BuffCopy异常{0}：", ex.ToString());
                return true;
            }
        }

        public static bool BuffCopyRecursion(SocketComponent socketComponent)
        {
            try
            {
                SocketAsyncEventArgs AsyncEventArgs = socketComponent.mRecvSocketArg;
                if (null == socketComponent.recvBuff)
                {
                    socketComponent.recvBuff = new byte[AsyncEventArgs.BytesTransferred];
                    Buffer.BlockCopy(AsyncEventArgs.Buffer, AsyncEventArgs.Offset, socketComponent.recvBuff, 0, AsyncEventArgs.BytesTransferred);
                }
                else
                {
                    byte[] tmp = new byte[socketComponent.recvBuff.Length + AsyncEventArgs.BytesTransferred];
                    Buffer.BlockCopy(socketComponent.recvBuff, 0, tmp, 0, socketComponent.recvBuff.Length);
                    Buffer.BlockCopy(AsyncEventArgs.Buffer, AsyncEventArgs.Offset, tmp, socketComponent.recvBuff.Length, AsyncEventArgs.BytesTransferred);

                    socketComponent.recvBuff = tmp;
                }

                return DePacket(socketComponent, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine("BuffCopy异常{0}：", ex.ToString());
                socketComponent.IsConnected = false;
                return true;
            }
        }

        public static bool DePacket(SocketComponent socketComponent, int headbufOffset)
        {
            int invalidCount = socketComponent.recvBuff.Length - headbufOffset;

            if (headbufOffset == socketComponent.recvBuff.Length  )
            {
                // 处理完了
                ResetComponentCmdBuf(socketComponent);
                return true;
            }

            // 包头没接收完
            if (invalidCount < 8)
            {
                // 在递归之后, socketComponent.headerBuff.Count 是包含多个包的数据的, 余下 不到几个字节是下一个包的数据 ,不过没有接收完毕
                byte[] tmpBuf = new byte[invalidCount];
                Buffer.BlockCopy(socketComponent.recvBuff, headbufOffset, tmpBuf, 0, invalidCount);
                socketComponent.recvBuff = tmpBuf;

                socketComponent.hadRecvNum = invalidCount;
                socketComponent.needRecvNum = 0;
                return false;
            }

            // 处理包头
            socketComponent.cmdID = Global.Global.Byte2Int(socketComponent.recvBuff, Proto.cmdIDOffset + headbufOffset);//获取 cmdid
            socketComponent.needRecvNum = Global.Global.Byte2Int(socketComponent.recvBuff, Proto.PacketLenOffset + headbufOffset);//获取命令包长度

            // 处理包体
            // 不够一个包, 
            if (invalidCount < socketComponent.needRecvNum)
            {
                // 在递归之后, socketComponent.headerBuff.Count 是包含多个包的数据的, 余下 不到几个字节是下一个包的数据 ,不过没有接收完毕
                byte[] tmpBuf = new byte[invalidCount];
                Buffer.BlockCopy(socketComponent.recvBuff, headbufOffset, tmpBuf, 0, invalidCount);
                socketComponent.recvBuff = tmpBuf;

                socketComponent.hadRecvNum = invalidCount;

                return false;
            }

            byte[] onepacketbuff = new byte[socketComponent.needRecvNum]; // 单个packet 接收缓冲区

            Buffer.BlockCopy(socketComponent.recvBuff, headbufOffset, onepacketbuff, 0, onepacketbuff.Length);

            // 登录成功之前只处理 login
            if (socketComponent.IsLoginSuccess
                || socketComponent.cmdID == (int)TCPCMDS.LOGIN
                || socketComponent.cmdID == (int)TCPCMDS.HeartBeat)
            {
                // 投递
                TCPPacket packet = new TCPPacket(socketComponent.mOwner, socketComponent.cmdID, onepacketbuff, socketComponent.needRecvNum);
                CMDDispatcher.DispatcherTask(packet);
            }
            

            headbufOffset += socketComponent.needRecvNum;

            // 递归 栈溢出! 改成循环
            return DePacket(socketComponent, headbufOffset);
        }

        public static void ResetComponentCmdBuf(SocketComponent socketComponent)
        {
            socketComponent.recvBuff = null;
            socketComponent.hadRecvNum = 0;
            socketComponent.needRecvNum = 0;
        }
    }
}
