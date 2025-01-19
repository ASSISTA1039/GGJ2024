using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using QxFramework.Core;
using System;
using UnityEngine.UI;
using TMPro;

public class DeathUI : UIBase
{
    private string[] objectsToDestroy = { "Player(Clone)", "FuncBlocks(Clone)", "NoFuncBlocks(Clone)" };//TODO：此处应写需要再切换时候删除的元素
    public override void OnDisplay(object args)
    {
        //RegisterTrigger("Mode1").onClick = OnMusicUp;
        foreach (string objName in objectsToDestroy)
        {
            GameObject obj = GameObject.Find(objName);
            if (obj != null)
            {
                Destroy(obj); // 销毁物体
                Debug.Log(objName + " destroyed.");
            }
            else
            {
                Debug.LogWarning(objName + " not found.");
            }
        }
        RegisterTrigger("DeathButton").onClick = OnDeath;
        AudioManager.Instance.StopBGM();

    }
    private void OnDeath(GameObject obj, PointerEventData pData)
    {
        UIManager.Instance.CloseAll();
        UIManager.Instance.Open("StartPage", 2, "StartPage");
        GameManager.Instance.SwitchToLevelSelectUI(false);
    }
}
