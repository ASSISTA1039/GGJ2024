using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody Rig;
    public float MoveSpeed=2;
    public float JumpSpeed=5;
    public LayerMask layerMask;

    public float yOffset;                            //主角相对于地面碰撞体的高度
    bool isGround;

    public Transform CurBoxCollider;                 //主角当前脚底下的碰撞体
    void Start()
    {
        Rig = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        RaycastHit hit;
        isGround = Physics.Raycast(transform.position, Vector3.down,  out hit, 0.6f, layerMask);
        if(hit.transform != null)
        {
            CurBoxCollider = hit.transform;
            yOffset = transform.position.y - CurBoxCollider.position.y;    //记录主角与地面的相对高度
        }
    }

    /// <summary>
    /// 角色移动
    /// </summary>
    /// <param name="h">角色横向移动的数值</param>
    /// <param name="v">角色纵向移动的数值</param>
    /// <param name="isJump">是否跳跃</param>
    public void Move(Vector2 input, bool isJump)
    {
        RotationType type = GameManager.Instance.rotationType;
        Vector3 vel;
        if(GameManager.Instance.isThird)
        {
            Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized; // 去掉y分量
            Vector3 cameraRight = Camera.main.transform.right;

            // 基于摄像机方向计算角色的移动方向
            Vector3 desiredMove = cameraForward * input.y + cameraRight * input.x;
            desiredMove *= MoveSpeed;

            // 应用角色的移动速度
            Vector3 velocity = new Vector3(desiredMove.x, Rig.velocity.y, desiredMove.z);
            //Rig.velocity = velocity;

            // 控制角色朝向移动方向
            if (desiredMove.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMove), Time.deltaTime * 2f);
            }
            vel = new Vector3(velocity.x, velocity.y, velocity.z);
        }
        else if (type == RotationType.Up)
        {
            vel = Quaternion.Euler(0, 135, 0) * new Vector3(input.x, 0, input.y) * MoveSpeed;
        }
        else
        {
            Vector3 dir = Vector3.zero;
            switch (GameManager.Instance.rotationType)
            {
                case RotationType.Front:
                    dir = Vector3.right;
                    break;
                case RotationType.Right:
                    dir = Vector3.forward;
                    break;
                case RotationType.Back:
                    dir = Vector3.left;
                    break;
                case RotationType.Left:
                    dir = Vector3.back;
                    break;
            }
            vel = dir * MoveSpeed * input.x;
        }

        if (isJump && isGround)
        {
            Rig.AddForce(JumpSpeed * Vector3.up, ForceMode.Impulse);
        }
        Rig.velocity = new Vector3(vel.x, Rig.velocity.y, vel.z);
    }

    public void FollowCollider()
    {
        if (CurBoxCollider != null)
        {
            transform.position = new Vector3(CurBoxCollider.position.x,
            CurBoxCollider.position.y + yOffset, CurBoxCollider.position.z);
        }
    }
}
