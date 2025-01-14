using UnityEngine;
using DG.Tweening;

public class DragBlockOnGrid : MonoBehaviour
{
    public float gridSize = 1f;         // 网格大小（整格长度）
    private Camera mainCamera;         // 主摄像机
    private Transform draggedBlock;    // 当前被拖动的方块
    private Vector3 startDragPosition; // 鼠标拖动开始时的世界坐标
    private Vector3 blockStartPosition;// 方块开始拖动时的位置
    public float dragPlaneHeight = 0f; // 拖动的平面高度（Y 轴）
    private float tolerance = -0.5f; // 设置移动容差值，小于0则使方块更易于挪动。

    public bool directionx;//x方向旋转
    public bool directiony;//y方向旋转
    public bool directionz;//z方向旋转
    public int rotationAngle;//旋转角度
    public float duration;//旋转时间
    private bool isRotating = false; // 标记是否正在旋转
    private Vector3 targetRotation = Vector3.zero;

    void Start()
    {
        mainCamera = Camera.main; // 获取主摄像机
    }

    void Update()
    {
        if (GameManager.Instance.rotationType==RotationType.Up)
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

            if (Input.GetMouseButtonDown(1))  // 右键点击
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
            if (hit.collider.CompareTag("Draggable"))
            {
                // 拖动父物体
                draggedBlock = hit.transform.parent;
                blockStartPosition = draggedBlock.position; // 记录父物体初始位置
                startDragPosition = GetMousePositionOnPlane(); // 记录鼠标初始位置（基于平面）
            }
        }
    }

    private void Drag()
    {
        Vector3 currentMousePosition = GetMousePositionOnPlane(); // 当前鼠标在平面上的位置
        Vector3 dragOffset = currentMousePosition - startDragPosition; // 计算鼠标拖动的偏移量

        // 判断是否达到整格的移动条件
        if (Mathf.Abs(dragOffset.x) >= gridSize || Mathf.Abs(dragOffset.z) >= gridSize)
        {
            // 计算目标位置（整格对齐）
            float newX = Mathf.Round((blockStartPosition.x + dragOffset.x) / gridSize) * gridSize;
            float newZ = Mathf.Round((blockStartPosition.z + dragOffset.z) / gridSize) * gridSize;
            Vector3 targetPosition = new Vector3(newX, blockStartPosition.y, newZ);

            // 遍历父物体的每一个子物体，检测其目标位置是否可移动
            foreach (Transform child in draggedBlock)
            {
                Vector3 childTargetPosition = targetPosition + (child.position - blockStartPosition);
                if (!CanMoveToPosition(childTargetPosition))
                {
                    // 检测到阻碍，如果鼠标移动距离过远则退出拖动状态
                    if (dragOffset.magnitude > gridSize * 2)
                    {
                        draggedBlock = null; // 退出拖动状态
                    }
                    return;
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
        draggedBlock = null; // 停止拖动
        for (int i = 0; i < transform.childCount; i++)
        {
            // 获取子物体
            Transform childTransform = transform.GetChild(i);
            childTransform.gameObject.GetComponent<BlockAct>().Act(); // 触发每一个子物体的反应检测
        }
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
        if (isRotating) return;

        // 设置正在旋转
        isRotating = true;

        // 根据方向选择旋转的轴
        if (directionx)
        {
            targetRotation.x += rotationAngle;
        }
        if (directiony)
        {
            targetRotation.y += rotationAngle;
        }
        if (directionz)
        {
            targetRotation.z += rotationAngle;
        }
        // 使用 DORotate 来旋转物体
        transform.DORotateQuaternion(Quaternion.Euler(targetRotation.x, targetRotation.y, targetRotation.z), duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // 旋转完成后，允许再次调用
                isRotating = false;
            });
    }

}