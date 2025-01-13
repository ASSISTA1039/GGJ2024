using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using XLua;
using QxFramework.Core;

namespace EventLogicSystem
{
    public partial class EventContext
    {
    }

    public class BaseFunc
    {
        public struct ParamDefinition
        {
            public string Name;
            public Type ParamType;
            public object DefualtValue;

            public ParamDefinition(string name, Type paramType, object defualtValue)
            {
                Name = name;
                ParamType = paramType;
                DefualtValue = defualtValue;
            }
        }

        public struct ParamInfo
        {
            public string Name;
            public object Value;

            public ParamInfo(string name, object value)
            {
                Name = name;
                Value = value;
            }
        }

        private List<ParamDefinition> _params;

        public List<ParamDefinition> Params
        {
            get
            {
                if (_params == null)
                {
                    _paramsCount = GetParams(out _params);
                }

                return _params;
            }
        }

        private int _paramsCount;

        public int ParamsCount
        {
            get
            {
                if (_params == null)
                {
                   CanAddParamsCount = GetParams(out _params);
                    _paramsCount = _params.Count;
                }

                return _paramsCount;
            }
        }

        public int CanAddParamsCount;

        public bool CanAdd
        {
            get
            {
                return CanAddParamsCount> 0;
            }
        }

        /// <summary>
        /// 返回参数类型
        /// </summary>
        /// <returns>数量</returns>
        [CanBeNull]
        protected virtual int GetParams(out List<ParamDefinition> list)
        {
            list = null;

            return 0;
        }

        public virtual bool Run(EventContext context, List<ParamInfo> list)
        {
            return true;
        }

        public virtual string GetDescription(EventContext context, List<ParamInfo> list)
        {
            return "";
        }

        public virtual string GetItemName()
        {
            return "空";
        }
    }

    public class Log : BaseFunc
    {
        protected override int GetParams(out List<ParamDefinition> list)
        {
            list = new List<ParamDefinition>(){
                new ParamDefinition("日志",typeof(string), "")
            };
            return list.Count;
        }

        public override bool Run(EventContext context, List<ParamInfo> list)
        {
            Debug.Log("[EventLogicSystem]Log：" + list[0].Value);
            return true;
        }

        public override string GetItemName()
        {
            return "次要功能/输出日志";
        }

        public override string GetDescription(EventContext context, List<ParamInfo> list)
        {
            return "输出日志：" + list[0].Value;
        }
    }
    public class OpenUI : BaseFunc
    {
        protected override int GetParams(out List<ParamDefinition> list)
        {
            list = new List<ParamDefinition>(){
                new ParamDefinition("UI名称",typeof(string), "")
            };
            return list.Count;
        }

        public override bool Run(EventContext context, List<ParamInfo> list)
        {
            UIManager.Instance.Open(list[0].Value.ToString());
            Debug.Log("打开UI：" + list[0].Value);
            return true;
        }

        public override string GetItemName()
        {
            return "主要功能/打开UI";
        }

        public override string GetDescription(EventContext context, List<ParamInfo> list)
        {
            return "打开UI：" + list[0].Value;
        }
    }
    public class TryEvent : BaseFunc
    {
        protected override int GetParams(out List<ParamDefinition> list)
        {
            list = new List<ParamDefinition>(){
                new ParamDefinition("触发事件",typeof(int), 0)
            };
            return list.Count;
        }

        public override bool Run(EventContext context, List<ParamInfo> list)
        {
            GameMgr.Get<IEventManager>().ForceEvent((int)list[0].Value);
            Debug.Log("触发事件" + list[0].Value);
            return true;
        }

        public override string GetItemName()
        {
            return "主要功能/触发事件";
        }

        public override string GetDescription(EventContext context, List<ParamInfo> list)
        {
            return "触发事件：" + list[0].Value;
        }
    }

    public class MoreThan : BaseFunc
    {
        protected override int GetParams(out List<ParamDefinition> list)
        {
            list = new List<ParamDefinition>(){
                new ParamDefinition("值1",typeof(int), 0),
                new ParamDefinition("值2",typeof(int), 0)
            };
            return list.Count;
        }

        public override bool Run(EventContext context, List<ParamInfo> list)
        {
            return (int)list[0].Value > (int)list[1].Value;
        }

        public override string GetItemName()
        {
            return "判断/大于";
        }

        public override string GetDescription(EventContext context, List<ParamInfo> list)
        {
            return list[0].Value.ToString() + " >" + list[1].Value.ToString();
        }
    }

    public class DialogHit : BaseFunc
    {
        protected override int GetParams(out List<ParamDefinition> list)
        {
            list = new List<ParamDefinition>(){
                new ParamDefinition("提示",typeof(string), "")
            };
            //数量可重复增加参数数量
            return 0;
        }

        /// <summary>
        /// 运行函数
        /// </summary>
        /// <param name="context"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public override bool Run(EventContext context, List<ParamInfo> list)
        {
            UIManager.Instance.Open("DialogWindowUI", args: new DialogWindowUI.DialogWindowUIArg
            ("提示", list[0].Value.ToString(), null, "确定", () =>
            {
            }));
            return true;
        }

        public override string GetItemName()
        {
            return "主要功能/提示框";
        }

        public override string GetDescription(EventContext context, List<ParamInfo> list)
        {
            return "提示框：" + list[0].Value;
        }
    }


    /// <summary>
    /// 基础类型转换
    /// </summary>
    public class ValueFromLuaTable
    {
        public virtual Type GetTargetType()
        {
            return null;
        }

        public virtual object GetValue(LuaTable luaTable, object key)
        {
            return null;
        }

        public virtual string WriteValue(object obj)
        {
            return obj.ToString();
        }
    }

    /// <summary>
    /// int类型转换
    /// </summary>
    public class IntFromLuaTable : ValueFromLuaTable
    {
        public override Type GetTargetType()
        {
            return typeof(int);
        }

        public override object GetValue(LuaTable luaTable, object key)
        {
            int res = 0;
            luaTable.Get(key, out res);
            return res;
        }

        public override string WriteValue(object obj)
        {
            return obj.ToString();
        }
    }

    /// <summary>
    /// Bool类型转换
    /// </summary>
    public class BoolFromLuaTable : ValueFromLuaTable
    {
        public override Type GetTargetType()
        {
            return typeof(bool);
        }

        public override object GetValue(LuaTable luaTable, object key)
        {
            bool res = false;
            luaTable.Get(key, out res);
            return res;
        }

        public override string WriteValue(object obj)
        {
            //注意这里直接 ToString 会变成大写字母开头的True或者False
            return (obj.Equals("true") || obj.Equals(true)) ? "true" : "false";
        }
    }

    /// <summary>
    /// Float类型转换
    /// </summary>
    public class FloatFromLuaTable : ValueFromLuaTable
    {
        public override Type GetTargetType()
        {
            return typeof(float);
        }

        public override object GetValue(LuaTable luaTable, object key)
        {
            float res = 0;
            luaTable.Get(key, out res);
            return res;
        }

        public override string WriteValue(object obj)
        {
            return obj.ToString();
        }
    }

    /// <summary>
    /// String类型转换
    /// </summary>
    public class StringFromLuaTable : ValueFromLuaTable
    {
        public override Type GetTargetType()
        {
            return typeof(string);
        }

        public override object GetValue(LuaTable luaTable, object key)
        {
            string res = "";
            luaTable.Get(key, out res);
            return res;
        }

        public override string WriteValue(object obj)
        {
            string trans = obj.ToString();

            trans = "\"" + trans.Replace("\n", "\\n") + "\"";
            return trans;
        }
    }

    /// <summary>
    /// int列表类型转换
    /// </summary>
    public class IntListFromLuaTable : ValueFromLuaTable
    {
        public override Type GetTargetType()
        {
            return typeof(List<int>);
        }

        public override object GetValue(LuaTable luaTable, object key)
        {
            List<int> res = new List<int>();
            luaTable.Get(key, out res);
            return res;
        }

        public override string WriteValue(object obj)
        {
            // 直接传入的就是list的字符串
            return obj != null ? obj.ToString() : "{}";
        }
    }
}