#if UNITY_EDITOR
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;

using UnityEngine;

namespace ERAnimation
{
    public class SelectAnimationControllerPanel : EditorWindow
    {
        const float screenWidth = 1920;
        const float screenHeight = 1080;
        const float panelWidth = 480;
        const float panelHeight = 540;
        ERAnimatorController controller;
        Vector2 pos;
        string searchText;

        [MenuItem("Tools/ERAnimation/AnimatorControler")]
        public static void OpenWindow()
        {
            OpenWindow(null);
        }

        public static void OpenWindow(ERAnimatorController animatorControler)
        {
            if (HasOpenInstances<AnimatorControllerPanel>())
            {
                GetWindow<AnimatorControllerPanel>().Close();
            }
            if (HasOpenInstances<EditConflictWindow>())
            {
                GetWindow<EditConflictWindow>().Close();
            }
            var window = GetWindow<SelectAnimationControllerPanel>();
            string data = PlayerPrefs.GetString("Editor_SelectAnimationControllerPanel_position", "");
            if (string.IsNullOrEmpty(data))
            {
                window.position = new Rect(screenWidth / 2 - panelWidth / 2, screenHeight / 2 - panelHeight / 2, panelWidth, panelHeight);
            }
            else
            {
                string[] datas = data.Split(',');
                window.position = new Rect(Convert.ToSingle(datas[0]), Convert.ToSingle(datas[1]), Convert.ToSingle(datas[2]), Convert.ToSingle(datas[3]));
            }
            window.controller = animatorControler;
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

        void OnGUI()
        {
            if (controller == null)
            {
                ERAnimatorController controller = Selection.activeObject is GameObject obj ? obj.GetComponentInChildren<ERAnimatorController>(true) : null;

                if (controller != null && controller != this.controller)
                {
                    if (PrefabUtility.IsPartOfPrefabAsset(controller))
                    {
                        this.controller = controller;
                    }
                    else if (PrefabUtility.IsPartOfPrefabInstance(controller))
                    {
                        this.controller = PrefabUtility.GetCorrespondingObjectFromSource(controller);
                    }
                    else
                    {
                        if (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null)
                        {
                            this.controller = AssetDatabase.LoadAssetAtPath<GameObject>(UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage().prefabAssetPath).GetComponentInChildren<ERAnimatorController>(true);
                        }
                        else
                        {
                            if (Application.isPlaying)
                            {
                                this.controller = controller;
                            }
                            else
                            {
                                GUILayout.Box("该GameObject必须是预制件。", GUILayout.ExpandWidth(true), GUILayout.Height(AnimatorControllerPanel.height));
                            }
                        }
                    }
                }
                else
                {
                    GUILayout.Box("请选择一个有ERAnimationController组件的游戏物体", GUILayout.ExpandWidth(true), GUILayout.Height(AnimatorControllerPanel.height));
                }
            }
            else
            {
                if (controller.Animator == null)
                {
                    GUILayout.Box("请先添加一个Animator。", GUILayout.ExpandWidth(true));
                }
                else if (controller.Animator.runtimeAnimatorController is AnimatorController runtimeAnimatorController)
                {
                    if (Application.isPlaying)
                    {
                        titleContent = new GUIContent(controller.gameObject.name);
                        pos = GUILayout.BeginScrollView(pos, GUILayout.ExpandWidth(true));
                        GUILayout.BeginVertical();

                        //获取所有动画状态，创建自己的动画状态
                        for (int i = 0; i < runtimeAnimatorController.layers.Length; i++)
                        {
                            GUILayout.Box(runtimeAnimatorController.layers[i].name, GUILayout.ExpandWidth(true), GUILayout.Height(AnimatorControllerPanel.height));
                            var animations = runtimeAnimatorController.layers[i].stateMachine.states;
                            foreach (var clip in animations)
                            {
                                GUILayout.BeginHorizontal();
                                var state = controller.States.Find(Item => { return Item.layerIndex == i && Item.linkedState == clip.state.name; });
                                bool edited = state != null;
                                if (GUILayout.Button(clip.state.name + (!edited ? " <color=#ff0000>(Not Edited)</color>" : " <color=#00ff00>(Edited)</color>"), new GUIStyle(GUI.skin.button) { richText = true }, GUILayout.Height(18), GUILayout.MinWidth(100)))
                                {
                                    if (edited)
                                    {
                                        AnimatorControllerPanel.OpenWindow(state, controller);
                                        Close();
                                    }
                                }
                                GUILayout.EndHorizontal();
                            }
                        }
                        GUILayout.EndVertical();
                        GUILayout.EndScrollView();
                    }
                    else
                    {
                        titleContent = new GUIContent(controller.gameObject.name);
                        pos = GUILayout.BeginScrollView(pos, GUILayout.ExpandWidth(true));
                        GUILayout.BeginVertical();

                        //创建文件夹保存数据
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

                        GUILayout.BeginHorizontal();
                        searchText = GUILayout.TextField(searchText, GUILayout.Height(18), GUILayout.MinWidth(100));
                        if (GUILayout.Button("CheckAll", GUILayout.MaxWidth(100)))
                        {
                            for (int i = 0; i < runtimeAnimatorController.layers.Length; i++)
                            {
                                var animations = runtimeAnimatorController.layers[i].stateMachine.states;
                                foreach (var clip in animations)
                                {
                                    string path = PATH + "/" + folderName + "/" + i.ToString() + "_" + clip.state.name + ".asset";
                                    if (File.Exists(path))
                                    {
                                        var asset = AssetDatabase.LoadAssetAtPath<ERAnimationState>(path);
                                        asset.animationTime = clip.state.motion.averageDuration;
                                    }
                                }
                            }
                            AssetDatabase.Refresh();
                            AssetDatabase.SaveAssets();
                        }
                        GUILayout.EndHorizontal();
                        if (string.IsNullOrEmpty(searchText))
                        {
                            //获取所有动画状态，创建自己的动画状态
                            for (int i = 0; i < runtimeAnimatorController.layers.Length; i++)
                            {
                                GUILayout.Box(runtimeAnimatorController.layers[i].name, GUILayout.ExpandWidth(true), GUILayout.Height(AnimatorControllerPanel.height));
                                var animations = runtimeAnimatorController.layers[i].stateMachine.states;
                                foreach (var clip in animations)
                                {
                                    GUILayout.BeginHorizontal();
                                    string path = PATH + "/" + folderName + "/" + i.ToString() + "_" + clip.state.name + ".asset";
                                    bool edited = File.Exists(path);
                                    if (GUILayout.Button(clip.state.name + (!edited ? " <color=#ff0000>(Not Edited)</color>" : " <color=#00ff00>(Edited)</color>"), new GUIStyle(GUI.skin.button) { richText = true }, GUILayout.Height(18), GUILayout.MinWidth(100)))
                                    {
                                        if (!edited)
                                        {
                                            AssetDatabase.CreateAsset(CreateInstance<ERAnimationState>().Init(controller, i, clip.state.name, clip.state.motion.name), path);
                                            AssetDatabase.Refresh();
                                        }
                                        var asset = AssetDatabase.LoadAssetAtPath<ERAnimationState>(path);
                                        if (!controller.States.Contains(asset))
                                        {
                                            controller.States.Add(asset);
                                        }
                                        EditorUtility.SetDirty(controller);
                                        AssetDatabase.SaveAssets();
                                        AnimatorControllerPanel.OpenWindow(asset, controller);
                                        Close();
                                    }
                                    if (GUILayout.Button("Delete", GUILayout.MaxWidth(50)))
                                    {
                                        if (edited)
                                        {
                                            if (EditorUtility.DisplayDialog("警告", "你确定要删除吗？此操作无法撤销", "确定", "取消"))
                                            {
                                                controller.States.Remove(AssetDatabase.LoadAssetAtPath<ERAnimationState>(path)); Debug.Log(AssetDatabase.DeleteAsset(path) ? "成功删除" + clip.state.name : "删除" + clip.state.name + "失败");
                                            }
                                        }
                                    }
                                    GUILayout.EndHorizontal();
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < runtimeAnimatorController.layers.Length; i++)
                            {
                                var animations = runtimeAnimatorController.layers[i].stateMachine.states;
                                foreach (var clip in animations)
                                {
                                    if (Regex.IsMatch(clip.state.name, searchText, RegexOptions.IgnoreCase))
                                    {
                                        GUILayout.BeginHorizontal();
                                        string path = PATH + "/" + folderName + "/" + i.ToString() + "_" + clip.state.name + ".asset";
                                        bool edited = File.Exists(path);
                                        if (GUILayout.Button(clip.state.name + (!edited ? " <color=#ff0000>(Not Edited)</color>" : " <color=#00ff00>(Edited)</color>"), new GUIStyle(GUI.skin.button) { richText = true }, GUILayout.Height(18), GUILayout.MinWidth(100)))
                                        {
                                            if (!edited)
                                            {
                                                AssetDatabase.CreateAsset(CreateInstance<ERAnimationState>().Init(controller, i, clip.state.name, clip.state.motion.name), path);
                                                AssetDatabase.Refresh();
                                            }
                                            var asset = AssetDatabase.LoadAssetAtPath<ERAnimationState>(path);
                                            if (!controller.States.Contains(asset))
                                            {
                                                controller.States.Add(asset);
                                            }
                                            EditorUtility.SetDirty(controller);
                                            AssetDatabase.SaveAssets();
                                            AnimatorControllerPanel.OpenWindow(asset, controller);
                                            Close();
                                        }
                                        if (GUILayout.Button("Delete", GUILayout.MaxWidth(50)))
                                        {
                                            if (edited)
                                            {
                                                if (EditorUtility.DisplayDialog("警告", "你确定要删除吗？此操作无法撤销", "确定", "取消"))
                                                {
                                                    controller.States.Remove(AssetDatabase.LoadAssetAtPath<ERAnimationState>(path)); Debug.Log(AssetDatabase.DeleteAsset(path) ? "成功删除" + clip.state.name : "删除" + clip.state.name + "失败");
                                                }
                                            }
                                        }
                                        GUILayout.EndHorizontal();
                                    }
                                }
                            }
                        }
                        GUILayout.EndVertical();
                        GUILayout.EndScrollView();
                    }
                }
                else
                {
                    GUILayout.Box("请给Animator赋值RuntimeController。", GUILayout.ExpandWidth(true));
                }
            }
        }
    }
}
#endif