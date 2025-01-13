using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using QxFramework;


namespace QxFramework.Core
{
    /// <summary>
    /// 事件监听
    /// </summary>
    public class UIEventTrigger : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler//,IPointerDownHandler,IPointerUpHandler
    {
        public Action<GameObject, PointerEventData> onClick;
        public Action<GameObject, PointerEventData> onPointerEnter;
        public Action<GameObject, PointerEventData> onPointerExit;
        public Action<GameObject, PointerEventData> onDrag;
        public Action<GameObject, PointerEventData> onBeginDrag;
        public Action<GameObject, PointerEventData> onEndDrag;
        public static UIEventTrigger Get(GameObject obj)
        {
            UIEventTrigger trigger = obj.GetComponent<UIEventTrigger>();
            if (trigger == null)
            {
                trigger = obj.AddComponent<UIEventTrigger>();
            }
            return trigger;
        }
        /// <summary>
        /// 监听点击事件
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            

            if (onClick != null)
            {
                onClick(gameObject, eventData);
            }
        }
        public void OnPointerEnter(PointerEventData eventData)//此处OnPointerEnter是接口的实现，不是自定义的函数
        {
            if (onPointerEnter != null)
            {
                onPointerEnter(gameObject, eventData);
            }
        }

        public void OnPointerExit(PointerEventData eventData)//此处OnPointerExit是接口的实现，不是自定义的函数
        {
            if (onPointerExit != null)
            {
                onPointerExit(gameObject, eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (onDrag != null)
            {
                onDrag(gameObject, eventData);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (onBeginDrag != null)
            {
                onBeginDrag(gameObject, eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (onEndDrag != null)
            {
                onEndDrag(gameObject, eventData);
            }
        }
    }
}
