using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// 存储所有游戏相关可存档数据
/// 因为集中数据所以方便做Debug和调试功能
/// 并且也方便做数据更新时通知
/// </summary>
[Serializable]
public class GameDataContainer
{
    /// <summary>
    /// key和类型名字字典
    /// </summary>
    [SerializeField]
    private readonly Dictionary<string, Dictionary<Type, SavedGameData>> _objDics
        = new Dictionary<string, Dictionary<Type, SavedGameData>>();

    [Serializable]
    public class SavedGameData
    {
        [SerializeField]
        public GameDataBase Data = null;
        public List<Action<GameDataBase>> Listeners = new List<Action<GameDataBase>>();
    }

    /// <summary>
    /// 取得游戏数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T Get<T>(string key = "Default") where T : GameDataBase, new()
    {
        var saved = GetOrInitSaved<T>(key);
        return (T)saved.Data;
    }

    /// <summary>
    /// 初始化游戏数据
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="data">引用的数据</param>
    /// <param name="key">名称</param>
    /// <returns>是否已经成功赋值</returns>
    public bool InitData<T>(out T data, string key = "Default") where T : GameDataBase, new()
    {
        //已经存在的Init类型
        var saved = GetOrInitSaved<T>(key); 

        if (saved.Data == null)
        {
            saved.Data = new T();
            data = (T)saved.Data;
            return false;
        }
        data = (T)saved.Data;
        return true;
    }

    /// <summary>
    /// 设置被修改
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="t">修改的对象</param>
    /// <param name="modifier">修改者</param>
    /// <param name="key">字符串</param>
    public void SetModify<T>(T t, object modifier, string key) where T : GameDataBase, new()
    {
        var saved = GetOrInitSaved<T>(key);
        foreach (var action in saved.Listeners)
        {
            try
            {
                action(t);
            }
            catch (Exception e)
            {
                Debug.LogError($"[{nameof(GameDataContainer)}: 执行修改消息异常 : {e}");
            }
        }
    }

    /// <summary>
    /// 注册更新监听
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    /// <param name="key"></param>
    public void RegisterUpdateListener<T>(Action<GameDataBase> action, string key = "Default") where T : GameDataBase, new()
    {
        //已经存在的Init类型
        var saved = GetOrInitSaved<T>(key);

        saved.Listeners.UniqueAdd(action);
    }

    /// <summary>
    /// 消除有关某个对象的所有监听
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="obj">添加过监听的对象</param>
    public void RemoveAbout(object obj)
    {
        foreach (var dic in _objDics)
        {
            foreach (var pair in dic.Value)
            {
                pair.Value.Listeners.RemoveAll((action) => action.Target == obj);
            }
        }
    }

    /// <summary>
    /// 取得或者初始化游戏数据存储类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    private SavedGameData GetOrInitSaved<T>(string key = "Default") where T : GameDataBase, new()
    {
        if (_objDics.TryGetValue(key, out var dic))
        {
            if (dic.TryGetValue(typeof(T), out var value))
            {
                return value;
            }
        }
        else
        {
            dic = new Dictionary<Type, SavedGameData>();
            _objDics[key] = dic;
        }
        var saved = new SavedGameData();
        dic[typeof(T)] = saved;
        return saved;
    }

    /// <summary>
    /// 清空
    /// </summary>
    public void Clear()
    {
        _objDics.Clear();
    }

    [Serializable]
   public class GameDataPair
    {
        [SerializeField]
        public string Key;

        [SerializeField]
        public List<GameDataBase> Value;

        public GameDataPair(string key, List<GameDataBase> value)
        {
            Key = key;
            Value = value;
        }
    }

    public string ToSaveJson()
    {
        List<GameDataPair> list = new List<GameDataPair>();
        foreach (var pair in _objDics)
        {
            var kyPair = new GameDataPair(pair.Key, new List<GameDataBase>());

            list.Add(kyPair);
            foreach (var savedGame in pair.Value)
            {
                {
                    kyPair.Value.Add(savedGame.Value.Data);
                }
            }
        }

        var json = JsonUtil.Serialize(list);
        return json;
    }

    public void FromSaveJson(string json)
    {
        var list  = JsonUtil.Deserialize<List<GameDataPair>>(json);
        _objDics.Clear();
        foreach (var item in list)
        {
            Dictionary<Type, SavedGameData> dic;
            if (!_objDics.TryGetValue(item.Key, out dic))
            {
                dic = new Dictionary<Type, SavedGameData>();
                _objDics.Add(item.Key,dic);
            }
            foreach (var data in item.Value)
            {
                dic[data.GetType()] = new SavedGameData();
                dic[data.GetType()].Data = data;
                Debug.Log(data.GetType());
            }
        }
    }

    public Dictionary<string, Dictionary<Type, SavedGameData>> GetAll()
    {
        return _objDics;
    }
}

[XLua.LuaCallCSharp]
[Serializable]
public class GameDataBase
{
}

internal class SerializedGameData
{
    public string Key;
    public string TypeName;
    public string SerializedData;
}