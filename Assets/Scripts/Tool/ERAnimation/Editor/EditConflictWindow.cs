using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace ERAnimation
{
    public class EditConflictWindow : EditorWindow
    {
        private class Conflict
        {
            public ERAnimationState state;
            public int selected;

            public Conflict(ERAnimationState state)
            {
                this.state = state;
                this.selected = 0;
            }
        }

        const float screenWidth = 1920;
        const float screenHeight = 1080;
        const float panelWidth = 480;
        const float panelHeight = 540;
        List<Conflict> conflicts;

        public static void OpenWindow(List<ERAnimationState> conflicts)
        {
            if (HasOpenInstances<AnimatorControllerPanel>())
            {
                GetWindow<AnimatorControllerPanel>().Close();
            }
            if (HasOpenInstances<SelectAnimationControllerPanel>())
            {
                GetWindow<SelectAnimationControllerPanel>().Close();
            }
            var window = GetWindow<EditConflictWindow>();
            string data = PlayerPrefs.GetString("Editor_EditConflictWindow_position", "");
            if (string.IsNullOrEmpty(data))
            {
                window.position = new Rect(screenWidth / 2 - panelWidth / 2, screenHeight / 2 - panelHeight / 2, panelWidth, panelHeight);
            }
            else
            {
                string[] datas = data.Split(',');
                window.position = new Rect(Convert.ToSingle(datas[0]), Convert.ToSingle(datas[1]), Convert.ToSingle(datas[2]), Convert.ToSingle(datas[3]));
            }
            window.conflicts = new List<Conflict>();
            foreach (var state in conflicts)
            {
                window.conflicts.Add(new Conflict(state));
            }
        }

        private void OnGUI()
        {
            if (conflicts == null || conflicts.Count == 0)
            {
                EditorGUILayout.HelpBox("无冲突。", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("冲突指某一内部状态没有在动画机中找到他对应的动画状态。这通常由动画机中的动画状态改了名称导致，通常不会影响运行，但改名后的状态将无法连接到原状态。将内部状态重新链接到一个动画状态即可解决冲突。", MessageType.Info);
                ERAnimatorController controller = conflicts[0].state.linkedController;
                List<string> allStates = new List<string>();
                foreach (var ac in ((AnimatorController)controller.Animator.runtimeAnimatorController).layers[conflicts[0].state.layerIndex].stateMachine.states)
                {
                    bool flag = true;
                    foreach (ERAnimationState state in controller.States)
                    {
                        if (state.linkedState == ac.state.name)
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        allStates.Add(ac.state.name);
                    }
                }
                string[] states = allStates.ToArray();
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.Box("冲突状态", GUILayout.Width(position.width / 2));
                GUILayout.Box("对应状态", GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
                HashSet<int> repeatCheck = new HashSet<int>();
                bool repeat = false;
                foreach (Conflict conflict in conflicts)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Box(conflict.state.linkedState, GUILayout.Width(position.width / 2));
                    conflict.selected = EditorGUILayout.Popup(conflict.selected, states, GUILayout.ExpandWidth(true));
                    if (repeatCheck.Contains(conflict.selected))
                    {
                        repeat = true;
                    }
                    else
                    {
                        repeatCheck.Add(conflict.selected);
                    }
                    GUILayout.EndHorizontal();
                }
                if (repeat)
                {
                    EditorGUILayout.HelpBox("不能链接到同一动画状态。", MessageType.Error);
                }
                else
                {
                    if (GUILayout.Button("保存", GUILayout.ExpandWidth(true)))
                    {
                        string assetPath = AssetDatabase.GetAssetPath(controller.gameObject);
                        string[] splited = assetPath.Split('/');
                        string folderName = splited[splited.Length - 1].Split('.')[0];
                        string PATH = assetPath.Replace("/" + folderName + ".prefab", "");
                        folderName += "ERAnimations";
                        if (!AssetDatabase.IsValidFolder(PATH + "/" + folderName))
                        {
                            AssetDatabase.CreateFolder(PATH, folderName);
                            AssetDatabase.Refresh();
                        }
                        foreach (Conflict conflict in conflicts)
                        {
                            string oldPath = PATH + "/" + folderName + "/" + conflict.state.layerIndex.ToString() + "_" + conflict.state.linkedState + ".asset";
                            conflict.state.linkedState = states[conflict.selected];
                            string newPath = PATH + "/" + folderName + "/" + conflict.state.layerIndex.ToString() + "_" + conflict.state.linkedState + ".asset";
                            if (File.Exists(newPath))
                            {
                                AssetDatabase.DeleteAsset(newPath);
                                AssetDatabase.CreateAsset(conflict.state, newPath);
                            }
                            else if (File.Exists(oldPath))
                            {
                                Debug.Log(AssetDatabase.RenameAsset(oldPath, conflict.state.layerIndex.ToString() + "_" + conflict.state.linkedState + ".asset"));
                            }
                            else
                            {
                                AssetDatabase.CreateAsset(conflict.state, newPath);
                            }
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                            Close();
                        }
                    }
                }
                GUILayout.EndVertical();
            }
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        public static void CloseThis()
        {
            if (HasOpenInstances<AnimatorControllerPanel>())
            {
                GetWindow<AnimatorControllerPanel>().Close();
            }
        }

        private void OnDestroy()
        {
            string data = position.x.ToString() + "," + position.y.ToString() + "," + position.width.ToString() + "," + position.height.ToString();
            PlayerPrefs.SetString("Editor_SelectAnimationControllerPanel_position", data);
        }
    }
}