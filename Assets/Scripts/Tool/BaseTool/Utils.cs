using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using UnityEngine;
using System.Security.Cryptography;
using System.Net;
using System.Runtime.InteropServices;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// 通用工具类。
/// </summary>
public static class Utils
{
    #region 常量

    /// <summary>
    /// 键值分隔符： ‘:’
    /// </summary>
    private const Char KEY_VALUE_SPRITER = ':';

    /// <summary>
    /// 字典项分隔符： ‘,’
    /// </summary>
    private const Char MAP_SPRITER = ',';

    /// <summary>
    /// 数组分隔符： ','
    /// </summary>
    private const Char LIST_SPRITER = ',';

    public const String RPC_HEAD = "RPC_";

    public const String SVR_RPC_HEAD = "SVR_RPC_";

    #endregion 常量

    #region 时间格式化

    /// <summary>
    /// 格式化日期格式。（yyyy-MM-dd HH:mm:ss）
    /// </summary>
    /// <param name="datetime">日期对象</param>
    /// <returns>日期字符串</returns>
    public static String FormatTime(this DateTime datetime)
    {
        return datetime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// 格式化日期格式。（yyyy-MM-dd HH:mm:ss）
    /// </summary>
    /// <param name="datetime">日期值</param>
    /// <returns>日期字符串</returns>
    public static String FormatTime(this long datetime)
    {
        DateTime.FromBinary(datetime);
        return datetime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// 时间戳转为C#格式时间。
    /// </summary>
    /// <param name="timeStamp">时间戳</param>
    /// <returns></returns>
    public static DateTime GetTime(this int timeStamp)
    {
        DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
        return startTime.AddSeconds(timeStamp);
    }

    #endregion 时间格式化

    #region 字典转换

    /// <summary>
    /// 将字典字符串转换为键类型与值类型都为整型的字典对象。
    /// </summary>
    /// <param name="strMap">字典字符串</param>
    /// <param name="keyValueSpriter">键值分隔符</param>
    /// <param name="mapSpriter">字典项分隔符</param>
    /// <returns>字典对象</returns>
    public static Dictionary<Int32, Int32> ParseMapIntInt(this String strMap, Char keyValueSpriter = KEY_VALUE_SPRITER, Char mapSpriter = MAP_SPRITER)
    {
        Dictionary<Int32, Int32> result = new Dictionary<Int32, Int32>();
        var strResult = ParseMap(strMap, keyValueSpriter, mapSpriter);
        foreach (var item in strResult)
        {
            int key;
            int value;
            if (int.TryParse(item.Key, out key) && int.TryParse(item.Value, out value))
                result.Add(key, value);
            else
                Debug.LogWarning(String.Format("Parse failure: {0}, {1}", item.Key, item.Value));
        }
        return result;
    }

    /// <summary>
    /// 将字典字符串转换为键类型为整型，值类型为单精度浮点数的字典对象。
    /// </summary>
    /// <param name="strMap">字典字符串</param>
    /// <param name="keyValueSpriter">键值分隔符</param>
    /// <param name="mapSpriter">字典项分隔符</param>
    /// <returns>字典对象</returns>
    public static Dictionary<Int32, float> ParseMapIntFloat(this String strMap, Char keyValueSpriter = KEY_VALUE_SPRITER, Char mapSpriter = MAP_SPRITER)
    {
        var result = new Dictionary<Int32, float>();
        var strResult = ParseMap(strMap, keyValueSpriter, mapSpriter);
        foreach (var item in strResult)
        {
            int key;
            float value;
            if (int.TryParse(item.Key, out key) && float.TryParse(item.Value, out value))
                result.Add(key, value);
            else
                Debug.LogWarning(String.Format("Parse failure: {0}, {1}", item.Key, item.Value));
        }
        return result;
    }

    /// <summary>
    /// 将字典字符串转换为键类型为整型，值类型为字符串的字典对象。
    /// </summary>
    /// <param name="strMap">字典字符串</param>
    /// <param name="keyValueSpriter">键值分隔符</param>
    /// <param name="mapSpriter">字典项分隔符</param>
    /// <returns>字典对象</returns>
    public static Dictionary<Int32, String> ParseMapIntString(this String strMap, Char keyValueSpriter = KEY_VALUE_SPRITER, Char mapSpriter = MAP_SPRITER)
    {
        Dictionary<Int32, String> result = new Dictionary<Int32, String>();
        var strResult = ParseMap(strMap, keyValueSpriter, mapSpriter);
        foreach (var item in strResult)
        {
            int key;
            if (int.TryParse(item.Key, out key))
                result.Add(key, item.Value);
            else
                Debug.LogWarning(String.Format("Parse failure: {0}", item.Key));
        }
        return result;
    }

    /// <summary>
    /// 将字典字符串转换为键类型为字符串，值类型为单精度浮点数的字典对象。
    /// </summary>
    /// <param name="strMap">字典字符串</param>
    /// <param name="keyValueSpriter">键值分隔符</param>
    /// <param name="mapSpriter">字典项分隔符</param>
    /// <returns>字典对象</returns>
    public static Dictionary<String, float> ParseMapStringFloat(this String strMap, Char keyValueSpriter = KEY_VALUE_SPRITER, Char mapSpriter = MAP_SPRITER)
    {
        Dictionary<String, float> result = new Dictionary<String, float>();
        var strResult = ParseMap(strMap, keyValueSpriter, mapSpriter);
        foreach (var item in strResult)
        {
            float value;
            if (float.TryParse(item.Value, out value))
                result.Add(item.Key, value);
            else
                Debug.LogWarning(String.Format("Parse failure: {0}", item.Value));
        }
        return result;
    }

    /// <summary>
    /// 将字典字符串转换为键类型为字符串，值类型为整型的字典对象。
    /// </summary>
    /// <param name="strMap">字典字符串</param>
    /// <param name="keyValueSpriter">键值分隔符</param>
    /// <param name="mapSpriter">字典项分隔符</param>
    /// <returns>字典对象</returns>
    public static Dictionary<String, Int32> ParseMapStringInt(this String strMap, Char keyValueSpriter = KEY_VALUE_SPRITER, Char mapSpriter = MAP_SPRITER)
    {
        Dictionary<String, Int32> result = new Dictionary<String, Int32>();
        var strResult = ParseMap(strMap, keyValueSpriter, mapSpriter);
        foreach (var item in strResult)
        {
            int value;
            if (int.TryParse(item.Value, out value))
                result.Add(item.Key, value);
            else
                Debug.LogWarning(String.Format("Parse failure: {0}", item.Value));
        }
        return result;
    }

    /// <summary>
    /// 将字典字符串转换为键类型为 T，值类型为 U 的字典对象。
    /// </summary>
    /// <typeparam name="T">字典Key类型</typeparam>
    /// <typeparam name="U">字典Value类型</typeparam>
    /// <param name="strMap">字典字符串</param>
    /// <param name="keyValueSpriter">键值分隔符</param>
    /// <param name="mapSpriter">字典项分隔符</param>
    /// <returns>字典对象</returns>
    public static Dictionary<T, U> ParseMapAny<T, U>(this String strMap, Char keyValueSpriter = KEY_VALUE_SPRITER, Char mapSpriter = MAP_SPRITER)
    {
        var typeT = typeof(T);
        var typeU = typeof(U);
        var result = new Dictionary<T, U>();
        //先转为字典
        var strResult = ParseMap(strMap, keyValueSpriter, mapSpriter);
        foreach (var item in strResult)
        {
            try
            {
                T key = (T)GetValue(item.Key, typeT);
                U value = (U)GetValue(item.Value, typeU);

                result.Add(key, value);
            }
            catch (Exception)
            {
                Debug.LogWarning(String.Format("Parse failure: {0}, {1}", item.Key, item.Value));
            }
        }
        return result;
    }

    /// <summary>
    /// 将字典字符串转换为键类型与值类型都为字符串的字典对象。
    /// </summary>
    /// <param name="strMap">字典字符串</param>
    /// <param name="keyValueSpriter">键值分隔符</param>
    /// <param name="mapSpriter">字典项分隔符</param>
    /// <returns>字典对象</returns>
    public static Dictionary<String, String> ParseMap(this String strMap, Char keyValueSpriter = KEY_VALUE_SPRITER, Char mapSpriter = MAP_SPRITER)
    {
        Dictionary<String, String> result = new Dictionary<String, String>();
        if (String.IsNullOrEmpty(strMap))
        {
            return result;
        }

        var map = strMap.Split(mapSpriter);//根据字典项分隔符分割字符串，获取键值对字符串
        for (int i = 0; i < map.Length; i++)
        {
            if (String.IsNullOrEmpty(map[i]))
            {
                continue;
            }

            var keyValuePair = map[i].Split(keyValueSpriter);//根据键值分隔符分割键值对字符串
            if (keyValuePair.Length == 2)
            {
                if (!result.ContainsKey(keyValuePair[0]))
                    result.Add(keyValuePair[0], keyValuePair[1]);
                else
                    Debug.LogWarning(String.Format("Key {0} already exist, index {1} of {2}.", keyValuePair[0], i, strMap));
            }
            else
            {
                Debug.LogWarning(String.Format("KeyValuePair are not match: {0}, index {1} of {2}.", map[i], i, strMap));
            }
        }
        return result;
    }

    /// <summary>
    /// 将字典对象转换为字典字符串。
    /// </summary>
    /// <typeparam name="T">字典Key类型</typeparam>
    /// <typeparam name="U">字典Value类型</typeparam>
    /// <param name="map">字典对象</param>
    /// <returns>字典字符串</returns>
    public static String PackMap<T, U>(this IEnumerable<KeyValuePair<T, U>> map, Char keyValueSpriter = KEY_VALUE_SPRITER, Char mapSpriter = MAP_SPRITER)
    {
        if (map.Count() == 0)
            return "";
        else
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in map)
            {
                sb.AppendFormat("{0}{1}{2}{3}", item.Key, keyValueSpriter, item.Value, mapSpriter);
            }
            return sb.ToString().Remove(sb.Length - 1, 1);
        }
    }

    #endregion 字典转换

    #region 列表转换

    /// <summary>
    /// 将列表字符串转换为类型为 T 的列表对象。
    /// </summary>
    /// <typeparam name="T">列表值对象类型</typeparam>
    /// <param name="strList">列表字符串</param>
    /// <param name="listSpriter">数组分隔符</param>
    /// <returns>列表对象</returns>
    public static List<T> ParseListAny<T>(this String strList, Char listSpriter = LIST_SPRITER)
    {
        var type = typeof(T);
        var list = strList.ParseList(listSpriter);
        var result = new List<T>();
        foreach (var item in list)
        {
            result.Add((T)GetValue(item, type));
        }
        return result;
    }

    /// <summary>
    /// 将列表字符串转换为字符串的列表对象。
    /// </summary>
    /// <param name="strList">列表字符串</param>
    /// <param name="listSpriter">数组分隔符</param>
    /// <returns>列表对象</returns>
    public static List<String> ParseList(this String strList, Char listSpriter = LIST_SPRITER)
    {
        var result = new List<String>();
        if (String.IsNullOrEmpty(strList))
            return result;

        var trimString = strList.Trim();
        if (String.IsNullOrEmpty(strList))
        {
            return result;
        }
        var detials = trimString.Split(listSpriter);//.Substring(1, trimString.Length - 2)
        foreach (var item in detials)
        {
            if (!String.IsNullOrEmpty(item))
                result.Add(item.Trim());
        }

        return result;
    }

    /// <summary>
    /// 将列表对象转换为列表字符串。
    /// </summary>
    /// <typeparam name="T">列表值对象类型</typeparam>
    /// <param name="list">列表对象</param>
    /// <param name="listSpriter">列表分隔符</param>
    /// <returns>列表字符串</returns>
    public static String PackList<T>(this List<T> list, Char listSpriter = LIST_SPRITER)
    {
        if (list.Count == 0)
            return "";
        else
        {
            StringBuilder sb = new StringBuilder();
            //sb.Append("[");
            foreach (var item in list)
            {
                sb.AppendFormat("{0}{1}", item, listSpriter);
            }
            sb.Remove(sb.Length - 1, 1);
            //sb.Append("]");

            return sb.ToString();
        }
    }

    public static String PackArray<T>(this T[] array, Char listSpriter = LIST_SPRITER)
    {
        var list = new List<T>();
        list.AddRange(array);
        return PackList(list, listSpriter);
    }

    #endregion 列表转换

    #region 类型转换

    /// <summary>
    /// 将字符串转换为对应类型的值。
    /// </summary>
    /// <param name="value">字符串值内容</param>
    /// <param name="type">值的类型</param>
    /// <returns>对应类型的值</returns>
    public static object GetValue(String value, Type type)
    {
        if (type == null)
            return null;
        else if (type == typeof(string))
            return value;
        else if (type == typeof(Int32))
            return Convert.ToInt32(Convert.ToDouble(value));
        else if (type == typeof(float))
            return float.Parse(value);
        else if (type == typeof(byte))
            return Convert.ToByte(Convert.ToDouble(value));
        else if (type == typeof(sbyte))
            return Convert.ToSByte(Convert.ToDouble(value));
        else if (type == typeof(UInt32))
            return Convert.ToUInt32(Convert.ToDouble(value));
        else if (type == typeof(Int16))
            return Convert.ToInt16(Convert.ToDouble(value));
        else if (type == typeof(Int64))
            return Convert.ToInt64(Convert.ToDouble(value));
        else if (type == typeof(UInt16))
            return Convert.ToUInt16(Convert.ToDouble(value));
        else if (type == typeof(UInt64))
            return Convert.ToUInt64(Convert.ToDouble(value));
        else if (type == typeof(double))
            return double.Parse(value);
        else if (type == typeof(bool))
        {
            if (value == "0")
                return false;
            else if (value == "1")
                return true;
            else
                return bool.Parse(value);
        }
        else if (type.BaseType == typeof(Enum))
            return GetValue(value, Enum.GetUnderlyingType(type));
        else if (type == typeof(Vector3))
        {
            Vector3 result;
            ParseVector3(value, out result);
            return result;
        }
        else if (type == typeof(Quaternion))
        {
            Quaternion result;
            ParseQuaternion(value, out result);
            return result;
        }
        else if (type == typeof(Color))
        {
            Color result;
            ParseColor(value, out result);
            return result;
        }
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            Type[] types = type.GetGenericArguments();
            var map = ParseMap(value);
            var result = type.GetConstructor(Type.EmptyTypes).Invoke(null);
            foreach (var item in map)
            {
                var key = GetValue(item.Key, types[0]);
                var v = GetValue(item.Value, types[1]);
                type.GetMethod("Add").Invoke(result, new object[] { key, v });
            }
            return result;
        }
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            Type t = type.GetGenericArguments()[0];
            var list = ParseList(value);
            var result = type.GetConstructor(Type.EmptyTypes).Invoke(null);
            foreach (var item in list)
            {
                var v = GetValue(item, t);
                type.GetMethod("Add").Invoke(result, new object[] { v });
            }
            return result;
        }
        else
            return null;
    }

    /// <summary>
    /// 将指定格式(255, 255, 255, 255) 转换为 Color
    /// </summary>
    /// <param name="_inputString"></param>
    /// <param name="result"></param>
    /// <returns>返回 true/false 表示是否成功</returns>
    public static bool ParseColor(string _inputString, out Color result)
    {
        string trimString = _inputString.Trim();
        result = Color.clear;
        if (trimString.Length < 9)
        {
            return false;
        }
        //if (trimString[0] != '(' || trimString[trimString.Length - 1] != ')')
        //{
        //    return false;
        //}
        try
        {
            string[] _detail = trimString.Split(LIST_SPRITER);//.Substring(1, trimString.Length - 2)
            if (_detail.Length != 4)
            {
                return false;
            }
            result = new Color(float.Parse(_detail[0]) / 255, float.Parse(_detail[1]) / 255, float.Parse(_detail[2]) / 255, float.Parse(_detail[3]) / 255);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Parse Color error: " + trimString + e.ToString());
            return false;
        }
    }

    /// <summary>
    /// 将指定格式(1.0, 2, 3.4) 转换为 Vector3
    /// </summary>
    /// <param name="_inputString"></param>
    /// <param name="result"></param>
    /// <returns>返回 true/false 表示是否成功</returns>
    public static bool ParseVector3(string _inputString, out Vector3 result)
    {
        string trimString = _inputString.Trim();
        result = new Vector3();
        if (trimString.Length < 7)
        {
            return false;
        }
        //if (trimString[0] != '(' || trimString[trimString.Length - 1] != ')')
        //{
        //    return false;
        //}
        try
        {
            string[] _detail = trimString.Split(LIST_SPRITER);//.Substring(1, trimString.Length - 2)
            if (_detail.Length != 3)
            {
                return false;
            }
            result.x = float.Parse(_detail[0]);
            result.y = float.Parse(_detail[1]);
            result.z = float.Parse(_detail[2]);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Parse Vector3 error: " + trimString + e.ToString());
            return false;
        }
    }

    /// <summary>
    /// 将指定格式(1.0, 2, 3.4) 转换为 Vector3
    /// </summary>
    /// <param name="_inputString"></param>
    /// <param name="result"></param>
    /// <returns>返回 true/false 表示是否成功</returns>
    public static bool ParseQuaternion(string _inputString, out Quaternion result)
    {
        string trimString = _inputString.Trim();
        result = new Quaternion();
        if (trimString.Length < 9)
        {
            return false;
        }
        try
        {
            string[] detail = trimString.Split(LIST_SPRITER);
            if (detail.Length != 4)
            {
                return false;
            }
            result.x = float.Parse(detail[0]);
            result.y = float.Parse(detail[1]);
            result.z = float.Parse(detail[2]);
            result.w = float.Parse(detail[3]);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Parse Quaternion error: " + trimString + e.ToString());
            return false;
        }
    }

    /// <summary>
    /// 替换字符串中的子字符串。
    /// </summary>
    /// <param name="input">原字符串</param>
    /// <param name="oldValue">旧子字符串</param>
    /// <param name="newValue">新子字符串</param>
    /// <param name="count">替换数量</param>
    /// <param name="startAt">从第几个字符开始</param>
    /// <returns>替换后的字符串</returns>
    public static String ReplaceFirst(this string input, string oldValue, string newValue, int startAt = 0)
    {
        int pos = input.IndexOf(oldValue, startAt);
        if (pos < 0)
        {
            return input;
        }
        return string.Concat(input.Substring(0, pos), newValue, input.Substring(pos + oldValue.Length));
    }

    #endregion 类型转换

    #region 文件路径处理

    public static string GetFileNameWithoutExtention(string fileName, char separator = '/')
    {
        var name = GetFileName(fileName, separator);
        return GetFilePathWithoutExtention(name);
    }

    public static string GetFilePathWithoutExtention(string fileName)
    {
        return fileName.Substring(0, fileName.LastIndexOf('.'));
    }

    public static string GetDirectoryName(string fileName)
    {
        return fileName.Substring(0, fileName.LastIndexOf('/'));
    }

    public static string GetFileName(string path, char separator = '/')
    {
        return path.Substring(path.LastIndexOf(separator) + 1);
    }

    public static string PathNormalize(this string str)
    {
        return str.Replace("\\", "/").ToLower();
    }

    #endregion 文件路径处理

    #region MD5

    public static Byte[] CreateMD5(Byte[] data)
    {
        using (var md5 = MD5.Create())
        {
            return md5.ComputeHash(data);
        }
    }

    public static string FormatMD5(Byte[] data)
    {
        return System.BitConverter.ToString(data).Replace("-", "").ToLower();
    }

    /// <summary>
    /// 生成文件的md5(冯委)
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static String BuildFileMd5(String filename)
    {
        String filemd5 = null;
        try
        {
            using (var fileStream = File.OpenRead(filename))
            {
                //UnityEditor.AssetDatabase
                var md5 = MD5.Create();
                var fileMD5Bytes = md5.ComputeHash(fileStream);//计算指定Stream 对象的哈希值
                                                               //fileStream.Close();//流数据比较大，手动卸载
                                                               //fileStream.Dispose();
                                                               //由以连字符分隔的十六进制对构成的String，其中每一对表示value 中对应的元素；例如“F-2C-4A”
                filemd5 = FormatMD5(fileMD5Bytes);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }
        return filemd5;
    }

    #endregion MD5

    #region state

    public static ulong BitSet(ulong data, int nBit)
    {
        if (nBit >= 0 && nBit < sizeof(ulong) * 8)
        {
            data = (uint)data | (uint)(1 << nBit);
        }

        return data;
    }

    public static ulong BitReset(ulong data, int nBit)
    {
        if (nBit >= 0 && nBit < (int)sizeof(ulong) * 8)
        {
            data &= (ulong)(~(1 << nBit));
        };

        return data;
    }

    public static int BitTest(ulong data, int nBit)
    {
        int nRet = 0;
        if (nBit >= 0 && nBit < (int)sizeof(ulong) * 8)
        {
            data &= (ulong)(1 << nBit);
            if (data != 0) nRet = 1;
        }
        return nRet;
    }

    #endregion state

    #region 几何相关

    public static void CircleXYByAngle(float angle, Vector3 O, Vector3 A, out Vector3 rnt)
    {
        float r = Vector3.Distance(O, A);
        //rnt = new Vector3();
        rnt.y = A.y;
        rnt.x = r * (float)Math.Cos(angle) + O.x;
        rnt.z = r * (float)Math.Sin(angle) + O.z;

        //return rnt;
    }

    #endregion 几何相关

    #region 密钥管理

    [DllImport("key", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GetIndexKey(int i);

    [DllImport("key", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GetResKey(int i);

    public static byte[] GetResNumber()
    {
        List<byte> result = new List<byte>();

        for (int i = 0; i < 8; i++)
        {
            result.Add((byte)GetResKey(i));
        }
        return result.ToArray();
    }

    public static byte[] GetIndexNumber()
    {
        List<byte> result = new List<byte>();

        for (int i = 0; i < 8; i++)
        {
            result.Add((byte)GetIndexKey(i));
        }
        return result.ToArray();
    }

    #endregion 密钥管理

    static public string GetFullName(Transform rootTransform, Transform currentTransform)
    {
        string fullName = String.Empty;

        while (currentTransform != rootTransform)
        {
            fullName = currentTransform.name + fullName;

            if (currentTransform.parent != rootTransform)
            {
                fullName = String.Concat('/', fullName);
            }

            currentTransform = currentTransform.parent;
        }

        return fullName;
    }

    /// <summary>
    /// 第一个出现数字的位置
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    static public int IndexOfNumber(this string str)
    {
        for (int index = 0; index < str.Length; index++)
        {
            if (str[index] >= '0' && str[index] <= '9')
            {
                return index;
            }
        }
        return -1;
    }

    /// <summary>
    /// 挂载object在某父上并保持本地坐标、转向、大小不变
    /// </summary>
    /// <param name="child"></param>
    /// <param name="parent"></param>
    public static void MountToSomeObjWithoutPosChange(Transform child, Transform parent)
    {
        Vector3 scale = child.localScale;
        Vector3 position = child.localPosition;
        Vector3 angle = child.localEulerAngles;
        child.parent = parent;
        child.localScale = scale;
        child.localEulerAngles = angle;
        child.localPosition = position;
    }

    public static void AddTrigger(this EventTrigger trigger, EventTriggerType eventTriggerType, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        UnityEngine.EventSystems.EventTrigger.Entry entry = new UnityEngine.EventSystems.EventTrigger.Entry();
        entry.eventID = eventTriggerType;
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }

    public static void SetListener(this UnityEvent events, UnityAction call)
    {
        events.RemoveAllListeners();
        events.AddListener(call);
    }

    public static void SetPositionX(this Transform t, float newX)
    {
        t.position = new Vector3(newX, t.position.y, t.position.z);
    }

    public static void SetPositionY(this Transform t, float newY)
    {
        t.position = new Vector3(t.position.x, newY, t.position.z);
    }

    public static void SetPositionZ(this Transform t, float newZ)
    {
        t.position = new Vector3(t.position.x, t.position.y, newZ);
    }

    public static T UniqueAdd<T>(this List<T> list, T item)
    {
        if (!list.Contains(item))
        {
            list.Add(item);
        }
        return item;
    }

    public static int FindCount<T>(this List<T> list, Func<T, bool> func)
    {
        int count = 0;
        foreach (var item in list)
        {
            if (func(item))
            {
                count++;
            }
        }
        return count;
    }

    public static void IteratorChild(this Transform parent, int count, Action<int, Transform> action)
    {
        for (int i = count; i < parent.childCount; i++)
        {
            parent.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = parent.childCount; i < count; i++)
        {
            GameObject.Instantiate(parent.GetChild(0), parent);
        }
        for (int i = 0; i < count; i++)
        {
            parent.GetChild(i).gameObject.SetActive(true);
            action(i, parent.GetChild(i));
        }
    }
}