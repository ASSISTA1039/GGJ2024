using QxFramework.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public partial class ChildBindTool
{
    public Dictionary<string, GameObject> Gos { get; private set; }

    /// <summary>
    /// 设置物体属性的方法
    /// </summary>
    private static Dictionary<string, Action<Transform, object>> _setPropActionMap;

    /// <summary>
    /// 绑定的数据显示方法
    /// </summary>
    private readonly List<ChildValueBindInfo> _bindInfos = new List<ChildValueBindInfo>();

    /// <summary>
    /// 默认父物体
    /// </summary>
    private readonly Transform _transform;

    /// <summary>
    /// 使用对象
    /// </summary>
    private readonly object _userObject;

    #region 绑定方法

    /// <summary>
    /// 初始化字符串对应函数赋值表，仅限于Unity泛用类型，不要加入某个工程特有的类
    /// </summary>
    private static void InitSetPropActionMap()
    {
        if (_setPropActionMap == null)
        {
            _setPropActionMap = new Dictionary<string, Action<Transform, object>>();
        }
        _setPropActionMap.Clear();

        //文本
        _setPropActionMap[nameof(Text.text)] = (transfrom, value) =>
        {
            var text = transfrom.GetComponent<Text>();
            if (text.text != (string)value)
            {
                text.text = (string)value;
            }
        };

        //按钮
        _setPropActionMap[nameof(Button.onClick)] = (transfrom, value) =>
        {
            var btn = transfrom.GetComponent<Button>();
            var action = value as Action;
            btn.onClick.RemoveAllListeners();
            if (action != null)
            {
                //注册回调
                btn.onClick.AddListener(() => { action(); });
            }
        };

        //物体激活与否
        _setPropActionMap[nameof(GameObject.activeSelf)] = (obj, value) =>
        {
            var go = obj.gameObject;
            go.SetActive((bool)value);
        };

        //图片名称
        _setPropActionMap[nameof(Image.sprite.name)] = (obj, value) =>
        {
            var image = obj.GetComponent<Image>();
            image.sprite = ResourceManager.Instance.Load<Sprite>(((string)value));
        };

        //图片本身
        _setPropActionMap[nameof(Image.sprite)] = (obj, value) =>
        {
            var image = obj.GetComponent<Image>();
            image.sprite = (Sprite)value;
        };
    }

    #endregion 绑定方法

    #region 公开方法

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="userObject">使用这个工具的绑定对象</param>
    /// <param name="transform">用于查找的子物体</param>
    public ChildBindTool(object userObject, Transform transform)
    {
        _userObject = userObject;
        _transform = transform;
        InitSetPropActionMap();
        InitSetPropActionMapEx();
        CollectObject();
        SetDefaultComponent();
    }

    /// <summary>
    /// 建立一个包含所有子物体的字典
    /// </summary>
    public void CollectObject()
    {
        if (Gos != null)
        {
            Gos.Clear();
        }
        else
        {
            Gos = new Dictionary<string, GameObject>();
        }
        //获取所有子物体
        var childs = _transform.GetComponentsInChildren<Transform>(true);//包括隐藏物体

        foreach (Transform child in childs)
        {
            Gos[child.name] = child.gameObject;
        }
    }

    /// <summary>
    /// 一个快速获取的函组件数，以物体名为Key，相同值则直接覆盖
    /// </summary>
    /// <typeparam name="T">组件</typeparam>
    /// <param name="name">物体名。</param>
    /// <returns>组件对象</returns>
    /// <exception cref="System.ArgumentNullException">没找到组件。</exception>
    public T Get<T>(string name)
    {
        if (Gos == null || !Gos.ContainsKey(name))
        {
            CollectObject();
        }
        if (!Gos.ContainsKey(name))
        {
            //throw new ArgumentNullException(name);
            return default(T);
        }

        return Gos[name].GetComponent<T>();
    }

    /// <summary>
    /// 清空缓存
    /// </summary>
    public void Clear()
    {
        Gos.Clear();
    }

    /// <summary>
    /// 提交绑定的值
    /// </summary>
    public void CommitValue()
    {
        foreach (ChildValueBindInfo bindInfo in _bindInfos)
        {
            bindInfo.BindAction(bindInfo.Target, bindInfo.Field.GetValue(_userObject));
        }
    }

    #endregion 公开方法

    #region 私有方法和对象

    /// <summary>
    /// 设置默认的绑定组件
    /// </summary>
    private void SetDefaultComponent()
    {
        var vt = _userObject.GetType();
        var fields = vt.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

        foreach (var f in fields)
        {
            //自动获取节点
            var attrs = f.GetCustomAttributes(typeof(ChildComponentBind), false); //as Attribute[];
            if (attrs.Length > 0)
            {
                var attr = attrs.ToList().Find((a) => a is ChildComponentBind) as ChildComponentBind;
                if (attr == null)
                {
                    continue;
                }
                //获取节点,并且获取组件
                var trans = Get<Transform>(attr.Name);
                if (trans == null)
                {
                    Debug.LogError($"未找到子物体：{vt.FullName} - {attr.Name}");
                    continue;
                }
                object com = null;
                if (f.FieldType.IsSubclassOf(typeof(Component)))
                {
                    com = trans.GetComponent(f.FieldType);
                }
                if (f.FieldType == (typeof(GameObject)))
                {
                    com = trans.gameObject;
                }
                if (com == null)
                {
                    Debug.LogError($"节点没有对应组件：type【{f.FieldType}】 - {attr.Name}");
                }

                //设置属性
                f.SetValue(_userObject, com);
            }

            //获取绑定
            attrs = f.GetCustomAttributes(typeof(ChildValueBind), false); //as Attribute[];
            if (attrs.Length > 0)
            {
                var attr = attrs.ToList().Find((a) => a is ChildValueBind) as ChildValueBind;
                if (attr == null)
                {
                    continue;
                }
                //获取节点,并且获取组件
                var trans = Get<Transform>(attr.Name);
                if (trans == null)
                {
                    Debug.LogError($"未找到子物体：{vt.FullName}  {attr.Name}");
                    continue;
                }

                if (_setPropActionMap.TryGetValue(attr.BindPath, out Action<Transform, object> action))
                {
                    _bindInfos.Add(new ChildValueBindInfo(trans, f, action));
                }
                else
                {
                    Debug.LogError($"节点没有对应绑定函数：{f.FieldType}  {attr.BindPath}");
                }
            }
        }
    }

    private class ChildValueBindInfo
    {
        /// <summary>
        /// 某个子物体或者子物体上的组件
        /// </summary>
        public readonly Transform Target;

        /// <summary>
        /// 归属脚本的某个字段值
        /// </summary>
        public readonly FieldInfo Field;

        public readonly Action<Transform, object> BindAction;

        public ChildValueBindInfo(Transform target, FieldInfo field, Action<Transform, object> bindAction)
        {
            Target = target;
            Field = field;
            BindAction = bindAction;
        }
    }

    #endregion 私有方法和对象
}

/// <summary>
/// 节点发现属性
/// </summary>
public class ChildComponentBind : Attribute
{
    public string Name;

    public ChildComponentBind(string name)
    {
        Name = name;
    }
}

/// <summary>
/// 节点发现属性
/// </summary>
public class ChildValueBind : Attribute
{
    public string Name;
    public string BindPath;

    public ChildValueBind(string name, string bindPath)
    {
        Name = name;
        BindPath = bindPath;
    }
}