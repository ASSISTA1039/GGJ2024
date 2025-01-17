using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BlockAct: MonoBehaviour
{
    //tag为Block0是出生点方块，Block1红色方块，Block2紫色方块，Block3蓝色方块,Block4黑色，Block5终点方块,Block6 普通方块
    [HideInInspector] public string thetag;
    public bool isELC = false;

    public Vector3 targetposition;
    public float raycastDistance = 1f;  // 射线检测的最大距离
    public LayerMask collisionLayer;  // 碰撞层

    private Tween moveTween;

    private void Start()
    {
        thetag = gameObject.tag;
    }

    #region 方块反应
    public void Act()
    {
        if (tag == "Block 1")
        {
            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Block 2");
            foreach (var obj in gameObjects)
            {
                if (Vector3.Distance(transform.position,obj.transform.position) < 1.3f)
                {
                    Boom(obj);
                }

            }

            GameObject[] gameObjects2 = GameObject.FindGameObjectsWithTag("Block 3");
            foreach (var obj in gameObjects2)
            {
                if (Vector3.Distance(transform.position, obj.transform.position) < 1.3f)
                {
                    Evaporation(obj);
                }
            }
        }
        if (tag == "Block 2")
        {
            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Block 1");
            bool isboom = false;
            foreach (var obj in gameObjects)
            {
                if (Vector3.Distance(transform.position,obj.transform.position) < 1.3f)
                {
                    Boom(obj);
                }
            }

            GameObject[] gameObjects2 = GameObject.FindGameObjectsWithTag("Block 3");
            foreach (var obj in gameObjects2)
            {
                if (Vector3.Distance(transform.position, obj.transform.position) < 1.3f)
                {
                    obj.GetComponent<BlockAct>().Electric();
                }
            }
        }
    }

    public void Boom(GameObject _Object)//爆炸效果
    {
        //TODO：生成黑色方块
        Destroy(_Object);
    }

    public void Evaporation(GameObject _Object)//漂浮效果
    {
        Vector3 targetposition = _Object.transform.position + new Vector3(0, 1, 0);
        moveTween = _Object.transform.DOMove(targetposition, 0.5f).SetEase(Ease.InOutQuad).OnUpdate(CheckForCollision);
    }

    public void Electric()
    {
        isELC = true;
    }

    void CheckForCollision()
    {
        // 射线从物体当前位置到目标位置进行检测
        RaycastHit hit;
        if (Physics.Raycast(transform.position, targetposition - transform.position, out hit, raycastDistance, collisionLayer))
        {
            // 如果射线检测到碰撞体，停止移动
            Debug.Log("遇到碰撞体，停止移动: " + hit.collider.gameObject.name);
            moveTween.Kill();  // 停止 DoTween 动画
        }
    }
    #endregion

}
