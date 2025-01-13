using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EventLogicSystem
{
    /// <summary>
    /// 用于标记名称的特性
    /// </summary>
    public class EditorNameAttribute : Attribute
    {
        public string Name;

        public EditorNameAttribute(string name)
        {
            Name = name;
        }

        public static string TryGetName(Type type)
        {
            var attrs = type.GetCustomAttributes(typeof(EditorNameAttribute), false); //as Attribute[];
            if (attrs.Length > 0)
            {
                foreach (var att in attrs)
                {
                    if (att is EditorNameAttribute)
                    {
                        return ((EditorNameAttribute)att).Name;
                    }
                }
            }
            return type.Name;
        }
    }
}