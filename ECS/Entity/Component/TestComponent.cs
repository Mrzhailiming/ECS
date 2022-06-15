using Entity.DataStruct;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Entity.Component
{
    /// <summary>
    /// 连接组件
    /// 1.socket
    /// </summary>
    [ComponentAttribute(mComponentType = ComponentType.Base, mInitComponentType = InitComponentType.TestComponent)]
    public class TestComponent : IComponent
    {
        public Socket mSocket;

        public string mName = "666";

        public IEntity mOwner { get; set; }
    }

}
