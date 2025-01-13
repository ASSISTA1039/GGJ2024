using UnityEngine;

public class DragBlockOnGrid : MonoBehaviour
{
    public float gridSize = 1f;         // 网格大小（整格长度）
    private Camera mainCamera;         // 主摄像机
    private Transform draggedBlock;    // 当前被拖动的方块
    private Vector3 startDragPosition; // 鼠标拖动开始时的世界坐标
    private Vector3 blockStartPosition;// 方块开始拖动时的位置
    public float dragPlaneHeight = 0f; // 拖动的平面高度（Y 轴）

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
                draggedBlock = hit.transform;
                blockStartPosition = draggedBlock.position;       // 记录方块的初始位置
                startDragPosition = GetMousePositionOnPlane();   // 记录鼠标初始位置（基于平面）
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

            // 更新方块位置
            draggedBlock.position = new Vector3(newX, blockStartPosition.y, newZ);

            // 重置参考点
            startDragPosition = GetMousePositionOnPlane();
            blockStartPosition = draggedBlock.position;
        }
    }

    private void EndDrag()
    {
        draggedBlock = null; // 停止拖动
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
}