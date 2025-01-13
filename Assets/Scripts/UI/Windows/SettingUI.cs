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
        RegisterTrigger("Quit").onClick = Quit;
       
    }
    private void Quit(GameObject obj, PointerEventData pData)
    {
        UIManager.Instance.CloseAll();
        UIManager.Instance.Open("LoginUI",1,"LoginUI");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DoClose();

        }
        DateTime currentTime = DateTime.Now;
        string formattedTime = currentTime.ToString("HH:mm:ss");
        Get<Text>("Time").text = formattedTime;
       
       
    }

}
