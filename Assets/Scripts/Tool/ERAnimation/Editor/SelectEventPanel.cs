#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ERAnimation
{
    public class SelectEventPanel : EditorWindow
    {
        const float screenWidth = 1920;
        const float screenHeight = 1080;
        const float panelWidth = 480;
        const float panelHeight = 540;
        const float height = 18;
        const float clearButtonWidth = 50;

        string searchText;

        public delegate void SelectEvent(List<ERAnimationEvent> animationEvent);

        TreeNode<Type> currentTree;
        List<Type> allTypes;
        SelectEvent callback;
        Vector2 pos;

        public static void OpenWindow(SelectEvent selectEvent)
        {
            var window = GetWindow<SelectEventPanel>();
            string data = PlayerPrefs.GetString("Editor_SelectEventPanel_position", "");
            if (string.IsNullOrEmpty(data))
            {
                window.position = new Rect(screenWidth / 2 - panelWidth / 2, screenHeight / 2 - panelHeight / 2, panelWidth, panelHeight);
            }
            else
            {
                string[] datas = data.Split(',');
                window.position = new Rect(Convert.ToSingle(datas[0]), Convert.ToSingle(datas[1]), Convert.ToSingle(datas[2]), Convert.ToSingle(datas[3]));
            }
            window.callback = selectEvent;
            window.searchText = "";
        }

        private void OnDestroy()
        {
            string data = position.x.ToString() + "," + position.y.ToString() + "," + position.width.ToString() + "," + position.height.ToString();
            PlayerPrefs.SetString("Editor_SelectEventPanel_position", data);
        }

        void OnGUI()
        {
            if (callback == null)
            {
                GUILayout.Box("Callback is null", GUILayout.ExpandWidth(true), GUILayout.Height(AnimatorControllerPanel.height));
            }
            else
            {
                pos = GUILayout.BeginScrollView(pos, GUILayout.ExpandWidth(true));
                GUILayout.BeginVertical();
                GUILayout.Box("Select Event", GUILayout.ExpandWidth(true), GUILayout.Height(AnimatorControllerPanel.height));
                GUILayout.BeginHorizontal();
                searchText = GUILayout.TextField(searchText, GUILayout.ExpandWidth(true), GUILayout.Height(AnimatorControllerPanel.height));
                if (GUILayout.Button("Clear", GUILayout.MaxWidth(clearButtonWidth), GUILayout.Height(AnimatorControllerPanel.height)))
                {
                    searchText = "";
                }
                GUILayout.EndHorizontal();
                if (currentTree == null)
                {
                    BuildTree();
                }
                if (string.IsNullOrEmpty(searchText))
                {
                    DrawTree(currentTree);
                }
                else
                {
                    foreach (var t in allTypes)
                    {
                        if (Regex.IsMatch(t.Name, searchText, RegexOptions.IgnoreCase))
                        {
                            if (GUILayout.Button(t.Name, GUILayout.Height(height)))
                            {
                                callback.Invoke(new List<ERAnimationEvent>() { (ERAnimationEvent)CreateInstance(t) });
                                Close();
                            }
                        }
                    }
                }
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
        }

        void BuildTree()
        {
            TreeNode<Type> parent = new TreeNode<Type>("");
            allTypes = new List<Type>();
            foreach (var t in typeof(ERAnimationEvent).Assembly.GetTypes())
            {
                if (t.IsSubclassOf(typeof(ERAnimationEvent)) && !t.IsAbstract)
                {
                    CategoryAttribute categoryAttribute = t.GetCustomAttribute<CategoryAttribute>();
                    if (categoryAttribute != null)
                    {
                        string[] paths = categoryAttribute.path.Split('/');
                        TreeNode<Type> current = parent;
                        for (int i = 0; i < paths.Length; i++)
                        {
                            TreeNode<Type> node = current.GetNode(paths[i]);
                            if (node == null)
                            {
                                node = new TreeNode<Type>(paths[i]);
                                current.AddNode(node);
                            }
                            current = node;
                        }
                        current.AddData(t);
                    }
                    else
                    {
                        parent.AddData(t);
                    }
                    allTypes.Add(t);
                }
            }
            currentTree = parent;
        }

        void DrawTree(TreeNode<Type> tree)
        {
            if (tree != null)
            {
                GUILayout.BeginVertical();
                if (tree.childs.Count == 0)
                {
                    foreach (var data in tree.datas)
                    {
                        if (GUILayout.Button(data.Name, GUILayout.Height(height)))
                        {
                            callback.Invoke(new List<ERAnimationEvent>() { (ERAnimationEvent)CreateInstance(data) });
                            Close();
                        }
                    }
                }
                else
                {
                    foreach (var child in tree.childs)
                    {
                        child.foldout = EditorGUILayout.Foldout(child.foldout, child.name);
                        if (child.foldout)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(height);
                            DrawTree(child);
                            GUILayout.EndHorizontal();
                        }
                    }
                    foreach (var data in tree.datas)
                    {
                        if (GUILayout.Button(data.Name, GUILayout.Height(18)))
                        {
                            callback.Invoke(new List<ERAnimationEvent>() { (ERAnimationEvent)CreateInstance(data) });
                            Close();
                        }
                    }
                }
                GUILayout.EndVertical();
            }
        }
    }
}
#endif