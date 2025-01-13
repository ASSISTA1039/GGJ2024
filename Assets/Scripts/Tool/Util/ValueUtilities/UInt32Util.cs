using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UInt32Util : ValueUtilBase
{
    public override object DefaultValue => default(UInt32);

    public override string TypeName => "UInt32";

    public override Type ValueType => typeof(UInt32);

    public override void LayoutField(string name, ref object v)
    {
        object nv = (int)(UInt32)v;
        ValueUtil.Get<int>().LayoutField(name,ref nv);
        v = (UInt32)(int)nv;
    }
}