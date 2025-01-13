using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BlockManager))]
public class BlockEditor : Editor
{
    public override void OnInspectorGUI()
    {

        BlockManager spawner = (BlockManager)target;
        EditorGUILayout.LabelField("基本信息", EditorStyles.boldLabel);
        spawner.prefab = (GameObject)EditorGUILayout.ObjectField("填充物体", spawner.prefab, typeof(GameObject), false);
        spawner.remind = (GameObject)EditorGUILayout.ObjectField("提示方块", spawner.remind, typeof(GameObject), false);

        spawner.x = EditorGUILayout.IntField("X Position", spawner.x);
        spawner.y = EditorGUILayout.IntField("Y Position", spawner.y);
        spawner.z = EditorGUILayout.IntField("Z Position", spawner.z);
        spawner.scale = EditorGUILayout.FloatField("Scale", spawner.scale);

        if (GUILayout.Button("启用位置提示"))
        {
            spawner.RemindBlock();
        }
        if (GUILayout.Button("提示方块位置刷新"))
        {
            spawner.RemindBlockUpdate(0, 0, 0);
        }
        if (GUILayout.Button("结束编辑"))
        {
            spawner.RemindBlock();
        }
        EditorGUILayout.Space();
        if(GUILayout.Button("X正向移动并生成"))
        {
            spawner.GBlockQ(spawner.prefab, spawner.x, spawner.y, spawner.z, spawner.scale);
            spawner.RemindBlockUpdate(1,0,0);
        }
        if (GUILayout.Button("X反向移动并生成"))
        {
            spawner.GBlockQ(spawner.prefab, spawner.x, spawner.y, spawner.z, spawner.scale);
            spawner.RemindBlockUpdate(-1, 0, 0);
        }
        if (GUILayout.Button("Y正向移动并生成"))
        {
            spawner.GBlockQ(spawner.prefab, spawner.x, spawner.y, spawner.z, spawner.scale);
            spawner.RemindBlockUpdate(0, 1, 0);
        }
        if (GUILayout.Button("Y反向移动并生成"))
        {
            spawner.GBlockQ(spawner.prefab, spawner.x, spawner.y, spawner.z, spawner.scale);
            spawner.RemindBlockUpdate(0, -1, 0);
        }
        if (GUILayout.Button("Z正向移动并生成"))
        {
            spawner.GBlockQ(spawner.prefab, spawner.x, spawner.y, spawner.z, spawner.scale);
            spawner.RemindBlockUpdate(0, 0, 1);
        }
        if (GUILayout.Button("Z反向移动并生成"))
        {
            spawner.GBlockQ(spawner.prefab, spawner.x, spawner.y, spawner.z, spawner.scale);
            spawner.RemindBlockUpdate(0, 0, -1);
        }
        // 添加空隙
        EditorGUILayout.Space();

        if (GUILayout.Button("生成"))
        {
            // 调用 GBlock 方法
            spawner.GBlockQ(spawner.prefab, spawner.x, spawner.y, spawner.z, spawner.scale);
        }
        // 生成方向
        EditorGUILayout.LabelField("填充数量", EditorStyles.boldLabel);
        spawner.xrow = EditorGUILayout.IntField("X Row", spawner.xrow);
        spawner.yrow = EditorGUILayout.IntField("Y Row", spawner.yrow);
        spawner.zrow = EditorGUILayout.IntField("Z Row", spawner.zrow);

        // 添加空隙
        EditorGUILayout.Space();

        if (GUILayout.Button("填充生成"))
        {
            // 调用 GBlock 方法
            spawner.GBlockFull(spawner.prefab, spawner.x, spawner.y, spawner.z, spawner.xrow, spawner.yrow, spawner.zrow, spawner.scale);
        }
        // 强制更新 Inspector（当有字段修改时）
        serializedObject.ApplyModifiedProperties();
    }


}
