using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector3Util : ValueUtilBase
{
    public override object DefaultValue => default(Vector3);

    public override string TypeName => "Vector3";

    public override Type ValueType => typeof(Vector3);
    public override bool CompareEqual(object a, object b)
    {
        CheckValueType(a);
        CheckValueType(b);
        var aa = (Vector3)a;
        var bb = (Vector3)b;
        return ValueUtil.Get<float>().CompareEqual(aa.x, bb.x)&& ValueUtil.Get<float>().CompareEqual(aa.y, bb.y)&& ValueUtil.Get<float>().CompareEqual(aa.z, bb.z);
    }

}