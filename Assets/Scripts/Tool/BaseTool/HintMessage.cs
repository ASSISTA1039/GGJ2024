using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HintMessage : EventArgs
{
    public string Content;
    public HintMessage(string Content)
    {
        this.Content=Content;
    }
}
public class ClickMessage:EventArgs
{
    public string Title;
    public string Content;
    public ClickMessage(string Title,string Content)
    {
        this.Title=Title;
        this.Content=Content;
    }
}
public class ChooseMessage:EventArgs
{
    public string EventTitle;
    public string EventContent;
    public string ImagePath;
    public List<Choose> ChooseList;
    public ChooseMessage(string EventTitle,string EventContent,string ImagePath,List<Choose> _chooseList)
    {
        this.EventTitle=EventTitle;
        this.EventContent=EventContent;
        this.ImagePath=ImagePath;
        this.ChooseList=_chooseList;
    }
}
public class Choose
    {
        public string ChooseButton;
        public string ChooseContent;
        public Action ChooseAction;
        public bool ActionBool=true;
    }
