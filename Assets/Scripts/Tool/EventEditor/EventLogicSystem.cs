using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Utilities;

namespace EventLogicSystem
{
    #region 逻辑元素

    [EditorName("功能")]
    public class BaseFuncObj : ILogicBoolBaseObj, ILogicFrameBaseObj
    {
        private static List<BaseFunc> _basnFuncs = null;

        /// <summary>
        /// 函数数组，首位是函数名，1位开始是参数列表
        /// </summary>
        public string[] Func { get; set; } = new[] { "", "" };

        /// <summary>
        /// 自定义的创建函数
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="para"></param>
        /// <returns></returns>
        public static BaseFuncObj Create(string funcName, string para)
        {
            var l = new BaseFuncObj();
            l.Func = new string[2];

            l.Func[0] = funcName;

            //for (int i = 0; i < para.Length; i++)
            {
                l.Func[1] = para;
            }
            return l;
        }

        public string GetDescription()
        {
            if (string.IsNullOrEmpty(Func[0]))
            {
                return string.Empty;
            }
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("\"..(");
            if (_basnFuncs == null)
            {
                _basnFuncs = TypeUtilities.GetChildren<BaseFunc>();
            }

            {
                stringBuilder.Append(Func[0] + "Description");
            }

            stringBuilder.Append("({");

            for (int i = 1; i < Func.Length; i++)
            {
                stringBuilder.Append(Func[i]);
                if (i < Func.Length - 1)
                {
                    stringBuilder.Append(",");
                }
            }

            stringBuilder.Append("}))..\"");
            return stringBuilder.ToString();
        }

        /// <summary>
        /// 构建函数表达式
        /// </summary>
        /// <returns></returns>
        public string GetExpression()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(Func[0]);
            stringBuilder.Append("({");

            for (int i = 1; i < Func.Length; i++)
            {
                stringBuilder.Append(Func[i]);
                if (i < Func.Length - 1)
                {
                    stringBuilder.Append(",");
                }
            }

            stringBuilder.Append("})");

            return stringBuilder.ToString();
        }
    }

    [EditorName("选项")]
    public class SelectFuncObj : ILogicFrameBaseObj
    {
        public List<Selection> Selections { get; set; } = new List<Selection>() { };

        public string GetDescription()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < Selections.Count; i++)
            {
                stringBuilder.Append(i + "[" + Selections[i].SelectionText + "] 当 ");
                stringBuilder.Append(Selections[i].Condition?.GetDescription());
                stringBuilder.Append(" ");
                stringBuilder.Append("可");
                stringBuilder.Append(Selections[i].Effect?.GetDescription());
                stringBuilder.Append("\n");
            }
            return stringBuilder.ToString();
        }

        public string GetExpression()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < Selections.Count; i++)
            {
                stringBuilder.Append(Selections[i].GetExpression() + "\n");
                {
                    stringBuilder.Append($"GetSelectionText{i} = GetSelectionText\n");
                    stringBuilder.Append($"SelectionDisable{i} = SelectionDisable\n");
                    stringBuilder.Append($"SelectionShowCondition{i} = SelectionShowCondition\n");
                    stringBuilder.Append($"SelectionCondition{i} = SelectionCondition\n");
                    stringBuilder.Append($"SelectionEffect{i} = SelectionEffect\n");
                    stringBuilder.Append($"SelectionConditionDescription{i} = SelectionConditionDescription \n");
                    stringBuilder.Append($"SelectionEffectDescription{i} = SelectionEffectDescription\n");
                }
            }
            stringBuilder.Append("SelectionCount = " + Selections.Count);

            return stringBuilder.ToString();
        }

        public class Selection : ILogicBaseObj
        {
            public SelectionDisableType DisableShow = SelectionDisableType.CantClick;

            public bool ShowCondition = true;

            public string SelectionText;

            public BaseAndObj Condition { get; set; } = new BaseAndObj();

            public BaseSequenceObj Effect { get; set; } = new BaseSequenceObj();

            public string GetDescription()
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("当 ");
                stringBuilder.Append(Condition?.GetDescription());
                stringBuilder.Append(" ");
                stringBuilder.Append("可");
                stringBuilder.Append(Effect?.GetDescription());
                stringBuilder.Append("\n");
                return stringBuilder.ToString();
            }

            public string GetExpression()
            {
                StringBuilder stringBuilder = new StringBuilder();
                string textStr = SelectionText.Replace("<<", "\"..(").Replace(">>", ")..\"").Replace("\n", "\\n");
                stringBuilder.Append($"GetSelectionText = function()\n return \"{textStr}\" \n end\n");
                stringBuilder.Append($"SelectionDisable = {(int)DisableShow}\n");

                stringBuilder.Append($"SelectionShowCondition = {ShowCondition.ToString().ToLower()}\n");

                stringBuilder.Append("SelectionCondition = function() \n");
                if (Condition != null)
                {
                    stringBuilder.Append("return " + Condition.GetExpression() + "\n");
                }
                else
                {
                    stringBuilder.Append("return true\n");
                }
                stringBuilder.Append("end\n");
                stringBuilder.Append("SelectionEffect = function() \n");
                if (Effect != null)
                {
                    stringBuilder.Append(Effect.GetExpression());
                }
                stringBuilder.Append("end\n");

                stringBuilder.Append("SelectionConditionDescription = function() \n");
                if (Condition != null)
                {
                    stringBuilder.Append("return \"" + Condition.GetDescription() + "\"\n");
                }
                else
                {
                    stringBuilder.Append("return \"\"\n");
                }
                stringBuilder.Append("end\n");
                stringBuilder.Append("SelectionEffectDescription = function() \n");
                if (Effect != null)
                {
                    stringBuilder.Append("return \"" + Effect.GetDescription() + "\"\n");
                }
                else
                {
                    stringBuilder.Append("return \"\"\n");
                }

                stringBuilder.Append("end\n");
                return stringBuilder.ToString();
            }
        }
    }

    [EditorName("与")]
    public class BaseAndObj : ILogicBoolBaseObj
    {
        public List<ILogicBoolBaseObj> And { get; set; } = new List<ILogicBoolBaseObj>();

        public string GetDescription()
        {
            var stringBuilder = new StringBuilder();
            //stringBuilder.Append("(");

            for (int i = 0; i < And.Count; i++)
            {
                stringBuilder.Append(And[i].GetDescription());
                if (i < And.Count - 1)
                {
                    stringBuilder.Append(" 且 ");
                }
            }

            // stringBuilder.Append(")");
            return stringBuilder.ToString();
        }

        public string GetExpression()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("(");

            if (And.Count == 0)
            {
                stringBuilder.Append("true");
            }
            for (int i = 0; i < And.Count; i++)
            {
                stringBuilder.Append(And[i].GetExpression());
                if (i < And.Count - 1)
                {
                    stringBuilder.Append(" and ");
                }
            }

            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }
    }

    [EditorName("或")]
    public class BaseOrObj : ILogicBoolBaseObj
    {
        public List<ILogicBoolBaseObj> Or { get; set; } = new List<ILogicBoolBaseObj>();

        public string GetDescription()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("(");

            if (Or.Count == 0)
            {
                stringBuilder.Append("true");
            }

            for (int i = 0; i < Or.Count; i++)
            {
                stringBuilder.Append(Or[i].GetDescription());
                if (i < Or.Count - 1)
                {
                    stringBuilder.Append(" 或 ");
                }
            }

            stringBuilder.Append(")");

            return stringBuilder.ToString();
        }

        public string GetExpression()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("(");

            for (int i = 0; i < Or.Count; i++)
            {
                stringBuilder.Append(Or[i].GetExpression());
                if (i < Or.Count - 1)
                {
                    stringBuilder.Append(" or ");
                }
            }

            stringBuilder.Append(")");

            return stringBuilder.ToString();
        }
    }

    [EditorName("非")]
    public class BaseNotObj : ILogicBoolBaseObj
    {
        public ILogicBoolBaseObj Not { get; set; }

        public string GetDescription()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("(非 ");
            {
                stringBuilder.Append(Not?.GetDescription());

                stringBuilder.Append(")");
            }

            return stringBuilder.ToString();
        }

        public string GetExpression()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("(not ");
            {
                stringBuilder.Append(Not.GetExpression());

                stringBuilder.Append(")");
            }

            return stringBuilder.ToString();
        }
    }

    /// <summary>
    /// 循环语句
    /// </summary>
    [EditorName("循环")]
    public class BaseWhileObj : ILogicFrameBaseObj
    {
        public ILogicBoolBaseObj While { get; set; }
        public List<ILogicFrameBaseObj> Then { get; set; } = new List<ILogicFrameBaseObj>();

        public string GetDescription()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("当(");
            {
                stringBuilder.Append(While?.GetExpression());

                stringBuilder.Append(")循环");

                for (int i = 0; i < Then.Count; i++)
                {
                    stringBuilder.Append(Then[i].GetDescription());
                    if (i < Then.Count - 1)
                    {
                        stringBuilder.Append("\n");
                    }
                }
            }

            return stringBuilder.ToString();
        }

        public string GetExpression()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("while(");
            {
                stringBuilder.Append(While != null ? While.GetExpression() : "false");
                stringBuilder.Append(") do ");

                for (int i = 0; i < Then.Count; i++)
                {
                    stringBuilder.Append(Then[i].GetDescription());
                    if (i < Then.Count - 1)
                    {
                        stringBuilder.Append("\n");
                    }
                }

                stringBuilder.Append(" end ");
            }

            return stringBuilder.ToString();
        }
    }

    /// <summary>
    /// 条件语句
    /// </summary>
    [EditorName("如果")]
    public class BaseIfObj : ILogicFrameBaseObj
    {
        public List<ILogicBoolBaseObj> If { get; set; } = new List<ILogicBoolBaseObj>();
        public List<ILogicFrameBaseObj> Then { get; set; } = new List<ILogicFrameBaseObj>();
        public List<ILogicFrameBaseObj> Else { get; set; } = new List<ILogicFrameBaseObj>();

        public string GetDescription()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("如果[");
            {
                for (int i = 0; i < If.Count; i++)
                {
                    stringBuilder.Append(If[i].GetDescription());
                    if (i < If.Count - 1)
                    {
                        stringBuilder.Append(" ");
                    }
                }

                stringBuilder.Append("] 则 ");

                for (int i = 0; i < Then.Count; i++)
                {
                    stringBuilder.Append(Then[i].GetDescription());
                    if (i < Then.Count - 1)
                    {
                        stringBuilder.Append(" ");
                    }
                }
                if (Else != null && Else.Count > 0)
                {
                    stringBuilder.Append(" 否则 ");
                    for (int i = 0; i < Else.Count; i++)
                    {
                        stringBuilder.Append(Else[i].GetDescription());
                        if (i < Then.Count - 1)
                        {
                            stringBuilder.Append(" ");
                        }
                    }
                }
                //stringBuilder.Append(" end ");
            }
            return stringBuilder.ToString();
        }

        public string GetThenDescription()
        {
            var stringBuilder = new StringBuilder();

            {
                for (int i = 0; i < Then.Count; i++)
                {
                    stringBuilder.Append(Then[i].GetExpression());
                    if (i < Then.Count - 1)
                    {
                        stringBuilder.Append("\n");
                    }
                }

                //stringBuilder.Append(" end ");
            }
            return stringBuilder.ToString();
        }

        public string GetExpression()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("if(");
            {
                stringBuilder.Append(If.Count > 0 ? "" : "false");

                for (int i = 0; i < If.Count; i++)
                {
                    stringBuilder.Append(If[i].GetExpression());
                    if (i < Then.Count - 1)
                    {
                        stringBuilder.Append("  ");
                    }
                }

                stringBuilder.Append(") then ");

                for (int i = 0; i < Then.Count; i++)
                {
                    stringBuilder.Append(Then[i].GetExpression());

                    if (i < Then.Count - 1)
                    {
                        stringBuilder.Append("\n");
                    }
                }

                if (Else != null && Else.Count > 0)
                {
                    stringBuilder.Append(" else ");
                    for (int i = 0; i < Else.Count; i++)
                    {
                        stringBuilder.Append(Else[i].GetExpression());

                        if (i < Else.Count - 1)
                        {
                            stringBuilder.Append("\n");
                        }
                    }
                }
                stringBuilder.Append(" end ");
            }

            return stringBuilder.ToString();
        }
    }

    /// <summary>
    /// 顺序语句
    /// </summary>
    [EditorName("顺序")]
    public class BaseSequenceObj : ILogicFrameBaseObj
    {
        public List<ILogicFrameBaseObj> List { get; set; } = new List<ILogicFrameBaseObj>();

        public string GetDescription()
        {
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < List.Count; i++)
            {
                stringBuilder.Append(List[i].GetDescription());
                if (i < List.Count - 1)
                {
                    stringBuilder.Append(" ");
                }
            }

            return stringBuilder.ToString();
        }

        public string GetExpression()
        {
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < List.Count; i++)
            {
                stringBuilder.Append(List[i].GetExpression());
                if (i < List.Count - 1)
                {
                    stringBuilder.Append("\n");
                }
            }

            return stringBuilder.ToString();
        }
    }

    /// </summary>
    [EditorName("随机")]
    public class BaseRandomObj : ILogicFrameBaseObj
    {
        public List<string> RandomWeight { get; set; } = new List<string>();

        public List<BaseSequenceObj> RandomList { get; set; } = new List<BaseSequenceObj>();

        public string GetDescription()
        {
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < RandomWeight.Count && i < RandomList.Count; i++)
            {
                stringBuilder.Append(RandomWeight[i]);
                stringBuilder.Append("的几率 ");
                stringBuilder.Append(RandomList[i].GetDescription());
                if (i < RandomList.Count - 1)
                {
                    stringBuilder.Append(" ");
                }
            }

            return stringBuilder.ToString();
        }

        public string GetExpression()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("local weight = 0\n");
            stringBuilder.Append("local weight0 = 0\n");
            for (int i = 0; i < RandomWeight.Count && i < RandomList.Count; i++)
            {
                //这里乘以100是为了保证万一是小数也能转成整数
                stringBuilder.Append("weight = weight +  math.ceil(100 *");
                stringBuilder.Append(RandomWeight[i]);
                stringBuilder.Append($") \nweight{i + 1} = weight\n");
            }

            stringBuilder.Append("local rand = math.random(0,weight)\n");
            for (int i = 0; i < RandomWeight.Count && i < RandomList.Count; i++)
            {
                stringBuilder.Append($"if (rand > weight{i} and rand<=weight{i + 1} ) then \n");
                stringBuilder.Append(RandomList[i].GetExpression());
                if (i < RandomList.Count - 1)
                {
                    stringBuilder.Append("\n");
                }
                stringBuilder.Append($"end\n");
            }

            return stringBuilder.ToString();
        }
    }

    /// <summary>
    /// 逻辑布尔对象基类
    /// </summary>
    public interface ILogicBoolBaseObj : ILogicBaseObj
    {
    }

    /// <summary>
    /// 逻辑结构对象基类
    /// </summary>
    public interface ILogicFrameBaseObj : ILogicBaseObj
    {
    }

    /// <summary>
    /// 逻辑结构对象基类
    /// </summary>
    public interface ILogicBaseObj
    {
        string GetExpression();

        string GetDescription();
    }

    #endregion 逻辑元素

    #region 额外工具

    public abstract class JsonCreationConverter<T> : JsonConverter
    {
        protected abstract T Create(Type objectType, JObject jsonObject);

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var target = Create(objectType, jsonObject);
            serializer.Populate(jsonObject.CreateReader(), target);
            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

    public class JsonLogicConverter : JsonCreationConverter<ILogicBaseObj>
    {
        protected override ILogicBaseObj Create(Type objectType, JObject jsonObject)
        {
            JToken v;
            if (jsonObject.TryGetValue("And", out v))
            {
                return new BaseAndObj();
            }
            if (jsonObject.TryGetValue("Or", out v))
            {
                return new BaseOrObj();
            }

            if (jsonObject.TryGetValue("Not", out v))
            {
                return new BaseNotObj();
            }

            if (jsonObject.TryGetValue("Func", out v))
            {
                return new BaseFuncObj();
            }
            if (jsonObject.TryGetValue("List", out v))
            {
                return new BaseSequenceObj();
            }
            if (jsonObject.TryGetValue("While", out v))
            {
                return new BaseWhileObj();
            }

            if (jsonObject.TryGetValue("If", out v))
            {
                return new BaseIfObj();
            }
            if (jsonObject.TryGetValue("Selections", out v))
            {
                return new SelectFuncObj();
            }
            if (jsonObject.TryGetValue("SelectionText", out v))
            {
                return new SelectFuncObj.Selection();
            }
            if (jsonObject.TryGetValue("RandomList", out v))
            {
                return new BaseRandomObj();
            }
            Debug.LogError("找不到反序列化对象");
            return null;
        }
    }

    #endregion 额外工具
}