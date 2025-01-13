using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

public class FloatUtil : ValueUtilBase
{
    readonly static float EPS = 1e-6f;
    public override object DefaultValue => 0.0f;

    public override string TypeName => "float";

    public override Type ValueType => typeof(float);
    public override bool CompareEqual(object a, object b)
    {
        CheckValueType(a);
        CheckValueType(b);
            return Mathf.Abs((float)a - (float)b) < EPS;
    }

}