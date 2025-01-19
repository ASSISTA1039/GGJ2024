using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static Cinemachine.DocumentationSortingAttribute;
using QxFramework.Core;

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
   
    Vector3[] ColliderOldPos;                        //所有碰撞体的初始位置
    private Transform Cube;                          //所有方块的父物体  
    public Transform CollidersParent;                //所有碰撞体的父物体
    public Transform UpPos;                          //斜上方视角碰撞体位置

    Transform CurCamera;                             //主摄像机
    public PlayerCharacter Player;
    public float Time1;

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
            //ColliderMove(value);

        }
    }

    public void Awake()
    {
        Instance = this;
    }
    public void Start()
    {
        CurCamera = Camera.main.transform;
        Center = new Vector3(0, 0, 0);
        _rotationType = RotationType.Up;
        CameraMove(_rotationType, false);
        GameMgr.Get<IDataManager>().ChangePlayerLevel(0);//第0关
    }

    //Init 方法，用于关卡的加载和一些基本信息的同步
    //放在这里是因为camrea的逻辑也在这里。
    public void GameStart(int level)
    {
        Center= LevelMgr.Instance.LoadMap(level);//生成 方块地图 prefab
        //Center = new Vector3(Cube.position.x, Cube.position.y, Cube.position.z);//确定相机位置
        _rotationType = RotationType.Up;//更改视角
        CreatePlayer();//创建玩家
        
        if (QXData.Instance.Get<PlayerData>().PLevel == 0)
        {
            Center = new Vector3(-8.5f, 9.6f, 11.2f);
        }
        if (QXData.Instance.Get<PlayerData>().PLevel == 1)
        {
            Center = new Vector3(-10.5f, 25.9f, 15.2f);
        }
        if (QXData.Instance.Get<PlayerData>().PLevel == 2)
        {
            Center = new Vector3(-19.3f, 33.4f, 22.3f);
        }
        CameraMove(_rotationType, true);
        //ColliderMove(_rotationType);
    }

    public void Update()
    {
        Time1 = Time.timeScale;
        if (Input.GetMouseButton(0))
        {
            AudioManager.Instance.PlayEffect("Click");
        }
    }

    //碰撞体移动

    //void ColliderMove(RotationType type)
    //{
    //    for (int i = 0; i < Colliders.Length; i++)
    //    {
    //        if (type == RotationType.Front || type == RotationType.Back)
    //        {
    //            Colliders[i].position = new Vector3(ColliderOldPos[i].x, ColliderOldPos[i].y, 0);
    //        }
    //        else if (type == RotationType.Right || type == RotationType.Left)
    //        {
    //            Colliders[i].position = new Vector3(0, ColliderOldPos[i].y, ColliderOldPos[i].z);
    //        }
    //        else
    //        {
    //            //Colliders[i].position = UpPos.GetChild(i).position;
    //        }
    //    }
    //    Player.FollowCollider();
    //}
    #region 相机逻辑
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
            //case RotationType.Front:
            //    Camera.main.orthographic = true;
            //    endRotation = Vector3.zero;
            //    endPos = new Vector3(Center.x, Center.y, MinPos.z - 4);
            //    break;
            //case RotationType.Right:
            //    Camera.main.orthographic = true;
            //    endRotation = new Vector3(0, -90, 0);
            //    endPos = new Vector3(MaxPos.x + 4, Center.y, Center.z);
            //    break;
            //case RotationType.Back:
            //    Camera.main.orthographic = true;
            //    endRotation = new Vector3(0, 180, 0);
            //    endPos = new Vector3(Center.x, Center.y, MaxPos.z + 4);
            //    break;
            //case RotationType.Left:
            //    Camera.main.orthographic = true;
            //    endRotation = new Vector3(0, 90, 0);
            //    endPos = new Vector3(MinPos.x - 4, Center.y, Center.z);
            //    break;
            case RotationType.Up:
                Camera.main.orthographic = true;
                endRotation = new Vector3(35, 135, 0);
                endPos = Center;
                break;
        }

        if (!isPlay)
        {
            CurCamera.position = endPos;
            CurCamera.rotation = Quaternion.Euler(endRotation);
            // 直接设置 Timescale 为 0
            //if(rotationType==RotationType.Up)
                //Time.timeScale = 0;
        }
        else
        {
            CurCamera.DOMove(endPos, 1).OnKill(() => OnCameraMoveComplete()); // 添加回调
            CurCamera.DORotate(endRotation, 1).OnKill(() => OnCameraMoveComplete()); // 添加回调
        }
    }

    // 相机移动完成后的回调
    void OnCameraMoveComplete()
    {
        
        // 确保 Timescale 设置为 0
        //if (rotationType == RotationType.Up)
        //Time.timeScale = 0;
        //此处为时间暂停，已删除
    }


    ////获取到所有方块的中心点
    //public Vector3 GetCenter(Transform Cubes)
    //{
    //    MinPos = Cubes.GetChild(0).position;
    //    for (int i = 1; i < Cubes.childCount; i++)
    //    {
    //        var pos = Cubes.GetChild(i).position;
    //        MaxPos.x = Mathf.Max(MaxPos.x, pos.x);
    //        MaxPos.y = Mathf.Max(MaxPos.y, pos.y);
    //        MaxPos.z = Mathf.Max(MaxPos.z, pos.z);
    //        MinPos.x = Mathf.Min(MinPos.x, pos.x);
    //        MinPos.y = Mathf.Min(MinPos.y, pos.y);
    //        MinPos.z = Mathf.Min(MinPos.z, pos.z);
    //    }
    //    return (MaxPos + MinPos) / 2;
    //}

    #endregion

    #region 弃置的视角转换逻辑
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
        Time.timeScale = 1;
    }
    #endregion

    public void CreatePlayer()
    {
        GameObject block = GameObject.FindGameObjectWithTag("Block 0");
        if (block != null)
        {
            
            Vector3 spawnPosition = block.transform.position + Vector3.up;
            Debug.Log(spawnPosition);
            Player = Instantiate(Resources.Load<GameObject>("Prefabs/Player/Player")).GetComponent<PlayerCharacter>();
            Player.transform.position = spawnPosition;
            Camera.main.GetComponent<CameraController>().LookAttarGet = Player.transform;//相机位置追踪
        }
        else
        {
            Debug.LogError("No block with tag 'Block 0' found in the scene.");
        }
    }


    private string[] objectsToDestroy= { "Player(Clone)", "FuncBlocks(Clone)", "NoFuncBlocks(Clone)" };//TODO：此处应写需要再切换时候删除的元素
    
    // 切换回关卡选择UI的功能
    public void SwitchToLevelSelectUI(bool isAdd=false)
    {

        // 删除指定的物体
        foreach (string objName in objectsToDestroy)
        {
            GameObject obj = GameObject.Find(objName);
            if (obj != null)
            {
                Destroy(obj); // 销毁物体
                Debug.Log(objName + " destroyed.");
            }
            else
            {
                Debug.LogWarning(objName + " not found.");
            }
        }
        if (isAdd)
        {
            GameMgr.Get<IDataManager>().ChangePlayerLevel(QXData.Instance.Get<PlayerData>().PLevel+1);
        }
        // 返回关卡选择UI界面
        //TODO:此处应该修正为关卡胜利UI
        UIManager.Instance.Open("LevelUI",3, "LevelUI", null);
    }


}
