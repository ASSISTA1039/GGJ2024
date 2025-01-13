using System;
using System.Collections.Generic;
using UnityEngine;

namespace QxFramework.Core
{
    /// <summary>
    /// 消息管理队列，用于分发消息。
    /// </summary>
    /// <typeparam name="TEventType">事件类型的枚举。</typeparam>
    public class MessageQueue<TEventType> : IMessageQueue where TEventType : struct
    {
        #region Private Members

        private bool _immediate;

        private Dictionary<TEventType, MessageHandlerCollection> _handlerMap;

        private Queue<Message> _messageQueue;

        #endregion Private Members

        #region Public Methods

        /// <summary>
        /// 注册一个消息处理方法。
        /// </summary>
        /// <param name="msgId">消息ID</param>
        /// <param name="callback">接受到消息的回调</param>
        public void RegisterHandler(TEventType msgId, EventHandler<EventArgs> callback)
        {
            if (callback == null)
            {
                return;
            }

            if (_handlerMap.ContainsKey(msgId))
            {
                _handlerMap[msgId].AddHandler(callback);
            }
            else
            {
                var mhc = new MessageHandlerCollection();
                mhc.AddHandler(callback);
                _handlerMap.Add(msgId, mhc);
            }
        }

        /// <summary>
        /// 移除指定消息处理方法。
        /// </summary>
        /// <param name="msgId">消息ID</param>
        /// <param name="callback">添加时的回调</param>
        public void RemoveHandler(TEventType msgId, EventHandler<EventArgs> callback)
        {
            if (callback == null)
            {
                return;
            }

            if (_handlerMap.ContainsKey(msgId))
            {
                var mhc = _handlerMap[msgId];
                mhc.RemoveHandler(callback);

                if (mhc.Count == 0)
                {
                    _handlerMap.Remove(msgId);
                }
            }
        }

        /// <summary>
        /// 移除指定消息处理方法。
        /// </summary>
        /// <param name="obj">消息的处理对象</param>
        public void RemoveAbout(object obj)
        {
            if (obj == null)
            {
                return;
            }

            foreach (var mhc in _handlerMap)
            {
                mhc.Value.RemoveAbout(obj);
            }
        }

        /// <summary>
        /// 移除指定消息处理方法。
        /// </summary>
        public void RemoveAll()
        {
            _handlerMap.Clear();
         }


        /// <summary>
        /// 向所有注册的处理方法分发指定消息。
        /// </summary>
        /// <param name="msgId">消息ID</param>
        /// <param name="sender">消息发送者</param>
        /// <param name="param">消息参数</param>
        /// <param name="immediate">非直接</param>
        public void DispatchMessage(TEventType msgId, object sender, EventArgs param = null,bool immediate = true)
        {
            if (!_handlerMap.ContainsKey(msgId))
            {
                return;
            }

            var msg = new Message()
            {
                Id = msgId,
                Sender = sender,
                Param = param
            };

            //消息直接分发或者加入队列
            if (_immediate && immediate)
            {
                _handlerMap[msg.Id].DispatchMessage(msg.Sender, msg.Param);
            }
            else
            {
                _messageQueue.Enqueue(msg);
            }
        }

        /// <summary>
        /// 初始化消息管理器。
        /// </summary>
        /// <param name="immediate">接受到消息时立即执行，不进入队列。若为false，则消息在下一帧开始逐个执行。</param>
        /// <exception cref="ArgumentException">类型参数不是枚举类型。</exception>
        public void Initialize(bool immediate = true)
        {
            //如果不是枚举类型，抛出异常
            if (!typeof(TEventType).IsEnum)
            {
                throw new ArgumentException();
            }

            _handlerMap = new Dictionary<TEventType, MessageHandlerCollection>();
            _messageQueue = new Queue<Message>();
            _immediate = immediate;
        }

        /// <summary>
        /// 推动消息队列。
        /// </summary>
        public void Update()
        {
            if (_messageQueue.Count>0)
            {
                var msg = _messageQueue.Dequeue();
                if (_handlerMap[msg.Id]!=null)
                {
                    _handlerMap[msg.Id].DispatchMessage(msg.Sender, msg.Param);
                }
               
            }
        }

        /// <summary>
        /// 释放消息管理器的资源。
        /// </summary>
        public void Dispose()
        {
            _messageQueue.Clear();
            _messageQueue = null;

            foreach (var pair in _handlerMap)
            {
                pair.Value.Dispose();
            }

            _handlerMap.Clear();
            _handlerMap = null;
        }

    
        #endregion Public Methods

        /// <summary>
        /// 消息对象。
        /// </summary>
        private class Message
        {
            public TEventType Id;
            public object Sender;
            public EventArgs Param;
        }

        /// <summary>
        /// 消息处理方法集合。
        /// </summary>
        private class MessageHandlerCollection
        {
            /// <summary>
            /// 处理方法数量。
            /// </summary>
            public int Count
            {
                get { return _handlerList.Count; }
            }

            /// <summary>
            /// 处理方法列表。
            /// </summary>
            private List<EventHandler<EventArgs>> _handlerList;

            /// <summary>
            /// 构造 <see cref="MessageHandlerCollection"/> 。
            /// </summary>
            public MessageHandlerCollection()
            {
                _handlerList = new List<EventHandler<EventArgs>>();
            }

            /// <summary>
            /// 增加处理方法。
            /// </summary>
            /// <param name="callback">The p callback.</param>
            public void AddHandler(EventHandler<EventArgs> callback)
            {
                if (!_handlerList.Contains(callback))
                {
                    _handlerList.Add(callback);
                }
            }

            /// <summary>
            /// 移除处理方法。
            /// </summary>
            /// <param name="callback">The callback.</param>
            public void RemoveHandler(EventHandler<EventArgs> callback)
            {
                if (_handlerList.Contains(callback))
                {
                    _handlerList.Remove(callback);
                }
            }


            /// <summary>
            /// 移除对象的所有处理方法。
            /// </summary>
            /// <param name="obj">对象</param>
            public void RemoveAbout(object obj)
            {
                for (int i = _handlerList.Count-1; i >= 0; i--)
                {
                    if (_handlerList[i].Target == obj)
                    {
                        _handlerList.RemoveAt(i);
                    }
                }
            }

            /// <summary>
            ///  执行消息处理。
            /// </summary>
            /// <param name="sender">The p sender.</param>
            /// <param name="param">The <see cref="EventArgs"/> instance containing the event data.</param>
            public void DispatchMessage(object sender, EventArgs param)
            {
                var list = new List<EventHandler<EventArgs>>(_handlerList);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != null)
                    {
                        list[i].Invoke(sender, param);
                      
                    }
                    else
                    {
                        Debug.LogWarning("Handler is destroyed.");
                    }
                }
            }

            /// <summary>
            /// 释放资源。
            /// </summary>
            public void Dispose()
            {
                _handlerList.Clear();
                _handlerList = null;
            }
        }
    }

    /// <summary>
    /// 消息队列的接口，由消息管理负责维护生命周期。
    /// </summary>
    public interface IMessageQueue
    {
        /// <summary>
        /// 初始化消息管理器。
        /// </summary>
        /// <param name="immediate">接受到消息时立即执行，不进入队列。若为false，则消息在下一帧开始逐个执行。</param>
        void Initialize(bool immediate = true);

        /// <summary>
        /// 推动消息队列。
        /// </summary>
        void Update();

        /// <summary>
        /// 释放消息管理器的资源。
        /// </summary>
        void Dispose();

        /// <summary>
        ///  移除所有有跟这个对象有关的方法
        /// </summary>
        /// <param name="target">The target.</param>
        void RemoveAbout(object target);
    }
}