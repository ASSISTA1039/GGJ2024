using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntUtil : ValueUtilBase
{
    public override object DefaultValue => 0;

    public override string TypeName => "int";

    public override Type ValueType => typeof(int);

}
