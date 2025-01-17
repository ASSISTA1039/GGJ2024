using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerAct : MonoBehaviour
{
    public int health;
    Transform CurBoxCollider = null;

    public Vector3 targetposition;
    public float raycastDistance = 1f;  // 射线检测的最大距离
    public LayerMask collisionLayer;  // 碰撞层
    private Tween moveTween;

    private void Start()
    {
        health = 1;
    }
    private void Update()
    {
        CurBoxCollider = GetComponent<PlayerCharacter>().CurBoxCollider;
        EHurt();
    }
    public void BlockDrop(GameObject pref ,float height , int time)//下落的方块
    {
        if (CurBoxCollider == null)
        {
            Debug.Log("主角脚底没有方块");
        }
        else
        {
            GameObject @object = Instantiate(pref, gameObject.transform.position + new Vector3(0, height, 0), Quaternion.identity);
            targetposition = GetComponent<PlayerCharacter>().CurBoxCollider.position + new Vector3(0,1,0);
            moveTween = @object.transform.DOMove(targetposition, time).SetEase(Ease.InOutQuad).OnUpdate(CheckForCollision);//平滑移动到脚下方块上
        }

    }

    public void EHurt()
    {
        if (CurBoxCollider == null)
        {
            Debug.Log("主角脚底没有方块");
        }
        else
        {
            if (CurBoxCollider.gameObject.GetComponent<BlockAct>() != null)
            {
                if (CurBoxCollider.gameObject.GetComponent<BlockAct>().iselc)
                {
                    health -= 1;
                }

            }
        }
    }

    public void Death()
    {
        if (health <= 0)
        {
            Debug.Log("玩家死亡");
            //TODO：玩家死亡的相关流程
        }

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

}
