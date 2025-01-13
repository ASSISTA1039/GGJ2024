using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;
using TMPro;


public class SystemManager : LogicModuleBase, ISystemManager
{
    bool isPause { get; set; }
    public override void Init()
    {
        base.Init();
        isPause = false;
    }

    public bool Show(string message,string title,int type=1)
    {
        int answer=Messagebox.MessageBox(IntPtr.Zero, message, title, type);
        switch (answer)
        {
            case 1:
                return true;
            case 2:
                return false;
            default:
                break;
        }
        return false;
    }

   

    public void Pause()
    {
        if (!isPause)
        {
            Time.timeScale = 0; // 暂停游戏
            isPause = true;
        }
    }
    public void Resume()
    {
        if (isPause)
        {
            Time.timeScale = 1;
            isPause = false;
        }
    }

}

interface ISystemManager
{
    /// <summary>
    /// 返回值：true是玩家点了确定，false是玩家点了取消
    /// </summary>
    /// <param name="message"></param>
    /// <param name="title"></param>
    /// <param name="type">1,1|32(黄标),1|48(红标)</param>
    /// <returns></returns>
    public bool Show(string message, string title, int type=1);//展示windows弹窗
    public void Pause();//暂停游戏
    public void Resume();//恢复游戏
}


public class Messagebox
{
    [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int MessageBox(IntPtr handle, string message, string title, int type);

    // 显示最顶层的MessageBox
    public static void Show(string message, string title)
    {
        // MB_OK(0x00000000) | MB_TOPMOST(0x00040000)
        int type = 0x00000000 | 0x00040000;
        MessageBox(IntPtr.Zero, message, title, type);
    }
}