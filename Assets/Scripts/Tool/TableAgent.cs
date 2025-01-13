using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// 配置表代理
/// </summary>
public class TableAgent
{
    private Dictionary<string, Dictionary<TableKey, string>> _tableAgent;

    private Dictionary<string, List<string>> _keys;
    private Dictionary<string, List<string>> _key2s;

    //缓存，减少GC
    private Dictionary<TableItemKey, int> _ints = new Dictionary<TableItemKey, int>();

    private Dictionary<TableItemKey, float> _floats = new Dictionary<TableItemKey, float>();
    private Dictionary<TableItemKey, string[]> _strings = new Dictionary<TableItemKey, string[]>();
    private readonly string[] _screen_tables={"Base","Plot","City","DangerArea","InitWareHouse"};
    private string CheckTable(string name){
        //for(int i=0;i<_screen_tables.Length;i++){
        //    if(name==_screen_tables[i]){
        //        name+="_"+Data.Instance._nowScreen;
        //    }
        //}
        return name;
    }

    /// <summary>
    /// 加载多个CSV文件
    /// </summary>
    /// <param name="tables">The tables.</param>
    public void Add(List<string> tables)
    {
        for (int i = 0; i < tables.Count; i++)
        {
            Add(tables[i]);
        }
    }

    public void Add(string tableStr)
    {
        var table = CSV.Decode(tableStr);
        string name = table[0][0];
        if (_tableAgent == null)
        {
            _tableAgent = new Dictionary<string, Dictionary<TableKey, string>>();
            _keys = new Dictionary<string, List<string>>();
            _key2s = new Dictionary<string, List<string>>();
        }

        if (!_tableAgent.ContainsKey(name))
        {
           
            _tableAgent[name] = new Dictionary<TableKey, string>();
            _keys[name] = new List<string>();
            _key2s[name] = new List<string>();

         }

        try
        {
            for (int k = 1; k < table[0].Count; k++)
            {
                _key2s[name].Add(table[0][k]);
            }

            for (int j = 1; j < table.Count; j++)
            {
                if (!_keys[name].Contains(table[j][0]))
                {
                    _keys[name].Add(table[j][0]);
                }
              
                for (int k = 1; k < table[j].Count && j > 0; k++)
                {
                    _tableAgent[name][new TableKey(table[j][0].Trim(), table[0][k].Trim())] = table[j][k].Trim();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Table Error ->[" + name + "]" + e);
            throw;
        }
    }

    /// <summary>
    /// 添加单个条目
    /// </summary>
    /// <typeparam name="T">添加的值类型</typeparam>
    /// <param name="name">表名称.</param>
    /// <param name="key1">键1.</param>
    /// <param name="key2">键2</param>
    /// <param name="value">值</param>
    public void Add<T>(string name, string key1, string key2, T value)
    {
        if (_tableAgent == null)
        {
            _tableAgent = new Dictionary<string, Dictionary<TableKey, string>>();
            _keys = new Dictionary<string, List<string>>();
            _key2s = new Dictionary<string, List<string>>();
        }
        if (!_tableAgent.ContainsKey(name))
        {
            _tableAgent[name] = new Dictionary<TableKey, string>();
            _keys[name] = new List<string>();
            _key2s[name] = new List<string>();

        }
        if (!_keys[name].Contains(key1))
        {
            _keys[name].Add(key1);
        }
        if (!_key2s[name].Contains(key2))
        {
            _key2s[name].Add(key2);
        }

        _tableAgent[name][new TableKey(key1, key2)] = value.ToString();
    }

    /// <summary>
    /// 获取字符串值
    /// </summary>
    /// <param name="name">The index.</param>
    /// <param name="key1">The key1.</param>
    /// <param name="key2">The key2.</param>
    /// <returns></returns>
    public string GetString(string name, string key1, string key2)
    {
        name=CheckTable(name);
        if (!_tableAgent.ContainsKey(name))
        {
            Debug.LogError("Cant Find Table->[" + name + "]");
            //为了获取未知表长的内容数量加了个这个
            return "";
        }
        var key = new TableKey(key1.Trim(), key2.Trim());
        if (!_tableAgent[name].ContainsKey(key))
        {
           // Debug.LogError("Table[" + name + "] Cant Find Key->[" + key1 + "," + key2 + "]");
            return "";
        }
        var dic = _tableAgent[name];
        return dic[key];
    }

    /// <summary>
    /// 获取某个表的长度
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public int GetDicCount(string name)
    {
        name=CheckTable(name);
        List<string> key1 = new List<string>();
        key1.Add("zhanwei");
        var dic = _tableAgent[name].Keys;
        foreach (var key in dic)
        {
            if (!key1.Contains(key.Key1))
            {
                key1.Add(key.Key1);
            }
        }
        return key1.Count - 1;
    }

    /// <summary>
    /// 获取浮点值
    /// </summary>
    /// <param name="name">The index.</param>
    /// <param name="key1">The key1.</param>
    /// <param name="key2">The key2.</param>
    /// <returns></returns>
    public float GetFloat(string name, string key1, string key2)
    {
        name=CheckTable(name);
        float output = 0f;

        if (_floats.TryGetValue(new TableItemKey(name, key1, key2), out output))
        {
            return output;
        }

        if (!_tableAgent.ContainsKey(name))
        {
            Debug.LogError("Cant Find Table->[" + name + "]");
        }
        if (!_tableAgent[name].ContainsKey(new TableKey(key1, key2)))
        {
            Debug.LogError("Table[" + name + "] Cant Find Key->[" + key1 + "," + key2 + "]");
        }
        try
        {
            output = float.Parse(_tableAgent[name][new TableKey(key1, key2)]);
            _floats[new TableItemKey(name, key1, key2)] = output;
            return output;
        }
        catch (Exception)
        {
            Debug.LogError("Table[" + name + "] Parse Float Error Key->[" + key1 + "," + key2 + "]");
            throw;
        }
    }

    /// <summary>
    /// 获取整数值
    /// </summary>
    /// <param name="name">The index.</param>
    /// <param name="key1">The key1.</param>
    /// <param name="key2">The key2.</param>
    /// <returns></returns>
    public int GetInt(string name, string key1, string key2)
    {
        name=CheckTable(name);
        int output = 0;

        if (_ints.TryGetValue(new TableItemKey(name, key1, key2), out output))
        {
            return output;
        }
        
        if (!_tableAgent.ContainsKey(name))
        {
            Debug.LogError("Cant Find Table->[" + name + "]");
        }
        if (!_tableAgent[name].ContainsKey(new TableKey(key1, key2)))
        {
            Debug.LogError("Table[" + name + "] Cant Find Key->[" + key1 + "," + key2 + "]");
        }
        try
        {
            output = int.Parse(_tableAgent[name][new TableKey(key1, key2)]);
            _ints[new TableItemKey(name, key1, key2)] = output;
            return output;
        }
        catch (Exception)
        {
            Debug.LogError("Table[" + name + "] Parse Int Error Key->[" + key1 + "," + key2 + "]");
            throw;
        }
    }

    /// <summary>
    /// 获取分隔字符串
    /// </summary>
    /// <param name="name">The index.</param>
    /// <param name="key1">The key1.</param>
    /// <param name="key2">The key2.</param>
    /// <returns></returns>
    public string[] GetStrings(string name, string key1, string key2)
    {
        name=CheckTable(name);
        string[] output;

        if (_strings.TryGetValue(new TableItemKey(name, key1, key2), out output))
        {
            return output;
        }
        if (!_tableAgent.ContainsKey(name))
        {
            Debug.LogError("Cant Find Table->[" + name + "]");
        }
        if (!_tableAgent[name].ContainsKey(new TableKey(key1, key2)))
        {
            Debug.LogError("Table[" + name + "] Cant Find Key->[" + key1 + "," + key2 + "]");
        }
        string strs = (_tableAgent[name][new TableKey(key1, key2)]);
        if (strs.Contains('|'))
        {
            output = strs.Split('|');
        }
        else
        {
            output = new[] { strs };
        }
        _strings[new TableItemKey(name, key1, key2)] = output;

        return output;
    }

    /// <summary>
    /// 获取所有Key1值
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public List<string> CollectKey1(string name)
    {
        name=CheckTable(name);
        return _keys[name];
    }


    /// <summary>
    /// 获取所有Key2值
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public List<string> CollectKey2(string name)
    {
        name=CheckTable(name);
        return _key2s[name];
    }

    /// <summary>
    /// 改变带<<>>的字符.
    /// </summary>
    /// <param name="des">原字符串</param>
    /// <param name="trs">转换函数</param>
    /// <returns>转换后的字符串</returns>
    public static string TransformString(string des, Func<string, string> trs)
    {        
        //第一个不是<<>>
        List<string> secList = SecStrings(des);
        if (secList == null)
        {
            return des;
        }
        //替换所有下标为单数的.
        for (int i = 1; i < secList.Count; i += 2)
        {
            secList[i] = trs(secList[i]);
        }

        string value = "";
        for (int i = 0; i < secList.Count; i++)
        {
            value += secList[i];
        }
        return value;
    }
    /// <summary>
    /// 重载一种带ID的
    /// </summary>
    /// <param name="des"></param>
    /// <param name="id"></param>
    /// <param name="trs"></param>
    /// <returns></returns>
    public static string TransformString(string des, int id, Func<int,string, string> trs)
    {
        //第一个不是<<>>
        List<string> secList = SecStrings(des);
        if (secList == null)
        {
            return des;
        }
        //替换所有下标为单数的.
        for (int i = 1; i < secList.Count; i += 2)
        {
            secList[i] = trs(id,secList[i]);
        }

        string value = "";
        for (int i = 0; i < secList.Count; i++)
        {
            value += secList[i];
        }
        return value;
    }

    /// <summary>
    /// 将带<<>>的分段.
    /// </summary>
    /// <param name="des"></param>
    /// <returns>分段</returns>
    private static List<string> SecStrings(string des)
    {
        List<string> secList = new List<string>();
        Match match = Regex.Match(des, "<<([^>])+>>");
        if (!match.Success)
        {
            return null;
        }
        string str1 = des.Substring(0, match.Index);//第一段.
        string str3 = des.Substring(match.Index + match.Length);//第三段
        string str2 = des.Substring(match.Index + 2, match.Length - 4); //第二段
        secList.Add(str1);
        secList.Add(str2);
        List<string> subList = SecStrings(str3);
        if (subList != null)
        {
            for (int i = 0; i < subList.Count; i++)
            {
                secList.Add(subList[i]);
            }
        }
        else
        {
            if (str3 != "")
            {
                secList.Add(str3);
            }
        }
        return secList;
    }

    /// <summary>
    /// 清除操作，用于测试
    /// </summary>
    public void Clear()
    {
        _tableAgent.Clear();
    }
}

/// <summary>
/// 表格键值对结构
/// </summary>
public struct TableKey
{
    public string Key1;
    public string Key2;

    public TableKey(string key1, string key2)
    {
        Key1 = key1;
        Key2 = key2;
    }
}

/// <summary>
/// 表格键值对结构
/// </summary>
public struct TableItemKey
{
    public string TableName;
    public string Key1;

    public string Key2;

    public TableItemKey(string table, string key1, string key2)
    {
        TableName = table;
        Key1 = key1;
        Key2 = key2;
    }
}