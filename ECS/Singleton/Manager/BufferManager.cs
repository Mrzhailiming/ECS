using System;
using System.Collections.Generic;
using System.Text;

namespace Singleton.Manager
{
    

    public class BufferManager : Singleton<BufferManager>
    {
        public byte[] SendTotalBuffer { get; } = new byte[PerBufLen * MaxConnect];
        public byte[] RecvTotalBuffer { get; } = new byte[PerBufLen * MaxConnect];
        public int SendConnectCount { get; set; } = 0;
        public int RecvConnectCount { get; set; } = 0;

        public const int PerBufLen = 1024 * 1024;
        public const int MaxConnect = 100;

        public Dictionary<int, bool> SendBufUseMap { get; set; } = new Dictionary<int, bool>();
        public Dictionary<int, bool> RecvBufUseMap { get; set; } = new Dictionary<int, bool>();

    }
}
