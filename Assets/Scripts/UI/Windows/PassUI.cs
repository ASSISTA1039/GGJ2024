using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using QxFramework.Core;
using QxFramework;
using System;
using UnityEngine.UI;
using TMPro;

public class PassUI : UIBase
{
    public override void OnDisplay(object args)
    {
        //RegisterTrigger("Mode1").onClick = OnMusicUp;
        RegisterTrigger("PassButton").onClick = OnPass;
        AudioManager.Instance.StopBGM();
        

    }
    private void OnPass(GameObject obj, PointerEventData pData)
    {
        UIManager.Instance.Close("GameUI");
        UIManager.Instance.Close("PassUI");
        GameManager.Instance.SwitchToLevelSelectUI(true);
    }
}
