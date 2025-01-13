using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BlockT : MonoBehaviour
{
    public int x;
    public int y;
    public int z;//旋转原点
    public bool directionx;//x方向旋转
    public bool directiony;//y方向旋转
    public bool directionz;//z方向旋转
    public int rotationAngle;//旋转角度
    public float duration;//旋转时间
    private bool isRotating = false; // 标记是否正在旋转
    public Vector3 targetRotation = Vector3.zero;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))  // 左键点击
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {

                Rotate();
            }
        }
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
