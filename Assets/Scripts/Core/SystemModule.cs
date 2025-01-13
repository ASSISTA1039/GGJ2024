using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QxFramework.Core
{
    /// <summary>
    /// 系统组件，由Laucher管理
    /// </summary>
    public interface ISystemModule
    {
        /// <summary>
        /// 初始化函数
        /// </summary>
        void Initialize();

        /// <summary>
        /// 每帧更新
        /// </summary>
        /// <param name="deltaTime">帧间隔</param>
        void Update(float deltaTime);

        /// <summary>
        /// 物理每帧更新
        /// </summary>
        /// <param name="deltaTime">帧间隔</param>
        void FixedUpdate(float deltaTime);

        /// <summary>
        /// 当程序退出时
        /// </summary>
        void Dispose();
    }
}