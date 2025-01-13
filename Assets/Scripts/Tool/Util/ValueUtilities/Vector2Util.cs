using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector2Util : ValueUtilBase
{
    public override object DefaultValue => default(Vector2);

    public override string TypeName => "Vector2";

    public override Type ValueType => typeof(Vector2);
    public override bool CompareEqual(object a, object b)
    {
        CheckValueType(a);
        CheckValueType(b);
        Vector2 aa = (Vector2)a, bb = (Vector2)b;
        return ValueUtil.Get<float>().CompareEqual(aa.x, bb.x) && ValueUtil.Get<float>().CompareEqual(aa.y, bb.y);
    }

    /// <summary>
    /// 旋转一个二维矢量。
    /// </summary>
    /// <param name="vector">被旋转的矢量。</param>
    /// <param name="angle">旋转的角度（角度值）。</param>
    /// <returns>旋转后的矢量。</returns>
    public static Vector2 Rotate(Vector2 vector, float angle)
    {
        float rad = angle * Mathf.PI / 180;
        float x = Mathf.Cos(rad);
        float y = Mathf.Sin(rad);
        return new Vector2(vector.x * x - vector.y * y, vector.x * y + vector.y * x);
    }
    public static Vector2[] FourDir
    {
        get 
        {
            return new Vector2[4] { Vector2.up, Vector2.left, Vector2.down, Vector2.right };
        }
    }
    public static Vector2[] EightDir
    {
        get 
        {
            return new Vector2[8] { 
                Vector2.up,
                (Vector2.up+Vector2.left).normalized,
                Vector2.left,
                 (Vector2.down+Vector2.left).normalized,
                Vector2.down,
                (Vector2.down+Vector2.right).normalized,
                Vector2.right,
                (Vector2.up+Vector2.right).normalized,
                 };
        }
    }


    /// <summary>
    /// from向量在to方向上的投影。
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public static Vector2 Project(Vector2 from, Vector2 to)
    {
        return to.normalized * (Vector2.Dot(from, to.normalized));
    }

    
}

public static class Vector2Extensions
{
    /// <summary>
    /// 旋转一个二维矢量。
    /// </summary>
    /// <param name="vector">被旋转的矢量。</param>
    /// <param name="angle">旋转的角度（角度值）。</param>
    /// <returns>旋转后的矢量。</returns>
    public static Vector2 Rotate(this Vector2 vector, float angle)
    {
        float rad = angle * Mathf.PI / 180;
        float x = Mathf.Cos(rad);
        float y = Mathf.Sin(rad);
        return new Vector2(vector.x * x - vector.y * y, vector.x * y + vector.y * x);
    }

    /// <summary>
    /// from向量在to方向上的投影。
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public static Vector2 Project(this Vector2 from, Vector2 to)
    {
        return to.normalized * (Vector2.Dot(from, to.normalized));
    }
}

public struct Vector2Distict
{
    public Vector2 from;
    public Vector2 to;

    public float Angle
    {
        get
        {
            float angle = Vector2.SignedAngle(from, to);
            if (angle < 0)
            {
                angle += 360;
            }
            return angle;
        }
    }

    public Vector2 Center
    {
        get
        {
            return from.Rotate(Angle / 2);
        }
    }

    public Vector2Distict(Vector2 f, Vector2 t)
    {
        if (f == Vector2.zero || t == Vector2.zero)
        {
            from = to = Vector2.zero;
        }
        else
        {
            from = f.normalized;
            to = t.normalized;
        }
    }

    public Vector2Distict(Vector2 center, float angle)
    {
        from = center.Rotate(-angle).normalized;
        to = center.Rotate(angle).normalized;
    }

    public bool Contains(Vector2 v)
    {
        float a = Vector2.SignedAngle(Vector2.right, v);
        float a1 = Vector2.SignedAngle(Vector2.right, from);
        float a2 = Vector2.SignedAngle(Vector2.right, to);
        a = a > 0 ? a : 360 + a;
        a1 = a1 > 0 ? a1 : 360 + a1;
        a2 = a2 > 0 ? a2 : 360 + a2;
        if (a1 > a2)
        {
            a2 += 360;
        }

        if (a >= a1 && a <= a2)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static Vector2Distict operator -(Vector2Distict a, Vector2Distict b)
    {
        if (a.from == a.to)
        {
            return b;
        }

        if (b.from == b.to)
        {
            return a;
        }

        float[] temp = new float[4];
        temp[0] = Vector2.SignedAngle(Vector2.right, a.from);
        temp[1] = Vector2.SignedAngle(Vector2.right, a.to);
        temp[2] = Vector2.SignedAngle(Vector2.right, b.from);
        temp[3] = Vector2.SignedAngle(Vector2.right, b.to);
        temp[0] = temp[0] > 0 ? temp[0] : 360 + temp[0];
        temp[1] = temp[1] > 0 ? temp[1] : 360 + temp[1];
        temp[2] = temp[2] > 0 ? temp[2] : 360 + temp[2];
        temp[3] = temp[3] > 0 ? temp[3] : 360 + temp[3];
        if (temp[0]> temp[1])
        {
            temp[1] += 360;
        }
        if (temp[2]> temp[3])
        {
            temp[3] += 360;
        }

        List<Tuple<int, float>> list = new List<Tuple<int, float>>
        {
            new Tuple<int, float>(0, temp[0]),
            new Tuple<int, float>(1, temp[1]),
            new Tuple<int, float>(2, temp[2]),
            new Tuple<int, float>(3, temp[3]),
        };

        list.Sort((item1, item2) => { return item1.Item2 > item2.Item2 ? 1 : (item1.Item2 == item2.Item2 ? 0 : -1); });

        if (list[1].Item2 == list[2].Item2 || list[1].Item1 % 2 == 1)
        {
            return new Vector2Distict();
        }
        else if (list[1].Item1 % 2 == 0)
        {
            return new Vector2Distict(Vector2.right.Rotate(temp[list[1].Item1]), Vector2.right.Rotate(temp[list[2].Item1]));
        }
        else
        {
            Debug.LogWarning("你看到了不该看到的东西......");
            return new Vector2Distict();
        }
    }
}