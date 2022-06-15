using Entity;
using Singleton;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using SystemShare;

namespace SystemClient
{
    /// <summary>
    /// 管理所有的System
    /// </summary>
    public class ServerClient : Singleton<ServerClient>
    {
        public List<ISystem> mSystems = new List<ISystem>();

        public void AddSystem(ISystem system)
        {
            mSystems.Add(system);
            LoggerHelper.Instance().Log(LogType.Console, $"AddSystem");
        }

        public void Tick(long tick)
        {
            foreach (ISystem system in mSystems)
            {
                foreach (Entity.IEntity entity in EntityManager.Instance())
                {
                    system.Tick(tick, entity);
                }
            }
        }
    }
}
