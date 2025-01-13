using System;
using System.Collections;
using System.Collections.Generic;
using QxFramework.Utilities;
using UnityEngine;

namespace QxFramework.Core
{

    /// <summary>
    /// 消息管理，提供程序逻辑监听、抛出事件的机制，负责界面和功能实现的通信。
    /// </summary>
    public class MessageManager : Singleton<MessageManager>, ISystemModule
    {
        #region Private Members

        /// <summary>
        /// 类型与消息队列的字典。
        /// </summary>
        private Dictionary<Type, IMessageQueue> _queues;

        #endregion Private Members

        /// <summary>
        /// 获得一个消息队列。
        /// </summary>
        /// <typeparam name="TEventType">事件类型的枚举。</typeparam>
        /// <param name="immediate">接受到消息时立即执行，不进入队列。若为false，则消息在下一帧开始逐个执行。</param>
        /// <returns>消息队列。</returns>
        public MessageQueue<TEventType> Get<TEventType>(bool immediate = true) where TEventType : struct
        {
            //获得事件类型
            Type type = typeof(TEventType);
            if (_queues == null)
            {
                Initialize();
            }

            //检查是否已存在
            if (_queues.ContainsKey(type))
            {
                return _queues[type] as MessageQueue<TEventType>;
            }
            else
            {
                //生成新的队列并初始化
                MessageQueue<TEventType> mq = new MessageQueue<TEventType>();
                mq.Initialize(immediate);
                _queues.Add(type, mq);
                return mq;
            }
        }

        /// <summary>
        /// 移除所有跟这个对象有关系的队列
        /// </summary>
        /// <param name="this">The this.</param>
        public void RemoveAbout(object target)
        {
            if (_queues != null)
            {
                foreach (KeyValuePair<Type, IMessageQueue> pair in _queues)
                {
                    pair.Value.RemoveAbout(target);
                }
            }
        }

        /// <summary>
        /// 释放指定消息类型的消息队列。
        /// </summary>
        /// <typeparam name="TEventType">消息类型。</typeparam>
        public void Dispose<TEventType>() where TEventType : struct
        {
            //获得事件类型
            Type type = typeof(TEventType);
            if (_queues.ContainsKey(type))
            {
                Get<TEventType>().Dispose();
                _queues.Remove(typeof(TEventType));
            }
        }

        /// <summary>
        /// 释放指定消息队列。
        /// </summary>
        /// <param name="mq">队列对象</param>
        public void Dispose(IMessageQueue mq)
        {
            System.Type type = null;
            foreach (KeyValuePair<Type, IMessageQueue> pair in _queues)
            {
                if (pair.Value == mq)
                {
                    type = pair.Key;
                    break;
                }
            }
            //没有找到对应的队列
            if (type != null)
            {
                _queues.Remove(type);
            }
            else
            {
                throw new ArgumentNullException();
            }
        }

        /// <summary>
        /// 初始化函数。
        /// </summary>
        public override void Initialize()
        {
            //创建字典
            _queues = new Dictionary<Type, IMessageQueue>();
        }

        /// <summary>
        /// 推动所有消息队列。
        /// </summary>
        public void Update(float deltaTime)
        {
            foreach (KeyValuePair<Type, IMessageQueue> pair in _queues)
            {
                pair.Value.Update();
            }
        }

        /// <summary>
        /// 释放所有消息队列。
        /// </summary>
        public void Dispose()
        {
            foreach (KeyValuePair<Type, IMessageQueue> pair in _queues)
            {
                pair.Value.Dispose();
            }
            _queues.Clear();
        }

        public void FixedUpdate(float deltaTime)
        {
        }
    }
}