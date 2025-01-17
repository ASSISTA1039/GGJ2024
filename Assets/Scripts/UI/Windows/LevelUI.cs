using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using QxFramework.Core;
using UnityEngine.UI;
using TMPro;

public class LevelUI : UIBase
{
    public override void OnDisplay(object args)
    {
        base.OnDisplay(args);
        //开始按钮的注册,当按钮被点击时，onClick函数会将参数传递给onStartGameButton并触发该函数。
        RegisterTrigger("Level0").onClick = OnLevel0Select;
        RegisterTrigger("Level1").onClick = OnLevel1Select;
        //RegisterTrigger("Mode2").onPointerEnter = EnterMode2Button;
        //RegisterTrigger("Mode2").onPointerExit = ExitMode2Button;
    }
    private void OnLevel0Select(GameObject obj, PointerEventData pData)
    {
        GameManager.Instance.GameStart(0);
        OnClose();
    }
    private void OnLevel1Select(GameObject obj, PointerEventData pData)
    {
        GameManager.Instance.GameStart(1);
        OnClose();
    }

    private void OnMode1Select(GameObject obj, PointerEventData pData)
    {

    }


    private void OnModeSelect2(GameObject obj, PointerEventData pData)
    {

    }
    private void EnterMode2Button(GameObject obj, PointerEventData pData)
    {

    }
    private void ExitMode2Button(GameObject obj, PointerEventData pData)
    {

    }
    protected override void OnClose()
    {
        base.OnClose();
    }
}
