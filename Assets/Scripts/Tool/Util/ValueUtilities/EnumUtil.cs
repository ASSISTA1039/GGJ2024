using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class EnumUtil<T> : ValueUtilBase where T : Enum

{
    public override object DefaultValue => default(T);

    public override string TypeName => typeof(T).Name;

    public override Type ValueType => typeof(T);
    public override void LayoutField(string name, ref object v)
    {
        CheckValueType(v);
        //   base.LayoutField(name, ref v);
#if UNITY_EDITOR
        v = UnityEditor.EditorGUILayout.EnumPopup(name,(Enum)v);
#endif
    }
}