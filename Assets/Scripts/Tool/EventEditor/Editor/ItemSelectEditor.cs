using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 对象选择GUI，可以弹出一个带搜索的选项列表
/// </summary>
public class ItemSelectEditor : OdinMenuEditorWindow
{
    private OdinMenuTree _tree;

    private List<string> _items;

    private Action<int> _onSelect;

    public static void ShowItemSelectEditor(List<string> items, Action<int> onSelect)
    {
        var window = GetWindow<ItemSelectEditor>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(400, 800);

        window.BuildMenuTree(items);
        window._onSelect = onSelect;

        window.Show();
    }

    protected void BuildMenuTree(List<string> items)
    {
        _tree = new OdinMenuTree(true);
        _items = items;
        var customMenuStyle = new OdinMenuStyle
        {
            BorderPadding = 0f,
            AlignTriangleLeft = true,
            TriangleSize = 16f,
            TrianglePadding = 0f,
            Offset = 20f,
            Height = 23,
            IconPadding = 0f,
            BorderAlpha = 0.323f
        };

        _tree.DefaultMenuStyle = customMenuStyle;

        _tree.Config.DrawSearchToolbar = true;

        // Adds the custom menu style to the tree, so that you can play around with it.
        // Once you are happy, you can press Copy C# Snippet copy its settings and paste it in code.
        // And remove the "Menu Style" menu item from the tree.

        for (int i = 0; i < items.Count; i++)
        {
            string path = "";
            string name = items[i];
            if (name.Contains("/"))
            {
                path = name.Substring(0, name.LastIndexOf('/'));
                name = name.Substring(name.LastIndexOf('/') + 1);
            }
            var customObject = new SomeCustomClass() { Name = name, Index = i };
            var customMenuItem = new MenuItem(_tree, customObject);
            _tree.AddMenuItemAtPath(path, customMenuItem);
        }

        _tree.EnumerateTree()
            .AddThumbnailIcons()
            .SortMenuItemsByName();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        if (_tree == null)
        {
            BuildMenuTree(_items);
        }
        return _tree;
    }

    private bool db = false;

    protected override void OnGUI()
    {
        
        //检测鼠标双击事件
        Event e = Event.current;
        if (e.isMouse && (e.clickCount == 2))
        {
            db = true;
        }

        if (_tree == null)
        {
            BuildMenuTree(_items);
        }
        _tree.DrawMenuTree();
        if ((GUILayout.Button("确定") || db)&& _tree.Selection.Count > 0)
        {
            Close();
            _onSelect(((SomeCustomClass)(_tree.Selection[0].Value)).Index);
        }

        this.RepaintIfRequested();
    }

    private class MenuItem : OdinMenuItem
    {
        private readonly SomeCustomClass instance;

        public MenuItem(OdinMenuTree tree, SomeCustomClass instance) : base(tree, instance.Name, instance)
        {
            this.instance = instance;
        }

        public override string SmartName { get { return this.instance.Name; } }
    }

    private class SomeCustomClass
    {
        public string Name;
        public int Index;
    }
}