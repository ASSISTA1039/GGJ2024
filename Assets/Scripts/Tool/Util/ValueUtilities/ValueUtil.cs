using QxFramework.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ValueUtil
{
    static List<ValueUtilBase> utils;

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
#endif
    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
    //    Debug.Log($"{typeof(ValueUtil)}: Init");
    //获取所有valueutilbase子类的名字，移除泛型
        var lst = new List<string>( TypeUtilities.GetTypeNames(typeof(ValueUtilBase)));
        var list = lst.FindAll(s =>Type.GetType(s).IsGenericType);
        foreach (var x in list) lst.Remove(x);
        list.Remove(typeof(EnumUtil<>).Name);
    //    lst.RemoveAll(list);

        utils = new List<ValueUtilBase>();
        foreach(var x in lst)
        {
            utils.Add(Activator.CreateInstance(Type.GetType(x))as ValueUtilBase);
            foreach(var y in list)
            {
      
              utils.Add(Activator.CreateInstance(Type.GetType(y).MakeGenericType(utils.Last().ValueType)) as ValueUtilBase);
             
            }
        }

    }
    public static ValueUtilBase Get(Type type)
    {
        try
        {
            if (utils.FindAll(s => s.ValueType == type).Count != 1)
            {
                Debug.Log(type);
            }
            if (type.IsEnum)
            {
                if (utils.Find(s => s.ValueType == type) == null)
                {
                    utils.Add(Activator.CreateInstance(typeof(EnumUtil<>).MakeGenericType(type)) as ValueUtilBase);
                } 
            }
            Debug.Assert(utils.FindAll(s => s.ValueType == type).Count == 1);


        }catch(Exception e)
        {
            Debug.LogWarning($"此数据类型{type}不可用"+e);
            return null;
        }
        return utils.Find(s => s.ValueType==type);
    }

    public static ValueUtilBase Get<T>()
    {
        return Get(typeof(T));
    }
    public static ValueUtilBase Get(string typeName)
    {
        return Get(Type.GetType(typeName));
        /*
        try
        {
        //    Debug.Log(typeName);
            Debug.Assert(utils.FindAll(s => s.TypeName == typeName).Count == 1);

        }
        catch (Exception e)
        {
            
            Debug.LogError(e + $"此数据类型{typeName}不可用");
        }
        return utils.Find(s => s.TypeName == typeName);*/
    }

    public static bool EqualsDefault(object v)
    {
        var tp = Get(v.GetType());
        return tp.CompareEqual(v, tp.DefaultValue);
    }


}


public abstract class ValueUtilBase
{
    public abstract object DefaultValue { get; }
    /// <summary>
    /// 检查数据类型
    /// </summary>
    /// <param name="v"></param>
    protected void CheckValueType(object v)
    {
        if (v is null) throw new NullReferenceException();
        else if (v.GetType() != ValueType) throw new Exception();
    }
   /// <summary>
   /// 比较值的大小
   /// </summary>
   /// <param name="a"></param>
   /// <param name="b"></param>
   /// <returns></returns>
    public virtual bool CompareEqual(object a, object b)
    {
        CheckValueType(a);
        CheckValueType(b);
       // Debug.Log(a + " " + b + " " + (a .Equals( b)));
        return a.Equals(b);
    }

    public abstract string TypeName { get; }


    public abstract Type ValueType { get; }

    public virtual void LayoutField(string name,ref object v)
    {
        CheckValueType(v);
#if UNITY_EDITOR
      //  EditorGUILayout.IntField()

        var flag = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Static;
        var methods = typeof(EditorGUILayout).GetMethods(flag);
        foreach(var x in methods)
        {

            if (x.Name.ToLower() == (TypeName + "Field").ToLower())
            {
                if (x.GetParameters()[0].Name == "label"&&x.GetParameters().Length==3&&x.GetParameters()[2].Name=="options"&&x.GetParameters()[0].ParameterType==typeof(string))
                {
         //          Debug.Log(x+" "+ x.GetParameters()[0].Name+" "+ x.GetParameters()[1].Name);
                    v = x.Invoke(null, new object[] { name+"("+TypeName+")",v,null});
                    return;
                }
            }
        }
        throw new NotImplementedException(name+"|"+v);
   //     v =typeof(EditorGUILayout).GetMethod(TypeName + "Field",flag).Invoke(null, new object[] {name, v });
#endif 
    }
    public virtual object DeepClone(object v)
    {
        return v;
    }

}

public static class Extensions
{
    public static Transform FindParent(this Transform transform)
    {
        return transform.parent == null ? transform : FindParent(transform.parent);
    }
}