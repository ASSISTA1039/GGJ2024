using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QxFramework.Core;
using QxFramework;
using static Unity.VisualScripting.Member;

public class LevelMgr : MonoBehaviour
{
    public static LevelMgr Instance;
    public void Awake()
    {
        Instance = this;
    }

    GameObject MapPart1;
    GameObject MapPart2;
    public Transform CollidersParent;             //方块碰撞体父物体
    Vector3[] ColliderOldPos;                        //所有碰撞体的初始位置
    Transform[] Colliders;                            //所有碰撞体

    public void Start()
    {
        
    }

    /// <summary>
    /// 关卡读取器
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public Transform LoadMap(int _level)
    {
        MapPart1 = Resources.Load<GameObject>("Prefabs/Map/level" + _level.ToString()+ "/FuncBlocks");
        MapPart2 = Resources.Load<GameObject>("Prefabs/Map/level" + _level.ToString()+ "/NoFuncBlocks");
        if (MapPart1 == null || MapPart2 == null)
        {
            Debug.LogError("Failed to load one or more prefabs.");
            return null;
        }
        GameObject instantiatedChild = Instantiate(MapPart1.gameObject);
        GameObject instantiatedChild2 = Instantiate(MapPart2.gameObject);
        //CreatCollider(MapPart1.transform);
        //CreatCollider(MapPart2.transform);

        return CalculateAveragePosition();
            //Merge(instantiatedChild, instantiatedChild2);
    }

    /// <summary>
    /// 遍历场景中所有带有特定标签的物体并计算它们的平均坐标，返回该位置的Transform
    /// </summary>
    /// <returns>所有方块的平均位置的Transform</returns>
    public Transform CalculateAveragePosition()
    {
        // 定义需要遍历的多个标签
        List<string> tagsToCheck = new List<string> { "NormalCube", "Draggable", "Tag3" };//TODO:此处标签需要修改

        // 获取所有场景中的物体
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        // 初始化坐标总和
        Vector3 totalPosition = Vector3.zero;
        int objectCount = 0;

        // 遍历所有物体并检查其Tag
        foreach (GameObject obj in allObjects)
        {
            // 检查物体是否具有 Collider 组件并且其标签在 tagsToCheck 中
            if (obj.GetComponent<Collider>() != null && tagsToCheck.Contains(obj.tag))
            {
                totalPosition += obj.transform.position;
                objectCount++;
            }
        }

        // 计算平均值
        if (objectCount > 0)
        {
            // 创建一个新的空物体并设置其位置为计算出来的平均位置
            GameObject averageObject = new GameObject("AveragePosition");
            averageObject.transform.position = totalPosition / objectCount;

            // 返回该物体的Transform
            return averageObject.transform;
        }
        else
        {
            return null; // 如果没有符合条件的物体，返回null
        }
    }

    #region useless logic
    /// <summary>
    /// 对方法中子物体的合并
    /// </summary>
    /// <param name="source1"></param>
    /// <param name="source2"></param>
    /// <param name="targetParent"></param>
    public Transform Merge(GameObject source1, GameObject source2, GameObject targetParent = null)
    {
        if (targetParent == null)
        {
            targetParent = new GameObject("LevelParent");
        }
       
        // 合并第一个源对象的子物体
        MergeChildObjects(source1, targetParent);
        // 合并第二个源对象的子物体
        MergeChildObjects(source2, targetParent);

        return targetParent.transform;
    }

    // 合并一个源对象的子物体
    private void MergeChildObjects(GameObject source, GameObject targetParent)
    {
        if (source == null) return;

        // 遍历所有子物体
        foreach (Transform child in source.transform)
        {
            // 将子物体的父对象设置为目标父对象
            child.SetParent(targetParent.transform);

            // 如果需要，可以重置子物体的本地位置、旋转和缩放
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
            child.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// 遍历方块的所有子物体及其子物体，在保留父物体结构的基础上，将其碰撞体移动到 CollidersParent 下。
    /// 如果原物体包含某些 tag，则碰撞体也会包含对应的 tag。
    /// 父物体和第一层子物体本身并不需要碰撞体。
    /// </summary>
    /// <param name="Cubes">方块父物体</param>
    public void CreatCollider(Transform Cubes)
    {
        // 初始化数组大小，延迟分配
        List<Vector3> colliderPositions = new List<Vector3>();
        List<Transform> colliderReferences = new List<Transform>();

        // 定义需要检测的 tag 列表
        List<string> tagsToCheck = new List<string> { "Draggable", "Tag2", "Tag3" };

        // 递归遍历子物体并创建碰撞体
        void ProcessChild(Transform child, Transform parentInCollidersParent)
        {
            // 跳过第一层子物体
            foreach (Transform grandChild in child)
            {
                // 创建新的父结构节点
                Transform newParent = parentInCollidersParent.Find(child.name);
                if (newParent == null)
                {
                    newParent = new GameObject(child.name).transform;
                    newParent.parent = parentInCollidersParent;
                }

                // 创建碰撞体
                Transform collider = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                collider.position = grandChild.position;
                collider.localScale = grandChild.localScale; // 保留缩放比例
                collider.gameObject.layer = 8;

                // 检测并复制 tag
                if (tagsToCheck.Contains(grandChild.tag))
                {
                    collider.tag = grandChild.tag;
                }

                collider.parent = newParent; // 将碰撞体作为新的父节点的子物体

                // 添加记录
                colliderPositions.Add(grandChild.position);
                colliderReferences.Add(collider);

                // 递归处理子物体
                foreach (Transform greatGrandChild in grandChild)
                {
                    ProcessChild(greatGrandChild, newParent);
                }
            }
        }

        // 开始处理传入的 Cubes 的所有子物体
        foreach (Transform child in Cubes)
        {
            ProcessChild(child, CollidersParent);
        }

        // 将结果转为数组
        ColliderOldPos = colliderPositions.ToArray();
        Colliders = colliderReferences.ToArray();
    }
    #endregion
}
