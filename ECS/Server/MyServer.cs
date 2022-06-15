using Entity;
using Singleton;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using SystemShare;

namespace Server
{
    /// <summary>
    /// 管理所有的System
    /// </summary>
    public class MyServer : Singleton<MyServer>
    {
        public ConcurrentDictionary<long, ISystem> mSystems = new ConcurrentDictionary<long, ISystem>();

        /// <summary>
        /// 侦听socket
        /// </summary>
        public SocketSystem mSocketSystem;

        public void Run(SocketSystem socketSystem, IEntity entity, IPEndPoint iPEndPoint, SocketType socketType, ProtocolType protocolType
            , string ip, int port, int backlog)
        {
            mSocketSystem = socketSystem;
            mSocketSystem.Init();

            mSocketSystem.RunServer(entity, iPEndPoint, socketType, protocolType, ip, port, backlog);
        }

        public void AddSystem(ISystem system)
        {
            if(!mSystems.TryAdd(mSystems.Count + 1, system))
            {
                LoggerHelper.Instance().Log(LogType.Test, $"AddSystem failed {system}");
            }
            else
            {
                LoggerHelper.Instance().Log(LogType.Test, $"AddSystem success {system}");
            }
        }

        public void Tick(long tick)
        {
            var ids = mSystems.Keys;
            // 防止迭代器失效
            foreach (long id in ids)
            {
                var system = mSystems[id];
                foreach (IEntity entity in EntityManager.Instance())
                {
                    system?.Tick(tick, entity);
                }
            }
        }
    }
}
