using System;
using System.Collections.Generic;
using System.Text;

namespace Entity.DataStruct
{
    public enum LoginState
    {
        Loging,
        Complete,
        TimeOut,
    }

    /// <summary>
    /// 实体需要创建的组件类型
    /// </summary>
    public enum ComponentType
    {
        /// <summary>
        /// 基础组件
        /// </summary>
        Base,
        /// <summary>
        /// 在 server 端 实体需要创建的组件
        /// </summary>
        Server,
        /// <summary>
        /// 在 client 端 实体需要创建的组件
        /// </summary>
        Client,
    }

    /// <summary>
    /// 该方法用于初始化哪个组件
    /// </summary>
    public enum InitComponentType
    {
        LoginComponent,
        MoveComponent,
        SocketComponent,
        TestComponent,
    }
}
