using QxFramework.Core;
using EventLogicSystem;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 新事件管理器
/// </summary>
public class EventManager : LogicModuleBase, IEventManager
{

    [Serializable]
    public class EventHistory
    {
        public int Id;
        public int TemplateId;
        public List<int> ParamList;
        public int Selected;

        public EventHistory(int id, int templateId, List<int> paramList, int selected)
        {
            Id = id;
            TemplateId = templateId;
            ParamList = ((paramList != null) ? new List<int>(paramList) : null);
            Selected = selected;
        }
    }

    /// <summary>
    /// 事件生成器
    /// </summary>
    private ConditionEventCreator _conditionEventCreator;

    /// <summary>
    /// 序列化的时间事件列表，注意需要倒序
    /// </summary>
    private class PresetEventList : GameDataBase
    {
        /// <summary>
        /// 随机列表
        /// </summary>
        [SerializeField]
        public List<int> RandomList = new List<int>();

        /// <summary>
        /// 历史记录
        /// </summary>
        [SerializeField]
        public List<EventHistory> Histories = new List<EventHistory>();
    }


    /// <summary>
    /// 预设事件列表
    /// </summary>
    private PresetEventList _presetEvents;

    /// <summary>
    /// 事件列表
    /// </summary>
    private List<ConditionEventTemplate> _list;
    private EventContext _eventContext;

    private int _eventMaxId;

    public override void Init()
    {
        if (!RegisterData(out _presetEvents))
        {
        }
        LoadFile();
        InitEventCreator();
    }

    private void InitEventCreator()
    {
        _eventContext = new EventContext();
        _eventContext.Init();

        _conditionEventCreator = new ConditionEventCreator();
        _conditionEventCreator.Init(_list, _eventContext);

        if (Application.isPlaying)
        {
            //注入数据
            //这种方法可以使各种lua通过xx.xx来调用C#数据
            foreach (var dic in QXData.Instance.GetAllData())
            {
                foreach (var pair in dic.Value)
                {
                    if (dic.Key == "Default")
                    {
                        _conditionEventCreator.SetEnvParam(pair.Value.Data.GetType().Name, pair.Value.Data);
                    }
                    else
                    {
                        _conditionEventCreator.SetEnvParam(dic.Key + "_" + pair.Value.Data.GetType().Name, pair.Value.Data);
                    }
                }
            }
        }
        _eventContext.EventCreator = _conditionEventCreator;
    }

    /// <summary>
    /// 覆盖事件模板，会替换掉已有的
    /// </summary>
    /// <param name="conditionEventTemplate"></param>
    public void OverwriteTemplate(ConditionEventTemplate conditionEventTemplate)
    {
        _list.RemoveAll(t => t.TemplateId == conditionEventTemplate.TemplateId);
        _list.Add(conditionEventTemplate);
        _conditionEventCreator.OverwriteTemplate(conditionEventTemplate);
    }

    /// <summary>
    /// 读取文件
    /// </summary>
    private void LoadFile()
    {
        _list = new List<ConditionEventTemplate>();
        //读取目录下的所有
        var assets = Resources.LoadAll<TextAsset>(ConditionEventCreator.DirPath);
        foreach (var asset in assets)
        {
            var list = JsonConvert.DeserializeObject<List<ConditionEventTemplate>>(asset.text, new JsonLogicConverter());
            if (list != null)
            {
                _list.AddRange(list);
            }
        }
    }


    /// <summary>
    /// 描述翻译，把Lua形式的描述文案翻译一下
    /// </summary>
    /// <param name="desLua"></param>
    /// <returns></returns>
    public string TanslateDescription(string desLua)
    {
        return _conditionEventCreator.TranslateDescription(desLua);
    }

    /// <summary>
    /// 强制执行某事件，运行条件但无视结果
    /// </summary>
    /// <param name="templateId"></param>
    /// <param name="paramList"></param>
    public void ForceEvent(int templateId, List<int> paramList = null)
    {
        RunEvent(templateId, true, paramList);
    }

    /// <summary>
    /// 试图执行某事件，条件满足则执行
    /// </summary>
    /// <param name="templateId"></param>
    /// <param name="paramList"></param>
    public bool TryEvent(int templateId, List<int> paramList = null)
    {
        return RunEvent(templateId, false, paramList);
    }

  
    /// <summary>
    /// 获取所有事件模板列表
    /// </summary>
    /// <returns></returns>
    public List<ConditionEventTemplate> GetAllConditionEventTemplate()
    {
        return _list;
    }

    /// <summary>
    /// 运行事件
    /// </summary>
    /// <param name="templateId"></param>
    /// <param name="force"></param>
    /// <param name="paramList"></param>
    private bool RunEvent(int templateId, bool force, List<int> paramList = null)
    {
        //Debug.Log("执行事件Id:" + templateId);
        if (templateId == 0)
        {
            return false;
        }

        _eventContext.TemplateId = templateId;
        var evt = new ConditionEvent(templateId, _eventMaxId, _conditionEventCreator, paramList);

        _eventMaxId += 1;

        Debug.Log("Name:" + evt.GetName());
        var cond = evt.TryCondition();
        Debug.Log("条件测试：" + cond);
        Debug.Log("force：" + force);

        //非强制执行并且条件不通过则返回
        if (!force && !cond)
        {
            return false;
        }
        evt.DoEffect();
        ShowEvent(evt);
        return true;
    }
    /// <summary>
    /// 显示事件的Hint或者弹窗
    /// </summary>
    /// <param name="evt"></param>
    private void ShowEvent(ConditionEvent evt)
    {
        //结果Hint
        if (evt.Show == ShowType.Hint)
        {
            MessageManager.Instance.Get<HintType>().DispatchMessage(HintType.CommonHint, this, new HintMessage(evt.GetText()));
            _presetEvents.Histories.Add(new EventHistory(evt.UniqueId, evt.TemplateId, evt.Params, 0));
        }

        //弹窗
        else if (evt.Show == ShowType.Dialog)
        {
            //普通弹窗
            if (evt.GetSelections() == null || evt.GetSelections().Count == 0)
            {
                MessageManager.Instance.Get<HintType>().DispatchMessage(HintType.ClickWindow, this, new ClickMessage(evt.GetName(), evt.GetText()));
                _presetEvents.Histories.Add(new EventHistory(evt.UniqueId, evt.TemplateId, evt.Params, 0));
            }
            else
            {
                //选项框
                List<Choose> tmpChooseList = new List<Choose>();
                for (int i = 0; i < evt.GetSelections().Count; i++)
                {
                    var selection = evt.GetSelections()[i];
                    bool enable = selection.TryCondition();

                    if (selection.Disable == SelectionDisableType.Hide && !enable)
                    {
                        continue;
                    }

                    Choose newChoose = new Choose();
                    newChoose.ActionBool = enable;
                    newChoose.ChooseButton = selection.GetSelectionText();
                    if (selection.ShowCondition && (enable || selection.Disable != SelectionDisableType.HideComdition))
                    {
                        newChoose.ChooseContent = selection.ConditionDescription();
                    }
                    else
                    {
                        newChoose.ChooseContent = string.Empty;
                    }
                    int selectionIndex = i;
                    newChoose.ChooseAction = () =>
                    {
                        //记录选项历史
                        _presetEvents.Histories.Add(new EventHistory(evt.UniqueId, evt.TemplateId, evt.Params, selectionIndex));

                        selection.DoEffect();
                    };
                    tmpChooseList.Add(newChoose);
                }
                MessageManager.Instance.Get<HintType>().DispatchMessage(HintType.ChooseWindow, this, new ChooseMessage(evt.GetName(), evt.GetText(), evt.ImagePath, tmpChooseList));
            }
        }
        else
        {
            //记录历史
            _presetEvents.Histories.Add(new EventHistory(evt.UniqueId, evt.TemplateId, evt.Params, 0));
        }
    }
    /// <summary>
    /// 得到模板名称
    /// </summary>
    /// <param name="templateId"></param>
    /// <returns></returns>
    public string GetTemplateName(int templateId)
    {
        var t = _list.Find((e) => e.TemplateId == templateId);
        if (t != null)
        {
            return t.Name;
        }

        return string.Empty;
    }

    /// <summary>
    /// 得到模板文本
    /// </summary>
    /// <param name="templateId"></param>
    /// <returns></returns>
    public string GetTemplateText(int templateId)
    {
        var t = _list.Find((e) => e.TemplateId == templateId);
        if (t != null)
        {
            return _conditionEventCreator.TranslateDescription(t.Text);
        }
        return string.Empty;
    }

    /// <summary>
    /// 得到描述文本
    /// </summary>
    /// <param name="templateId"></param>
    /// <returns></returns>
    public string GetConditionText(int templateId)
    {
        var t = _list.FindAll((e) => e.TemplateId == templateId);
        _eventContext.TemplateId = templateId;
        if (t.Count > 0)
        {
            return _conditionEventCreator.TranslateDescription(t[0].Condition.GetDescription());
        }
        return string.Empty;
    }


    public override void Update()
    {
        base.Update();
        //每隔60帧主动GC
        if (Time.frameCount %60 == 0)
        {
            _conditionEventCreator.TickGC();
        }
    }
    /// <summary>
    /// 全量垃圾回收
    /// </summary>
    public void FullGC()
    {
        if (_conditionEventCreator != null)
        {
            _conditionEventCreator.ForceFullGC();
        }
    }
}