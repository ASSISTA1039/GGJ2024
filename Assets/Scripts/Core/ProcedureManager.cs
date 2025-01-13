using System;
using System.Collections.Generic;
using System.Reflection;
using QxFramework.Utilities;
using Type = System.Type;
using UnityEngine;

namespace QxFramework.Core
{
    /// <summary>
    /// 流程管理，管理整个软件使用流程，以及流程之间的切换。
    /// </summary>
    public class ProcedureManager : Singleton<ProcedureManager>, ISystemModule
    {
        /// <summary>
        /// 获取当前流程。
        /// </summary>
        public ProcedureBase Current
        {
            get
            {
                return _current;
            }
        }

        /// <summary>
        /// 获取上一个流程。
        /// </summary>
        public ProcedureBase Previous
        {
            get
            {
                return _previous;
            }
        }

        /// <summary>
        /// 当前的流程。
        /// </summary>
        private ProcedureBase _current;

        /// <summary>
        /// 上一个流程。
        /// </summary>
        private ProcedureBase _previous;

        private readonly Dictionary<Type, ProcedureBase> _procedureDictionary = new Dictionary<Type, ProcedureBase>();

        /// <summary>
        /// 初始化函数，实例化所有流程并完成初始化。
        /// </summary>
        public override void Initialize()
        {
            //获取所有流程名称
            string[] procedures = Utilities.TypeUtilities.GetTypeNames(typeof(ProcedureBase));

            //实例化所有流程对象
            for (int i = 0; i < procedures.Length; i++)
            {
                Type t = Type.GetType(procedures[i]);
                ProcedureBase pb = Activator.CreateInstance(t) as ProcedureBase;
                _procedureDictionary[t] = pb;

                //执行初始化函数
                pb.Init();
            }
        }

        /// <summary>
        /// 将流程切换至另一个流程。
        /// </summary>
        /// <typeparam name="T">流程类型。</typeparam>
        public void ChangeTo<T>(object args = null) where T : ProcedureBase
        {
            //切换至下一个流程
            ChangeTo(_procedureDictionary[typeof(T)], args);
        }

        /// <summary>
        /// 将流程切换至另一个流程。
        /// </summary>
        /// <param name="name">流程名。</param>
        public void ChangeTo(string name, object args = null)
        {
            foreach (KeyValuePair<Type, ProcedureBase> pair in _procedureDictionary)
            {
                if (pair.Key.Name == name)
                {
                    //切换至下一个流程
                    ChangeTo(_procedureDictionary[pair.Key], args);
                    return;
                }
            }
            //没有找到则抛出异常
            Debug.LogError("ProduceManager:Didn't find "+name);
            //throw new ArgumentException();
            return;
        }

        /// <summary>
        /// 将流程切换至另一个流程。
        /// </summary>
        /// <param name="pb">流程对象。</param>
        public void ChangeTo(ProcedureBase pb, object args = null)
        {
            //执行离开函数
            if (_current != null)
            {
                _current.Leave();
            }

            //保存上一个流程
            _previous = _current;

            if (_procedureDictionary.ContainsValue(pb))
            {
                //切换至下一个流程
                _current = pb;

                //执行进入函数
                pb.Enter(args);
            }
            else
            {
                //该流程不是默认创建的流程，抛出异常
                throw new ArgumentException();
            }
        }

        /// <summary>
        /// 更新流程。
        /// </summary>
        /// <param name="elapseSeconds">流逝时间，以秒为单位。</param>
        public void Update(float elapseSeconds)
        {
            if (_current != null)
            {
                _current.Update(elapseSeconds);
            }
        }

        /// <summary>
        /// 物理更新流程。
        /// </summary>
        /// <param name="elapseSeconds">流逝时间，以秒为单位。</param>
        public void FixedUpdate(float elapseSeconds)
        {
            if (_current != null)
            {
                _current.FixedUpdate(elapseSeconds);
            }
        }

        /// <summary>
        /// 退出时执行。
        /// </summary>
        public void Dispose()
        {
            foreach (KeyValuePair<Type, ProcedureBase> pair in _procedureDictionary)
            {
                pair.Value.Destroy();
            }
        }

        public T GetModule<T>() where T : Submodule
        {
            if (Current != null)
            {
                foreach (var module in Current.Submodules)
                {
                    if (module is T t)
                    {
                        return t;
                    }
                }
            }
            return null;
        }

        public bool TryGetModule<T>(out T module) where T : Submodule
        {
            if (Current != null)
            {
                foreach (var submodule in Current.Submodules)
                {
                    if (submodule is T t)
                    {
                        module = t;
                        return true;
                    }
                }
            }
            module = null;
            return false;
        }
    }
}