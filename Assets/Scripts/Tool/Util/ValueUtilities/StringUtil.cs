using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif 
public class StringUtil : ValueUtilBase
{
    public override object DefaultValue => "";

    public override string TypeName => "String";

    public override Type ValueType => typeof(String);

    public override void LayoutField(string name, ref object v)
    {
        CheckValueType(v);
#if UNITY_EDITOR
        v = EditorGUILayout.TextField(name, (string)v);
#endif
    }
}