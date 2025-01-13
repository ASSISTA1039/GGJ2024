using System.Collections.Generic;
using UnityEngine;

namespace QxFramework.Core
{
    /// <summary>
    /// 所有流程的基类。
    /// </summary>
    public abstract class ProcedureBase
    {
        private List<Submodule> _submodules = new List<Submodule>();
        public Submodule[] Submodules => _submodules.ToArray();

        /// <summary>
        /// 跳转流程
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void ChangeTo<T>() where T : ProcedureBase
        {
            ProcedureManager.Instance.ChangeTo<T>();
        }

        /// <summary>
        /// 跳转流程
        /// </summary>
        /// <param name="procedure"></param>
        public void ChangeTo(string procedure)
        {
            ProcedureManager.Instance.ChangeTo(procedure);
        }

        protected void AddSubmodule(Submodule submodule)
        {
            submodule.SetRootProcedure(this);
            _submodules.Add(submodule);
            submodule.Init();
        }
        

        public void Init()
        {
            OnInit();
        }
        /// <summary>
        ///进入该流程。
        /// </summary>
        public void Enter(object args)
        {
            OnEnter(args);
        }

        /// <summary>
        ///更新该流程。
        /// </summary>
        public void Update(float elapseSeconds)
        {
            foreach (var m in _submodules)
            {
                m.Update();
            }
            OnUpdate(elapseSeconds);
        }

        /// <summary>
        ///当进入该流程时执行。
        /// </summary>
        public void FixedUpdate(float elapseSeconds)
        {
            foreach (var m in _submodules)
            {
                m.FixedUpdate();
            }
            OnFixedUpdate(elapseSeconds);
        }

        /// <summary>
        ///当进入该流程时执行。
        /// </summary>
        public void Leave()
        {
            foreach (var m in _submodules)
            {
                m.Destory();
            }
            _submodules.Clear();
            OnLeave();
        }

        /// <summary>
        /// 程序退出时执行，在程序退出时会完成所有流程初始化。
        /// </summary>
        public void Destroy()
        {
            OnDestroy();
        }

        /// <summary>
        /// 程序初始化时执行，在打开程序时会完成所有流程初始化。
        /// </summary>
        protected virtual void OnInit()
        {
        }

        /// <summary>
        ///当进入该流程时执行。
        /// </summary>
        protected virtual void OnEnter(object args)
        {
        }

        /// <summary>
        /// 每次更新时执行。
        /// </summary>
        /// <param name="elapseSeconds">流逝时间，以秒为单位。</param>
        protected virtual void OnUpdate(float elapseSeconds)
        {
        }

        /// <summary>
        /// 每次物理更新时执行。
        /// </summary>
        /// <param name="elapseSeconds">流逝时间，以秒为单位。</param>
        protected virtual void OnFixedUpdate(float elapseSeconds)
        {
        }

        /// <summary>
        /// 每次离开这个流程时执行。
        /// </summary>
        protected virtual void OnLeave()
        {
        }

        /// <summary>
        /// 程序退出时执行，在程序退出时会完成所有流程初始化。
        /// </summary>
        protected virtual void OnDestroy()
        {
        }

    }
}