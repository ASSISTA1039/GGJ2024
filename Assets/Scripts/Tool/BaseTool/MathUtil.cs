using System;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtil
{
    /// <summary>
    /// 二分查找While循环实现
    /// </summary>
    /// <param name="list">数组</param>
    /// <param name="target">要查找的对象</param>
    /// <param name="comp">查找依据</param>
    /// <returns>返回对象</returns>
    public static T BinarySearch<T, T2>(this List<T> list, T2 target, Func<T2, T, int> comp)
    {
        int low, high;
        low = 0;
        high = list.Count - 1;
        while (low <= high)
        {
            int middle = (low + high) / 2;
            var c = comp(target, list[middle]);
            if (c == 0)
            {
                return list[middle];
            }
            else if (c > 0)
            {
                low = middle + 1;
            }
            else if (c < 0)
            {
                high = middle - 1;
            }
        }
        return default(T);
    }

    /// <summary>
    /// 添加后自动排序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="target"></param>
    public static void BinaryAdd<T>(this List<T> list, T target)
    {
        list.Add(target);
        list.Sort();
    }

    /// <summary>
    /// 二分查找While循环实现
    /// </summary>
    /// <param name="list">数组</param>
    /// <param name="low">开始索引</param>
    /// <param name="high">结束索引</param>
    /// <param name="comp">要查找的对象</param>
    /// <returns>返回索引</returns>
    public static int BinarySearchIndex(List<int> list, int target)
    {
        int low, high;
        low = 0;
        high = list.Count - 1;
        while (low <= high)
        {
            int middle = (low + high) / 2;
            var c = target - list[middle];
            if (c == 0)
            {
                return middle;
            }
            else if (c > 0)
            {
                low = middle + 1;
            }
            else if (c < 0)
            {
                high = middle - 1;
            }
        }
        return -1;
    }

    /// <summary>
    /// 二分查找While循环实现
    /// </summary>
    /// <param name="list">数组</param>
    /// <param name="low">开始索引</param>
    /// <param name="high">结束索引</param>
    /// <param name="comp">要查找的对象</param>
    /// <returns>返回索引</returns>
    public static bool BinarySearchContains(this List<int> list, int target)
    {
        return MathUtil.BinarySearchIndex(list, target) >= 0;
    }

    /// <summary>
    /// 二分查找While循环实现
    /// </summary>
    /// <param name="list">数组</param>
    /// <param name="low">开始索引</param>
    /// <param name="high">结束索引</param>
    /// <param name="comp">要查找的对象</param>
    /// <returns>返回索引</returns>
    public static bool BinarySearchRemove(this List<int> list, int target)
    {
        var index = MathUtil.BinarySearchIndex(list, target);
        if (index >= 0)
        {
            list.RemoveAt(index);
            return true;
        }
        return false;
    }

    /// <summary>
    /// string 轉Int
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static int StringToInt(string key)
    {
        return Int32.Parse(key);
    }

    /// <summary>
    /// 线段与圆相交
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="center"></param>
    /// <param name="r"></param>
    /// <returns></returns>
    public static bool CircleIntersect(Vector3 p1, Vector3 p2, Vector3 center, float r)
    {
        float a, b, c, dist1, dist2, angle1, angle2;//ax+by+c=0;
        if (p1.x == p2.x)
        {
            a = 1;
            b = 0;
            c = -p1.x;
        }//特殊情况判断，分母不能为零
        else if (p1.y == p2.y)
        {
            a = 0;
            b = 1;
            c = -p1.y;
        }//特殊情况判断，分母不能为零
        else
        {
            a = p1.y - p2.y;
            b = p2.x - p1.x;
            c = p1.x * p2.y - p1.y * p2.x;
        }
        dist1 = a * center.x + b * center.y + c;
        dist1 *= dist1;
        dist2 = (a * a + b * b) * r * r;
        if (dist1 > dist2)
        {
            return false;
        }//点到直线距离大于半径
        angle1 = (center.x - p1.x) * (p2.x - p1.x) + (center.y - p1.y) * (p2.y - p1.y);
        angle2 = (center.x - p2.x) * (p1.x - p2.x) + (center.y - p2.y) * (p1.y - p2.y);
        if (angle1 > 0 && angle2 > 0)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 求最大值
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static int MaxValue(this List<int> list)
    {
        List<int> orderList = new List<int>(list);
        orderList.Sort((left, right) =>
        {
            if (left > right)
                return 1;
            else if (left == right)
                return 0;
            else
                return -1;
        });
        return orderList[0];
    }

    /// <summary>
    /// 随机排序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ListT"></param>
    /// <returns></returns>
    public static List<T> Shuffle<T>(this List<T> ListT)
    {
        System.Random random = new System.Random();
        List<T> newList = new List<T>();
        foreach (T item in ListT)
        {
            newList.Insert(random.Next(newList.Count + 1), item);
        }
        return newList;
    }

    public static T Last<T>(this List<T> list)
    {
        return list[list.Count - 1];
    }

    #region 随机数

    /// <summary>
    /// 创建一个产生不重复随机数的随机数生成器。
    /// </summary>
    /// <returns>随机数生成器</returns>
    public static System.Random CreateRandom()
    {
        long tick = DateTime.Now.Ticks;
        return new System.Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
    }
    
    public static T RandomGet<T>(this List<T> list)
    {
        if (list.Count == 0)
        {
            return default(T);
        }
        System.Random random = CreateRandom();

        int index = random.Next(0, list.Count);
        return list[index];
    }

    /// <summary>
    /// 在非主线程中也能用
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static T GetRandom<T>(this List<T> list)
    {
        System.Random random = new System.Random();
        return list[random.Next(0, list.Count)];
    }


    #endregion 随机数
}