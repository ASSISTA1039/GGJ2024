#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ERAnimation
{
    public class Preference : EditorWindow
    {
        [MenuItem("Tools/ERAnimation/Setting")]
        public static void OpenWindow()
        {

        }

        [MenuItem("Tools/ERAnimation/Upgrade")]
        public static void Upgrade()
        {
            if (PrefabUtility.IsPartOfPrefabAsset(Selection.activeObject))
            {
                string[] strs = AssetDatabase.GetAssetPath(Selection.activeObject).Split('/');
                string result = strs[0];
                for (int i = 0; i < strs.Length; i++)
                {
                    if (i != strs.Length - 1 && i != 0)
                    {
                        result += "/" + strs[i];
                    }
                }
                strs = AssetDatabase.FindAssets("t:ERAnimationEvent", new string[] { result });
                HashSet<string> allPaths = new HashSet<string>();
                foreach (var guid in strs)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!allPaths.Contains(path))
                    {
                        allPaths.Add(path);
                    }
                }
                foreach (var path in allPaths)
                {
                    if (AssetDatabase.LoadMainAssetAtPath(path) is ERAnimationState state)
                    {
                        AssetDatabase.RenameAsset(path, state.layerIndex.ToString() + "_" + state.name);
                    }
                }
            }
        }
    }
}
#endif