using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Collections;

public class DragBlockOnGrid : MonoBehaviour
{
    public float gridSize = 1f;         // 网格大小（整格长度）
    private Camera mainCamera;         // 主摄像机
    private Transform draggedBlock;    // 当前被拖动的方块
    private Vector3 startDragPosition; // 鼠标拖动开始时的世界坐标
    private Vector3 blockStartPosition;// 方块开始拖动时的位置
    public float dragPlaneHeight = 0f; // 拖动的平面高度（Y 轴）
    private float tolerance = -0.2f; // 设置移动容差值，小于0则使方块更易于挪动。
    public Vector3 dragOffset;
    private MoveDirection moveDirection;

    public bool directionx;//x方向旋转
    public bool directiony = true;//y方向旋转
    public bool directionz;//z方向旋转
    public int rotationAngle = 90;//旋转角度
    public float duration = 1f;//旋转时间
    private bool isRotating = false; // 标记是否正在旋转
    bool isPlayerStand = false;

    private Tween moveTween;
    Transform _tran;
    GameObject clone;
    GameObject mark;

    public bool VisualMove = false;
    void Start()
    {
        mainCamera = Camera.main; // 获取主摄像机
    }

    void Update()
    {
        if (GameManager.Instance.rotationType == RotationType.Up)
        {
            // 鼠标点击开始检测
            if (Input.GetMouseButtonDown(0))
            {
                StartDrag();
            }

            // 鼠标拖动
            if (Input.GetMouseButton(0) && draggedBlock != null)
            {
                Drag();
            }

            // 鼠标释放
            if (Input.GetMouseButtonUp(0))
            {
                EndDrag();
            }

            if (Input.GetMouseButtonDown(0))
            {
                Rotate();
            }

        }
    }



    private void StartDrag()
    {
        // 发射射线检测鼠标点击的方块
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // 获取物体标签并根据标签设置拖动规则

            //string hitTag = hit.collider.tag;
            if (hit.transform.parent.GetComponent<BlockMove>() == null)
                return;
            if (hit.transform.parent.GetComponent<BlockMove>().x || hit.transform.parent.GetComponent<BlockMove>().z)
            {
                if (hit.transform.parent.GetComponent<BlockMove>().x && hit.transform.parent.GetComponent<BlockMove>().z)
                {
                    moveDirection = MoveDirection.XZ;
                }
                else if (!hit.transform.parent.GetComponent<BlockMove>().x)
                {
                    moveDirection = MoveDirection.Z;
                }
                else
                {
                    moveDirection = MoveDirection.X;
                }
            }
            else
            {
                moveDirection = MoveDirection.None;
            }
            //moveDirection = MoveDirection.XZ;//GetMoveDirectionForTag(hitTag);
            //GameObject Player = GameObject.FindWithTag("Player");
            draggedBlock = hit.transform.parent;


            // 只有当物体标签为有效标签时才进行拖动
            if (moveDirection != MoveDirection.None && !isPlayerStand)
            {
                // 拖动父物体
                //draggedBlock = hit.transform.parent;
                blockStartPosition = draggedBlock.position; // 记录父物体初始位置
                startDragPosition = GetMousePositionOnPlane(); // 记录鼠标初始位置（基于平面）
            }
        }
    }

    private void Drag()
    {
        //GameObject Player = GameObject.FindWithTag("Player");
        Time.timeScale = 0;
        //if (Player != null)
        //{
        //    Transform playerPosition = Player.GetComponent<PlayerCharacter>().CurBoxCollider;
        //    Debug.Log("P=" + playerPosition.parent.gameObject + "D=" + draggedBlock.gameObject);
        //    if (playerPosition.parent.gameObject != draggedBlock.gameObject)
        //    {
        //        isPlayerStand = false;
        //    }
        //    else
        //    {
        //        isPlayerStand = true;
        //    }
        //}
        Vector3 currentMousePosition = GetMousePositionOnPlane(); // 当前鼠标在平面上的位置
        dragOffset = currentMousePosition - startDragPosition; // 计算鼠标拖动的偏移量

        // 判断是否达到整格的移动条件
        if ((Mathf.Abs(dragOffset.x) >= 0.7f * gridSize || Mathf.Abs(dragOffset.z) >= 0.7f * gridSize) && Mathf.Abs(dragOffset.x) < 1.4f * gridSize && Mathf.Abs(dragOffset.z) < 1.4f * gridSize && !isPlayerStand)
        {
            // 根据拖动方向限制偏移量
            Vector3 constrainedDragOffset = dragOffset;
            if (moveDirection == MoveDirection.X)
            {
                constrainedDragOffset = new Vector3(dragOffset.x, 0, 0); // 只允许沿X轴拖动
            }
            else if (moveDirection == MoveDirection.Z)
            {
                constrainedDragOffset = new Vector3(0, 0, dragOffset.z); // 只允许沿Z轴拖动
            }

            // 计算目标位置（整格对齐）
            float newX = Mathf.Round((blockStartPosition.x + constrainedDragOffset.x) / gridSize) * gridSize;
            float newZ = Mathf.Round((blockStartPosition.z + constrainedDragOffset.z) / gridSize) * gridSize;
            Vector3 targetPosition = new Vector3(newX, blockStartPosition.y, newZ);

            // 遍历父物体的每一个子物体，检测其目标位置是否可移动
            foreach (Transform child in draggedBlock)
            {
                Vector3 childTargetPosition = targetPosition + (child.position - blockStartPosition);
                if (!CanMoveToPosition(childTargetPosition))
                {
                    return; // 如果不能移动，退出拖动
                }
            }

            // 如果所有子物体的位置均可移动，允许移动
            draggedBlock.position = targetPosition;

            // 重置参考点
            startDragPosition = GetMousePositionOnPlane();
            blockStartPosition = draggedBlock.position;
        }
    }

    private bool CanMoveToPosition(Vector3 targetPosition)
    {
        // 设置检测范围和偏移量
        Vector3 halfExtents = new Vector3(gridSize / 2 + tolerance, gridSize / 2 + tolerance, gridSize / 2 + tolerance);
        Collider[] colliders = Physics.OverlapBox(targetPosition, halfExtents, Quaternion.identity);

        foreach (var collider in colliders)
        {
            // 确保碰撞到的不是当前父物体
            if (collider.transform.parent != draggedBlock)
            {
                return false; // 检测到其他方块，阻止移动
            }
        }

        return true; // 没有检测到阻碍，允许移动
    }

    private void EndDrag()
    {
        if (draggedBlock != null)
        {
            Time.timeScale = 1;
            mark = draggedBlock.gameObject;
            BlockA(mark);
            draggedBlock = null; // 停止拖动
        }
    }

    // 根据标签返回相应的拖动方向
    private MoveDirection GetMoveDirectionForTag(string tag)
    {
        switch (tag)//TODO:在此处修改tag对应的拖动能力
        {
            //case "Block 1": return MoveDirection.X;    // 只能沿X轴拖动
            case "Block 2": return MoveDirection.XZ;    // 只能沿Z轴拖动
            case "Block 3": return MoveDirection.XZ;   // 可以沿X和Z轴拖动
            default: return MoveDirection.None;        // 无效标签，返回None
        }
    }

    // 定义拖动方向
    private enum MoveDirection
    {
        None, // 无效标签，不能拖动
        X,    // 只能沿X轴移动
        Z,    // 只能沿Z轴移动
        XZ    // 可以沿XZ轴移动
    }
    private void BlockA(GameObject game)
    {
        for (int i = 0; i < game.transform.childCount; i++)
        {
            // 获取子物体
            Transform childTransform = game.transform.GetChild(i);
            if (childTransform.gameObject.GetComponent<BlockAct>() != null)
            {
                childTransform.gameObject.GetComponent<BlockAct>().Act();//触发每一个子物体的反应检测
            }
        }
        mark = null;
    }

    private Vector3 GetMousePositionOnPlane()
    {
        // 定义一个平面（水平面，Y = dragPlaneHeight）
        Plane dragPlane = new Plane(Vector3.up, new Vector3(0, dragPlaneHeight, 0));

        // 从鼠标位置发射射线
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // 计算射线与平面的交点
        if (dragPlane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter); // 返回交点的世界坐标
        }

        return Vector3.zero; // 默认返回值（不应该出现此情况）
    }

    public void Rotate()
    {
        // 发射射线检测鼠标点击的方块
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        // 获取旋转对象
        BlockMove blockMove = hit.transform.parent.GetComponent<BlockMove>();
        if (blockMove == null || (!blockMove.rotatex && !blockMove.rotatey && !blockMove.rotatez)) return;
        _tran = hit.transform.parent;
        BlockA(_tran.gameObject);
        if (isRotating) return;

        // 设置正在旋转
        isRotating = true;

        // 根据方向选择旋转的轴
        Vector3 rotationAxis = Vector3.zero;
        if (blockMove.rotatex) rotationAxis = Vector3.right;
        if (blockMove.rotatey) rotationAxis = Vector3.up;
        if (blockMove.rotatez) rotationAxis = new Vector3(0, 1, 1);

        // 优先顺时针旋转
        if (CanRotate(rotationAxis))
        {
            RotateInDirection(rotationAxis);
        }
        else if (CanRotate(-rotationAxis)) // 尝试逆时针旋转
        {
            RotateInDirection(-rotationAxis);
        }
        else
        {
            isRotating = false; // 无法旋转
        }
    }

    /// <summary>
    /// 按指定轴进行旋转
    /// </summary>
    private void RotateInDirection(Vector3 rotationAxis)
    {
        Transform pivotChild = _tran.GetChild(0);
        Vector3 pivotPosition = pivotChild.position;

        // 启动协程进行旋转
        StartCoroutine(RotateAroundPivot(pivotPosition, rotationAxis, 90));
    }

    private IEnumerator RotateAroundPivot(Vector3 pivot, Vector3 axis, int angle)
    {
        List<Vector3> initialPositions = new List<Vector3>();
        List<Quaternion> initialRotations = new List<Quaternion>();

        for (int i = 0; i < _tran.childCount; i++)
        {
            initialPositions.Add(_tran.GetChild(i).position);
            initialRotations.Add(_tran.GetChild(i).localRotation);
        }

        float time = 0f;
        while (time < 1f)
        {
            float progress = time / 1f;

            // 将每个子节点绕枢轴旋转
            for (int i = 0; i < _tran.childCount; i++)
            {
                Transform child = _tran.GetChild(i);
                Vector3 newPosition = GetPositionAfterRotation(initialPositions[i], pivot, axis, angle, progress, out Quaternion newRotation);
                child.position = newPosition;
                child.localRotation = newRotation;
            }

            time += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < _tran.childCount; i++)
        {
            Transform child = _tran.GetChild(i);
            child.position = GetPositionAfterRotation(initialPositions[i], pivot, axis, angle, 1, out Quaternion newRotation);
            child.localRotation = newRotation;
        }
        isRotating = false;
        BlockA(_tran.gameObject);
    }

    private Vector3 GetPositionAfterRotation(Vector3 position, Vector3 pivot, Vector3 axis, int angle, float slerpT, out Quaternion rotation)
    {
        rotation = Quaternion.AngleAxis(angle, axis);

        if (slerpT < 1)
        {
            rotation = Quaternion.Slerp(Quaternion.identity, rotation, slerpT);
        }

        // 绕枢轴旋转
        return pivot + rotation * (position - pivot);
    }


    void Check()
    {
        for (int i = 0; i < clone.transform.childCount; i++)
        {
            // 获取子物体
            Transform childTransform = clone.transform.GetChild(i);

            Collider[] colliders = Physics.OverlapBox(childTransform.position, new Vector3(0.4f, 0.4f, 0.4f), Quaternion.identity);

            foreach (var collider in colliders)
            {
                if (collider.gameObject.transform.parent != childTransform.parent && collider.gameObject.transform.parent != _tran)
                {
                    isRotating = false;
                    moveTween.Kill();
                    Destroy(clone);
                }
            }

        }
    }
    /// <summary>
    /// 检测当前方块能否旋转
    /// </summary>
    private bool CanRotate(Vector3 axis)
    {
        Quaternion testRotation = Quaternion.AngleAxis(90, axis) * _tran.rotation;
        foreach (Transform child in _tran)
        {
            Vector3 newPosition = testRotation * (child.position - _tran.position) + _tran.position;
            Collider[] colliders = Physics.OverlapBox(newPosition, new Vector3(0.4f, 0.4f, 0.4f));

            foreach (Collider collider in colliders)
            {
                if (collider.transform.parent != _tran) return false; // 目标位置被占用
            }
        }
        return true;
    }
}