using CommandLine;
using Entity;
using Entity.Component;
using Entity.Table;
using Global;
using Server;
using Singleton;
using Singleton.Manager;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SystemServer;
using SystemShare;

namespace TestECSServer
{
    class Program
    {
        /// <summary>
        /// ECSServer
        /// 实体没有函数
        /// 系统没有字段
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) => { LoggerHelper.Instance().Log(LogType.Exception, $"{e.ExceptionObject}"); };
            CMDDispatcher.Init(CmdHandlerType.Server);
            LoggerHelper.Instance().Init(CmdHandlerType.Server);
            DBManager.Instance().Init("localhost", 3306, "root", "123456", "test", 50);

            ComponentAttributeHelper.Instance().Init();
            Data2objectAttributeHelper.Instance().Init();
            TableAttributeHelper.Instance().Init();

            DBManager.Instance().Load<TableLogIn>(new SocketEntity() { mOwner = "root"});

            //SqlHelper.Test();

            //RunAsync();
            //PrintThread("main");

            //Options options = null;
            //Parser.Default.ParseArguments<Options>(args)
            //    .WithNotParsed(error => throw new Exception($"命令行格式错误!"))
            //    .WithParsed(o => { options = o; });

            IEntity entity = new SocketEntity();
            entity.mOwner = "server";
            // 服务器的监听组件
            SocketComponent socketComponent = new SocketComponent();
            socketComponent.mOwner = entity;
            entity.AddComponent(socketComponent);

            string ip = "127.0.0.1";
            int port = 8888;
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            SocketType socketType = SocketType.Stream;
            ProtocolType protocolType = ProtocolType.Tcp;
            int backlog = 10;

            SocketSystem socketSystem = new SocketSystem();
            TestSysytem testSysytem = new TestSysytem();
            LogInSystem logInSystem = new LogInSystem();
            MoveSystem moveSystem = new MoveSystem();

            MyServer.Instance().AddSystem(testSysytem);
            MyServer.Instance().AddSystem(socketSystem);
            MyServer.Instance().AddSystem(logInSystem);
            MyServer.Instance().AddSystem(moveSystem);
            MyServer.Instance().AddSystem(new BattleSystem());


            MyServer.Instance().Run(socketSystem, entity, iPEndPoint, socketType, protocolType, ip, port, backlog);

            //// 模拟多线程投递action
            //Task.Run(Enqueue);
            //Task.Run(Enqueue);
            //Task.Run(Enqueue);
            //Task.Run(Enqueue);

            while (true)
            {
                MyServer.Instance().Tick(DateTime.Now.Ticks);

                LoggerHelper.Instance().Log(LogType.Console, $"server main tick");

                Thread.Sleep(1000 * 2);
            }

            Console.ReadKey();
        }

        public static void Enqueue()
        {
            while (true)
            {
                int id = Thread.CurrentThread.ManagedThreadId;
                Action action = () => { Console.WriteLine($"Enqueue thread:{id}"); };

                CMDDispatcher.DispatcherAction(ThreadMode.Battle, action);

                Thread.Sleep(10);
            }
        }

        public static async void RunAsync()
        {
            PrintThread("RunAsync begin");
            Data task = await TestAsync();
            PrintThread($"RunAsync end res :{task.name}");
        }

        public class Data
        {
            public string name;
        }

        public static Task<Data> TestAsync()
        {
            TaskCompletionSource<Data> @bool = new TaskCompletionSource<Data>();
            PrintThread("TestAsync");

            Thread thread = new Thread(() => Wait(@bool));
            thread.Start();
            return @bool.Task;
        }

        public static void Wait(TaskCompletionSource<Data> @bool)
        {
            PrintThread("Wait");
            Thread.Sleep(3 * 1000);
            Data data = new Data() { name = $"curID:{Thread.CurrentThread.ManagedThreadId}" };
            @bool.SetResult(data);
        }

        public static void PrintThread(string pre)
        {
            LoggerHelper.Instance().Log(LogType.Info, $"{pre} curThreadID:{Thread.CurrentThread.ManagedThreadId}");
        }

    }

    public class Options
    {
        [Option("mOwner", Required = false, Default = "默认")]
        public string mOwner { get; }
    }
}
