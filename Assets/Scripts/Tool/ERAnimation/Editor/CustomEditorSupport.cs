#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;

using UnityEngine;

namespace ERAnimation
{
    [CustomEditor(typeof(ERAnimatorController))]
    public class AnimatorControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ERAnimatorController realController;
            if (target is ERAnimatorController controller)
            {
                if (PrefabUtility.IsPartOfPrefabAsset(controller))
                {
                    realController = controller;
                }
                else if (PrefabUtility.IsPartOfPrefabInstance(controller))
                {
                    realController = PrefabUtility.GetCorrespondingObjectFromSource(controller);
                }
                else
                {
                    if (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null)
                    {
                        realController = AssetDatabase.LoadAssetAtPath<GameObject>(UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage().prefabAssetPath).GetComponentInChildren<ERAnimatorController>(true);
                    }
                    else
                    {
                        realController = null;
                    }
                }
            }
            else
            {
                realController = null;
            }
            if (realController != null)
            {
                List<ERAnimationState> conflicts = AnimatorControllerPanel.GetConflict(realController);
                if (conflicts.Count == 0)
                {
                    if (GUILayout.Button("Show Editor Panel"))
                    {
                        SelectAnimationControllerPanel.OpenWindow(realController);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("控制器中存在冲突。这大概率是因为直接更改了动画状态的名字。点击下方按钮编辑冲突。", MessageType.Error);
                    if (GUILayout.Button("Edit Conflicts"))
                    {
                        EditConflictWindow.OpenWindow(conflicts);
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("GameObject必须是预制件才可编辑。", MessageType.Error);
            }
        }
    }

    [CustomEditor(typeof(ERAnimationState))]
    public class AnimationStateEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ERAnimationState obj = target as ERAnimationState;
            if (obj.linkedController == null)
            {
                EditorGUILayout.HelpBox("该事件找不到控制器。请通过控制器打开来解决此问题。", MessageType.Error);
            }
            else
            {
                //base.OnInspectorGUI();
                List<ERAnimationState> conflicts = AnimatorControllerPanel.GetConflict(obj.linkedController);
                if (conflicts.Count == 0)
                {
                    if (GUILayout.Button("Editor Event"))
                    {
                        AnimatorControllerPanel.OpenWindow(obj, obj.linkedController);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("控制器中存在冲突。这大概率是因为直接更改了动画状态的名字。点击下方按钮编辑冲突。", MessageType.Error);
                    if (GUILayout.Button("Edit Conflicts"))
                    {
                        EditConflictWindow.OpenWindow(conflicts);
                    }
                }
            }
        }
    }
}
#endif