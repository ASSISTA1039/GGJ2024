using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EventLogicSystem;
using Newtonsoft.Json;
using Utilities;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Assertions.Must;
using XLua;
using XLua.Cast;
using System.Reflection;

public class GameEventEditor : OdinMenuEditorWindow
{
    public static ConditionEventCreator ConditionEventCreator = new ConditionEventCreator();
    public DrawNode RootConditionNode { get; private set; }
    public DrawNode RootEffectNode { get; private set; }

    private OdinMenuTree _tree;
    private Vector2 _scrollView;

    private List<ConditionEventTemplate> _list;

    private string[] _showTypeStrs = new string[] { "隐藏 (Hide)", "￣ 提示 (Hint)", "☐ 对话框 (Dialog)" };
    private string[] _autoRunTypeStrs = new string[] { "当满足条件 (ConditionIsTrue)", "不自动执行 (Non)", "数量随机 (Random)", };

    //private bool _needUpdate = false;

    //显示贴图
    private Texture _showTexture;

    private EventContext _eventContext;

    private const string DefaultFileName = "ConditionEvents";

    [MenuItem("Tools/事件编辑器Ex")]
    private static void OpenWindow()
    {
        var window = GetWindow<GameEventEditor>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
        window.InitMenuTree();
    }

    protected void InitMenuTree()
    {
        /* 读取数据 */
        if (_list == null)
        {
            LoadFromFile();
        }

        if (_list == null)
        {
            /* 测试数据*/
            _list = new List<ConditionEventTemplate>();
            for (int i = 0; i < 10; i++)
            {
                var conditionEvent = new ConditionEventTemplate() { TemplateId = i + 100, Name = "事件" + i.ToString(), Path = "事件链1", Text = "事件文本" };
                TestValue(conditionEvent.TemplateId, out conditionEvent.Condition, out conditionEvent.Effect);
                _list.Add(conditionEvent);
            }

            for (int i = 0; i < 10; i++)
            {
                var conditionEvent = new ConditionEventTemplate() { TemplateId = i + 200, Name = "事件" + i.ToString(), Path = "事件链2", Text = "事件文本" };
                TestValue(conditionEvent.TemplateId, out conditionEvent.Condition, out conditionEvent.Effect);
                _list.Add(conditionEvent);
            }
        }

        _eventContext = new EventContext();
        _eventContext.Init();
        _eventContext.EventCreator = ConditionEventCreator;
        ConditionEventCreator.Init(_list, _eventContext);

        _tree = new OdinMenuTree(false);

        //事件列表UI
        var customMenuStyle = new OdinMenuStyle
        {
            BorderPadding = 0f,
            AlignTriangleLeft = true,
            TriangleSize = 16f,
            TrianglePadding = 0f,
            Offset = 20f,
            Height = 28,
            IconPadding = 0f,
            BorderAlpha = 0.323f
        };

        MenuWidth = 300f;
        _tree.DefaultMenuStyle = customMenuStyle;

        _tree.Config.DrawSearchToolbar = true;
        _tree.Config.UseCachedExpandedStates = true;
        // Adds the custom menu style to the tree, so that you can play around with it.
        // Once you are happy, you can press Copy C# Snippet copy its settings and paste it in code.
        // And remove the "Menu Style" menu item from the tree.

        for (int i = 0; i < _list.Count; i++)
        {
            var conditionEvent = _list[i];
            var customMenuItem = new EventMenuItem(_tree, conditionEvent);
            _tree.AddMenuItemAtPath(conditionEvent.Path, customMenuItem);
        }

        _tree.UpdateMenuTree();
    }

    protected override void OnGUI()
    {
        if (_tree == null)
        {
            InitMenuTree();
        }

        GUILayout.BeginHorizontal();
        int helpBoxFontSize = EditorStyles.helpBox.fontSize;
        int labelFontSize = EditorStyles.label.fontSize;
        int foldoutFontSize = EditorStyles.foldout.fontSize;
        int foldoutHeaderFontSize = EditorStyles.foldoutHeader.fontSize;
        int textFieldFontSize = EditorStyles.textField.fontSize;
        int textAreaFontSize = EditorStyles.textArea.fontSize;
        int toggleFontSize = EditorStyles.toggle.fontSize;
        int fontSize = 14;
        EditorStyles.helpBox.fontSize = fontSize;
        EditorStyles.label.fontSize = fontSize;
        EditorStyles.foldout.fontSize = fontSize;
        EditorStyles.foldoutHeader.fontSize = fontSize;
        EditorStyles.textField.fontSize = fontSize;
        EditorStyles.textArea.fontSize = fontSize;
        EditorStyles.toggle.fontSize = fontSize;
        //---------------------
        //左侧按钮
        GUILayout.BeginVertical(GUILayoutOptions.Width(this.MenuWidth).ExpandHeight(true));
        _tree.DrawMenuTree();
        EditorGUILayout.Space();
        if (GUILayout.Button("↻ 刷新"))
        {
            CheckAllFunc();
            RefreshTree();
        }
        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+ 新增"))
        {
            var cet = (_tree.Selection.SelectedValue as ConditionEventTemplate);
            int id = GetNewId(cet != null ? cet.Path : null);
            var conditionEvent = new ConditionEventTemplate()
            {
                TemplateId = id,
                Name = "事件" + id.ToString(),
                Path = (cet != null) ? cet.Path : "ConditionEvents",
                Text = "",
                Condition = new BaseAndObj(),
                Effect = new BaseSequenceObj()
            };

            _list.Add(conditionEvent);
            _list.Sort((t1, t2) =>
                 {
                     return t1.TemplateId.ToString().CompareTo(t2.TemplateId.ToString());
                 });

            InitMenuTree();
            var omi = FindItem(_tree.MenuItems, conditionEvent.TemplateId);
            if (omi != null)
            {
                _tree.Selection.Add(omi);
            }
        }
        if (GUILayout.Button("✉ 克隆"))
        {
            var cet = (_tree.Selection.SelectedValue as ConditionEventTemplate);
            if (cet != null)
            {
                //var str = JsonConvert.SerializeObject(cet);
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                JsonSerializer serializer = new JsonSerializer();

                serializer.Serialize(jsonWriter, cet);

                var conditionEvent = JsonConvert.DeserializeObject<ConditionEventTemplate>(textWriter.ToString(), new JsonLogicConverter());

                int id = GetNewId(conditionEvent.Path != null ? conditionEvent.Path : null);
                conditionEvent.TemplateId = id;

                conditionEvent.Name += "副本";
                _list.Add(conditionEvent);
                _list.Sort((t1, t2) =>
                {
                    return t1.TemplateId.ToString().CompareTo(t2.TemplateId.ToString());
                });

                InitMenuTree();
                {
                    var omi = FindItem(_tree.MenuItems, conditionEvent.TemplateId);
                    if (omi != null)
                    {
                        _tree.Selection.Add(omi);
                    }
                }
            }
        }

        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
        if (GUILayout.Button("☺ 保存"))
        {
            RefreshTree();
            SaveFile();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("☏ 读取"))
        {
            LoadFromFile();

            InitMenuTree();
        }
        GUILayout.EndVertical();

        //右侧面板
        GUILayout.BeginVertical();
        var cond = (_tree.Selection.SelectedValue as ConditionEventTemplate);

        if (cond != null)
        {
            //刷新当前界面节点
            if (RootConditionNode == null || RootConditionNode.mInstance != cond.Condition)
            {
                RootConditionNode = DrawNodeUtil.CreateNode(null, null, cond.Condition.GetType(), cond.Condition, "条件", 0);

                RootEffectNode = DrawNodeUtil.CreateNode(null, null, cond.Effect.GetType(), cond.Effect, "效果", 0);

                _showTexture = Resources.Load<Texture>(cond.ImagePath);
            }

            _eventContext.TemplateId = cond.TemplateId;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("刷新" + cond.Name))
            {
                RootConditionNode = null;
                RootEffectNode = null;
                _showTexture = Resources.Load<Texture>(cond.ImagePath);
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("删除" + cond.Name))
            {
                if (EditorUtility.DisplayDialog("警告", "是否删除" + ((ConditionEventTemplate)_tree.Selection[0].Value).Name, "确定", "取消"))
                {
                    _list.Remove(cond);
                    InitMenuTree();
                }
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("测试 " + cond.Name))
            {
                if (Application.isPlaying)
                {
                    GameMgr.Get<IEventManager>().OverwriteTemplate(cond);
                    GameMgr.Get<IEventManager>().ForceEvent(cond.TemplateId);
                }
                else
                {
                    EditorUtility.DisplayDialog("提示", "请运行游戏", "知道了");
                }
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            _scrollView = GUILayout.BeginScrollView(_scrollView);

            GUILayout.BeginHorizontal();

            //----------------
            //基本参数的填写
            GUILayout.BeginVertical();
            cond.TemplateId = EditorGUILayout.IntField("事件类型Id：", cond.TemplateId);
            EditorGUILayout.Space();
            cond.Name = EditorGUILayout.TextField("事件名：", cond.Name);
            EditorGUILayout.Space();
            cond.Path = EditorGUILayout.TextField("路径：", cond.Path);
            GUILayout.EndVertical();

            EditorGUILayout.Space();

            //-------
            //设置图片

            var newTexture = EditorGUILayout.ObjectField("图像：", _showTexture, typeof(Texture), false) as Texture;
            if (newTexture != _showTexture)
            {
                var imagePath = AssetDatabase.GetAssetPath(newTexture);
                if (imagePath.Contains("/Resources/") && !imagePath.Contains("/Editor/"))
                {
                    _showTexture = newTexture;
                    imagePath = imagePath.Substring(imagePath.IndexOf("/Resources/") + "/Resources/".Length).Replace(".jpg", "").Replace(".png", "");
                    cond.ImagePath = imagePath;
                }
                else
                {
                    EditorUtility.DisplayDialog("提示", "请选用Resources路径下的图片", "懂了");
                }
                _showTexture = newTexture;
            }
            var imageModity = GUILayout.TextArea(cond.ImagePath);
            if (imageModity != cond.ImagePath)
            {
                var modity = Resources.Load<Texture>(imageModity);

                _showTexture = modity;
                cond.ImagePath = imageModity;
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space(20);

            //----------------
            //显示  "当满足条件 (ConditionIsTrue)", "不自动执行 (Non)", "权重随机 (Random)",
            //意味着 cond.AutoRun其实就是随机权重，当为-1时就是满足执行
            GUILayout.BeginHorizontal();
            int autoRunSelect = Mathf.Clamp(cond.AutoRun, -1, 1);

            autoRunSelect = GUILayout.SelectionGrid(autoRunSelect + 1, _autoRunTypeStrs, _autoRunTypeStrs.Length) - 1;

            if (autoRunSelect <= 0)
            {
                cond.AutoRun = autoRunSelect;
                EditorGUILayout.Space();
            }
            else
            {
                if (cond.AutoRun <= 0)
                {
                    cond.AutoRun = 1;
                }
                cond.AutoRun = EditorGUILayout.IntField(cond.AutoRun);
            }

            if (cond.AutoRun == -1 && cond.Condition.And.Count == 0)
            {
                cond.AutoRun = 0;
                EditorUtility.DisplayDialog("提示", "条件为空", "确定");
            }

            cond.MaxCount = EditorGUILayout.IntField("次数上限(0为无限)", cond.MaxCount);
            GUILayout.EndHorizontal();
            //------------------------------

            //------------------------------
            //显示"隐藏 (Hide)", "￣ 提示 (Hint)", "☐ 对话框 (Dialog)"
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            cond.Show = (ShowType)GUILayout.SelectionGrid((int)cond.Show, _showTypeStrs, _autoRunTypeStrs.Length);
            EditorGUILayout.Space();
            GUILayout.EndHorizontal();

            //如果是隐藏事件，不填写文案
            //if (cond.Show != ShowType.Hide)
            //{
            //    cond.Text = GUILayout.TextArea(cond.Text);
            //}
            //else
            //{
            //    cond.Text = "";
            //}

            cond.Text = EditorGUILayout.TextArea(cond.Text, GUILayout.MaxHeight(50));

            //------------------------------
            //填写条件和效果
            if (RootConditionNode != null)
            {
                GUILayout.Label("条件", EditorStyles.largeLabel);
                EditorGUILayout.HelpBox("当 " + ConditionEventCreator.TranslateDescription(cond.Condition.GetDescription()), MessageType.Info);
                RootConditionNode.FoldOut = true;
                RootConditionNode.OnGUI(Rect.zero);
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if (RootEffectNode != null)
            {
                GUILayout.Label("效果", EditorStyles.largeLabel);
                EditorGUILayout.HelpBox("执行 " + ConditionEventCreator.TranslateDescription(cond.Effect.GetDescription()), MessageType.Info);
                RootEffectNode.FoldOut = true;
                RootEffectNode.OnGUI(Rect.zero);
            }
            GUILayout.EndScrollView();
        }

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        EditorStyles.helpBox.fontSize = helpBoxFontSize;
        EditorStyles.label.fontSize = labelFontSize;
        EditorStyles.foldout.fontSize = foldoutFontSize;
        EditorStyles.foldoutHeader.fontSize = foldoutHeaderFontSize;
        EditorStyles.textField.fontSize = textFieldFontSize;
        EditorStyles.textArea.fontSize = textAreaFontSize;
        EditorStyles.toggle.fontSize = toggleFontSize;

        this.RepaintIfRequested();
    }

    /// <summary>
    /// 这个函数生成所有节点，主要是为了FuncNode里面的简易纠错
    /// </summary>
    private void CheckAllFunc()
    {
        foreach (var cond in _list)
        {
            RootConditionNode = DrawNodeUtil.CreateNode(null, null, cond.Condition.GetType(), cond.Condition, "条件", 0);

            RootEffectNode = DrawNodeUtil.CreateNode(null, null, cond.Effect.GetType(), cond.Effect, "效果", 0);
        }
        RootConditionNode = null;
        RootEffectNode = null;
    }

    private void LoadFromFile()
    {
        _list = new List<ConditionEventTemplate>();
        // Debug.LogError($"[{nameof(EventManager)}] 事件模板读取失败，请检查{ConditionEventCreator.FilePath}");

        //读取目录下的所有
        var assets = Resources.LoadAll<TextAsset>(ConditionEventCreator.DirPath);
        foreach (var asset in assets)
        {
            var list = JsonConvert.DeserializeObject<List<ConditionEventTemplate>>(asset.text, new JsonLogicConverter());
            if (list != null)
            {
                foreach (var item in list)
                {
                    string fileName = asset.name;
                    if (string.IsNullOrEmpty(item.Path))
                    {
                        item.Path = fileName;
                    }
                    else if (item.Path != fileName && item.Path.IndexOf(fileName + "/") != 0)
                    {
                        item.Path = fileName + "/" + item.Path;
                    }
                }
                _list.AddRange(list);
            }
        }
    }

    private void RefreshTree()
    {
        var cet = (_tree.Selection.SelectedValue as ConditionEventTemplate);

        InitMenuTree();
        if (cet != null)
        {
            var omi = FindItem(_tree.MenuItems, cet.TemplateId);
            if (omi != null)
            {
                _tree.Selection.Add(omi);
            }
        }
    }

    private void SaveFile()
    {
        Dictionary<string, List<ConditionEventTemplate>> files = new Dictionary<string, List<ConditionEventTemplate>>();
        List<int> ids = new List<int>();
        foreach (var item in _list)
        {
            if (string.IsNullOrEmpty(item.Path))
            {
                item.Path = DefaultFileName;
            }

            string fileName = item.Path;

            if (item.Path.Contains("/"))
            {
                fileName = item.Path.Substring(0, item.Path.IndexOf("/"));
            }
            if (!files.ContainsKey(fileName))
            {
                files[fileName] = new List<ConditionEventTemplate>();
            }
            //防止因为各种原因导致ID的重复
            int id = item.TemplateId;
            while (ids.Contains(id))
            {
                id = id * 10;
            }

            if (item.TemplateId != id)
            {
                Debug.LogError($"[{nameof(EventManager)}] ID{item.TemplateId}重复，其中一个事件{item.Name}ID变更为{id}");
                item.TemplateId = id;
                item.Name += id.ToString();
            }

            files[fileName].Add(item);
            ids.Add(id);
        }

        DirectoryInfo dir = new DirectoryInfo(Application.dataPath + "/Resources/" + ConditionEventCreator.DirPath);

        //删除目录下现存的Json 文件
        if (dir.Exists)
        {
            foreach (var f in dir.GetFiles())
            {
                if (f.Name.EndsWith(".json"))
                {
                    f.Delete();
                }
            }
        }
        else
        {
            dir.Create();
        }
        foreach (var pair in files)
        {
            //这一串都是为了格式化输出json
            StringWriter textWriter = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
            {
                Formatting = Formatting.Indented,
                Indentation = 4,
                IndentChar = ' '
            };
            JsonSerializer serializer = new JsonSerializer();

            serializer.Serialize(jsonWriter, pair.Value);
            var file = new FileInfo(Application.dataPath + "/Resources/" + ConditionEventCreator.DirPath + "/" + pair.Key + ".json");

            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }
            System.IO.File.WriteAllText(file.FullName, textWriter.ToString());
            jsonWriter.Close();
            textWriter.Close();
        }
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 递归删除对象
    /// </summary>
    /// <param name="list"></param>
    /// <param name="item"></param>
    private void RemoveItem(List<OdinMenuItem> list, OdinMenuItem item)
    {
        if (list.Remove(item))
        {
            return;
        }

        for (int i = 0; i < list.Count; i++)
        {
            list[i].ChildMenuItems.Remove(item);
        }
    }

    private OdinMenuItem FindItem(List<OdinMenuItem> list, int id)
    {
        var omi = list.Find(o => (o.Value as ConditionEventTemplate)?.TemplateId == id);
        if (omi != null)
        {
            return omi;
        }

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].ChildMenuItems.Count > 0)
            {
                omi = FindItem(list[i].ChildMenuItems, id);
            }
            if (omi != null)
            {
                return omi;
            }
        }
        return null;
        ;
    }

    private void TestValue(int id, out BaseAndObj bo, out BaseSequenceObj so)
    {
        BaseAndObj ands = new BaseAndObj();

        ands.And = new List<ILogicBoolBaseObj>(){
                    BaseFuncObj.Create("MoreThan","5,3 +1"),
                    BaseFuncObj.Create("MoreThan","1,1")
        };
        BaseIfObj ifobj = new BaseIfObj();
        ifobj.If = new List<ILogicBoolBaseObj> { BaseFuncObj.Create("MoreThan", "11,2") };
        ifobj.Then = new List<ILogicFrameBaseObj>()
        {
            BaseFuncObj.Create("Log", $"\"事件模板编号<<TemplateId>>的日志1\"")
        };
        bo = ands;
        so = new BaseSequenceObj()
        {
            List = new List<ILogicFrameBaseObj>() { ifobj, BaseFuncObj.Create("Log", "\"事件模板编号<<TemplateId>>的日志2\""), BaseFuncObj.Create("Log", "\"事件模板编号<<TemplateId>>的日志的后面一个编号是<<TemplateId+1>>\"") }
        };
    }

    /// <summary>
    /// 取得一个新的Id是与当前选择的事件链上+1
    /// </summary>
    /// <returns></returns>
    private int GetNewId(string path)
    {
        int pathMaxId = 0;
        if (path == null)
        {
            path = DefaultFileName;
        }
        foreach (var item in _list)
        {
            if (item.Path != null && item.Path.IndexOf(path) == 0)
            {
                pathMaxId = Mathf.Max(item.TemplateId, pathMaxId);
            }
        }

        if (_list.Find(e => e.TemplateId == pathMaxId + 1) != null)
        {
            return ConditionEventCreator.GetMaxTemplateId() + 1;
        }
        return pathMaxId + 1;
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        InitMenuTree();
        return _tree;
    }

    private class EventMenuItem : OdinMenuItem
    {
        private readonly ConditionEventTemplate _instance;

        public EventMenuItem(OdinMenuTree tree, ConditionEventTemplate instance) : base(tree, instance.TemplateId + "-" + instance.Name, instance)
        {
            this._instance = instance;
        }

        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
        }

        public override string SmartName => _instance.TemplateId + "-" + this._instance.Name;
    }
}

#region 节点化工具

public class BaseFuncObjNode : DrawNode
{
    private BaseFuncObj _instance;
    private Dictionary<string, BaseFunc> _name2Func;
    private Dictionary<string, ParamTypeEditorGUI> _name2ParamsGuis;
    private Dictionary<Type, ParamTypeEditorGUI> _type2ParamsGuis;
    private Dictionary<Type, ValueFromLuaTable> _typeValueTables;
    private LuaEnv _luaEnv;

    private List<string> _paramsObjectList;

    public BaseFuncObjNode(DrawNode parent, Action<object> setValue, object instance, string nodeName, int index) : base(parent, setValue, instance, nodeName, index)
    {
        _instance = (BaseFuncObj)instance;
        InitFunc();
    }

    public void InitFunc()
    {
        _paramsObjectList = new List<string>();
        _name2Func = new Dictionary<string, BaseFunc>();

        _name2ParamsGuis = new Dictionary<string, ParamTypeEditorGUI>();
        _type2ParamsGuis = new Dictionary<Type, ParamTypeEditorGUI>();

        _typeValueTables = new Dictionary<Type, ValueFromLuaTable>();

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
        }

        var guis = TypeUtilities.GetChildrenEditor<ParamTypeEditorGUI>();

        foreach (var v in guis)
        {
            string name = v.GetName();
            if (!string.IsNullOrEmpty(name))
            {
                _name2ParamsGuis[name] = v;
            }
            if (v.GetTargetType() != null)
            {
                _type2ParamsGuis[v.GetTargetType()] = v;
            }
        }

        _luaEnv = new LuaEnv();

        //简易纠错 -----------------

        _paramsObjectList.Clear();
        if (_instance.Func.Length < 2)
        {
            _instance.Func = new string[] { _instance.Func[0], String.Empty };
        }
        ParamTypeEditorGUI.Split(_instance.Func[1], _paramsObjectList);

        string funcName = _instance.Func[0];
        BaseFunc func = null;
        if (funcName != "")
        {
            if (!_name2Func.TryGetValue(funcName, out func))
            {
                Debug.LogError($"[GameEventEditor] {funcName} 功能类不存在，请重新选择其他功能或者检查代码");
            }
            else
            {
                if (FillParamsStr(_paramsObjectList, func))
                {
                    _instance.Func[1] = GetParamsStr(_paramsObjectList, func);
                }
            }
        }
    }

    public override void OnGUI(Rect rect)
    {
        base.OnGUI(rect);

        //GUILayout.Space(mSpace * mDepth + 10 - 10);

        GUILayout.BeginHorizontal();
        mFoldOut = EditorGUILayout.BeginFoldoutHeaderGroup(mFoldOut, NodeName + " " + (mModify ? "*" : ""));
        if (mFoldOut)
        {
            GameEventEditorUtil.ShowCVButonGUI(ref _instance);
        }
        GUILayout.EndHorizontal();
        if (mParent != null && mFoldOut)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(mSpace);

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            string funcName = _instance.Func[0];
            string funcShowName = "";
            BaseFunc func = null;
            if (funcName != "")
            {
                if (_name2Func.TryGetValue(funcName, out func))
                {
                    funcShowName = func.GetItemName();
                }
                else
                {
                    Debug.LogError($"[GameEventEditor] {funcName} 功能类不存在，请重新选择其他功能或者检查代码");
                }
            }

            //函数按钮
            if (GUILayout.Button(funcShowName + " " + func?.GetType().Name))
            {
                var items = TypeUtilities.GetChildren<BaseFunc>();
                var itemsNames = new List<string>();

                for (int i = 0; i < items.Count; i++)
                {
                    itemsNames.Add(items[i].GetItemName() + " " + items[i].GetType().Name);
                }

                ItemSelectEditor.ShowItemSelectEditor(itemsNames, (index) =>
                {
                    _instance.Func[0] = items[index].GetType().Name;
                    func = items[index];

                    _instance.Func[1] = GetDefaultParamsStr(func);
                });
            }

            GUILayout.EndHorizontal();

            if (func != null)
            {
                _paramsObjectList.Clear();
                if (_instance.Func[1] == null)
                {
                    _instance.Func[1] = GetDefaultParamsStr(func);
                }
                ParamTypeEditorGUI.Split(_instance.Func[1], _paramsObjectList);

                GUILayout.BeginHorizontal();
                GUILayout.Space(mSpace);

                GUILayout.BeginVertical();

                for (int i = 0; i < _paramsObjectList.Count; i++)
                {
                    var info = func.Params[i % func.Params.Count];
                    ParamTypeEditorGUI gui = null;
                    string token = "";
                    if (_name2ParamsGuis.TryGetValue(info.Name, out gui))
                    {
                        token = _paramsObjectList[i];
                    }
                    else if (_type2ParamsGuis.TryGetValue(info.ParamType, out gui))
                    {
                        token = _paramsObjectList[i];
                    }

                    //传入修改委托
                    int index = i;
                    gui.OnGUI(func.Params[i % func.Params.Count].Name, token, (modity) =>
                     {
                         //当值被修改
                         if (!token.Equals(modity))
                         {
                             _paramsObjectList[index] = modity;

                             _instance.Func[1] = GetParamsStr(_paramsObjectList, func);
                         }
                     });
                }

                if (func.CanAdd)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    if (GUILayout.Button("+", GUILayout.MaxWidth(100f)))
                    {
                        _instance.Func[1] += ("," + GetDefaultParamsStr(func, func.Params.Count - func.CanAddParamsCount));
                    }
                    if (_paramsObjectList.Count > func.Params.Count && GUILayout.Button("-", GUILayout.MaxWidth(100f)))
                    {
                        _paramsObjectList.RemoveRange(_paramsObjectList.Count - func.Params.Count, func.CanAddParamsCount);

                        _instance.Func[1] = GetParamsStr(_paramsObjectList, func);
                    }
                    else
                    {
                        EditorGUILayout.Space();
                    }
                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
        EditorGUI.EndFoldoutHeaderGroup();
    }

    /// <summary>
    /// 得到初始默认参数
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    private string GetDefaultParamsStr(BaseFunc func, int start = 0)
    {
        StringBuilder sb = new StringBuilder();

        for (int i = start; i < func.Params.Count; i++)
        {
            var info = func.Params[i % func.Params.Count];
            if (_name2ParamsGuis.TryGetValue(info.Name, out ParamTypeEditorGUI gui))
            {
                var str = _typeValueTables[info.ParamType].WriteValue(info.DefualtValue);
                sb.Append(str);
            }
            else if (_type2ParamsGuis.TryGetValue(info.ParamType, out gui))
            {
                var str = _typeValueTables[info.ParamType].WriteValue(info.DefualtValue);
                sb.Append(str);
            }

            if (i < func.Params.Count - 1)
            {
                sb.Append(",");
            }
        }

        _instance.Func[1] = sb.ToString();
        return sb.ToString();
    }

    /// <summary>
    /// 得到初始默认参数
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    private bool FillParamsStr(List<string> funcStr, BaseFunc func)
    {
        bool fill = false;
        if (!func.CanAdd)
        {
            //填补不足
            for (int i = funcStr.Count; i < func.Params.Count; i++)
            {
                var info = func.Params[i % func.Params.Count];
                if (_name2ParamsGuis.TryGetValue(info.Name, out ParamTypeEditorGUI gui))
                {
                    var str = _typeValueTables[info.ParamType].WriteValue(info.DefualtValue);
                    funcStr.Add(str);
                    fill = true;

                    Debug.LogError($"[{nameof(GameEventEditor)}] {func.GetItemName()}函数配置参数不足，填充默认值{str}");
                }
                else if (_type2ParamsGuis.TryGetValue(info.ParamType, out gui))
                {
                    var str = _typeValueTables[info.ParamType].WriteValue(info.DefualtValue);
                    funcStr.Add(str);
                    fill = true;
                    Debug.LogError($"[{nameof(GameEventEditor)}] {func.GetItemName()}函数配置参数不足，填充默认值{str}");
                }
            }
        }
        return fill;
    }

    private string GetParamsStr(List<string> paramsObjectList, BaseFunc func)
    {
        StringBuilder sb = new StringBuilder();
        for (int j = 0; j < _paramsObjectList.Count; j++)
        {
            var funcInfo = func.Params[j % func.Params.Count];
            var str = _typeValueTables[funcInfo.ParamType].WriteValue(_paramsObjectList[j]);

            sb.Append(str);
            if (j < paramsObjectList.Count - 1)
            {
                sb.Append(",");
            }
        }
        return sb.ToString();
    }

    public override void Update()
    {
    }
}

public class BaseSelectFuncListObjNode : DrawNode
{
    public List<DrawNode> Children = new List<DrawNode>();

    public SelectFuncObj _instance;

    private LogicReorderableList<SelectFuncObj.Selection> _reorderableList = new LogicReorderableList<SelectFuncObj.Selection>();

    // private bool _listMode = false;

    public BaseSelectFuncListObjNode(DrawNode parent, Action<object> setValue, object instance, string nodeName, int index) : base(parent, setValue, instance, nodeName, index)
    {
        _instance = (SelectFuncObj)instance;
    }

    public void AddChildren(DrawNode node)
    {
        Children.Add(node);
    }

    public override void OnGUI(Rect rect)
    {
        base.OnGUI(rect);
        GUILayout.BeginHorizontal();
        //
        mFoldOut = EditorGUILayout.Foldout(mFoldOut, "选择", toggleOnLabelClick: true);

        if (mParent != null && mFoldOut)
        {
            //只有类对象的字段可以是NULL
            if (mParent is ClassNode)
            {
                if (GUILayout.Button("=null", GUILayout.Width(BUTTON_WIDTH)))
                {
                    mSetValue(null);
                    mParent.Update();
                    //这里节点会变更，不能直接标记自己
                    mParent.SetModifyChild(mNodeName);
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox(NodeName + " " + (mModify ? "*" : ""), MessageType.None);
        }
        if (mFoldOut)
        {
            GameEventEditorUtil.ShowAButonGUI(_instance.Selections, Update);
            GameEventEditorUtil.ShowCVButonGUI(ref _instance);
            _reorderableList.DrawSwitchButton();
        }

        GUILayout.EndHorizontal();

        if (mFoldOut)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(mSpace);
            GUILayout.BeginVertical();

            if (Children.Count == 0 || _reorderableList.Enable)
            {
                _reorderableList.OnGUI();
            }
            else
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i].OnGUI(Rect.zero);
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }

    public override void Update()
    {
        Children.Clear();
        foreach (var item in _instance.Selections)
        {
            AddChildren(DrawNodeUtil.CreateNode(this, null, item.GetType(), item, "[" + item.SelectionText
                + "]\n"
                + GameEventEditor.ConditionEventCreator.TranslateDescription(item?.GetDescription()), mDepth + 1));
        }
        _reorderableList.Init(_instance.Selections, Update);
    }

    public override void SetModifyChild(string name)
    {
        foreach (var child in Children)
        {
            if (child.NodeName == name)
            {
                child.SetModify();
            }
        }
    }
}

public class BaseSelectionObjNode : DrawNode
{
    private SelectFuncObj.Selection _selection;

    public DrawNode ChildCondition;
    public DrawNode ChildEffect;

    public static string[] _disableShowTypeStrs = new string[] {
        "无效不可点 (CantClick)","无效隐藏选项 (Hide)","无效不可点且隐藏条件(HideCondition)"
    };

    // private LogicReorderableList<ILogicBoolBaseObj> _reorderableList = new LogicReorderableList<ILogicBoolBaseObj>();

    public BaseSelectionObjNode(DrawNode parent, Action<object> setValue, object instance, string nodeName, int index) : base(parent, setValue, instance, nodeName, index)
    {
        _selection = (SelectFuncObj.Selection)instance;
    }

    public override void OnGUI(Rect rect)
    {
        base.OnGUI(rect);
        GUILayout.BeginHorizontal();

        mFoldOut = EditorGUILayout.Foldout(mFoldOut, "选项", toggleOnLabelClick:true);
        if (mFoldOut)
        {
            //        _reorderableList.DrawSwitchButton();
        }
        else
        {
            EditorGUILayout.HelpBox(NodeName + " " + (mModify ? "*" : ""), MessageType.None);
        }
        GUILayout.EndHorizontal();

        if (mFoldOut)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(mSpace);
            GUILayout.BeginVertical();
            //

            _selection.SelectionText = EditorGUILayout.TextArea(_selection.SelectionText);

            GUILayout.BeginHorizontal();
            _selection.ShowCondition = EditorGUILayout.Toggle("显示条件", _selection.ShowCondition, GUILayout.MaxWidth(200));
            // GUILayout.Label("选项无效时");
            _selection.DisableShow = (SelectionDisableType)GUILayout.SelectionGrid((int)_selection.DisableShow, _disableShowTypeStrs, 3);
            EditorGUILayout.Space();
            GUILayout.EndHorizontal();
            ChildCondition.OnGUI(rect);

            EditorGUILayout.LabelField("效果");
            ChildEffect.OnGUI(rect);

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }

    public override void Update()
    {
        ChildCondition = DrawNodeUtil.CreateNode(this, (o) => { _selection.Condition = (BaseAndObj)o; }, typeof(BaseAndObj), _selection.Condition,
           GameEventEditor.ConditionEventCreator.TranslateDescription(_selection.Condition?.GetDescription()), mDepth + 1);
        ChildEffect = DrawNodeUtil.CreateNode(this, (o) => { _selection.Effect = (BaseSequenceObj)o; }, typeof(BaseSequenceObj), _selection.Effect,
          GameEventEditor.ConditionEventCreator.TranslateDescription(_selection.Effect?.GetDescription()), mDepth + 1);
    }

    public override void SetModifyChild(string name)
    {
        //  foreach (var child in Children)
        {
            if (ChildCondition.NodeName == name)
            {
                ChildCondition.SetModify();
            }
            if (ChildEffect.NodeName == name)
            {
                ChildEffect.SetModify();
            }
        }
    }
}

public class BaseAndObjNode : DrawNode
{
    private BaseAndObj _instance;

    public List<DrawNode> Children = new List<DrawNode>();
    private LogicReorderableList<ILogicBoolBaseObj> _reorderableList = new LogicReorderableList<ILogicBoolBaseObj>();

    public BaseAndObjNode(DrawNode parent, Action<object> setValue, object instance, string nodeName, int index) : base(parent, setValue, instance, nodeName, index)
    {
        _instance = (BaseAndObj)instance;
    }

    public void AddChildren(DrawNode node)
    {
        Children.Add(node);
    }

    public override void OnGUI(Rect rect)
    {
        base.OnGUI(rect);
        GUILayout.BeginHorizontal();

        mFoldOut = EditorGUILayout.Foldout(mFoldOut, "与", toggleOnLabelClick: true);
        if (mFoldOut)
        {
            GameEventEditorUtil.ShowAButonGUI(_instance.And, Update);
            GameEventEditorUtil.ShowCVButonGUI(ref _instance);
            _reorderableList.DrawSwitchButton();
        }
        else
        {
            EditorGUILayout.HelpBox(NodeName + " " + (mModify ? "*" : ""), MessageType.None);
        }
        GUILayout.EndHorizontal();

        if (mFoldOut)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(mSpace);
            GUILayout.BeginVertical();
            if (Children.Count == 0 || _reorderableList.Enable)
            {
                _reorderableList.OnGUI();
            }
            else
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i].OnGUI(rect);
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }

    public override void Update()
    {
        Children.Clear();
        foreach (var item in _instance.And)
        {
            AddChildren(DrawNodeUtil.CreateNode(this, null, item.GetType(), item, GameEventEditor.ConditionEventCreator.TranslateDescription(item.GetDescription()), mDepth + 1));
        }
        _reorderableList.Init(_instance.And, Update);
    }

    public override void SetModifyChild(string name)
    {
        foreach (var child in Children)
        {
            if (child.NodeName == name)
            {
                child.SetModify();
            }
        }
    }
}

public class BaseOrObjNode : DrawNode
{
    private BaseOrObj _instance;

    public List<DrawNode> Children = new List<DrawNode>();
    private LogicReorderableList<ILogicBoolBaseObj> _reorderableList = new LogicReorderableList<ILogicBoolBaseObj>();

    public BaseOrObjNode(DrawNode parent, Action<object> setValue, object instance, string nodeName, int index) : base(parent, setValue, instance, nodeName, index)
    {
        _instance = (BaseOrObj)instance;
    }

    public void AddChildren(DrawNode node)
    {
        Children.Add(node);
    }

    public override void OnGUI(Rect rect)
    {
        base.OnGUI(rect);
        GUILayout.BeginHorizontal();

        mFoldOut = EditorGUILayout.Foldout(mFoldOut, "或", toggleOnLabelClick: true);
        if (mFoldOut)
        {
            GameEventEditorUtil.ShowAButonGUI(_instance.Or, Update);
            GameEventEditorUtil.ShowCVButonGUI(ref _instance);
            _reorderableList.DrawSwitchButton();
        }
        else
        {
            EditorGUILayout.HelpBox(NodeName + " " + (mModify ? "*" : ""), MessageType.None);
        }
        GUILayout.EndHorizontal();

        if (mFoldOut)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(mSpace);
            GUILayout.BeginVertical();
            if (Children.Count == 0 || _reorderableList.Enable)
            {
                _reorderableList.OnGUI();
            }
            else
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i].OnGUI(rect);
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }

    public override void Update()
    {
        Children.Clear();
        foreach (var item in _instance.Or)
        {
            AddChildren(DrawNodeUtil.CreateNode(this, null, item.GetType(), item, GameEventEditor.ConditionEventCreator.TranslateDescription(item.GetDescription()), mDepth + 1));
        }
        _reorderableList.Init(_instance.Or, Update);
    }

    public override void SetModifyChild(string name)
    {
        foreach (var child in Children)
        {
            if (child.NodeName == name)
            {
                child.SetModify();
            }
        }
    }
}

public class BaseNotObjNode : DrawNode
{
    private BaseNotObj _baseNotObj;

    public DrawNode Child;

    public BaseNotObjNode(DrawNode parent, Action<object> setValue, object instance, string nodeName, int index) : base(parent, setValue, instance, nodeName, index)
    {
        _baseNotObj = (BaseNotObj)instance;
    }

    public override void OnGUI(Rect rect)
    {
        base.OnGUI(rect);
        GUILayout.BeginHorizontal();

        mFoldOut = EditorGUILayout.Foldout(mFoldOut, "非" ,toggleOnLabelClick: true);

        {
            EditorGUILayout.HelpBox(NodeName + " " + (mModify ? "*" : ""), MessageType.None);
        }
        GUILayout.EndHorizontal();

        if (mFoldOut)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(mSpace);
            GUILayout.BeginVertical();

            Child.OnGUI(rect);

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }

    public override void Update()
    {
        Child = DrawNodeUtil.CreateNode(this, (o) => { _baseNotObj.Not = (ILogicBoolBaseObj)o; }, typeof(ILogicBoolBaseObj), _baseNotObj.Not,
            "非" + GameEventEditor.ConditionEventCreator.TranslateDescription(_baseNotObj.Not?.GetDescription()), mDepth + 1);
    }

    public override void SetModifyChild(string name)
    {
        //foreach (var child in Children)
        {
            if (Child.NodeName == name)
            {
                Child.SetModify();
            }
        }
    }
}

public class BaseSequenceObjNode : DrawNode
{
    public List<DrawNode> Children = new List<DrawNode>();

    public BaseSequenceObj _instance;

    private LogicReorderableList<ILogicFrameBaseObj> _reorderableList = new LogicReorderableList<ILogicFrameBaseObj>();

    // private bool _listMode = false;

    public BaseSequenceObjNode(DrawNode parent, Action<object> setValue, object instance, string nodeName, int index) : base(parent, setValue, instance, nodeName, index)
    {
        _instance = (BaseSequenceObj)instance;
    }

    public void AddChildren(DrawNode node)
    {
        Children.Add(node);
    }

    public override void OnGUI(Rect rect)
    {
        base.OnGUI(rect);
        GUILayout.BeginHorizontal();
        //
        mFoldOut = EditorGUILayout.Foldout(mFoldOut, "顺序", toggleOnLabelClick: true);

        if (mParent != null && mFoldOut)
        {
            //只有类对象的字段可以是NULL
            if (mParent is ClassNode)
            {
                if (GUILayout.Button("=null", GUILayout.Width(BUTTON_WIDTH)))
                {
                    mSetValue(null);
                    mParent.Update();
                    //这里节点会变更，不能直接标记自己
                    mParent.SetModifyChild(mNodeName);
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox(NodeName + " " + (mModify ? "*" : ""), MessageType.None);
        }
        if (mFoldOut)
        {
            GameEventEditorUtil.ShowAButonGUI(_instance.List, Update);
            GameEventEditorUtil.ShowCVButonGUI(ref _instance);
            _reorderableList.DrawSwitchButton();
        }

        GUILayout.EndHorizontal();

        if (mFoldOut)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(mSpace);
            GUILayout.BeginVertical();

            if (Children.Count == 0 || _reorderableList.Enable)
            {
                _reorderableList.OnGUI();
            }
            else
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i].OnGUI(Rect.zero);
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }

    public override void Update()
    {
        Children.Clear();
        foreach (var item in _instance.List)
        {
            AddChildren(DrawNodeUtil.CreateNode(this, null, item.GetType(), item, GameEventEditor.ConditionEventCreator.TranslateDescription(item.GetDescription()), mDepth + 1));
        }
        _reorderableList.Init(_instance.List, Update);
    }

    public override void SetModifyChild(string name)
    {
        foreach (var child in Children)
        {
            if (child.NodeName == name)
            {
                child.SetModify();
            }
        }
    }
}

public class BaseRandomObjNode : DrawNode
{
    public List<DrawNode> Children = new List<DrawNode>();

    public BaseRandomObj _baseRandomObj;

    // private bool _listMode = false;

    public BaseRandomObjNode(DrawNode parent, Action<object> setValue, object instance, string nodeName, int index) : base(parent, setValue, instance, nodeName, index)
    {
        _baseRandomObj = (BaseRandomObj)instance;
    }

    public void AddChildren(DrawNode node)
    {
        Children.Add(node);
    }

    public override void OnGUI(Rect rect)
    {
        base.OnGUI(rect);
        GUILayout.BeginHorizontal();
        //
        mFoldOut = EditorGUILayout.Foldout(mFoldOut, "随机", toggleOnLabelClick: true);
        if (mFoldOut)
        {
            //增加按钮
            if (GUILayout.Button("+ 增加随机项", GUILayout.MaxWidth(100f)))
            {
                _baseRandomObj.RandomList.Add(new BaseSequenceObj());
                _baseRandomObj.RandomWeight.Add("1");
                Update();
            }
            EditorGUILayout.Space();
        }
        if (mParent != null && mFoldOut)
        {
            //只有类对象的字段可以是NULL
            if (mParent is ClassNode)
            {
                if (GUILayout.Button("=null", GUILayout.Width(BUTTON_WIDTH)))
                {
                    mSetValue(null);
                    mParent.Update();
                    //这里节点会变更，不能直接标记自己
                    mParent.SetModifyChild(mNodeName);
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox(NodeName + " " + (mModify ? "*" : ""), MessageType.None);
        }

        GUILayout.EndHorizontal();

        if (mFoldOut)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(mSpace);
            GUILayout.BeginVertical();
            for (int i = 0; i < _baseRandomObj.RandomList.Count; i++)
            {
                GUILayout.BeginHorizontal();

                //保证长度一致
                while (_baseRandomObj.RandomWeight.Count <= i)
                {
                    _baseRandomObj.RandomWeight.Add("1");
                }
                for (int j = _baseRandomObj.RandomWeight.Count - 1; j >= _baseRandomObj.RandomList.Count; j--)
                {
                    _baseRandomObj.RandomWeight.RemoveAt(j);
                }

                _baseRandomObj.RandomWeight[i] = EditorGUILayout.TextField("权重", _baseRandomObj.RandomWeight[i]);

                if (GUILayout.Button("- 删除项", GUILayout.MaxWidth(100f)))
                {
                    _baseRandomObj.RandomList.RemoveAt(i);
                    _baseRandomObj.RandomWeight.RemoveAt(i);
                    Update();
                }
                GUILayout.EndHorizontal();
                Children[i].OnGUI(Rect.zero);
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }

    public override void Update()
    {
        Children.Clear();
        foreach (var item in _baseRandomObj.RandomList)
        {
            AddChildren(DrawNodeUtil.CreateNode(this, null, item.GetType(), item, GameEventEditor.ConditionEventCreator.TranslateDescription(item.GetDescription()), mDepth + 1));
        }
        //_reorderableList.Init(_baseRandomObj.RandomList, Update);
    }

    public override void SetModifyChild(string name)
    {
        foreach (var child in Children)
        {
            if (child.NodeName == name)
            {
                child.SetModify();
            }
        }
    }
}

public class BaseIfObjNode : DrawNode
{
    public List<DrawNode> ThenChildren = new List<DrawNode>();
    public List<DrawNode> ElseChildren = new List<DrawNode>();
    public List<DrawNode> IfChildren = new List<DrawNode>();

    public BaseIfObj _instance;

    private LogicReorderableList<ILogicBoolBaseObj> _ifReorderableList = new LogicReorderableList<ILogicBoolBaseObj>();
    private LogicReorderableList<ILogicFrameBaseObj> _thanReorderableList = new LogicReorderableList<ILogicFrameBaseObj>();

    private LogicReorderableList<ILogicFrameBaseObj> _elseReorderableList = new LogicReorderableList<ILogicFrameBaseObj>();

    public BaseIfObjNode(DrawNode parent, Action<object> setValue, object instance, string nodeName, int index) : base(parent, setValue, instance, nodeName, index)
    {
        _instance = (BaseIfObj)instance;
    }

    public void AddChildren(DrawNode node)
    {
        ThenChildren.Add(node);
    }

    public override void OnGUI(Rect rect)
    {
        base.OnGUI(rect);

        GUILayout.BeginHorizontal();
        mFoldOut = EditorGUILayout.Foldout(mFoldOut, "条件", toggleOnLabelClick: true);
        EditorGUILayout.HelpBox((!mFoldOut ? GameEventEditor.ConditionEventCreator.TranslateDescription(_instance.GetDescription()) : "") + (mModify ? "*" : ""), MessageType.None);
        if (mFoldOut)
        {
            GameEventEditorUtil.ShowCVButonGUI(ref _instance);
        }

        GUILayout.EndHorizontal();

        if (mFoldOut)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(mSpace);
            GUILayout.BeginVertical();

            //If
            GUILayout.BeginHorizontal();
            GUILayout.Label("当");
            GameEventEditorUtil.ShowAButonGUI(_instance.If, Update);
            _ifReorderableList.DrawSwitchButton();
            GUILayout.EndHorizontal();

            if (IfChildren.Count == 0 || _ifReorderableList.Enable)
            {
                _ifReorderableList.OnGUI();
            }
            else
            {
                for (int i = 0; i < IfChildren.Count; i++)
                {
                    IfChildren[i].OnGUI(rect);
                }
            }

            //Then
            GUILayout.BeginHorizontal();
            GUILayout.Label("执行 顺序");
            GameEventEditorUtil.ShowAButonGUI(_instance.Then, Update);
            _thanReorderableList.DrawSwitchButton();
            GUILayout.EndHorizontal();

            if (ThenChildren.Count == 0 || _thanReorderableList.Enable)
            {
                _thanReorderableList.OnGUI();
            }
            else
            {
                for (int i = 0; i < ThenChildren.Count; i++)
                {
                    ThenChildren[i].OnGUI(rect);
                }
            }

            //Else
            GUILayout.BeginHorizontal();
            GUILayout.Label("否则 顺序");
            GameEventEditorUtil.ShowAButonGUI(_instance.Else, Update);
            _elseReorderableList.DrawSwitchButton();
            GUILayout.EndHorizontal();

            if (ElseChildren.Count == 0 || _elseReorderableList.Enable)
            {
                _elseReorderableList.OnGUI();
            }
            else
            {
                for (int i = 0; i < ElseChildren.Count; i++)
                {
                    ElseChildren[i].OnGUI(rect);
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }

    public override void Update()
    {
        IfChildren.Clear();
        foreach (var item in _instance.If)
        {
            IfChildren.Add(DrawNodeUtil.CreateNode(this, null, item.GetType(), item, GameEventEditor.ConditionEventCreator.TranslateDescription(item.GetDescription()), mDepth + 1));
        }
        _ifReorderableList.Init(_instance.If, Update);

        ThenChildren.Clear();
        foreach (var item in _instance.Then)
        {
            ThenChildren.Add(DrawNodeUtil.CreateNode(this, null, item.GetType(), item, GameEventEditor.ConditionEventCreator.TranslateDescription(item.GetDescription()), mDepth + 1));
        }
        _thanReorderableList.Init(_instance.Then, Update);

        ElseChildren.Clear();
        foreach (var item in _instance.Else)
        {
            ElseChildren.Add(DrawNodeUtil.CreateNode(this, null, item.GetType(), item, GameEventEditor.ConditionEventCreator.TranslateDescription(item.GetDescription()), mDepth + 1));
        }
        _elseReorderableList.Init(_instance.Else, Update);
    }

    public override void SetModifyChild(string name)
    {
        foreach (var child in ThenChildren)
        {
            if (child.NodeName == name)
            {
                child.SetModify();
            }
        }
    }
}

public class BaseWhileObjNode : DrawNode
{
    public List<DrawNode> ThenChildren = new List<DrawNode>();
    public DrawNode WhileChildren;

    public BaseWhileObj _instance;

    private LogicReorderableList<ILogicFrameBaseObj> _reorderableList = new LogicReorderableList<ILogicFrameBaseObj>();

    public BaseWhileObjNode(DrawNode parent, Action<object> setValue, object instance, string nodeName, int index) : base(parent, setValue, instance, nodeName, index)
    {
        _instance = (BaseWhileObj)instance;
    }

    public void AddChildren(DrawNode node)
    {
        ThenChildren.Add(node);
    }

    public override void OnGUI(Rect rect)
    {
        base.OnGUI(rect);

        GUILayout.BeginHorizontal();
        mFoldOut = EditorGUILayout.Foldout(mFoldOut, "循环 " + "  " + (!mFoldOut ? GameEventEditor.ConditionEventCreator.TranslateDescription(_instance.GetDescription()) : "") + (mModify ? "*" : ""), toggleOnLabelClick: true);

        if (mFoldOut)
        {
            GameEventEditorUtil.ShowCVButonGUI(ref _instance);
        }
        GUILayout.EndHorizontal();

        if (mFoldOut)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(mSpace);
            GUILayout.BeginVertical();
            WhileChildren.OnGUI(rect);

            GUILayout.BeginHorizontal();
            GUILayout.Label("执行 顺序");
            GameEventEditorUtil.ShowAButonGUI(_instance.Then, Update);
            _reorderableList.DrawSwitchButton();
            GUILayout.EndHorizontal();

            if (ThenChildren.Count == 0 || _reorderableList.Enable)
            {
                _reorderableList.OnGUI();
            }
            else
            {
                for (int i = 0; i < ThenChildren.Count; i++)
                {
                    ThenChildren[i].OnGUI(rect);
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }

    public override void Update()
    {
        WhileChildren = DrawNodeUtil.CreateNode(this, (o) => { _instance.While = (ILogicBoolBaseObj)o; }, typeof(ILogicBoolBaseObj), _instance.While,
           "当" + GameEventEditor.ConditionEventCreator.TranslateDescription(_instance.While?.GetDescription()), mDepth + 1);
        ThenChildren.Clear();
        foreach (var item in _instance.Then)
        {
            AddChildren(DrawNodeUtil.CreateNode(this, null, item.GetType(), item, GameEventEditor.ConditionEventCreator.TranslateDescription(item.GetDescription()), mDepth + 1));
        }
        _reorderableList.Init(_instance.Then, Update);
    }

    public override void SetModifyChild(string name)
    {
        foreach (var child in ThenChildren)
        {
            if (child.NodeName == name)
            {
                child.SetModify();
            }
        }
    }
}

public class ClassNode : DrawNode
{
    public List<DrawNode> Children = new List<DrawNode>();

    public ClassNode(DrawNode parent, Action<object> setValue, object instance, string nodeName, int index) : base(parent, setValue, instance, nodeName, index)
    {
    }

    public void AddChildren(DrawNode node)
    {
        Children.Add(node);
        Children.Sort((n1, n2) => { return string.Compare(n1.NodeName, n2.NodeName); });
    }

    public override void OnGUI(Rect rect)
    {
        base.OnGUI(rect);
        GUILayout.BeginHorizontal();
        GUILayout.Space(mSpace * mDepth + 10 - 10);
        mFoldOut = EditorGUILayout.Foldout(mFoldOut, NodeName + " " + (mModify ? "*" : ""), toggleOnLabelClick: true);
        if (mParent != null && mFoldOut)
        {
            if (GUILayout.Button("清空", GUILayout.Width(BUTTON_WIDTH)))
            {
                mSetValue(DrawNodeUtil.New(mInstance.GetType()));
                mParent.Update();
                //这里节点会变更，不能直接标记自己
                mParent.SetModifyChild(mNodeName);
            }
            //只有类对象的字段可以是NULL
            if (mParent is ClassNode)
            {
                if (GUILayout.Button("=null", GUILayout.Width(BUTTON_WIDTH)))
                {
                    mSetValue(null);
                    mParent.Update();
                    //这里节点会变更，不能直接标记自己
                    mParent.SetModifyChild(mNodeName);
                }
            }
        }

        GUILayout.EndHorizontal();

        if (mFoldOut)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].OnGUI(rect);
            }
        }
    }

    public override void Update()
    {
        var ps = mInstance.GetType().GetProperties();
        Children.Clear();
        foreach (var item in ps)
        {
            var pInstace = item.GetValue(mInstance, null);
            AddChildren(DrawNodeUtil.CreateNode(this, (o) => { item.SetValue(mInstance, o, null); }, item.PropertyType, pInstace, item.Name, mDepth + 1));
        }
    }

    public override void SetModifyChild(string name)
    {
        foreach (var child in Children)
        {
            if (child.NodeName == name)
            {
                child.SetModify();
            }
        }
    }
}

internal class CollectionNode : DrawNode
{
    public List<DrawNode> Children = new List<DrawNode>();

    public CollectionNode(DrawNode parent, Action<object> setValue, object instance, string nodeName, int index) : base(parent, setValue, instance, nodeName, index)
    {
    }

    public void AddChildren(DrawNode node)
    {
        Children.Add(node);
        Children.Sort((n1, n2) => { return string.Compare(n1.NodeName, n2.NodeName); });
    }

    public override void OnGUI(Rect rect)
    {
        base.OnGUI(rect);

        GUILayout.BeginHorizontal();
        GUILayout.Space(mSpace * mDepth + 10 - 10);
        var list = ((IList)mInstance);
        int count = list.Count;
        mFoldOut = EditorGUILayout.Foldout(mFoldOut, string.Format("{0}[{1}] {2}", mNodeName, count, mModify ? "*" : ""), toggleOnLabelClick: true);
        GUILayout.EndHorizontal();
        if (mFoldOut)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(mSpace * mDepth + LABEL_WIDTH + 10 - 10);
            //增加一位
            if (GUILayout.Button("+", GUILayout.Width(BUTTON_WIDTH)))
            {
                Add(true);
                Update();
                SetModify();
            }
            if (list.Count > 0)
            {
                //删除一位
                if (GUILayout.Button("-", GUILayout.Width(BUTTON_WIDTH)))
                {
                    Remove(true);
                    Update();
                    SetModify();
                }

                //删除一位
                if (GUILayout.Button("清空", GUILayout.Width(BUTTON_WIDTH)))
                {
                    Clear();
                    Update();
                    SetModify();
                }
            }

            GUILayout.EndHorizontal();

            //子对象
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].OnGUI(rect);
            }

            //末尾操作
            if (list.Count > 1)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(mSpace * mDepth + LABEL_WIDTH + 10 - 10);
                //增加一位
                if (GUILayout.Button("+", GUILayout.Width(BUTTON_WIDTH)))
                {
                    Add();
                    Update();
                    SetModify();
                }

                //删除一位
                if (GUILayout.Button("-", GUILayout.Width(BUTTON_WIDTH)))
                {
                    Remove();
                    Update();
                    SetModify();
                }
                GUILayout.EndHorizontal();
            }
        }
    }

    private void Add(bool first = false)
    {
        Type type = GetChildType();
        if (first)
        {
            ((IList)mInstance).Insert(0, DrawNodeUtil.New(type));
        }
        else
        {
            ((IList)mInstance).Add(DrawNodeUtil.New(type));
        }
    }

    private void Remove(bool first = false)
    {
        if (((IList)mInstance).Count > 0)
        {
            if (first)
            {
                ((IList)mInstance).RemoveAt(0);
            }
            else
            {
                ((IList)mInstance).RemoveAt(((IList)mInstance).Count - 1);
            }
        }
    }

    private void Clear()
    {
        ((IList)mInstance).Clear();
    }

    private Type GetChildType()
    {
        //举个例子 名字有这么长
        //"System.Collections.Generic.List`1[[AllData.AIState, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]"
        string prefix = "System.Collections.Generic.List`1[[";
        string end = "]]";
        var typeName = mInstance.GetType().FullName;

        var childTypeName = typeName.Substring(prefix.Length);
        childTypeName = childTypeName.Substring(0, childTypeName.Length - end.Length);
        return Type.GetType(childTypeName);
    }

    public override void Update()
    {
        IList iList = mInstance as IList;
        //List
        if (iList != null)
        {
            int count = iList.Count;

            //这里是计算需要保留多少位数，这样便于排序
            string bit = "";
            var bitCount = Mathf.CeilToInt(Mathf.Log10(count));
            for (int i = 0; i < bitCount; i++)
            {
                bit += "0";
            }

            Children.Clear();
            for (int i = 0; i < iList.Count; i++)
            {
                int index = i;

                AddChildren(DrawNodeUtil.CreateNode(this, (o) => { iList[index] = o; }, iList[i].GetType(), iList[i], "[" + i.ToString(bit) + "]", mDepth + 1));
            }
        }
    }

    public override void SetModifyChild(string name)
    {
        foreach (var child in Children)
        {
            if (child.NodeName == name)
            {
                child.SetModify();
            }
        }
    }
}

internal class ValueNode : DrawNode
{
    public ValueNode(DrawNode parent, Action<object> setValue, object instance, string nodeName, int index) : base(parent, setValue, instance, nodeName, index)
    {
    }

    public override void OnGUI(Rect rect)
    {
        base.OnGUI(rect);
        ShowBase(mSetValue, NodeName);
    }
}

internal class NullNode : DrawNode
{
    private Type mType;

    public NullNode(DrawNode parent, Action<object> setValue, Type type, string nodeName, int depth) : base(parent, setValue, null, nodeName, depth)
    {
        mType = type;
    }

    public override void OnGUI(Rect rect)
    {
        base.OnGUI(rect);
        GUILayout.BeginHorizontal();
        GUILayout.Space(mSpace);
        GUILayout.Label(NodeName, GUILayout.Width(LABEL_WIDTH));
        GUILayout.Label("null");
        //  if (FoldOut)
        {
            if (GUILayout.Button("New", GUILayout.Width(BUTTON_WIDTH)))
            {
                DrawNodeUtil.AddInstance(mType, (instance) =>
                {
                    //instance = DrawNodeUtil.New(mType);
                    mSetValue(instance);
                    SetModify();
                    mParent.Update();
                });
            }
        }
        GUILayout.EndHorizontal();
    }
}

public static class DrawNodeUtil
{
    public static DrawNode CreateNode(DrawNode parent, Action<object> setValue, Type type, object instance, string name, int depth)
    {
        if (depth > 20)
        {
            return null;
        }
        if (instance == null)
        {
            //mWindow.mString += (AddSpace(depth) + name + "：NULL" +"\n");
            return new NullNode(parent, setValue, type, name, depth);
        }

        if (type.IsValueType || type == typeof(string))
        {
            // mWindow.mString += AddSpace(depth) + name + "：" + instance.ToString() + "\n";
            return new ValueNode(parent, setValue, instance, name, depth);
        }
        IList iList = instance as IList;
        //List
        if (iList != null)
        {              //mWindow.mString +=string.Format("{0}{1}[{2}]：\n", AddSpace(depth) ,name ,count);
            var listNode = new CollectionNode(parent, setValue, instance, name, depth);
            listNode.Update();

            return listNode;
        }
        if (instance is BaseSequenceObj)
        {
            // mWindow.mString += AddSpace(depth) + name + "：" + instance.ToString() + "\n";
            var node = new BaseSequenceObjNode(parent, setValue, instance, name, depth);
            node.Update();
            return node;
        }

        if (instance is BaseIfObj)
        {
            // mWindow.mString += AddSpace(depth) + name + "：" + instance.ToString() + "\n";
            var node = new BaseIfObjNode(parent, setValue, instance, name, depth);
            node.Update();
            return node;
        }
        if (instance is BaseWhileObj)
        {
            // mWindow.mString += AddSpace(depth) + name + "：" + instance.ToString() + "\n";
            var node = new BaseWhileObjNode(parent, setValue, instance, name, depth);
            node.Update();
            return node;
        }
        if (instance is BaseAndObj)
        {
            // mWindow.mString += AddSpace(depth) + name + "：" + instance.ToString() + "\n";
            var node = new BaseAndObjNode(parent, setValue, instance, name, depth);
            node.Update();
            return node;
        }
        if (instance is BaseNotObj)
        {
            // mWindow.mString += AddSpace(depth) + name + "：" + instance.ToString() + "\n";
            var node = new BaseNotObjNode(parent, setValue, instance, name, depth);
            node.Update();
            return node;
        }
        if (instance is BaseOrObj)
        {
            // mWindow.mString += AddSpace(depth) + name + "：" + instance.ToString() + "\n";
            var node = new BaseOrObjNode(parent, setValue, instance, name, depth);
            node.Update();
            return node;
        }
        if (instance is BaseFuncObj)
        {
            // mWindow.mString += AddSpace(depth) + name + "：" + instance.ToString() + "\n";
            var node = new BaseFuncObjNode(parent, setValue, instance, name, depth);
            node.Update();
            return node;
        }

        if (instance is SelectFuncObj)
        {
            // mWindow.mString += AddSpace(depth) + name + "：" + instance.ToString() + "\n";
            var node = new BaseSelectFuncListObjNode(parent, setValue, instance, name, depth);
            node.Update();

            return node;
        }

        if (instance is SelectFuncObj.Selection)
        {
            // mWindow.mString += AddSpace(depth) + name + "：" + instance.ToString() + "\n";
            var node = new BaseSelectionObjNode(parent, setValue, instance, name, depth);
            node.Update();

            return node;
        }
        if (instance is BaseRandomObj)
        {
            // mWindow.mString += AddSpace(depth) + name + "：" + instance.ToString() + "\n";
            var node = new BaseRandomObjNode(parent, setValue, instance, name, depth);
            node.Update();

            return node;
        }

        //Class类型
        //mWindow.mString += AddSpace(depth) + name + "：\n";
        var classNode = new ClassNode(parent, setValue, instance, name, depth);
        classNode.Update();
        return classNode;
    }

    /// <summary>
    /// 选择一个实例
    /// </summary>
    /// <param name="baseType"></param>
    /// <param name="onSelect"></param>
    /// <returns></returns>
    public static void AddInstance(Type baseType, Action<object> onSelect)
    {
        if (baseType.IsInterface || baseType.IsAbstract)
        {
            var items = TypeUtilities.GetChildren(baseType);
            var itemsNames = new List<string>();

            for (int i = 0; i < items.Count; i++)
            {
                itemsNames.Add(EventLogicSystem.EditorNameAttribute.TryGetName(items[i].GetType()) + " " + items[i].GetType().Name);
            }

            ItemSelectEditor.ShowItemSelectEditor(itemsNames, (index) =>
            {
                var obj = items[index];

                if (obj is BaseFuncObj)
                {
                    SetFunc((BaseFuncObj)obj, onSelect);
                }
                else
                {
                    onSelect(obj);
                }
            });
        }
        else
        {
            onSelect(New(baseType));
        }
    }

    public static void SetFunc(BaseFuncObj func, Action<object> onSelect)
    {
        var items = TypeUtilities.GetChildren<BaseFunc>();
        var itemsNames = new List<string>();

        for (int i = 0; i < items.Count; i++)
        {
            itemsNames.Add(items[i].GetItemName() + " " + items[i].GetType().Name);
        }

        ItemSelectEditor.ShowItemSelectEditor(itemsNames, (index) =>
        {
            func.Func[0] = items[index].GetType().Name;

            func.Func[1] = null;
            onSelect(func);
        });
    }

    public static object New(Type type)
    {
        return Activator.CreateInstance(type);
    }
}

public class DrawNode
{
    public const float LABEL_WIDTH = 300f;
    public const float BUTTON_WIDTH = 50f;
    protected string mNodeName;
    protected Action<object> mSetValue;
    protected int mDepth;
    protected int mSpace = 20;
    protected bool mFoldOut = false;
    protected bool mLastFoldOut = false;
    protected bool mModify = false;
    public object mInstance;
    protected DrawNode mParent;

    public string NodeName
    {
        get
        {
            return mNodeName;
        }

        set
        {
            mNodeName = value;
        }
    }

    public bool FoldOut
    {
        get
        {
            return mFoldOut;
        }

        set
        {
            mFoldOut = value;
        }
    }

    public DrawNode(DrawNode parent, Action<object> setValue, object instance, string nodeName, int index)
    {
        mParent = parent;
        mDepth = index + 1;
        mInstance = instance;
        NodeName = nodeName;

        mSetValue = setValue;
        mFoldOut = false;
        mLastFoldOut = false;
        mModify = false;
        Init();
    }

    public virtual void Init()
    {
    }

    public virtual void OnGUI(Rect rect)
    {
        if (mLastFoldOut != mFoldOut)
        {
            mLastFoldOut = mFoldOut;
            Event e = Event.current;
            if (e.button == 0 && e.control)
            {
                SetFoldoutRecursively(mFoldOut);
            }
        }
    }

    public virtual void SetFoldoutRecursively(bool foldoutBool)
    {
    }

    public virtual void Update()
    {
    }

    public void ShowBase(Action<object> setValue, string name)
    {
        if (null == setValue)
        {
            return;
        }

        GUILayout.BeginHorizontal();
        GUILayout.Space(mSpace * mDepth + 10);
        GUILayout.Label(name + (mModify ? " *" : ""), GUILayout.Width(LABEL_WIDTH));
        object fieldValue = mInstance;
        if (null == fieldValue)
        {
            fieldValue = "";
        }
        string value = EditorGUILayout.TextField(fieldValue.ToString());
        Type type = mInstance.GetType();
        object intValue = Parse(value, type);
        if (!intValue.Equals(mInstance))
        {
            try
            {
                setValue(intValue);
                mInstance = intValue;
                SetModify();
            }
            catch (Exception e)
            {
                Debug.LogError("[GameDataEditorTool]" + mInstance.GetType() + " 被赋值于 " + intValue.GetType() + " 错误" + e);
                throw;
            }
        }
        GUILayout.EndHorizontal();
    }

    public void SetModify()
    {
        mModify = true;
        if (mParent != null)
        {
            mParent.SetModify();
        }
    }

    public virtual void SetModifyChild(string name)
    {
        SetModify();
    }

    private object Parse(string text, Type type)
    {
        if (type == typeof(int))
        {
            try
            {
                int val = int.Parse(text);
                return val;
            }
            catch (Exception e)
            {
                Debug.LogError("[GameDataEditorTool]" + e.Message);
            }
        }
        else if (type == typeof(long))
        {
            try
            {
                long val = long.Parse(text);
                return val;
            }
            catch (Exception e)
            {
                Debug.LogError("[GameDataEditorTool]" + e.Message);
            }
        }
        else if (type == typeof(float))
        {
            try
            {
                float val = float.Parse(text);
                return val;
            }
            catch (Exception e)
            {
                Debug.LogError("[GameDataEditorTool]" + e.Message);
            }
        }
        else if (type == typeof(double))
        {
            try
            {
                double val = double.Parse(text);
                return val;
            }
            catch (Exception e)
            {
                Debug.LogError("[GameDataEditorTool]" + e.Message);
            }
        }
        else if (type == typeof(bool))
        {
            try
            {
                return bool.Parse(text);
            }
            catch (Exception e)
            {
                Debug.LogError("[GameDataEditorTool]" + e.Message);
            }
        }
        else if (type == typeof(string))
        {
            return text;
        }
        else if (type.IsEnum)
        {
            try
            {
                return Enum.Parse(type, text);
            }
            catch (Exception e)
            {
                Debug.LogError("[GameDataEditorTool]" + e.Message);
            }
        }
        return null;
    }

    public virtual float GetHeight()
    {
        return EditorGUIUtility.singleLineHeight;
    }
}

/// <summary>
/// 一个封装好的可调节顺序的列表
/// </summary>
/// <typeparam name="T"></typeparam>
public class LogicReorderableList<T> where T : ILogicBaseObj
{
    private ReorderableList _reorderableList;
    public bool Enable = false;
    private Action _onUpdate;

    public void Init(List<T> modityList, Action onUpdate)
    {
        _onUpdate = onUpdate;
        _reorderableList = new ReorderableList(modityList, typeof(T), true, true, true, true);

        _reorderableList.drawElementCallback = (rect, index, active, focused) =>
        {
            EditorGUI.LabelField(rect, GameEventEditor.ConditionEventCreator.TranslateDescription((modityList[index] as ILogicBaseObj)?.GetDescription()));

            //C V 复制粘贴工具
            Event evt = Event.current;
            switch (evt.type)
            {
                case EventType.KeyDown:
                    {
                        if (Event.current.keyCode == (KeyCode.C))
                        {
                            {
                                if (_reorderableList.index >= 0 && _reorderableList.index < modityList.Count)
                                {
                                    var obj = modityList[_reorderableList.index];

                                    GameEventEditorUtil.Copy(obj);
                                }
                            }

                            evt.Use();
                            // END EDIT
                        }

                        if (Event.current.keyCode == (KeyCode.V))
                        {
                            {
                                if (_reorderableList.index >= 0 && _reorderableList.index < modityList.Count)
                                {
                                    try
                                    {
                                        var obj = JsonConvert.DeserializeObject<T>(GUIUtility.systemCopyBuffer, new JsonLogicConverter());
                                        modityList.Add(obj);
                                        onUpdate();
                                    }
                                    catch (Exception e)
                                    {
                                        EditorUtility.DisplayDialog("提示", "[GameEvnentEditor] 粘贴内容格式不符，请确定是从同类型结构复制而来\n\n" + e.ToString(), "懂了");
                                        return;
                                    }
                              
                                }
                            }

                            evt.Use();
                            // END EDIT
                        }
                        break;
                    }
            }

        };
        _reorderableList.onAddCallback = (list) =>
        {
            DrawNodeUtil.AddInstance(typeof(T), (o) =>
            {
                modityList.Add((T)o);
                onUpdate();
            });
        };


       
    }

    private void OnMouseUp(ReorderableList list)
    {
        
    }

    public bool DrawSwitchButton()
    {
        if (GUILayout.Button("↹切换", GUILayout.MaxWidth(70f)))
        {
            Enable = !Enable;
            if (!Enable)
            {
                _onUpdate();
            }
        }
        return Enable;
    }

    public void OnGUI()
    {
        if (_reorderableList != null)
        {
            _reorderableList.DoLayoutList();
        }
    }
}

#endregion 节点化工具

#region 其他工具

internal static class GameEventEditorUtil
{
    public static void ShowCVButonGUI<T>(ref T obj) where T : class
    {
        if (GUILayout.Button("C", GUILayout.MaxWidth(20f)))
        {
            Copy(obj);
        }
        if (GUILayout.Button("V", GUILayout.MaxWidth(20f)))
        {
            Paste(ref obj);
        }
    }
    public static void ShowAButonGUI<T>(List<T> modityList, Action onUpdate) where T : class
    {
        if (GUILayout.Button("+", GUILayout.MaxWidth(25f)))
        {
            if (Event.current.button == 1)
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<T>(GUIUtility.systemCopyBuffer, new JsonLogicConverter());
                    modityList.Add(obj);
                    onUpdate();
                }
                catch (Exception e)
                {
                    EditorUtility.DisplayDialog("提示", "[GameEvnentEditor] 粘贴内容格式不符，请确定是从同类型结构复制而来\n\n" + e.ToString(), "懂了");
                    return;
                }
            }
            else
            {
                DrawNodeUtil.AddInstance(typeof(T), (o) =>
                {
                    modityList.Add((T)o);
                    onUpdate();
                });
            }
           
        }   
    }

    public static void Copy(object obj)
    {
        //这一串都是为了格式化输出json
        StringWriter textWriter = new StringWriter();
        JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
        {
            Formatting = Formatting.Indented,
            Indentation = 4,
            IndentChar = ' '
        };
        JsonSerializer serializer = new JsonSerializer();

        serializer.Serialize(jsonWriter, obj);
        GUIUtility.systemCopyBuffer = textWriter.ToString();
        textWriter.Close();
        jsonWriter.Close();
    }

    public static void Paste<T>(ref T target) where T : class
    {
        T obj = null;
        try
        {
            obj = JsonConvert.DeserializeObject<T>(GUIUtility.systemCopyBuffer, new JsonLogicConverter());
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("提示", "[GameEvnentEditor] 粘贴内容格式不符，请确定是从同类型结构复制而来\n\n" + e.ToString(), "懂了");
            return;
        }

        if (obj == null)
        {
            return;
        }
        FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        foreach (var field in fields)
        {
            try
            {
                field.SetValue(target, (field.GetValue(obj)));
            }
            catch { }
        }
    }
}

#endregion 其他工具