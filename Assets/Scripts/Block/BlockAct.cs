using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using QxFramework.Core;
using Unity.VisualScripting;
using UnityEngine.Events;

public class BlockAct: MonoBehaviour
{
    //tag为Block0是出生点方块，Block1红色方块，Block2紫色方块，Block3蓝色方块,Block4黑色，Block5终点方块,Block6 普通方块
    public bool isELC = false;
    public bool isELCbool = false;

    public Vector3 targetposition;
    public float raycastDistance = 1f;  // 射线检测的最大距离
    public LayerMask collisionLayer;  // 碰撞层

    private Tween moveTween;
    public bool isdes = false;
    public bool isdesbool = false;
    [SerializeField] private UnityEvent<BlockAct> _onMouseDown = new();
    [SerializeField] private UnityEvent<BlockAct> _onMouseUp = new();
    private void Start()
    {
        Act();
    }
    private void Update()
    {
        if (isdes && !isdesbool)
        {
            isdesbool = true;
            StartCoroutine(WaitForSecondsExample());
        }
        if (isELC && !isELCbool)
        {
            AudioManager.Instance.PlayEffect("ElectricShock");
            Instantiate(ResourceManager.Instance.Load<GameObject>("Prefabs/SpecialEffect/LightWater"));
        }

    }

    IEnumerator WaitForSecondsExample()
    {
        yield return new WaitForSeconds(5f);  // 等待 2 秒
        Destroy(gameObject);
    }
    #region 方块反应
    public void Act()
    {
        string tag = gameObject.tag;
        if (tag == "Block 1")
        {
            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Block 2");
            foreach (var obj in gameObjects)
            {
                if (Vector3.Distance(transform.position,obj.transform.position) < 1.3f)
                {
                    Boom(obj);
                    Boom(gameObject);
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
            foreach (var obj in gameObjects)
            {
                if (Vector3.Distance(transform.position,obj.transform.position) < 1.3f)
                {
                    Boom(obj);
                    Boom(gameObject);
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
        if (tag == "Block 3")
        {
            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Block 2");
            foreach (var obj in gameObjects)
            {
                if (Vector3.Distance(transform.position, obj.transform.position) < 1.3f)
                {
                    Electric();
                }
            }
        }
    }

    public void Boom(GameObject _Object)//爆炸效果
    {
        GameObject Black = Instantiate(ResourceManager.Instance.Load<GameObject>("Prefabs/Block/Block/Black"),_Object.transform.parent.parent);
        Black.transform.position = _Object.transform.position;
        GameObject boom = Instantiate(Resources.Load<GameObject>("Prefabs/SpecialEffect/boom"));
        boom.transform.position = _Object.transform.position;
        Destroy(boom, 2f);
        Destroy(_Object);
    }

    public void Evaporation(GameObject _Object)//漂浮效果
    {
        Vector3 targetposition = _Object.transform.position + new Vector3(0, 1, 0);
        moveTween = _Object.transform.DOMove(targetposition, 0.5f).SetEase(Ease.InOutQuad).OnUpdate(CheckForCollision);
    }

    public void Electric()
    {
        for (int i = 0; i < transform.parent.childCount; i++)
        {
            // 获取子物体
            Transform childTransform = transform.parent.GetChild(i);
            if (childTransform.gameObject.GetComponent<BlockAct>() != null)
            {
                childTransform.gameObject.GetComponent<BlockAct>().isELC = true;//触发每一个子物体的带电
            }
        }
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

    public event UnityAction<BlockAct> MouseDownEvent
    {
        add => _onMouseDown.AddListener(value);
        remove => _onMouseDown.RemoveListener(value);
    }
}
