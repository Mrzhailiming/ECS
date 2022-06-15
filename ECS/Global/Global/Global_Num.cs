using Entity;
using Entity.Component;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace Global
{
    public partial class Global
    {

        public static List<int> Parse2Listint(string str)
        {
            List<int> ret = new List<int>();
            string[] strs = str.Split(',');
            if (null == strs || strs.Length < 1)
            {
                return ret;
            }

            foreach (string item in strs)
            {
                if (int.TryParse(item, out int i))
                {
                    ret.Add(i);
                }
            }

            return ret;
        }

        public static PlayerInfo Parse2playerinfo(string str)
        {
            PlayerInfo playerInfo = new PlayerInfo();
            

            return playerInfo;
        }

        public static List<Assembly> LoadAllAssembly(string path = "")
        {
            if (string.IsNullOrEmpty(path))
            {
                path = Directory.GetCurrentDirectory();
            }

            List<Assembly> ret = new List<Assembly>();

            DirectoryInfo root = new DirectoryInfo(path);
            FileInfo[] files = root.GetFiles();

            foreach (FileInfo file in files)
            {
                if (!file.Name.Contains(".dll") || file.Name.Contains("MySql.Data.dll")
                    /*|| !file.Name.Contains("Entity.dll")*/)
                {
                    continue;
                }
                var assembly = Assembly.LoadFrom(file.FullName);

                AppDomain.CurrentDomain.Load(assembly.GetName());

                ret.Add(assembly);
            }

            return ret;
        }
        /// <summary>
        /// 获取所有 T 特性的 type
        /// </summary>
        public static List<Type> GetAllType<T>(string path = "") where T : Attribute
        {
            return GetAllType<T>(LoadAllAssembly(path));
        }
        /// <summary>
        /// 获取所有 type
        /// </summary>
        public static List<Type> GetAllTypes(string path = "") 
        {
            List<Type> types = new List<Type>();
            var ass = LoadAllAssembly(path);

            foreach(var assembly in ass)
            {
                types.AddRange(assembly.GetTypes());
            }

            return types;
        }

        //public static List<Type> GetAllType(Type attrtype) 
        //{
        //    return GetAllType(LoadAllAssembly(""), attrtype);
        //}
        //public static List<Type> GetAllType(List<Assembly> ass, Type attrtype)
        //{
        //    List<Type> ret = new List<Type>();
        //    //ass = AppDomain.CurrentDomain.GetAssemblies().ToList();

        //    // 获取所有 ComponentAttribute 的类
        //    foreach (var assembly in ass)
        //    {
        //        var types = assembly.GetTypes();

        //        foreach (var type in types)
        //        {
        //            var attr = type.GetCustomAttribute(attrtype);
        //            var attr2 = type.GetCustomAttributes(attrtype).ToList();
                    
        //            if (null == attr)
        //            {
        //                continue;
        //            }

        //            ret.Add(type);
        //        }
        //    }

        //    return ret;
        //}
        /// <summary>
        /// 获取所有 T 特性的 type
        /// </summary>
        public static List<Type> GetAllType<T>(List<Assembly> ass) where T : Attribute
        {
            List<Type> ret = new List<Type>();

            // 获取所有 ComponentAttribute 的类
            foreach (var assembly in ass)
            {
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    var attr = type.GetCustomAttribute(typeof(T), true);

                    if (null == attr)
                    {
                        continue;
                    }

                    ret.Add(type);
                }
            }

            return ret;
        }

        public static DateTime Now()
        {
            return DateTime.Now;
        }

        /// <summary>
        /// 设置cmd
        /// </summary>
        void SetCMD(byte[] srcBuf, int srcOffset, byte[] tarBuf, int tarOffset, int count)
        {
            Array.Copy(srcBuf, srcOffset, tarBuf, tarOffset, count);
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


        public static void SendAsync(SocketComponent socketComponent, TCPCMDS cmd, string str)
        {
            if (null == socketComponent || null == socketComponent.mSocket)
            {
                //LoggerHelper.Instance().Log(LogType.Console, $"SocketSystem SendAsync {null == socketComponent} || {null == socketComponent.mSocket}");
                return;
            }

            SendAsync(socketComponent, GenBuf(cmd, str));
        }

        public static void SendAsync(SocketComponent socketComponent, byte[] buf)
        {
            if (null == socketComponent || null == socketComponent.mSocket)
            {
                //LoggerHelper.Instance().Log(LogType.Console, $"SocketSystem SendAsync {null == socketComponent} || {null == socketComponent.mSocket}");
                return;
            }

            SetBuffer(socketComponent.mSendSocketArg, buf);

            if (!socketComponent.mSocket.SendAsync(socketComponent.mSendSocketArg))
            {
                socketComponent.mIsCompleteSend = true;
                //LoggerHelper.Instance().Log(LogType.Console, $"send sync");
            }
        }

        public static void SetBuffer(SocketAsyncEventArgs AsyncEventArgs, byte[] buf)
        {
            Buffer.BlockCopy(buf, 0, AsyncEventArgs.Buffer, AsyncEventArgs.Offset, buf.Length);

            AsyncEventArgs.SetBuffer(AsyncEventArgs.Offset, buf.Length);
        }

        public static byte[] GenBuf(TCPCMDS cmd, string str)
        {
            byte[] bodyBuf = Encoding.Default.GetBytes(str);
            int packetLen = Proto.protoHeadLen + bodyBuf.Length;

            byte[] buf = new byte[packetLen];

            // cmd
            Array.Copy(BitConverter.GetBytes((int)cmd), 0, buf, Proto.cmdIDOffset, 4);
            // paklen
            Array.Copy(BitConverter.GetBytes(packetLen), 0, buf, Proto.PacketLenOffset, 4);
            // body
            Array.Copy(bodyBuf, 0, buf, Proto.protoHeadLen, bodyBuf.Length);

            return buf;
        }
    }
}
