using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameMgr))]
public class GameMgrEditor : Editor
{
    static bool reflected = false;
    Dictionary<Type, Type> allModules = new Dictionary<Type, Type>();
    string[] allModulesString;
    Type[] codType;
    int selected;
    Dictionary<Type, Type> currentTypes = new Dictionary<Type, Type>();

    public override void OnInspectorGUI()
    {
        GameMgr gameMgr = (GameMgr)target;
        if (!reflected)
        {
            GetReflectResult();
        }
        GUILayout.Label("已加载的模块");
        if (currentTypes.Count > 0)
        {
            foreach (var pair in currentTypes)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Box($"{pair.Key.Name}\n({pair.Value.Name})", GUILayout.ExpandWidth(true));
                if (GUILayout.Button("删除", GUILayout.Width(60), GUILayout.ExpandHeight(true)))
                {
                    gameMgr.modulesToLoad.RemoveAll(e => e == pair.Key + ";" + pair.Value);
                    EditorUtility.SetDirty(gameMgr);
                    reflected = false;
                }
                GUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("未加载任何模块", MessageType.Info);
        }
        if (allModulesString.Length > 0)
        {
            GUILayout.BeginHorizontal();
            selected = EditorGUILayout.Popup(selected, allModulesString);
            if (GUILayout.Button("添加", GUILayout.Width(60)))
            {
                gameMgr.modulesToLoad.Add(codType[selected] + ";" + allModules[codType[selected]]);
                EditorUtility.SetDirty(gameMgr);
                reflected = false;
            }
            GUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.HelpBox("没有找到符合规则的模块", MessageType.Warning);
        }
    }

    private void OnEnable()
    {
        reflected = false;
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    public static void RefreshReflection()
    {
        reflected = false;
    }

    private void GetReflectResult()
    {
        reflected = true;
        allModules.Clear();
        currentTypes.Clear();
        GameMgr gameMgr = (GameMgr)target;
        Type logicModule = typeof(LogicModuleBase);
        foreach (var type in logicModule.Assembly.GetTypes())
        {
            if (type.IsSubclassOf(logicModule) && !type.IsAbstract)
            {
                foreach(var i in type.GetInterfaces())
                {
                    allModules.Add(i, type);
                }
            }
        }
        int index = 0;
        allModulesString = new string[allModules.Count];
        codType = new Type[allModules.Count];
        foreach (var pair in allModules)
        {
            allModulesString[index] = $"{pair.Key.Name}({pair.Value.Name})";
            codType[index] = pair.Key;
            index++;
        }

        for (int i = 0; i < gameMgr.modulesToLoad.Count; i++)
        {
            string load = gameMgr.modulesToLoad[i];
            string[] strs = load.Split(';');
            try
            {
                currentTypes.Add(logicModule.Assembly.GetType(strs[0]), logicModule.Assembly.GetType(strs[1]));
            }
            catch
            {
                gameMgr.modulesToLoad.RemoveAt(i);
                i--;
            }
        }
    }
}
