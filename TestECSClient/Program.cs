using Entity;
using Entity.Component;
using Singleton;
using Singleton.Manager;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SystemClient;
using SystemShare;

namespace TestECSClient
{
    class Program
    {

        public static SocketSystem socketSystem = new SocketSystem();
        public static MoveSystem moveSystem = new MoveSystem();
        public static BattleSystem battleSystem = new BattleSystem();
        public static LogInSystem logInSystem = new LogInSystem();
        static void Main(string[] args)
        {
            CMDDispatcher.Init(CmdHandlerType.Client);
            LoggerHelper.Instance().Init(CmdHandlerType.Client);
            ComponentAttributeHelper.Instance().Init();

            ServerClient.Instance().AddSystem(socketSystem);
            ServerClient.Instance().AddSystem(moveSystem);
            ServerClient.Instance().AddSystem(battleSystem);
            ServerClient.Instance().AddSystem(logInSystem);
            socketSystem.Init();



            IEntity entity = new SocketEntity();
            entity.mOwner = "client";
            //SocketComponent socketComponent = new SocketComponent();
            //socketComponent.mOwner = entity;
            //entity.AddComponent(typeof(SocketComponent), socketComponent);

            //string ip = "120.245.130.211";
            string ip = "127.0.0.1";
            int port = 8888;
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            SocketType socketType = SocketType.Stream;
            ProtocolType protocolType = ProtocolType.Tcp;

            socketSystem.RunClient(entity, iPEndPoint, socketType, protocolType, ip, port);

            SocketComponent socketComponent;
            while (true)
            {
                Thread.Sleep(1000 * 1);

                socketComponent = entity.GetComponent<SocketComponent>() as SocketComponent;
                if (null == socketComponent)
                {
                    continue;
                }

                if (!socketComponent.mSocketInvild)
                {
                    LoggerHelper.Instance().Log(LogType.Test, "Client connect server succcess!");
                }
                else
                {
                    break;
                }
            }

            Task.Run(Run);
            Action action = () =>
            {
                Thread.Sleep(10 * 1000);
                Console.WriteLine(ip);
                TestAction();
            };
            Task.Run(action);

            //Thread.Sleep(1000 * 2);

            //LoggerHelper.Instance().Log(LogType.Test, $"str:{Str.Length}");

            LoggerHelper.Instance().Log(LogType.Test, "Client Hello World!");

            while (true)
            {
                string cmd = Console.ReadLine();
                string[] cmds = cmd.Split(' ');

                if(cmds.Length < 1)
                {
                    continue;
                }

                switch (cmds[0])
                {
                    case "m":
                        {
                            Move(entity, socketComponent);
                        }
                        break;
                    case "a":
                        {
                            Attack(entity, socketComponent);
                        }
                        break;
                    case "l":
                        {
                            login(entity, socketComponent, cmds);
                        }
                        break;
                    default:
                        break;
                }

                
            }
        }
        public static void login(IEntity entity, SocketComponent socketComponent, string[] cmds)
        {
            if (cmds.Length < 3)
            {
                LoggerHelper.Instance().Log(LogType.Test, $"login params not match");
                return;
            }
            string str = $"{cmds[1]} {cmds[2]}";
            SocketSystem.SendAsync(socketComponent, TCPCMDS.LOGIN, str);
        }
        public static void Attack(IEntity entity, SocketComponent socketComponent)
        {
            SocketSystem.SendAsync(socketComponent, TCPCMDS.Attack, Str);
        }
        public static void Move(IEntity entity, SocketComponent socketComponent)
        {
            MoveComponent moveComponent = entity.GetComponent<MoveComponent>() as MoveComponent;

            if (null != moveComponent)
            {
                moveComponent.IsMoving = !moveComponent.IsMoving;
                LoggerHelper.Instance().Log(LogType.Test, $"client moveto:X-{moveComponent.X} Y-{moveComponent.Y} IsMoving-{moveComponent.IsMoving})");
            }

            SocketSystem.SendAsync(socketComponent, TCPCMDS.Move, Str);
        }

        public static void Run()
        {
            while (true)
            {
                ServerClient.Instance().Tick(DateTime.Now.Ticks);

                LoggerHelper.Instance().Log(LogType.Console, $"client main tick");

                Thread.Sleep(1000 * 2);
            }
        }

        public static void TestAction()
        {
            LoggerHelper.Instance().Log(LogType.Test, $"client TestAction");
        }

        public static string Str = $"hellohellohellohellohel";

        public static string Str2 = $"lohellohellohellohellohellohellohellohellohellohe" +
            $"llohellohellohel" +
            $"lohellohellohellohellohellohellohellohel" +
            $"lohellohellohellohellohellohellohellohel" +
            $"lohellohellohellohellohellohellohellohel" +
            $"lohellohellohellohellohellohellohellohel" +
            $"lohellohellohellohellohellohe" +
            $"llohellohellohellohellohellohellohellohe" +
            $"llohellohellohellohellohellohellohellohell" +
            $"ohellohellohellohellohellohellohellohellohe" +
            $"llohellohellohellohellohellohellohellohelloh" +
            $"ellohellohellohellohellohellohellohellohellohell" +
            $"ohellohellohellohellohellohellohellohellohellohe" +
            $"llohellohellohellohellohellohellohellohellohello" +
            $"hellohellohellohellohellohellohellohellohellohellohell" +
            $"ohellohellohellohellohellohellohellohell" +
            $"ohellohellohellohellohellohellohellohelloh" +
            $"ellohellohellohellohellohellohellohellohellohell" +
            $"ohellohellohellohellohellohellohellohellohellohe" +
            $"llohellohellohellohellohellohellohellohellohelloh" +
            $"ellohellohellohellohellohellohellohellohellohellohe" +
            $"llohellohellohellohellohellohellohellohellohellohelloh" +
            $"ellohellohellohellohellohellohellohellohellohellohelloh" +
            $"ellohellohellohellohellohellohellohellohellohellohell" +
            $"ohellohellohellohellohellohe" +
            $"llohellohellohellohellohellohellohello";
    }
}
