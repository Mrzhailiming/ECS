using System;

namespace Entity
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class CmdHandlerAttribute : Attribute
    {
        public ThreadMode mThreadMode { get; set; } = ThreadMode.Normal;

        public TCPCMDS mTCPCMDS { get; set; }
        public CmdHandlerType mCmdHandlerType { get; set; }
        
    }
}
