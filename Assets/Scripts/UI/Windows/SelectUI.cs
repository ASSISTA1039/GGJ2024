using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using QxFramework.Core;
using UnityEngine.SearchService;

//教程选择UI
public class SelectUI : UIBase
{
    //硬编码区
    string Fight = "FightProcedure";
    private int ButtonPressTime { get; set; }
    public override void OnDisplay(object args)
    {
        base.OnDisplay(args);

        //开始按钮的注册,当按钮被点击时，onClick函数会将参数传递给onStartGameButton并触发该函数。
        RegisterTrigger("Mode1").onClick = OnMode1Select;
        RegisterTrigger("Mode1").onPointerEnter = EnterMode1Button;
        

        RegisterTrigger("Mode2").onClick = OnModeSelect2;
        RegisterTrigger("Mode2").onPointerEnter = EnterMode2Button;
        RegisterTrigger("Mode2").onPointerExit = ExitMode2Button;
        ButtonPressTime = 0;
    }
    private void EnterMode1Button(GameObject obj, PointerEventData pData)
    { 
        Transform Button1 = Get<Transform>("Button1Text");
        TextMeshProUGUI TextButton1 = Button1.GetComponent<TextMeshProUGUI>();
       

    }

    private void OnMode1Select(GameObject obj, PointerEventData pData)
    {
        

      
    }


    private void OnModeSelect2(GameObject obj, PointerEventData pData)
    {
        if (ButtonPressTime >= 3)
        {
            GameMgr.Get<ISystemManager>().Show(System.Environment.UserName+"，你要记住，跟随指引", "……");
        }
        //关闭UI
        
    }
    private void EnterMode2Button(GameObject obj, PointerEventData pData)
    {
        Transform Mode2 = transform.Find("Mode2");
        Mode2.localScale = new Vector3(1.1f, 1.1f, 1.1f);
    }
    private void ExitMode2Button(GameObject obj, PointerEventData pData)
    {
        Transform Mode2 = transform.Find("Mode2");
        Mode2.localScale = new Vector3(1f, 1f, 1f);
    }
    protected override void OnClose()
    {
       
        base.OnClose();
    }


}
