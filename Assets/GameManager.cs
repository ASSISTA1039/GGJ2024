using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

//摄像机旋转方向
public enum RotationType
{
    Front,
    Right,
    Back,
    Left,
    Up,
    Third
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    // Start is called before the first frame update
    Vector3 Center;                                  //所有方块的中心点
    Vector3 MinPos;                                  //所有方块最小的位置
    Vector3 MaxPos;                                  //所有方块最大的位置
    Transform[] Colliders;                           //所有碰撞体
    Vector3[] ColliderOldPos;                        //所有碰撞体的初始位置
    public Transform Cube;                          //所有方块的父物体  
    public Transform CollidersParent;                //所有碰撞体的父物体
    public Transform UpPos;                          //斜上方视角碰撞体位置

    Transform CurCamera;                             //主摄像机
    public PlayerCharacter Player;

    public bool isThird;                             //是否进入第三人称视角状态

    [SerializeField]
    private RotationType _rotationType;              //当前的旋转方向
    public RotationType rotationType
    {
        get { return _rotationType; }
        set
        {
            _rotationType = value;
            CameraMove(value);
            ColliderMove(value);
        }
    }
    public void Awake()
    {
        Instance = this;
    }
    public void Start()
    {
        CurCamera = Camera.main.transform;
        Center = GetCenter(Cube);
        CreatCollider(Cube);
        _rotationType = RotationType.Up;
        CameraMove(_rotationType, false);
        //ColliderMove(_rotationType);
    }

    //碰撞体移动

    void ColliderMove(RotationType type)
    {
        for (int i = 0; i < Colliders.Length; i++)
        {
            if (type == RotationType.Front || type == RotationType.Back)
            {
                Colliders[i].position = new Vector3(ColliderOldPos[i].x, ColliderOldPos[i].y, 0);
            }
            else if (type == RotationType.Right || type == RotationType.Left)
            {
                Colliders[i].position = new Vector3(0, ColliderOldPos[i].y, ColliderOldPos[i].z);
            }
            else
            {
                //Colliders[i].position = UpPos.GetChild(i).position;
            }
        }
        Player.FollowCollider();
    }

    /// <summary>
    /// 摄像机移动
    /// </summary>
    /// <param name="type">旋转类型</param>
    /// <param name="isPlay">true表示播放摄像机移动旋转的动画，
    /// false表示使摄像机瞬间移动到指定位置</param>
    void CameraMove(RotationType type, bool isPlay = true)
    {
        if (type == RotationType.Third)
        {
            // 让 CameraController 接管，跳过强制移动
            Camera.main.GetComponent<CameraController>().enabled = true;
            return;
        }

        // 禁用 CameraController 控制
        Camera.main.GetComponent<CameraController>().enabled = false;

        Vector3 endPos = Vector3.zero;          //摄像机的目标位置
        Vector3 endRotation = Vector3.zero;    //摄像机的目标旋转角度

        switch (type)
        {
            case RotationType.Front:
                Camera.main.orthographic = true;
                endRotation = Vector3.zero;
                endPos = new Vector3(Center.x, Center.y, MinPos.z - 4);
                break;
            case RotationType.Right:
                Camera.main.orthographic = true;
                endRotation = new Vector3(0, -90, 0);
                endPos = new Vector3(MaxPos.x + 4, Center.y, Center.z);
                break;
            case RotationType.Back:
                Camera.main.orthographic = true;
                endRotation = new Vector3(0, 180, 0);
                endPos = new Vector3(Center.x, Center.y, MaxPos.z + 4);
                break;
            case RotationType.Left:
                Camera.main.orthographic = true;
                endRotation = new Vector3(0, 90, 0);
                endPos = new Vector3(MinPos.x - 4, Center.y, Center.z);
                break;
            case RotationType.Up:
                Camera.main.orthographic = true;
                endRotation = new Vector3(35, 135, 0);
                endPos = Center - Quaternion.Euler(endRotation) * Vector3.forward * 10;
                break;
        }

        if (!isPlay)
        {
            CurCamera.position = endPos;
            CurCamera.rotation = Quaternion.Euler(endRotation);
        }
        else
        {
            CurCamera.DOMove(endPos, 1);
            CurCamera.DORotate(endRotation, 1);
        }
    }


    //获取到所有方块的中心点
    public Vector3 GetCenter(Transform Cubes)
    {
        MinPos = Cubes.GetChild(0).position;
        for (int i = 1; i < Cubes.childCount; i++)
        {
            var pos = Cubes.GetChild(i).position;
            MaxPos.x = Mathf.Max(MaxPos.x, pos.x);
            MaxPos.y = Mathf.Max(MaxPos.y, pos.y);
            MaxPos.z = Mathf.Max(MaxPos.z, pos.z);
            MinPos.x = Mathf.Min(MinPos.x, pos.x);
            MinPos.y = Mathf.Min(MinPos.y, pos.y);
            MinPos.z = Mathf.Min(MinPos.z, pos.z);
        }
        return (MaxPos + MinPos) / 2;
    }



    //修改RotationType枚举
    public void ChangeRotationType(bool isRight)
    {
        if (rotationType == RotationType.Third || rotationType == RotationType.Up)
        {
            // 阻止更改方向
            return;
        }
        if (rotationType == RotationType.Up)
        {
            rotationType = RotationType.Left;
            return;
        }

        if (isRight)
        {
            if (rotationType == RotationType.Left)
            {
                rotationType = RotationType.Front;
            }
            else
            {
                rotationType++;
            }
        }
        else
        {
            if (rotationType == RotationType.Front)
            {
                rotationType = RotationType.Left;
            }
            else
            {
                rotationType--;
            }

        }
    }

    //修改RotationType枚举为斜上方视角
    public void ChangeTypeToUp()
    {
        rotationType = RotationType.Up;
    }
    ////修改RotationType枚举为斜上方视角
    public void ChangeTypeToThird()
    {
        rotationType = RotationType.Third;
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


}
