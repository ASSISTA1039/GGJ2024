using System;
using System.Collections.Generic;
using UnityEngine;

namespace ERAnimation
{
    /// <summary>
    /// 动画状态
    /// </summary>
    public sealed class ERAnimationState : ScriptableObject
    {
        public ERAnimatorController linkedController;
        public int layerIndex;
        public string linkedState;
        public float animationTime;
        public List<ERAnimationEvent> events = new List<ERAnimationEvent>();

        public ERAnimationState Init(ERAnimatorController controller, int index, string state, string animation)
        {
            layerIndex = index;
            linkedController = controller;
            linkedState = state;
            return this;
        }
    }

    public class CategoryAttribute : Attribute
    {
        public string path;

        public CategoryAttribute(string _path)
        {
            path = _path;
        }
    }

    public class TreeNode<T>
    {
        public bool foldout;
        public string name { get; private set; }
        public TreeNode<T> parent { get; private set; }
        public List<TreeNode<T>> childs { get; private set; }
        public List<T> datas { get; private set; }

        public TreeNode(string name)
        {
            foldout = false;
            this.name = name;
            parent = null;
            childs = new List<TreeNode<T>>();
            datas = new List<T>();
        }

        public void AddNode(TreeNode<T> node)
        {
            node.parent = this;
            childs.Add(node);
        }

        public void RemoveNode(TreeNode<T> node)
        {
            node.parent = null;
            childs.Remove(node);
        }

        public TreeNode<T> GetNode(string name)
        {
            return childs.Find((n) => { return n.name == name; });
        }

        public void AddData(T data)
        {
            datas.Add(data);
        }

        public void RemoveData(T data)
        {
            datas.Remove(data);
        }
    }
}