using System;
using System.Collections;
using System.Collections.Generic;
using Utilities;
using UnityEngine;
using XLua;
using System.Text;

namespace EventLogicSystem
{
    public class ConditionEventCreator
    {
        public const string DirPath = "Text/EventJson/";

        private List<ConditionEventTemplate> _conditionEventTemplateList;

        private Dictionary<Type, ValueFromLuaTable> _typeValueTables = new Dictionary<Type, ValueFromLuaTable>();

        private Dictionary<string, BaseFunc> _name2Func = new Dictionary<string, BaseFunc>();

        private LuaEnv _luaEnv;

        private EventContext _context;

        private Dictionary<int, BoolFunc> _allCondition = new Dictionary<int, BoolFunc>();

        public void Init(List<ConditionEventTemplate> list, EventContext context)
        {
            _context = context;
            InitLuaEnv();
            InitFunc();
            //导入每个模板
            foreach (var conditionEventTemplate in list)
            {
                InitTemplate(conditionEventTemplate);
            }

            _conditionEventTemplateList = list;
        }

        private void InitLuaEnv()
        {
            _luaEnv = new LuaEnv();
            _luaEnv.DoString("math.randomseed(tostring(os.time()):reverse():sub(1, 7))");
        }

        public void OverwriteTemplate(ConditionEventTemplate conditionEventTemplate)
        {
            InitTemplate(conditionEventTemplate);
        }

        /// <summary>
        /// 初始化各种方法对象
        /// </summary>
        public void InitFunc()
        {

            var fromLuaTables = TypeUtilities.GetChildren<ValueFromLuaTable>();
            foreach (var v in fromLuaTables)
            {
                _typeValueTables[v.GetTargetType()] = v;
            }

            var funcs = TypeUtilities.GetChildren<BaseFunc>();

            foreach (var v in funcs)
            {
                string name = v.GetType().Name;
                _name2Func[name] = v;
                //设置相应的执行函数
                _luaEnv.Global.Set<string, Func<LuaTable, bool>>(name, (LuaTable luaTable) => LuaCallRun(luaTable, name));
                _luaEnv.Global.Set<string, Func<LuaTable, string>>(name + "Description", (LuaTable luaTable) => LuaCallDescription(luaTable, name));
            }
        }

        private List<BaseFunc.ParamInfo> _callParamInfosCache = new List<BaseFunc.ParamInfo>();

        public bool LuaCallRun(LuaTable table, string funcName)
        {
            var func = _name2Func[funcName];
            _callParamInfosCache.Clear();

            for (int i = 0; i < table.Length && func.ParamsCount > 0; i++)
            {
                var paramInfo = func.Params[i % func.ParamsCount];
                //按顺序获得调用对象
                _callParamInfosCache.Add(
                    new BaseFunc.ParamInfo(paramInfo.Name,
                        _typeValueTables[paramInfo.ParamType].GetValue(table, i + 1)));
            }

            return func.Run(_context, _callParamInfosCache);
        }

        private string LuaCallDescription(LuaTable table, string funcName)
        {
            var func = _name2Func[funcName];
            _callParamInfosCache.Clear();

            for (int i = 0; i < table.Length && func.ParamsCount > 0; i++)
            {
                var paramInfo = func.Params[i % func.ParamsCount];
                //按顺序获得调用对象
                _callParamInfosCache.Add(
                    new BaseFunc.ParamInfo(paramInfo.Name,
                        _typeValueTables[paramInfo.ParamType].GetValue(table, i + 1)));
            }
            try
            {
                return func.GetDescription(_context, _callParamInfosCache);
            }
            //运行时代码有可能在编辑器下会抛异常
            catch (Exception)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(func.GetItemName());

                for (int i = 0; i < _callParamInfosCache.Count; i++)
                {
                    sb.Append(_callParamInfosCache[i].Value.ToString());
                    sb.Append(" ");
                }
                return sb.ToString();
            }
        }

        private BaseFunc GetFunc(string funcName)
        {
            return _name2Func[funcName];
        }

        public void InitTemplate(ConditionEventTemplate template)
        {
            LuaTable tamplateTable = _luaEnv.NewTable();
            _luaEnv.Global.Set("T" + template.TemplateId, tamplateTable);

            LuaTable meta = _luaEnv.NewTable();

            meta.Set("__index", _luaEnv.Global);
            tamplateTable.SetMetaTable(meta);
            meta.Dispose();

            //  tamplateTable.Set("self", tamplateTable);

            //转换字符串中的转义
            string nameStr = template.Name.Replace("<<", "\"..(").Replace(">>", ")..\"").Replace("\n", "\\n");
            string textStr = template.Text.Replace("<<", "\"..(").Replace(">>", ")..\"").Replace("\n", "\\n");

            //生成函数字符串
            string getNameStr = $"function GetName()\n return \"{nameStr}\" \n end";
            string getTextStr = $"function GetText()\n return \"{textStr}\" \n end";
            string tryConditionStr = $"function TryCondition()\n return {(template.Condition != null ? template.Condition.GetExpression() : "true")} \n end";
            string tryConditionDesStr = $"function TryConditionDescription()\n return {(template.Condition != null ? "\"" + template.Condition.GetDescription().Replace("\n", "\\n") + "\"" : "")} \n end";
            string doEffectStr = $"function T{template.TemplateId}:DoEffect()\n {(template.Effect != null ? template.Effect.GetExpression() : "")} \n end";

            string doEffectDesStr = $"function EffectDescription()\n return {(template.Effect != null ? "\"" + template.Effect.GetDescription().Replace("\n", "\\n") + "\"" : "")} \n end";

            tryConditionStr = tryConditionStr.Replace("<<", "\"..(").Replace(">>", ")..\"");
            doEffectStr = doEffectStr.Replace("<<", "\"..(").Replace(">>", ")..\"");

            try
            {
                tamplateTable.Set("p", _context.Params);

                _luaEnv.DoString(tryConditionStr, "TryCondition", tamplateTable);
                _luaEnv.DoString(tryConditionDesStr, "TryConditionDescription", tamplateTable);
                _luaEnv.DoString(doEffectStr, "DoEffect", tamplateTable);
                _luaEnv.DoString(doEffectDesStr, "EffectDescription", tamplateTable);

                _luaEnv.DoString(getNameStr, "GetName", tamplateTable);
                _luaEnv.DoString(getTextStr, "GetText", tamplateTable);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.LogError(template.Name);
            }

            //导入表格函数

            //输入需要的参数
            tamplateTable.Set("TemplateId", template.TemplateId);
            tamplateTable.Set("tid", template.TemplateId);

        }

        /// <summary>
        /// 设置环境参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="obj"></param>
        public void SetEnvParam(string name, object obj)
        {
            _luaEnv.Global.Set(name, obj);
        }

        /// <summary>
        /// 设置生成的事件
        /// </summary>
        /// <param name="conditionEvent"></param>
        public void SetConditionEvent(ConditionEvent conditionEvent, List<int> paramList)
        {
            //设置表环境
            LuaTable meta = _luaEnv.NewTable();
            meta.Set("__index", _luaEnv.Global.GetInPath<LuaTable>("T" + conditionEvent.TemplateId));
            LuaTable env = _luaEnv.NewTable();
            env.SetMetaTable(meta);
            meta.Dispose();

            //  env.Set("self", env);

            var tp = GetTemplate(conditionEvent.TemplateId);
            conditionEvent.Show = tp.Show;
            conditionEvent.ImagePath = tp.ImagePath;

            ConditionEvent.FunctionsInitArgs functionsInitArgs = new ConditionEvent.FunctionsInitArgs();
            _context.Params.Clear();
            if (paramList != null && paramList.Count > 0)
            {
                _context.Params.AddRange(paramList);
            }

            env.Get("GetName", out functionsInitArgs.GetName);
            env.Get("GetText", out functionsInitArgs.GetText);
            env.Get("TryCondition", out functionsInitArgs.TryCondition);
            env.Get("DoEffect", out functionsInitArgs.DoEffect);
            env.Get("TryConditionDescription", out functionsInitArgs.ConditionDescription);
            env.Get("EffectDescription", out functionsInitArgs.EffectDescription);

            functionsInitArgs.TableEnv = env;

            conditionEvent.InitFunction(functionsInitArgs);
        }

        public ConditionEventTemplate GetTemplate(int id)
        {
            return _conditionEventTemplateList.Find((c) => c.TemplateId == id);
        }

        /// <summary>
        /// 获取所有事件模板
        /// </summary>
        /// <returns></returns>
        public List<ConditionEventTemplate> GetAllEventTemplate()
        {
            return _conditionEventTemplateList;
        }

        /// <summary>
        /// 遍历并获取当前最大的id
        /// </summary>
        /// <returns></returns>
        public int GetMaxTemplateId()
        {
            int id = 0;
            foreach (ConditionEventTemplate conditionEventTemplate in _conditionEventTemplateList)
            {
                id = Mathf.Max(conditionEventTemplate.TemplateId, id);
            }

            return id;
        }

        /// <summary>
        /// 翻译
        /// </summary>
        /// <param name="desLua"></param>
        /// <returns></returns>
        public string TranslateDescription(string desLua)
        {
            try
            {
                int tid = _context.TemplateId;
                _luaEnv.Global.Get("T" + tid, out LuaTable luaTable);
                _luaEnv.DoString(("_tempDes = \"" + desLua + "\"").Replace("\n", "\\n"), env: luaTable);
                luaTable.Get("_tempDes", out string des);
                return des.Replace("\\n", "\n");
            }
            catch (Exception)
            {
                return desLua;
            }
        }

        /// <summary>
        /// 获取选项名称
        /// </summary>
        /// <param name="templateId"></param>
        /// <param name="selectedIndex"></param>
        /// <param name="selectedStr"></param>
        /// <returns></returns>
        public bool TryGetSelectionName(int templateId, int selectedIndex, out string selectedStr)
        {
            selectedStr = string.Empty;
            var evt = GetTemplate(templateId);
            foreach (var item in evt.Effect.List)
            {
                var selection = item as SelectFuncObj;
                if (selection != null)
                {
                    selectedStr = selection.Selections[selectedIndex].SelectionText;
                    selectedStr = TranslateDescription(selectedStr);
                    return true;
                }
            }
            return false;
        }

        public void ForceFullGC()
        {
            _luaEnv.FullGc();
        }

        public void TickGC()
        {
            _luaEnv.Tick();
        }
    }
}