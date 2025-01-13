using System;
using UnityEngine;

namespace ERAnimation
{
    public sealed class ERAnimationArg : ScriptableObject
    {
        public string argName;
        public string type;
        public string value;

        public ERAnimationArg Init(string name, Type type, string value)
        {
            this.argName = name;
            this.type = type.FullName;
            this.value = value;
            return this;
        }

        public ERAnimationArg Init(string name, string type, string value)
        {
            this.argName = name;
            this.type = type;
            this.value = value;
            return this;
        }
    }
}
