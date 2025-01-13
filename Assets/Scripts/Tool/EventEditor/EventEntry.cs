using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace EventLogicSystem
{
    /// <summary>
    /// 事件的提示类型
    /// </summary>
    public enum ShowType
    {
        Hide = 0,
        Hint,
        Dialog,
    }

    /// <summary>
    /// 自动执行方法
    /// </summary>
    public enum AutoRunType
    {
        Non = 0,
        Random,
        ConditionIsTrue,
    }

    /// <summary>
    /// 选项的不可点类型
    /// </summary>
    public enum SelectionDisableType
    {
        CantClick = 0,
        Hide,
        HideComdition,
    }

    public class ConditionEventTemplate
    {
        /// <summary>
        /// 类型Id
        /// </summary>
        public int TemplateId;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name;

        /// <summary>
        /// 路径
        /// </summary>
        public string Path;

        /// <summary>
        /// 自动运行
        /// </summary>
        public int AutoRun;

        /// <summary>
        /// 最大运行数量
        /// </summary>
        public int MaxCount;

        public ShowType Show;

        /// <summary>
        /// 文本描述
        /// </summary>
        public string Text;

        /// <summary>
        /// 图片路径
        /// </summary>
        public string ImagePath;

        public BaseAndObj Condition;

        public BaseSequenceObj Effect;
    }

    [CSharpCallLua]
    public delegate string StringFunc();

    [CSharpCallLua]
    public delegate bool BoolFunc();

    public class ConditionEvent
    {
        /// <summary>
        /// 模板Id
        /// </summary>
        [SerializeField]
        public int TemplateId
        {
            get; private set;
        }

        /// <summary>
        /// 唯一Id
        /// </summary>
        [SerializeField]
        public int UniqueId
        {
            get; private set;
        }

        [SerializeField]
        public ShowType Show;

        /// <summary>
        /// 运行参数
        /// </summary>
        [SerializeField]
        public List<int> Params;

        public string ImagePath;

        public ConditionEventTemplate Template
        {
            get; private set;
        }

        public class FunctionsInitArgs
        {
            public LuaTable TableEnv;
            public StringFunc GetName;
            public StringFunc GetText;
            public BoolFunc TryCondition;
            public StringFunc ConditionDescription;
            public Action DoEffect;
            public StringFunc EffectDescription;
        }

        public class SelectArgs
        {
            public StringFunc GetSelectionText;
            public SelectionDisableType Disable;
            public bool ShowCondition;
            public BoolFunc TryCondition;
            public StringFunc ConditionDescription;

            public Action DoEffect;

            public StringFunc EffectDescription;
        }

        [NonSerialized]
        private FunctionsInitArgs _functionsInitArgs;

        public ConditionEvent(int templateId, int uniqueId, ConditionEventCreator conditionEventManager, List<int> paramList = null)
        {
            this.TemplateId = templateId;
            this.UniqueId = uniqueId;
            this.Params = paramList;

            Template = conditionEventManager.GetTemplate(templateId);
            conditionEventManager.SetConditionEvent(this, paramList);
        }

        public void InitFunction(FunctionsInitArgs functionsInitArgs)
        {
            _functionsInitArgs = functionsInitArgs;
        }

        public string GetName()
        {
            var s = _functionsInitArgs.GetName();
            return s;
        }

        public string GetText()
        {
            return _functionsInitArgs.GetText();
        }

        public bool TryCondition()
        {
            return _functionsInitArgs.TryCondition();
        }

        public void DoEffect()
        {
            _functionsInitArgs.DoEffect();
            //hack 选项
            {
                //如果存在选项分支
                if (_functionsInitArgs.TableEnv.ContainsKey("SelectionCount"))
                {
                    //得到选项
                    _functionsInitArgs.TableEnv.Get("SelectionCount", out int selectionCount);
                    _selections = new List<SelectArgs>();
                    for (int i = 0; i < selectionCount; i++)
                    {
                        //得到选项细则
                        var selection = new ConditionEvent.SelectArgs();
                        _functionsInitArgs.TableEnv.Get($"SelectionDisable{i}", out int intEnum);
                        selection.Disable = (SelectionDisableType)intEnum;
                        _functionsInitArgs.TableEnv.Get($"SelectionShowCondition{i}", out selection.ShowCondition);
                        _functionsInitArgs.TableEnv.Get($"GetSelectionText{i}", out selection.GetSelectionText);
                        _functionsInitArgs.TableEnv.Get($"SelectionCondition{i}", out selection.TryCondition);
                        _functionsInitArgs.TableEnv.Get($"SelectionEffect{i}", out selection.DoEffect);
                        _functionsInitArgs.TableEnv.Get($"SelectionConditionDescription{i}", out selection.ConditionDescription);
                        _functionsInitArgs.TableEnv.Get($"SelectionEffectDescription{i}", out selection.EffectDescription);
                        _selections.Add(selection);
                    }
                }
            }
        }

        private List<SelectArgs> _selections;

        public List<SelectArgs> GetSelections()
        {
            return _selections;
        }
    }
}