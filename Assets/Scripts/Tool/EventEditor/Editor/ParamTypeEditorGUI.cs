using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace EventLogicSystem
{
    public class ParamTypeEditorGUI
    {
        /// <summary>
        /// 参数名称匹配
        /// </summary>
        /// <returns></returns>
        public virtual string GetName()
        {
            return string.Empty;
        }

        /// <summary>
        /// 参数类型匹配
        /// </summary>
        /// <returns></returns>
        public virtual Type GetTargetType()
        {
            return null;
        }

        /// <summary>
        /// 绘制函数
        /// </summary>
        /// <param name="obj"></param>
        public virtual void OnGUI(string name, string token, Action<string> setToken)
        {
        }

        public static void Split(string str, List<string> strs)
        {
            if (string.IsNullOrEmpty(str))
            {
                return;
            }
            if (!str.Contains("{"))
            {
                strs.AddRange(str.Split(','));
                return;
            }
            int lastIndex = 0;
            int stack = 0;
            for (int i = 0; i < str.Length; i++)
            {
                char ch = str[i];
                if (ch == '{')
                {
                    stack++;
                }
                else if (ch == '}')
                {
                    stack--;
                    if (i == str.Length - 1)
                    {
                        strs.Add(str.Substring(lastIndex));
                    }
                }
                else if (ch == ',')
                {
                    if (stack == 0 && lastIndex < i)
                    {
                        strs.Add(str.Substring(lastIndex, i - lastIndex));

                        lastIndex = i + 1;
                    }
                }
            }
        }
    }

    public class IntParamEditorGUI : ParamTypeEditorGUI
    {
        public override Type GetTargetType()
        {
            return typeof(int);
        }

        public override void OnGUI(string name, string token, Action<string> setToken)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name);
            token = EditorGUILayout.TextField(token);
            setToken(token);
            GUILayout.EndHorizontal();
        }
    }

    public class BoolParamEditorGUI : ParamTypeEditorGUI
    {
        public override Type GetTargetType()
        {
            return typeof(bool);
        }

        public override void OnGUI(string name, string token, Action<string> setToken)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name);
            token = EditorGUILayout.Toggle(token.Equals("true")) ? "true" : "false";
            setToken(token);
            GUILayout.EndHorizontal();
        }
    }

    public class FloatParamEditorGUI : ParamTypeEditorGUI
    {
        public override Type GetTargetType()
        {
            return typeof(float);
        }

        public override void OnGUI(string name, string token, Action<string> setToken)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name);
            token = EditorGUILayout.TextField(token);
            setToken(token);
            GUILayout.EndHorizontal();
        }
    }

    public class StringParamEditorGUI : ParamTypeEditorGUI
    {
        public override Type GetTargetType()
        {
            return typeof(string);
        }

        public override void OnGUI(string name, string token, Action<string> setToken)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name);
            //这里的传唤对应着StringFromLuaTable.WriteValue
            string trans = token.Replace("\"", "").Replace("\\n", "\n");

            token = GUILayout.TextArea(trans);
            setToken(token);
            GUILayout.EndHorizontal();
        }
    }

    public class ListIntParamEditorGUI : ParamTypeEditorGUI
    {
        private ReorderableList _reorderableList = null;
        private List<string> _stringList = null;

        public bool Enable = false;
        private Action<string> _setToken;

        private void OnDraw(Rect rect, int index, bool isActive, bool isFocused)
        {
            _stringList[index] = EditorGUI.TextField(rect, _stringList[index]);
        }

        public override Type GetTargetType()
        {
            return typeof(List<int>);
        }

        public override void OnGUI(string name, string token, Action<string> setToken)
        {
            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(name);
            if (_reorderableList == null)
            {
                _stringList = new List<string>();
                _reorderableList = new ReorderableList(_stringList, typeof(int), true, true, true, true);

                _reorderableList.drawElementCallback = OnDraw;
                _reorderableList.onAddCallback = OnAddNewItem;
            }

            _stringList.Clear();
            ParamTypeEditorGUI.Split(token.Substring(1, token.Length - 2), _stringList);
            if (_reorderableList != null)
            {
                _setToken = setToken;
                EditorGUILayout.BeginVertical();
                _reorderableList.DoLayoutList();
                EditorGUILayout.EndVertical();
            }
            SetListToToken(setToken);
            GUILayout.EndHorizontal();
        }

        private void OnAddNewItem(ReorderableList list)
        {
            _stringList.Add("0");
            SetListToToken(_setToken);
        }

        private void SetListToToken(Action<string> setToken)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("{");
            for (int i = 0; i < _stringList.Count; i++)
            {
                stringBuilder.Append(_stringList[i]);
                if (i < _stringList.Count - 1)
                {
                    stringBuilder.Append(',');
                }
            }
            stringBuilder.Append("}");

            setToken(stringBuilder.ToString());
        }
    }
}