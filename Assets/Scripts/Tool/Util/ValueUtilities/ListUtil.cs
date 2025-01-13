using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class ListUtil<T> : ValueUtilBase
{
    public override object DefaultValue => new List<T>();

    public override string TypeName => "List<" + ValueUtil.Get<T>().TypeName+">";


    public override Type ValueType => typeof(List<T>);
    public override bool CompareEqual(object a, object b)
    {
        CheckValueType(a);
        CheckValueType(b);
        var aa = a as List<T>;
        var bb = b as List<T>;
        if (aa.Count != bb.Count) return false;
        for(int i = 0; i < aa.Count; i++)
        {
            if (ValueUtil.Get<T>().CompareEqual(aa[i], bb[i]) == false)
            {
                return false;
            }
        }
        return true;


    }
    public override object DeepClone(object v)
    {
        var ans = new List<T>();
        foreach(var x in (List<T>)v)
        {
            ans.Add((T)ValueUtil.Get<T>().DeepClone(x));
        }
        return ans;
    }

    public override void LayoutField(string name, ref object v)
    {
        CheckValueType(v);

        //  base.LayoutField(name, ref v);
        var nv = (List<T>)v;
#if UNITY_EDITOR
        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField(name+"("+ValueUtil.Get<T>().TypeName+"):");

        EditorGUILayout.BeginHorizontal();
        //增加一个组
        if (GUILayout.Button("+"))
        {
            nv.Add((T)ValueUtil.Get<T>().DefaultValue);
        }
        //减少一个组
        if (GUILayout.Button("-"))
        {
            nv.RemoveAt(nv.Count - 1);
        }
        EditorGUILayout.EndHorizontal();

        for(int i = 0; i < nv.Count; i++)
        {
            object vv = nv[i];
            ValueUtil.Get<T>().LayoutField(i.ToString(),ref vv);
            nv[i] = (T)vv;
        }

            EditorGUILayout.EndVertical();
#endif
    }
}

public static class ListExtensions
{
    public delegate bool Condition<T>(T element);

    public static bool Contains<T>(this List<T> list, Condition<T> condition)
    {
        foreach (var element in list)
        {
            if (condition(element))
            {
                return true;
            }
        }
        return false;
    }
}