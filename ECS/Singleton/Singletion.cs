using System;
using System.Diagnostics;

namespace Singleton
{
    public class Singleton<T> where T : class, new()
    {
        private static T mInstance = default(T);

        private static readonly object mMutex = new object();
        [DebuggerHiddenAttribute]
        public static T Instance()
        {
            if (null == mInstance)
            {
                lock (mMutex)
                {
                    if (null == mInstance)
                    {
                        mInstance = Activator.CreateInstance(typeof(T), false) as T;
                    }
                }
            }

            return mInstance;
        }


    }
}
