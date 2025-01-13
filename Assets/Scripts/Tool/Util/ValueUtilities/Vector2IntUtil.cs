using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector2IntUtil : ValueUtilBase
{
    public override object DefaultValue => default(Vector2Int);

    public override string TypeName => "Vector2Int";
    private static Vector2Int[] fourDir=new Vector2Int[4] { Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right };
    private static Vector2Int[] eightDir= new Vector2Int[8] {
                (Vector2Int.up+Vector2Int.left),
                Vector2Int.up,
                (Vector2Int.up+Vector2Int.right),
                Vector2Int.left,
                Vector2Int.right,
                 (Vector2Int.down+Vector2Int.left),
                Vector2Int.down,
                (Vector2Int.down+Vector2Int.right),
                 };

    public override Type ValueType => typeof(Vector2Int);
    public static Vector2Int[] FourDir
    {
        get
        {
            return fourDir;
        }
    }
    public static Vector2Int[] EightDir
    {
        get
        {
            return eightDir;
        }
    }
}