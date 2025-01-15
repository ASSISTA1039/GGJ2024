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
    public Transform CollidersParent;             //������ײ�常����
    Vector3[] ColliderOldPos;                        //������ײ��ĳ�ʼλ��
    Transform[] Colliders;                            //������ײ��

    public void Start()
    {
        
    }

    /// <summary>
    /// �ؿ���ȡ��
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public Transform LoadMap(int _level)
    {
        MapPart1 = Resources.Load<GameObject>("Prefab/Map/level" + _level.ToString()+ "/FuncBlocks");
        MapPart2 = Resources.Load<GameObject>("Prefab/Map/level" + _level.ToString()+ "/NoFuncBlocks");
        Debug.Log("Prefab/Map/level" + _level.ToString()+ "/NoFuncBlocks.prefab");
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
    /// �������������д����ض���ǩ�����岢�������ǵ�ƽ�����꣬���ظ�λ�õ�Transform
    /// </summary>
    /// <returns>���з����ƽ��λ�õ�Transform</returns>
    public Transform CalculateAveragePosition()
    {
        // ������Ҫ�����Ķ����ǩ
        List<string> tagsToCheck = new List<string> { "NormalCube", "Draggable", "Tag3" };

        // ��ȡ���г����е�����
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        // ��ʼ�������ܺ�
        Vector3 totalPosition = Vector3.zero;
        int objectCount = 0;

        // �����������岢�����Tag
        foreach (GameObject obj in allObjects)
        {
            // ��������Ƿ���� Collider ����������ǩ�� tagsToCheck ��
            if (obj.GetComponent<Collider>() != null && tagsToCheck.Contains(obj.tag))
            {
                totalPosition += obj.transform.position;
                objectCount++;
            }
        }

        // ����ƽ��ֵ
        if (objectCount > 0)
        {
            // ����һ���µĿ����岢������λ��Ϊ���������ƽ��λ��
            GameObject averageObject = new GameObject("AveragePosition");
            averageObject.transform.position = totalPosition / objectCount;

            // ���ظ������Transform
            return averageObject.transform;
        }
        else
        {
            return null; // ���û�з������������壬����null
        }
    }

    #region useless logic
    /// <summary>
    /// �Է�����������ĺϲ�
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
       
        // �ϲ���һ��Դ�����������
        MergeChildObjects(source1, targetParent);
        // �ϲ��ڶ���Դ�����������
        MergeChildObjects(source2, targetParent);

        return targetParent.transform;
    }

    // �ϲ�һ��Դ�����������
    private void MergeChildObjects(GameObject source, GameObject targetParent)
    {
        if (source == null) return;

        // ��������������
        foreach (Transform child in source.transform)
        {
            // ��������ĸ���������ΪĿ�길����
            child.SetParent(targetParent.transform);

            // �����Ҫ����������������ı���λ�á���ת������
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
            child.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// ������������������弰�������壬�ڱ���������ṹ�Ļ����ϣ�������ײ���ƶ��� CollidersParent �¡�
    /// ���ԭ�������ĳЩ tag������ײ��Ҳ�������Ӧ�� tag��
    /// ������͵�һ�������屾������Ҫ��ײ�塣
    /// </summary>
    /// <param name="Cubes">���鸸����</param>
    public void CreatCollider(Transform Cubes)
    {
        // ��ʼ�������С���ӳٷ���
        List<Vector3> colliderPositions = new List<Vector3>();
        List<Transform> colliderReferences = new List<Transform>();

        // ������Ҫ���� tag �б�
        List<string> tagsToCheck = new List<string> { "Draggable", "Tag2", "Tag3" };

        // �ݹ���������岢������ײ��
        void ProcessChild(Transform child, Transform parentInCollidersParent)
        {
            // ������һ��������
            foreach (Transform grandChild in child)
            {
                // �����µĸ��ṹ�ڵ�
                Transform newParent = parentInCollidersParent.Find(child.name);
                if (newParent == null)
                {
                    newParent = new GameObject(child.name).transform;
                    newParent.parent = parentInCollidersParent;
                }

                // ������ײ��
                Transform collider = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                collider.position = grandChild.position;
                collider.localScale = grandChild.localScale; // �������ű���
                collider.gameObject.layer = 8;

                // ��Ⲣ���� tag
                if (tagsToCheck.Contains(grandChild.tag))
                {
                    collider.tag = grandChild.tag;
                }

                collider.parent = newParent; // ����ײ����Ϊ�µĸ��ڵ��������

                // ��Ӽ�¼
                colliderPositions.Add(grandChild.position);
                colliderReferences.Add(collider);

                // �ݹ鴦��������
                foreach (Transform greatGrandChild in grandChild)
                {
                    ProcessChild(greatGrandChild, newParent);
                }
            }
        }

        // ��ʼ������� Cubes ������������
        foreach (Transform child in Cubes)
        {
            ProcessChild(child, CollidersParent);
        }

        // �����תΪ����
        ColliderOldPos = colliderPositions.ToArray();
        Colliders = colliderReferences.ToArray();
    }
    #endregion
}
