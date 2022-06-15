using Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Singleton.Manager
{
    public class TCPPacket
    {
        public object mOwner;
        public int mCmd;
        public byte[] mBuff;
        public int mTotalLen;
        public TCPPacket() { }
        public TCPPacket(object owner, int cmd, byte[] buf, int len)
        {
            mOwner = owner;
            mCmd = cmd;
            mBuff = buf;
            mTotalLen = len;
        }

        public T GetObject<T>()
        {
            return (T)mOwner;
        }

        public string GetString()
        {
            byte[] recv = new byte[mTotalLen - Proto.protoHeadLen];
            Buffer.BlockCopy(mBuff, Proto.protoHeadLen, recv, 0, mTotalLen - Proto.protoHeadLen);
            string recvstr = Encoding.Default.GetString(recv);

            return recvstr;
        }
    }
}
