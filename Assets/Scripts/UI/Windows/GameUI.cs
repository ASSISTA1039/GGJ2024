using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using QxFramework.Core;
using System;
using UnityEngine.UI;

public class GameUI : UIBase
{
    public override void OnDisplay(object args)
    {
        //RegisterTrigger("Mode1").onClick = OnMusicUp;
        //RegisterTrigger("Quit").onClick = Quit;
       
    }
    private void Quit(GameObject obj, PointerEventData pData)
    {
        UIManager.Instance.CloseAll();
        UIManager.Instance.Open("LoginUI",1,"LoginUI");
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    DoClose();

        //}
        //TODO:可以在这里加入失败检测的实现逻辑。
    }

}
