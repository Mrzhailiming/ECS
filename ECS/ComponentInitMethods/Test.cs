using Entity;
using Entity.Component;
using Entity.DataStruct;
using Singleton;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ComponentInitMethods
{
    public class Test
    {
        [InitComponentAttribute(mInitComponentType = InitComponentType.SocketComponent)]
        public static void InitSocketComponent(params object[] objects)
        {
            try
            {
                IEntity entity = objects[0] as IEntity;
                SocketAsyncEventArgs sendArg = objects[1] as SocketAsyncEventArgs;
                SocketAsyncEventArgs recvArg = objects[2] as SocketAsyncEventArgs;


                SocketComponent socketComponent = new SocketComponent();
                socketComponent.mSendSocketArg = sendArg;

                socketComponent.mRecvSocketArg = recvArg;

                socketComponent.mSocket = sendArg.AcceptSocket;
                socketComponent.mSocketInvild = true;
                socketComponent.mHeartBeatTicks = DateTime.Now.Ticks;

                sendArg.UserToken = socketComponent;
                recvArg.UserToken = socketComponent;

                socketComponent.mOwner = entity;
                string userName = socketComponent.mSocket.RemoteEndPoint.ToString();

                entity.AddComponent(typeof(SocketComponent), socketComponent);
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance().Log(LogType.Exception, $"{ex}");
            }
        }


        [InitComponentAttribute(mInitComponentType = InitComponentType.LoginComponent)]
        public static void InitLoginComponent(params object[] objects)
        {
            try
            {
                IEntity entity = objects[0] as IEntity;
                SocketAsyncEventArgs sendArg = objects[1] as SocketAsyncEventArgs;
                SocketAsyncEventArgs recvArg = objects[2] as SocketAsyncEventArgs;

                LoginComponent loginComponent = new LoginComponent();
                loginComponent.mOwner = entity;
                loginComponent.mLoginState = LoginState.Loging;

                entity.AddComponent(typeof(LoginComponent), loginComponent);
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance().Log(LogType.Exception, $"{ex}");
            }
        }

        [InitComponentAttribute(mInitComponentType = InitComponentType.TestComponent)]
        public static void InitTestComponent(params object[] objects)
        {
            try
            {
                IEntity entity = objects[0] as IEntity;
                SocketAsyncEventArgs sendArg = objects[1] as SocketAsyncEventArgs;
                SocketAsyncEventArgs recvArg = objects[2] as SocketAsyncEventArgs;

                TestComponent testComponent = new TestComponent();
                testComponent.mSocket = sendArg.AcceptSocket;

                entity.AddComponent(typeof(TestComponent), testComponent);
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance().Log(LogType.Exception, $"{ex}");
            }
        }
        [InitComponentAttribute(mInitComponentType = InitComponentType.MoveComponent)]
        public static void InitMoveComponent(params object[] objects)
        {
            try
            {
                IEntity entity = objects[0] as IEntity;
                SocketAsyncEventArgs sendArg = objects[1] as SocketAsyncEventArgs;
                SocketAsyncEventArgs recvArg = objects[2] as SocketAsyncEventArgs;

                MoveComponent moveComponent= new MoveComponent();
                moveComponent.mOwner = entity;
                moveComponent.X = 6;
                moveComponent.Y = 6;

                entity.AddComponent(typeof(MoveComponent), moveComponent);
            }
            catch (Exception ex)
            {
                LoggerHelper.Instance().Log(LogType.Exception, $"{ex}");
            }
        }
    }
}
