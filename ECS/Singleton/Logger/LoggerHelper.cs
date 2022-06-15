
using Entity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Singleton
{

    public enum LogType
    {
        Console,
        Info,
        Exception,
        HeartBeat,
        Fatal,
        CmdInfo,
        System,
        Test,
    }

    public class LoggerHelper : Singleton<LoggerHelper>
    {
        ConcurrentQueue<LogMessage> mMessageQueue = new ConcurrentQueue<LogMessage>();

        Dictionary<LogType, FileStream> mStreams = new Dictionary<LogType, FileStream>();

        public LoggerHelper()
        {
        }
        public void Init(CmdHandlerType type)
        {
            InitStream(type.ToString());

            Thread thread = new Thread(Execute);
            thread.Name = "LoggerHelper";

            thread.Start();
        }

        private void InitStream(string cs)
        {
            string exePath = $"{Directory.GetCurrentDirectory()}\\{cs}\\Log";

            if (!Directory.Exists(exePath))
            {
                Directory.CreateDirectory(exePath);
            }


            foreach (LogType type in Enum.GetValues(typeof(LogType)))
            {
                string fileFullPath = $"{exePath}\\{type}.txt";

                //if (!File.Exists(fileFullPath))
                //{
                //    File.Create(fileFullPath);
                //}

                FileStream stream = new FileStream(fileFullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                mStreams.Add(type, stream);
            }
        }
        public void Log(LogType logType, string msg)
        {
            string str = $"{DateTime.Now} {logType} {msg}";
            if (logType == LogType.Test)
            {
                Console.WriteLine(str);
            }
            mMessageQueue.Enqueue(new LogMessage(logType, str));
        }

        private void Execute()
        {
            while (true)
            {
                LogMessage message;
                while (mMessageQueue.TryDequeue(out message))
                {
                    OutPut(message);
                }

                foreach (var stream in mStreams.Values)
                {
                    stream.Flush();
                }

                Thread.Sleep(5 * 1000);
            }
        }

        private void OutPut(LogMessage message)
        {
            FileStream stream;
            mStreams.TryGetValue(message.LogType, out stream);

            

            if (null == stream)
            {
                return;
            }

            byte[] buff = Encoding.Default.GetBytes($"{message.Message}\r\n");

            stream.WriteAsync(buff);
        }
    }

    internal class LogMessage
    {
        public LogType LogType { get; }

        public string Message { get; }

        public LogMessage(LogType logType, string msg)
        {
            LogType = logType;
            Message = msg;
        }
    }
}
