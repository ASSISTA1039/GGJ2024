using System.Collections.Generic;
using UnityEngine;

namespace ERAnimation
{
    public class AnimationEventExample : ERAnimationEvent
    {
        public enum TestAnimationEventEnum
        {
            野,
            兽,
            先,
            辈,
            suki,
        }
        public override List<ERAnimationArg> Args => new List<ERAnimationArg>()
        {
            ERAnimationArg.CreateInstance<ERAnimationArg>().Init("Bool", typeof(bool), "false"),
            ERAnimationArg.CreateInstance<ERAnimationArg>().Init("Int", typeof(int), "114514"),
            ERAnimationArg.CreateInstance<ERAnimationArg>().Init("Float", typeof(float), "1919.810"),
            ERAnimationArg.CreateInstance<ERAnimationArg>().Init("Vector2", typeof(Vector2), "(114, 514)"),
            ERAnimationArg.CreateInstance<ERAnimationArg>().Init("Vector3", typeof(Vector3), "(19, 19, 810)"),
            ERAnimationArg.CreateInstance<ERAnimationArg>().Init("Enum", typeof(TestAnimationEventEnum), "value1"),
            ERAnimationArg.CreateInstance<ERAnimationArg>().Init("BoolList", typeof(List<bool>), ""),
            ERAnimationArg.CreateInstance<ERAnimationArg>().Init("IntList", typeof(List<int>), ""),
            ERAnimationArg.CreateInstance<ERAnimationArg>().Init("FloatList", typeof(List<float>), ""),
            ERAnimationArg.CreateInstance<ERAnimationArg>().Init("Vector2List", typeof(List<Vector2>), ""),
            ERAnimationArg.CreateInstance<ERAnimationArg>().Init("Vector3List", typeof(List<Vector3>), ""),
            ERAnimationArg.CreateInstance<ERAnimationArg>().Init("EnumList", typeof(List<TestAnimationEventEnum>), ""),
            ERAnimationArg.CreateInstance<ERAnimationArg>().Init("StringList", typeof(List<string>), ""),
            ERAnimationArg.CreateInstance<ERAnimationArg>().Init("ListList", typeof(List<List<string>>), ""),
            ERAnimationArg.CreateInstance<ERAnimationArg>().Init("Other", typeof(string), "哼，哼，哼，啊啊啊啊啊啊啊啊啊啊啊啊啊啊啊"),
        };

        public override void OnEnter()
        {
            base.OnEnter();
            Debug.Log(GetBool("Bool"));
            Debug.Log(GetInt("Int"));
            Debug.Log(GetFloat("Float"));
            Debug.Log(GetVector2("Vector2"));
            Debug.Log(GetVector3("Vector3"));
            Debug.Log(GetEnum<TestAnimationEventEnum>("Enum"));
            foreach (var item in GetBoolList("BoolList"))
            {
                Debug.Log(item);
            }
            foreach (var item in GetIntList("IntList"))
            {
                Debug.Log(item);
            }
            foreach (var item in GetFloatList("FloatList"))
            {
                Debug.Log(item);
            }
            foreach (var item in GetVector2List("Vector2List"))
            {
                Debug.Log(item);
            }
            foreach (var item in GetVector3List("Vector3List"))
            {
                Debug.Log(item);
            }
            foreach (var item in GetEnumList<TestAnimationEventEnum>("EnumList"))
            {
                Debug.Log(item);
            }
            foreach (var item in GetStringList("StringList"))
            {
                Debug.Log(item);
            }
            foreach (var item1 in GetStringList("ListList"))
            {
                foreach (var item2 in GetStringListContent(item1))
                {
                    Debug.Log(item2);
                }
            }
            Debug.Log(GetArg("Other").value);
            Debug.Log("Enter Event");
        }

        public override void Update()
        {
            base.Update();
        }

        public override void OnLeave()
        {
            base.OnLeave();
            Debug.Log("Leave Event");
        }
    }
}
