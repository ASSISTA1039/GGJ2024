using System.Collections.Generic;
using QxFramework.Utilities;
using UnityEngine;

namespace QxFramework.Core
{
    /// <summary>
    /// 程序入口，对各项管理类进行初始化操作。
    /// </summary>
    public class Launcher : MonoSingleton<Launcher>
    {
        /// <summary>
        /// 入口流程。
        /// </summary>
        public string StartProcedure;

        /// <summary>
        /// 系统组件列表
        /// </summary>
        private List<ISystemModule> _modules;

        /// <summary>
        ///  初始化各项全局管理。
        /// </summary>
        private void Awake()
        {
            _modules = new List<ISystemModule>();

            _modules.Add(ProcedureManager.Instance);
            _modules.Add(MessageManager.Instance);
            _modules.Add(ResourceManager.Instance);
            _modules.Add(LevelManager.Instance);

            for (int i = 0; i < _modules.Count; i++)
            {
                _modules[i].Initialize();
            }
                        
        }

        /// <summary>
        ///  开始流程。
        /// </summary>
        private void Start()
        {
            ProcedureManager.Instance.ChangeTo(StartProcedure);
        }

        /// <summary>
        /// Unity每帧更新。
        /// </summary>
        private void Update()
        {
            for (int i = 0; i < _modules.Count; i++)
            {
                _modules[i].Update(Time.deltaTime);
            }
        }

        /// <summary>
        /// Unity每帧更新。
        /// </summary>
        private void FixedUpdate()
        {
            for (int i = 0; i < _modules.Count; i++)
            {
                _modules[i].FixedUpdate(Time.fixedDeltaTime);
            }
        }

        /// <summary>
        /// 当退出时调用。
        /// </summary>
        private void OnDestroy()
        {
            for (int i = 0; i < _modules.Count; i++)
            {
                _modules[i].Dispose();
            }
        }
    }
}