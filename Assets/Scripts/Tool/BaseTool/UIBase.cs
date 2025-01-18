using System;
using System.Collections.Generic;
using UnityEngine;

namespace QxFramework.Core
{
    /// <summary>
    /// 所有UI父对象的基类。
    /// </summary>
    public abstract class UIBase : MonoBehaviour
    {
        private ChildBindTool _childBindTool;

        /// <summary>
        /// 获得层级
        /// </summary>
        /// <returns></returns>
        public virtual int UILayer => -1;

        /// <summary>
        /// 当UI被显示时执行。
        /// </summary>
        public virtual void OnDisplay(object args)
        {
        }

        /// <summary>
        /// 当注册消息处理方法时执行。
        /// </summary>
        protected virtual void OnRegisterHandler()
        {
        }

        /// <summary>
        /// 当关闭时执行。
        /// </summary>
        protected virtual void OnClose()
        {
            //TODO 考虑改成隐藏
            //Destroy(gameObject);
            if (gameObject != null)
            {
                ObjectPool.Recycle(gameObject);
            }
            else
            {
                Debug.Log("Destroyed gameObject" + name);
            }
        }

        /// <summary>
        /// 在UIEventTrigger中注册物体，返回的是物体，该方法局限于GET的性质，并不支持多个重名元素的使用。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public UIEventTrigger RegisterTrigger(string name)
        {
            Transform transformFind = Get<Transform>(name);

            if (transformFind != null)
            {
                return UIEventTrigger.Get(transformFind.gameObject);
            }
            else
            {
                Debug.LogError("\nUIBase.RegisterTrigger:\nDidn't found UI"+name);
                return null;
            }
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="callback"></param>
        protected void RegisterMessage<T>(T t, EventHandler<EventArgs> callback) where T : struct
        {
            MessageManager.Instance.Get<T>().RegisterHandler(t, callback);
        }
    
        /// <summary>
        /// 注册游戏数据更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="key"></param>
        protected void RegisterDataUpdate<T>(Action<GameDataBase> action, string key = "Default")
            where T : GameDataBase, new()
        {
            QXData.Instance.RegisterUpdateListener<T>(action,key);
        }

        /// <summary>
        /// 一个快速获取组件的函数，以物体名为Key，相同值则直接覆盖
        /// </summary>
        /// <typeparam name="T">组件</typeparam>
        /// <param name="name">物体名。</param>
        /// <returns>组件对象</returns>
        /// <exception cref="System.ArgumentNullException">没找到组件。</exception>
        public T Get<T>(string name)
        {
            if (_childBindTool == null)
            {
                _childBindTool = new ChildBindTool(this, transform);
            }
            return _childBindTool.Get<T>(name);
        }

        /// <summary>
        /// 一个快速获取游戏物体的函数，以物体名为Key，相同值则直接覆盖
        /// </summary>
        /// <param name="name">物体名。</param>
        /// <returns>游戏物体。没有找到则返回null。</returns>
        public GameObject Find(string name)
        {
            if (_childBindTool == null)
            {
                _childBindTool = new ChildBindTool(this, transform);
            }

            if (!_childBindTool.Gos.ContainsKey(name))
            {
                _childBindTool.CollectObject();
            }
            if (!_childBindTool.Gos.ContainsKey(name))
            {
                return null;
            }

            return _childBindTool.Gos[name];
        }

        /// <summary>
        /// 当关闭时会移除有关注册的消息
        /// </summary>
        protected void RemoveHandler()
        {
            MessageManager.Instance.RemoveAbout(this);
            QXData.Instance.RemoveAbout(this);
        }

        /// <summary>
        /// 执行关闭行为流程
        /// </summary>
        public void DoClose()
        {
            OnClose();
            RemoveHandler();
        }

        public void CommitValue()
        {
            _childBindTool.CommitValue();
        }

        /// <summary>
        /// 执行显示行为流程
        /// </summary>
        public void DoDisplay(object args)
        {
            if (_childBindTool == null)
            {
                _childBindTool = new ChildBindTool(this, transform);
            }
         
            OnDisplay(args);
            OnRegisterHandler();
            _childBindTool.CommitValue();
        }

        /// <summary>
        /// 快速构建多个相同元素的UI。
        /// </summary>
        /// <typeparam name="T">遍历的集合的元素的类型</typeparam>
        /// <param name="parent">多个元素的父物体。该父物体下的第一个物体作为复制的对象。</param>
        /// <param name="collection">要遍历的集合。通过遍历该集合创建与之对应的子物体。</param>
        /// <param name="childAction">每个子物体只要执行的操作。在这里将集合的数据反映到UI上。</param>
        /// <param name="whiteList">每次构建会删除除了第一个物体外的所有子物体。如果有不想删除的对象可以通过这个白名单指定。</param>
        protected void BuildMultipleItem<T>(Transform parent, IEnumerable<T> collection, Action<GameObject, T> childAction, params string[] whiteList)
        {
            for (int i = 1; i < parent.childCount; i++)
            {
                bool flag = true;
                foreach (string item in whiteList)
                {
                    if (parent.GetChild(i).gameObject.name == item)
                    {
                        flag = false;
                    }
                }
                if (flag)
                {
                    Destroy(parent.GetChild(i).gameObject);
                }
            }
            Transform child = parent.GetChild(0);
            child.gameObject.SetActive(false);
            foreach (T item in collection)
            {
                GameObject go = Instantiate(child.gameObject, parent);
                go.SetActive(true);
                childAction.Invoke(go, item);
            }
        }
    }
}