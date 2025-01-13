using System;
using System.Threading;
using UnityEngine;

namespace QxFramework.Utilities
{
    /// <summary>
    ///实现单例模式的模版类。
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    public abstract class Singleton<T> where T : new()
    {
        private static T _instance;
        private static object _lock = new object();

        /// <summary>
        ///实例
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    object lockObj = Singleton<T>._lock;
                    Monitor.Enter(lockObj);
                    try
                    {
                        if (_instance == null)
                        {
                            _instance = Activator.CreateInstance<T>(); // 需要导入 using System;
                        }
                    }
                    finally
                    {
                        Monitor.Exit(lockObj); // 需要导入 System.Threading;
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// 初始化函数。
        /// </summary>
        public virtual void Initialize()
        {
        }
    }

    /// <summary>
    /// 单例的MonoBehaviour。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;
        private static object _lock = new object();

        /// <summary>
        ///实例
        /// </summary>
        public static T Instance
        {
            get
            {
                try
                {
                    if (_instance == null)
                    {
                        object obj = MonoSingleton<T>._lock;
                        Monitor.Enter(obj);
                        _instance = GameObject.FindObjectOfType((typeof(T))) as T;
                        if (_instance == null)
                        {
                            _instance = new GameObject(typeof(T).Name, typeof(T)).GetComponent<T>();
                        }
                        Monitor.Exit(obj);
                    }

                    return _instance;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}