using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using QxFramework.Core;
using System;
using UnityEngine.UI;

public class SettingUI : UIBase
{
    public override void OnDisplay(object args)
    {
        //RegisterTrigger("Mode1").onClick = OnMusicUp;
        RegisterTrigger("Continue").onClick = Continue;
        RegisterTrigger("Quit").onClick = Quit;

    }
    private void Continue(GameObject obj, PointerEventData pData)
    {
        DoClose();
        //UIManager.Instance.CloseAll();
        //UIManager.Instance.Open("LevelUI",2,"LevelUI");
    }
    private string[] objectsToDestroy = { "Player(Clone)", "FuncBlocks(Clone)", "NoFuncBlocks(Clone)" };//TODO：此处应写需要再切换时候删除的元素
    private void Quit(GameObject obj, PointerEventData pData)
    {
        OnClose();
        UIManager.Instance.CloseAll();
        foreach (string objName in objectsToDestroy)
        {
            GameObject objects = GameObject.Find(objName);
            if (objects != null)
            {
                Destroy(objects); // 销毁物体
                Debug.Log(objName + " destroyed.");
            }
            else
            {
                Debug.LogWarning(objName + " not found.");
            }
        }
        UIManager.Instance.Open("StartPage",2,"LevelUI");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DoClose();

        }
        //DateTime currentTime = DateTime.Now;
        //string formattedTime = currentTime.ToString("HH:mm:ss");
        //Get<Text>("Time").text = formattedTime;
       
       
    }

}
