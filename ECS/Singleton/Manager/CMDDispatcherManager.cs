using Entity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Singleton.Manager
{

    public class CMDHandler
    {
        public Action<TCPPacket> _action;

    }
    public class CMDDispatcher : Singleton<CMDDispatcher>
    {
        const int semaphoreMaximumCount = int.MaxValue;//100个不够吧
        Semaphore semaphoreTask = new Semaphore(0, semaphoreMaximumCount);
        Semaphore semaphoreAction = new Semaphore(0, semaphoreMaximumCount);
        int count = 0;
        /// <summary>
        /// 记录线程处理了多少个数据包
        /// </summary>
        long taskCount = 0;
        long actionCount = 0;
        /// <summary>
        /// 同步处理数据包队列
        /// </summary>
        ConcurrentQueue<TCPPacket> _taskQueue = new ConcurrentQueue<TCPPacket>();
        /// <summary>
        /// 同步处理 action 队列
        /// </summary>
        ConcurrentQueue<Action> _actionQueue = new ConcurrentQueue<Action>();

        ThreadMode mThreadMode { get; set; }

        #region 执行命令包区分线程
        public ConcurrentDictionary<TCPCMDS, Action<TCPPacket>> cmd2action = new ConcurrentDictionary<TCPCMDS, Action<TCPPacket>>();

        static Dictionary<ThreadMode, CMDDispatcher> mode2dispather = new Dictionary<ThreadMode, CMDDispatcher>();
        static Dictionary<TCPCMDS, ThreadMode> cmd2mode = new Dictionary<TCPCMDS, ThreadMode>();
        #endregion

        public CMDDispatcher()
        {

        }

        public void Start()
        {
            Thread thread = new Thread(ExecuteTask);
            thread.Name = mThreadMode.ToString();
            thread.Start();
            Thread thread2 = new Thread(ExecuteAction);
            thread2.Name = mThreadMode.ToString();
            thread2.Start();
        }

        public static void Init(CmdHandlerType handlerType)
        {
            foreach (ThreadMode mode in Enum.GetValues(typeof(ThreadMode)))
            {
                Init(mode, 1, handlerType);
            }
        }
        public static void Init(ThreadMode mode, int num, CmdHandlerType handlerType)
        {
            var dispather = new CMDDispatcher();
            dispather.mThreadMode = mode;
            mode2dispather.Add(mode, dispather);
            GetAllAss(dispather, mode, handlerType);

            dispather.Start();
        }


        public delegate void Do(TCPPacket packet);

        private void ExecuteTask()
        {
            TCPPacket task = null;
            Action<TCPPacket> outHandler;
            TCPCMDS cmdID;
            //使用生产者消费者模型?
            while (true)
            {
                try
                {
                    semaphoreTask.WaitOne();
                    //
                    if (_taskQueue.TryDequeue(out task))
                    {
                        cmdID = (TCPCMDS)Byte2Int(task.mBuff, 0);

                        if (!cmd2action.TryGetValue(cmdID, out outHandler))
                        {
                            LoggerHelper.Instance().Log(LogType.CmdInfo, $"CMDDispatcher cmdID:{cmdID} handler null mThreadMode:{mThreadMode}");
                        }
                        else
                        {
                            LoggerHelper.Instance().Log(LogType.CmdInfo, $"CMDDispatcher Dequeue cmdID:{cmdID} mThreadMode:{mThreadMode}");

                            try
                            {
                                outHandler.Invoke(task);
                            }
                            catch (Exception ex)
                            {
                                LoggerHelper.Instance().Log(LogType.Exception, ex.ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Instance().Log(LogType.Exception, ex.ToString());
                }
                finally
                {
                    task = null;
                    outHandler = null;
                    Interlocked.Increment(ref count);
                    LoggerHelper.Instance().Log(LogType.CmdInfo, $"mThreadMode:{mThreadMode} CurrentThreadname:{Thread.CurrentThread.Name}  ProcesseTaskCount:{++taskCount} packetIndex:{count}");
                }
            }
        }

        private void ExecuteAction()
        {
            Action action = null;
            //使用生产者消费者模型?
            while (true)
            {
                try
                {
                    semaphoreAction.WaitOne();
                    LoggerHelper.Instance().Log(LogType.Test, $"async action count:{_actionQueue.Count}");
                    //
                    if (_actionQueue.TryDequeue(out action))
                    {
                        try
                        {
                            action.Invoke();
                        }
                        catch (Exception ex)
                        {
                            LoggerHelper.Instance().Log(LogType.Exception, ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Instance().Log(LogType.Exception, ex.ToString());
                }
                finally
                {
                    action = null;
                    LoggerHelper.Instance().Log(LogType.CmdInfo, $"mThreadMode:{mThreadMode} CurrentThreadname:{Thread.CurrentThread.Name}  ProcesseActionCount:{++actionCount}");
                }
            }
        }

        public static void DispatcherTask(TCPPacket task)
        {
            TCPCMDS cmdID = (TCPCMDS)task.mCmd;

            if (!cmd2mode.TryGetValue(cmdID, out ThreadMode mode))
            {
                LoggerHelper.Instance().Log(LogType.CmdInfo, $"Dispatcher task cmd:{cmdID} ThreadMode:{mode} action 未注册");
            }
            if (!mode2dispather.TryGetValue(mode, out CMDDispatcher dispatcher))
            {
                LoggerHelper.Instance().Log(LogType.CmdInfo, $"Dispatcher task cmd:{cmdID} ThreadMode:{mode} dispatcher 未注册");
            }

            dispatcher?.EnqueueTask(task);

            LoggerHelper.Instance().Log(LogType.CmdInfo, $"Enqueue task cmd:{cmdID} ThreadMode:{mode}");
        }
        public static void DispatcherAction(ThreadMode mode, Action action)
        {
            if (!mode2dispather.TryGetValue(mode, out CMDDispatcher dispatcher))
            {
                LoggerHelper.Instance().Log(LogType.CmdInfo, $"DispatcherAction ThreadMode:{mode} dispatcher 未注册");
            }

            dispatcher?.EnqueueAction(action);

            LoggerHelper.Instance().Log(LogType.CmdInfo, $"DispatcherAction ThreadMode:{mode}");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        public void EnqueueTask(TCPPacket task)
        {
            _taskQueue.Enqueue(task);
            semaphoreTask.Release();
        }
        public void EnqueueAction(Action action)
        {
            _actionQueue.Enqueue(action);
            semaphoreAction.Release();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dispather"></param>
        /// <param name="mode"></param>
        /// <exception cref="Exception"></exception>
        public static void GetAllAss(CMDDispatcher dispather, ThreadMode mode, CmdHandlerType handlerType)
        {
            string path = Directory.GetCurrentDirectory();

            var ass = Global.Global.LoadAllAssembly(path);
            if (null == ass || ass.Count < 1)
            {
                LoggerHelper.Instance().Log(LogType.Test, $"no assembly");
                return;
            }

            foreach (var assembly in ass)
            {
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                    foreach (var method in methods)
                    {
                        var attrs = method.GetCustomAttributes(typeof(CmdHandlerAttribute), true).ToList();

                        if (null == attrs || attrs.Count < 1)
                        {
                            continue;
                        }

                        CmdHandlerAttribute att = attrs[0] as CmdHandlerAttribute;

                        if (null == att)
                        {
                            continue;
                        }

                        if (att.mCmdHandlerType != handlerType
                            && att.mCmdHandlerType != CmdHandlerType.Both)
                        {
                            continue;
                        }

                        if (att.mThreadMode != mode)
                        {
                            continue;
                        }

                        var mt = method.CreateDelegate(typeof(Do));

                        // 创建委托
                        var action = Delegate.CreateDelegate(typeof(Action<TCPPacket>), method) as Action<TCPPacket>;

                        if (null == action)
                        {
                            string msg = $"cmd:{att.mTCPCMDS} create action failed";
                            LoggerHelper.Instance().Log(LogType.CmdInfo, msg);
                            throw new Exception(msg);
                        }

                        if (!dispather.cmd2action.TryAdd(att.mTCPCMDS, action))
                        {
                            string msg = $"cmd:{att.mTCPCMDS} action 重复";
                            LoggerHelper.Instance().Log(LogType.CmdInfo, msg);
                            throw new Exception(msg);
                        }

                        if (!cmd2mode.TryAdd(att.mTCPCMDS, mode))
                        {
                            string msg = $"cmd:{att.mTCPCMDS} 重复";
                            LoggerHelper.Instance().Log(LogType.CmdInfo, msg);
                            throw new Exception(msg);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 确保 从offset 到len 有 4 字节
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static int Byte2Int(byte[] buf, int offset)
        {
            return ((buf[3 + offset] & 0xff) << 24) | ((buf[2 + offset] & 0xff) << 16) | ((buf[1 + offset] & 0xff) << 8) | (buf[0 + offset] & 0xff);
        }
    }
}