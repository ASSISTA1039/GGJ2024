using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using QxFramework.Core;
using System;
using UnityEngine.UI;

public class GameUI : UIBase
{
    Transform stopButton;
    Transform continueButton;
    public override void OnDisplay(object args)
    {
        RegisterTrigger("SettingButton").onClick = OnSettingButton;
        RegisterTrigger("StopButton").onClick = OnStopButton;
        RegisterTrigger("ContinueButton").onClick = OnContinueButton;
        //RegisterTrigger("Quit").onClick = Quit;
        stopButton = transform.Find("StopButton");
        continueButton = transform.Find("ContinueButton");

    }
    private void OnSettingButton(GameObject obj, PointerEventData pData)
    {
        //UIManager.Instance.CloseAll();
        UIManager.Instance.Open("SettingUI",4,"SettingUI");
    }
    private void OnStopButton(GameObject obj, PointerEventData pData)
    {
        Time.timeScale = 0; 
    }
    private void OnContinueButton(GameObject obj, PointerEventData pData)
    {
        Time.timeScale = 1;
    }

    private void Update()
    {
        if (Time.timeScale == 0)
        {
            stopButton.GetComponent<Button>().enabled = false;
            continueButton.GetComponent<Button>().enabled = true;
        }
        else 
        {
            stopButton.GetComponent<Button>().enabled = true;
            continueButton.GetComponent <Button>().enabled = false;
        }
        
        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    DoClose();

        //}
        //TODO:可以在这里加入失败检测的实现逻辑。
    }

}
