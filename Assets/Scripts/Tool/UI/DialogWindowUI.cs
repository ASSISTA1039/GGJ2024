using System.Collections.Generic;
using UnityEngine;
using QxFramework.Core;
using UnityEngine.UI;
using System;

/// <summary>
/// 收到外交信息的UI
/// </summary>
public class DialogWindowUI : UIBase
{
    //图文的文
    private Text _imageAndTextMessageText;

    public Text _titleText { get; private set; }

    private Text _onlyTextTextText;

    private Transform _buttonList;

    /// <summary>
    /// 两个组
    /// </summary>

    private GameObject _onlyTextGroup;

    private DialogWindowUIArg _arg;

    private float _autoCloseTime;

    /// <summary>
    /// 弹出框的参数
    /// </summary>
    public class DialogWindowUIArg
    {
        /// <summary>
        /// 标题文字
        /// </summary>
        public string Title;

        /// <summary>
        /// 消息
        /// </summary>
        public string Message;

        /// <summary>
        /// 图片，可以省略
        /// </summary>
        public Sprite Image;

        /// <summary>
        /// 被强行关闭了的窗口默认选哪个
        /// </summary>
        public int CloseSelect;

        /// <summary>
        /// 按钮的文字
        /// </summary>
        public List<string> ButtonText;

        /// <summary>
        /// 按钮的行为
        /// </summary>
        public List<Action> Actions;

        /// <summary>
        /// 设置默认结束时间事件
        /// </summary>
        public int FinishTimeEvent;

        public DialogWindowUIArg(string title, string message, Sprite image = null)
        {
            Title = title;
            Message = message;
            Image = image;
            //默认最简单的构造，一个确认键
            ButtonText = new List<string>() { "确定" };
            CloseSelect = 0;
        }
        public DialogWindowUIArg(string title, string message, Sprite image, string buttonText = "", Action action = null)
        {
            Title = title;
            Message = message;
            Image = image;
            if (string.IsNullOrEmpty(buttonText))
            {
                buttonText = "确定";
            }
            ButtonText = new List<string>() { "确定" };
            if (action != null)
            {
                Actions = new List<Action>() { action };
            }
            CloseSelect = 0;
        }
        public DialogWindowUIArg(string title, string message, Sprite image, string buttonOk, string buttonCancel, Action action)
        {
            Title = title;
            Message = message;
            Image = image;
            ButtonText = new List<string>() { buttonOk, buttonCancel };
            if (action != null)
            {
                Actions = new List<Action>() { action };
            }
            CloseSelect = 1;
        }

        public DialogWindowUIArg(string title, string message, Sprite image, List<string> buttonText, List<Action> actions)
        {
            Title = title;
            Message = message;
            Image = image;
            ButtonText = buttonText;
            Actions = actions;
            CloseSelect = ButtonText.Count - 1;
        }

        public DialogWindowUIArg(string title, string message, Sprite image,
            List<string> buttonText, List<Action> actions, int closeSelect, int finishTimeEvent)
        {
            Title = title;
            Message = message;
            Image = image;
            ButtonText = buttonText;
            Actions = actions;

            CloseSelect = closeSelect;
            FinishTimeEvent = finishTimeEvent;
        }
    }

    private void Awake()
    {
        _onlyTextGroup = Get<Transform>("OnlyText").gameObject;
        _onlyTextTextText = Get<Text>("OnlyTextText");
        _titleText = Get<Text>("TitleText");
        _buttonList = Get<Transform>("ButtonList");
        Get<Button>("CloseButton").onClick.AddListener(Close);
    }

    public override void OnDisplay(object args)
    {
        base.OnDisplay(args);
        if (args == null)
        {
            return;
        }

        _arg = args as DialogWindowUIArg;
        _titleText.text = _arg.Title;

        InitCloseTime();
        SetOnlyText();
        SetButtons();

    }

    private void Update()
    {
        //如果有自动关闭时间
        if (_arg.FinishTimeEvent > 0)
        {
        }
    }

    private void InitCloseTime()
    {
    }

    private void UpdateCloseTime()
    {
        //如果有自动关闭时间
        if (_autoCloseTime > 0)
        {
            if (Time.time >= _autoCloseTime)
            {
                OnClickClose();
                return;
            }
            _buttonList.GetChild(_arg.CloseSelect).GetComponentInChildren<Text>().text
             = string.Format("{0}({1})", _arg.ButtonText[_arg.CloseSelect],
              (_autoCloseTime - Time.time).ToString("0")); 
        }
    }

    private void SetOnlyText()
    {
        _onlyTextGroup.SetActive(true);
        _onlyTextTextText.text = _arg.Message;
    }

    private void SetButtons()
    {
        //简易内存池
        for (int i = _buttonList.childCount; i < _arg.ButtonText.Count; i++)
        {
            GameObject.Instantiate(_buttonList.GetChild(0), _buttonList);
        }

        for (int i = _arg.ButtonText.Count; i < _buttonList.childCount; i++)
        {
            _buttonList.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < _arg.ButtonText.Count; i++)
        {
            _buttonList.GetChild(i).gameObject.SetActive(true);
        }
        for (int i = 0; i < _arg.ButtonText.Count; i++)
        {
            if (_buttonList.GetChild(i).GetComponent<Button>().onClick.GetPersistentEventCount() == 0)
            {
                int index = i;
                _buttonList.GetChild(i).GetComponent<Button>().onClick.RemoveAllListeners();
                _buttonList.GetChild(i).GetComponent<Button>().onClick.AddListener(() => { OnButtonClick(index); });
            }
            _buttonList.GetChild(i).GetComponentInChildren<Text>().text = _arg.ButtonText[i];
        }
    }

    /// <summary>
    /// 当按下了关闭
    /// </summary>
    public void OnClickClose()
    {
        OnButtonClick(_arg.CloseSelect);
    }
    private void Close()
    {
        UIManager.Instance.Close(this);
    }

    /// <summary>
    /// 当按钮按下
    /// </summary>
    /// <param name="i"></param>
    private void OnButtonClick(int i)
    {
        if (_arg.Actions != null && _arg.Actions.Count > i && _arg.Actions[i] != null)
        {
            _arg.Actions[i]();
        }
        Close();
    }

    /// <summary>
    /// 打开一个窗口
    /// </summary>
    /// <param name="args"></param>
    public static void OpenDialog(DialogWindowUIArg args)
    {
        UIManager.Instance.Open("DialogWindowUI",args: args);
    }
}