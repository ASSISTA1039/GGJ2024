using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


//[CustomEditor(typeof(LevelButton))]
//public class UIElementEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        LevelButton uiElement = (LevelButton)target;

//        // 绘制默认Inspector
//        DrawDefaultInspector();

//        // 显示下拉选项
//        uiElement.uiType = (UIType)EditorGUILayout.EnumPopup("UI Type", uiElement.uiType);

//        // 更新贴图
//        uiElement.UpdateSprite();
//    }
//}