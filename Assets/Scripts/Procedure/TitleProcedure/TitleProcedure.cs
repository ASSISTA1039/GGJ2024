using QxFramework.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleProcedure : ProcedureBase {

    protected override void OnInit()
    {
        base.OnInit();

        QXData.Instance.SetTableAgent();
        GameMgr.Instance.InitModules();
        MessageManager.Instance.Initialize();
        //开局关闭所有UI，避免残留测试UI
        UIManager.Instance.CloseAll();

    }

    protected override void OnEnter(object args)
    {
        base.OnEnter(args);
        AudioManager.Instance.Init();
        //UIManager.Instance.StartBlackoutAndReveal(2,null,"StartUI",null);
        AudioManager.Instance.PlayBGM("BGM_main");
        UIManager.Instance.Open("StartPage",2, "StartPage");
        //副流程，Startmodule
        //AddSubmodule(new StartModule());
        //ProcedureManager.Instance.ChangeTo("FightProcedure");
    }
}
