using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using QxFramework.Core;
public class ExitUI : UIBase
{
    
    private void Start()
    {
        //寻找名称为以下三者的按钮并将其被点击的事件传递给相应的函数
        RegisterTrigger("ExitPanel/ReturnButton").onClick = OnReturnButton;
        RegisterTrigger("ExitPanel/ExitButton").onClick = OnExitButton;
        RegisterTrigger("ExitPanel/SettingButton").onClick = OnSettingButton;
    }
    //返回游戏
    private void OnReturnButton(GameObject obj, PointerEventData pData)
    { 
        DoClose();
    }
    //退出游戏
    private void OnExitButton(GameObject obj, PointerEventData pData)
    {
#if UNITY_EDITOR
        Debug.Log("退出游戏");
        UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    /// <summary>
    /// 打开设置菜单
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="pData"></param>
    private void OnSettingButton(GameObject obj, PointerEventData pData)
    {
        obj.SetActive(false);
        //UIManager.Instance.ShowUI<SelectUI>("SelectUI");
        DoClose();
    }
}
