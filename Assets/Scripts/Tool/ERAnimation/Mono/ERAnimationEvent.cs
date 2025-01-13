using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ERAnimation
{
    public abstract class ERAnimationEvent : ScriptableObject
    {
        public int line;
        public List<ERAnimationArg> args;
        public float startTime;
        public float endTime;

        /// <summary>
        /// 动画参数
        /// </summary>
        public virtual List<ERAnimationArg> Args => new List<ERAnimationArg>();

        /// <summary>
        /// 是否正在执行
        /// </summary>
        public bool isPlaying;

        /// <summary>
        /// 属于哪个状态机
        /// </summary>
        public ERAnimationState linkedState;
        public ERAnimatorController linkedController => linkedState.linkedController;

        /// <summary>
        /// 对应状态机隶属的AnimatorController在Start的时候调用
        /// </summary>
        public virtual void Init()
        {
            isPlaying = false;
        }

        /// <summary>
        /// 执行到状态时触发。
        /// </summary>
        public virtual void OnStateEnter()
        {

        }

        /// <summary>
        /// 状态执行结束时触发。
        /// </summary>
        public virtual void OnStateLeave()
        {

        }

        /// <summary>
        /// 状态重新循环时触发
        /// </summary>
        public virtual void OnStateReplay()
        {

        }

        /// <summary>
        /// 执行到该事件时触发。
        /// </summary>
        public virtual void OnEnter()
        {
            isPlaying = true;
        }

        /// <summary>
        /// 该事件执行结束时触发。
        /// </summary>
        public virtual void OnLeave()
        {
            isPlaying = false;
        }

        /// <summary>
        /// Update时调用
        /// </summary>
        public virtual void Update()
        {

        }

        /// <summary>
        /// Update时调用
        /// </summary>
        public virtual void LateUpdate()
        {

        }

        /// <summary>
        /// FixedUpdate时调用
        /// </summary>
        public virtual void FixedUpdate()
        {

        }

        /// <summary>
        /// 获取指定名字的动画参数。
        /// </summary>
        /// <param name="name">动画参数的名字</param>
        /// <returns>动画参数</returns>
        public ERAnimationArg GetArg(string name)
        {
            return args.Find((a) => a.argName == name);
        }

        /// <summary>
        /// 获取bool类型，指定名字的动画参数。
        /// </summary>
        /// <param name="name">动画参数的名字</param>
        /// <returns>参数的值</returns>
        public bool GetBool(string name)
        {
            var arg = GetArg(name);
            if (arg != null)
            {
                if (arg.type == typeof(bool).FullName)
                {
                    return GetBoolContent(arg.value);
                }
                Debug.LogError(GetType().Name + "的" + name + "参数类型不是bool！");
                return false;
            }
            Debug.LogError("未找到" + GetType().Name + "的" + name + "参数！");
            return false;
        }

        /// <summary>
        /// 将字符串content的内容转换为bool。
        /// </summary>
        /// <param name="content">待转化的字符串</param>
        /// <returns>转换之后的值</returns>
        public bool GetBoolContent(string content)
        {
            if (content == "true")
            {
                return true;
            }
            else if (content == "false")
            {
                return false;
            }
            else
            {
                Debug.LogError(GetType().Name + "的" + name + "参数不是合法的bool类型！");
                return false;
            }
        }

        /// <summary>
        /// 获取int类型，指定名字的动画参数。
        /// </summary>
        /// <param name="name">动画参数的名字</param>
        /// <returns>参数的值</returns>
        public int GetInt(string name)
        {
            var arg = GetArg(name);
            if (arg != null)
            {
                if (arg.type == typeof(int).FullName)
                {
                    try
                    {
                        return GetIntContent(arg.value);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        return 0;
                    }
                }
                Debug.LogError(GetType().Name + "的" + name + "参数类型不是int！");
                return 0;
            }
            Debug.LogError("未找到" + GetType().Name + "的" + name + "参数！");
            return 0;
        }

        public int GetIntContent(string content)
        {
            return Convert.ToInt32(content);
        }

        /// <summary>
        /// 获取float类型，指定名字的动画参数。
        /// </summary>
        /// <param name="name">动画参数的名字</param>
        /// <returns>参数的值</returns>
        public float GetFloat(string name)
        {
            var arg = GetArg(name);
            if (arg != null)
            {
                if (arg.type == typeof(float).FullName)
                {
                    try
                    {
                        return GetFloatContent(arg.value);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        return 0;
                    }
                }
                Debug.LogError(GetType().Name + "的" + name + "参数类型不是float！");
                return 0;
            }
            Debug.LogError("未找到" + GetType().Name + "的" + name + "参数！");
            return 0;
        }

        public float GetFloatContent(string content)
        {
            return Convert.ToSingle(content);
        }

        /// <summary>
        /// 获取Vector3类型，指定名字的动画参数。
        /// </summary>
        /// <param name="name">动画参数的名字</param>
        /// <returns>参数的值</returns>
        public Vector3 GetVector3(string name)
        {
            var arg = GetArg(name);
            if (arg != null)
            {
                if (arg.type == typeof(Vector3).FullName)
                {
                    try
                    {
                        return GetVector3Content(arg.value);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        return default(Vector3);
                    }
                }
                Debug.LogError(GetType().Name + "的" + name + "参数类型不是Vector3！");
                return default(Vector3);
            }
            Debug.LogError("未找到" + GetType().Name + "的" + name + "参数！");
            return default(Vector3);
        }

        public Vector3 GetVector3Content(string content)
        {
            string[] elements = content.Trim().Replace("(", "").Replace(")", "").Split(',');
            return new Vector3(Convert.ToSingle(elements[0]), Convert.ToSingle(elements[1]), Convert.ToSingle(elements[2]));
        }

        /// <summary>
        /// 获取Vector2类型，指定名字的动画参数。
        /// </summary>
        /// <param name="name">动画参数的名字</param>
        /// <returns>参数的值</returns>
        public Vector2 GetVector2(string name)
        {
            var arg = GetArg(name);
            if (arg != null)
            {
                if (arg.type == typeof(Vector2).FullName)
                {
                    try
                    {
                        return GetVector2Content(arg.value);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        return default(Vector2);
                    }
                }
                Debug.LogError(GetType().Name + "的" + name + "参数类型不是Vector3！");
                return default(Vector2);
            }
            Debug.LogError("未找到" + GetType().Name + "的" + name + "参数！");
            return default(Vector2);
        }

        public Vector2 GetVector2Content(string content)
        {
            string[] elements = content.Trim().Replace("(", "").Replace(")", "").Split(',');
            return new Vector2(Convert.ToSingle(elements[0]), Convert.ToSingle(elements[1]));
        }

        /// <summary>
        /// 获取Enum类型，指定名字的动画参数。
        /// </summary>
        /// <typeparam name="T">Enum的类型</typeparam>
        /// <param name="name">动画参数的名字</param>
        /// <returns>参数的值</returns>
        public T GetEnum<T>(string name) where T : struct
        {
            var arg = GetArg(name);
            if (arg != null)
            {
                if (arg.type == typeof(T).FullName)
                {
                    try
                    {
                        return GetEnumContent<T>(arg.value);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        return default(T);
                    }
                }
                Debug.LogError(GetType().Name + "的" + name + "参数类型不是" + typeof(T).Name);
                return default(T);
            }
            Debug.LogError("未找到" + GetType().Name + "的" + name + "参数！");
            return default(T);
        }

        public T GetEnumContent<T>(string content) where T : struct
        {
            T result;
            if (Enum.TryParse(content, out result))
            {
                return result;
            }
            return result;
        }

        public List<bool> GetBoolList(string name)
        {
            try
            {
                return GetBoolListContent(GetArg(name).value);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return default;
            }
        }

        public List<bool> GetBoolListContent(string content)
        {
            try
            {
                List<bool> result = new List<bool>();
                List<string> values = new List<string>();
                var _value = Regex.Match(content, "(?<=\\[).*(?=\\])").Value;
                Debug.Log(_value);
                if (Regex.IsMatch(_value, "(?<=\\[).*(?=\\])"))
                {
                    var collections = Regex.Matches(_value, "\\[.*?\\](?=,)");
                    for (int i = 0; i < collections.Count; i++)
                    {
                        values.Add(collections[i].Value);
                    }
                }
                else
                {
                    if (Regex.IsMatch(_value, "(?<=\\().*(?=\\))"))
                    {
                        var collections = Regex.Matches(_value, "\\(.*?\\)(?=,)");
                        for (int i = 0; i < collections.Count; i++)
                        {
                            values.Add(collections[i].Value);
                        }
                    }
                    else
                    {
                        var collections = _value.Split(',');
                        for (int i = 0; i < collections.Length - 1; i++)
                        {
                            values.Add(collections[i]);
                        }
                    }
                }
                foreach (string value in values)
                {
                    Debug.Log(value);
                    result.Add(GetBoolContent(value));
                }
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return default;
            }
        }

        public List<int> GetIntList(string name)
        {
            try
            {
                return GetIntListContent(GetArg(name).value);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return default;
            }
        }

        public List<int> GetIntListContent(string content)
        {
            try
            {
                List<int> result = new List<int>();
                List<string> values = new List<string>();
                var _value = Regex.Match(content, "(?<=\\[).*(?=\\])").Value;
                if (Regex.IsMatch(_value, "(?<=\\[).*(?=\\])"))
                {
                    var collections = Regex.Matches(_value, "\\[.*?\\](?=,)");
                    for (int i = 0; i < collections.Count; i++)
                    {
                        values.Add(collections[i].Value);
                    }
                }
                else
                {
                    if (Regex.IsMatch(_value, "(?<=\\().*(?=\\))"))
                    {
                        var collections = Regex.Matches(_value, "\\(.*?\\)(?=,)");
                        for (int i = 0; i < collections.Count; i++)
                        {
                            values.Add(collections[i].Value);
                        }
                    }
                    else
                    {
                        var collections = _value.Split(',');
                        for (int i = 0; i < collections.Length - 1; i++)
                        {
                            values.Add(collections[i]);
                        }
                    }
                }
                foreach (string value in values)
                {
                    result.Add(GetIntContent(value));
                }
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return default;
            }
        }

        public List<float> GetFloatList(string name)
        {
            try
            {
                return GetFloatListContent(GetArg(name).value);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return default;
            }
        }

        public List<float> GetFloatListContent(string content)
        {
            try
            {
                List<float> result = new List<float>();
                List<string> values = new List<string>();
                var _value = Regex.Match(content, "(?<=\\[).*(?=\\])").Value;
                if (Regex.IsMatch(_value, "(?<=\\[).*(?=\\])"))
                {
                    var collections = Regex.Matches(_value, "\\[.*?\\](?=,)");
                    for (int i = 0; i < collections.Count; i++)
                    {
                        values.Add(collections[i].Value);
                    }
                }
                else
                {
                    if (Regex.IsMatch(_value, "(?<=\\().*(?=\\))"))
                    {
                        var collections = Regex.Matches(_value, "\\(.*?\\)(?=,)");
                        for (int i = 0; i < collections.Count; i++)
                        {
                            values.Add(collections[i].Value);
                        }
                    }
                    else
                    {
                        var collections = _value.Split(',');
                        for (int i = 0; i < collections.Length - 1; i++)
                        {
                            values.Add(collections[i]);
                        }
                    }
                }
                foreach (string value in values)
                {
                    result.Add(GetFloatContent(value));
                }
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return default;
            }
        }

        public List<Vector2> GetVector2List(string name)
        {
            try
            {
                return GetVector2ListContent(GetArg(name).value);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return default;
            }
        }

        public List<Vector2> GetVector2ListContent(string content)
        {
            try
            {
                List<Vector2> result = new List<Vector2>();
                List<string> values = new List<string>();
                var _value = Regex.Match(content, "(?<=\\[).*(?=\\])").Value;
                if (Regex.IsMatch(_value, "(?<=\\[).*(?=\\])"))
                {
                    var collections = Regex.Matches(_value, "\\[.*?\\](?=,)");
                    for (int i = 0; i < collections.Count; i++)
                    {
                        values.Add(collections[i].Value);
                    }
                }
                else
                {
                    if (Regex.IsMatch(_value, "(?<=\\().*(?=\\))"))
                    {
                        var collections = Regex.Matches(_value, "\\(.*?\\)(?=,)");
                        for (int i = 0; i < collections.Count; i++)
                        {
                            values.Add(collections[i].Value);
                        }
                    }
                    else
                    {
                        var collections = _value.Split(',');
                        for (int i = 0; i < collections.Length - 1; i++)
                        {
                            values.Add(collections[i]);
                        }
                    }
                }
                foreach (string value in values)
                {
                    result.Add(GetVector2Content(value));
                }
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return default;
            }
        }

        public List<Vector3> GetVector3List(string name)
        {
            try
            {
                return GetVector3ListContent(GetArg(name).value);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return default;
            }
        }

        public List<Vector3> GetVector3ListContent(string content)
        {
            try
            {
                List<Vector3> result = new List<Vector3>();
                List<string> values = new List<string>();
                var _value = Regex.Match(content, "(?<=\\[).*(?=\\])").Value;
                if (Regex.IsMatch(_value, "(?<=\\[).*(?=\\])"))
                {
                    var collections = Regex.Matches(_value, "\\[.*?\\](?=,)");
                    for (int i = 0; i < collections.Count; i++)
                    {
                        values.Add(collections[i].Value);
                    }
                }
                else
                {
                    if (Regex.IsMatch(_value, "(?<=\\().*(?=\\))"))
                    {
                        var collections = Regex.Matches(_value, "\\(.*?\\)(?=,)");
                        for (int i = 0; i < collections.Count; i++)
                        {
                            values.Add(collections[i].Value);
                        }
                    }
                    else
                    {
                        var collections = _value.Split(',');
                        for (int i = 0; i < collections.Length - 1; i++)
                        {
                            values.Add(collections[i]);
                        }
                    }
                }
                foreach (string value in values)
                {
                    result.Add(GetVector3Content(value));
                }
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return default;
            }
        }

        public List<T> GetEnumList<T>(string name) where T : struct
        {
            try
            {
                return GetEnumListContent<T>(GetArg(name).value);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return default;
            }
        }

        public List<T> GetEnumListContent<T>(string content) where T : struct
        {
            try
            {
                List<T> result = new List<T>();
                List<string> values = new List<string>();
                var _value = Regex.Match(content, "(?<=\\[).*(?=\\])").Value;
                if (Regex.IsMatch(_value, "(?<=\\[).*(?=\\])"))
                {
                    var collections = Regex.Matches(_value, "\\[.*?\\](?=,)");
                    for (int i = 0; i < collections.Count; i++)
                    {
                        values.Add(collections[i].Value);
                    }
                }
                else
                {
                    if (Regex.IsMatch(_value, "(?<=\\().*(?=\\))"))
                    {
                        var collections = Regex.Matches(_value, "\\(.*?\\)(?=,)");
                        for (int i = 0; i < collections.Count; i++)
                        {
                            values.Add(collections[i].Value);
                        }
                    }
                    else
                    {
                        var collections = _value.Split(',');
                        for (int i = 0; i < collections.Length - 1; i++)
                        {
                            values.Add(collections[i]);
                        }
                    }
                }
                foreach (string value in values)
                {
                    result.Add(GetEnumContent<T>(value));
                }
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return default;
            }
        }

        public string GetString(string name)
        {
            return GetArg(name).value;
        }

        public List<string> GetStringList(string name)
        {
            try
            {
                return GetStringListContent(GetArg(name).value);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return default;
            }
        }

        public List<string> GetStringListContent(string content)
        {
            try
            {
                List<string> result = new List<string>();
                List<string> values = new List<string>();
                var _value = Regex.Match(content, "(?<=\\[).*(?=\\])").Value;
                if (Regex.IsMatch(_value, "(?<=\\[).*(?=\\])"))
                {
                    var collections = Regex.Matches(_value, "\\[.*?\\](?=,)");
                    for (int i = 0; i < collections.Count; i++)
                    {
                        values.Add(collections[i].Value);
                    }
                }
                else
                {
                    if (Regex.IsMatch(_value, "(?<=\\().*(?=\\))"))
                    {
                        var collections = Regex.Matches(_value, "\\(.*?\\)(?=,)");
                        for (int i = 0; i < collections.Count; i++)
                        {
                            values.Add(collections[i].Value);
                        }
                    }
                    else
                    {
                        var collections = _value.Split(',');
                        for (int i = 0; i < collections.Length - 1; i++)
                        {
                            values.Add(collections[i]);
                        }
                    }
                }
                foreach (string value in values)
                {
                    result.Add(value);
                }
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return default;
            }
        }
    }
}