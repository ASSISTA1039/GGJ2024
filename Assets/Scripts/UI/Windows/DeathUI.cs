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
    public override void OnDisplay(object args)
    {
        //RegisterTrigger("Mode1").onClick = OnMusicUp;
        RegisterTrigger("DeathButton").onClick = OnDeath;

    }
    private void OnDeath(GameObject obj, PointerEventData pData)
    {
        UIManager.Instance.CloseAll();
        UIManager.Instance.Open("StartPage", 2, "StartPage");
        GameManager.Instance.SwitchToLevelSelectUI(false);
    }
}
