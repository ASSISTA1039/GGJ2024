using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector3IntUtil : ValueUtilBase
{
    public override object DefaultValue => default(Vector3Int);

    public override string TypeName => "Vector3Int";

    public override Type ValueType => typeof(Vector3Int);
}