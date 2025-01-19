using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatableBlock : MonoBehaviour
{
    public Transform PivotBlock;
    public Vector3 RotationAxis = Vector3.up;
    public float RotateTime = 0.5f;
    private bool isRotating = false;
    private int rotateAngle = 90;

    private void Start()
    {
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out BlockAct blockAct))
            {
                blockAct.MouseDownEvent += _ => StartCoroutine(TryRotate());
            }
        }
    }

    private IEnumerator TryRotate()
    {
        if (isRotating || !CanRotate())
        {
            yield break;
        }

        isRotating = true;
        yield return StartCoroutine(DoRotateAnimation(rotateAngle));
        isRotating = false;
        NotifyBlocks();
    }

    private IEnumerator DoRotateAnimation(int angle)
    {
        float time = 0;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.AngleAxis(angle, RotationAxis) * startRotation;

        while (time < RotateTime)
        {
            float progress = time / RotateTime;
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, progress);
            time += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation; // 确保最终角度
    }

    private bool CanRotate()
    {
        HashSet<Vector3Int> occupiedBlocks = new HashSet<Vector3Int>();

        // 获取当前所有方块位置
        foreach (Transform child in transform)
        {
            occupiedBlocks.Add(Vector3Int.RoundToInt(child.position));
        }

        // 预测旋转后的位置
        foreach (Transform child in transform)
        {
            Vector3 rotatedPos = GetPositionAfterRotation(child.position, rotateAngle);
            Vector3Int roundedPos = Vector3Int.RoundToInt(rotatedPos);

            if (occupiedBlocks.Contains(roundedPos))
            {
                return false; // 旋转后位置被占用，无法旋转
            }
        }

        return true;
    }

    private Vector3 GetPositionAfterRotation(Vector3 position, int angle)
    {
        Vector3 pivot = PivotBlock.position;
        Quaternion rot = Quaternion.AngleAxis(angle, RotationAxis);
        return pivot + rot * (position - pivot);
    }

    private void NotifyBlocks()
    {
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out BlockAct blockAct))
            {
                blockAct.Act();
            }
        }
    }
}