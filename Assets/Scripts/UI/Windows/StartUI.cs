using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using QxFramework.Core;
using UnityEngine.UI;
//using UnityEngine.UIElements;
//using UnityEngine.UIElements;


//开始界面，具有四个按钮
public class StartUI : UIBase
{
    #region 参数区
    private string Fight = "FightProcedure";
    private int ButtonPressTime { get; set; }

    MonsterData monsterData = new MonsterData();
    #endregion
    public override void OnDisplay(object args)
    {
        
         //音乐资源启动

        base.OnDisplay(args);

        #region 按钮注册区
        //UnityEngine.UI.Button startButton = Get<UnityEngine.UI.Button>("NewGameButton");
        //startButton.onClick.RemoveAllListeners();
        //startButton.onClick.AddListener(OnNewGameButton);


        Color color = Find("Shade").GetComponent<Image>().color;
        color.a = 0;
        Find("Shade").GetComponent<Image>().color = color;

        //鼠标移动到按钮上
        RegisterTrigger("NewGameButton").onClick = OnNewGameButton;
        RegisterTrigger("NewGameButton").onPointerEnter = EnterButton;//实际上作用为startbutton
        RegisterTrigger("NewGameButton").onPointerExit = ExitButton;

        //RegisterTrigger("ContinueButton").onClick = OnContinueButton; //暂且闲置
        //RegisterTrigger("ContinueButton").onPointerEnter = EnterButton;
        //RegisterTrigger("ContinueButton").onPointerExit = ExitButton;

        RegisterTrigger("OptionButton").onClick = OnOptionButton;
        RegisterTrigger("OptionButton").onPointerEnter = EnterButton;
        RegisterTrigger("OptionButton").onPointerExit = ExitButton;

        RegisterTrigger("QuitButton").onClick= OnQuitButton;
        RegisterTrigger("QuitButton").onPointerEnter = EnterButton;
        RegisterTrigger("QuitButton").onPointerExit = ExitButton;

        
        #endregion

       
    }
    protected override void OnClose()
    {
        //UIManager.Instance.Open("SelectUI", 2, "TestUI2", null);
        base.OnClose();
       
    }

    #region NewGameButton
    private void OnNewGameButton(GameObject obj, PointerEventData pData)        //(GameObject obj, PointerEventData pData)
    {
        //GameManager.Instance.Init();
        UIManager.Instance.StartBlackoutAndReveal(1f, null, "LevelUI", null);
        UIManager.Instance.Close("StartUI");
    }
    #endregion

    #region ContinueButton
    private void OnContinueButton(GameObject obj, PointerEventData pData)
    {
        

    }
    #endregion

    #region OptionButton
    private void OnOptionButton(GameObject obj, PointerEventData pData)
    {
        UIManager.Instance.Open("SettingUI", 4, "SettingUI", null);
    }
    #endregion

    #region QuitButton
    private void OnQuitButton(GameObject obj, PointerEventData pData)
    {
        UIManager.Instance.FadeToBlack(Find("Shade"), 0.5f);   //黑幕并退出的实现
    }
    #endregion

    #region 共有方法
    /// <summary>
    /// 鼠标移动在上面触发的函数
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="pData"></param>
    private void EnterButton(GameObject obj, PointerEventData pData)
        {     
            obj.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
        }

    
    /// <summary>
    /// 鼠标离开触发的函数
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="pData"></param>
        private void ExitButton(GameObject obj, PointerEventData pData)
        {
          
            obj.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        }
    #endregion
}

