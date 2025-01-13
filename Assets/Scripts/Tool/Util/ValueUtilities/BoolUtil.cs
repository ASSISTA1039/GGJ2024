using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class BoolUtil : ValueUtilBase
{
    public override object DefaultValue => false;

    public override string TypeName => "bool";

    public override Type ValueType => typeof(bool);
    public override void LayoutField(string name, ref object v)
    {
#if UNITY_EDITOR
        v = EditorGUILayout.Toggle(name, (bool)v);
#endif
    }

}