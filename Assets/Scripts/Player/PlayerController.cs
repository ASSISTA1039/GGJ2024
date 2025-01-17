using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    private PlayerCharacter Player;
    private Camera MiniMapCamera;
    [SerializeField, Header("相机高度")] public float CameraHigh=20f;

    public void Awake()
    {
        Player= gameObject.GetComponent<PlayerCharacter>();
        MiniMapCamera = GameObject.Find("MiniMapCamera").GetComponent<Camera>();
        if(MiniMapCamera!=null)
            MiniMapCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    // Update is called once per frame
    public void Update()
    {
        #region 暂时隐藏的QE切换逻辑
        //if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    GameManager.Instance.isThird = false;
        //    GameManager.Instance.ChangeRotationType(true);
        //}
        //if (Input.GetKeyDown(KeyCode.E))
        //{
        //    GameManager.Instance.isThird = false;
        //    GameManager.Instance.ChangeRotationType(false);
        //}
        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    GameManager.Instance.isThird = true;
        //    Camera.main.orthographic = false;
        //    Camera.main.GetComponent<CameraController>().enabled = true;
        //}
        #endregion
        if (MiniMapCamera != null)
        {
            MiniMapCamera.transform.position = new Vector3(this.gameObject.transform.position.x, CameraHigh, this.gameObject.transform.position.z);
        }
            //此处的视角转换被删除
            if (Input.GetKeyDown(KeyCode.F))
        {
            
            //GameManager.Instance.isThird = false;
            //GameManager.Instance.ChangeTypeToUp();
            //GameManager.Instance.isThird = !GameManager.Instance.isThird;  // 如果是第三人称，则切换为第一人称，反之亦然

            //// 根据切换的状态，进行不同的操作
            //if (GameManager.Instance.isThird)
            //{
            //    GameManager.Instance.isThird = true;
            //    Camera.main.orthographic = false;
            //    Camera.main.GetComponent<CameraController>().enabled = true;
            //    GameManager.Instance.ChangeTypeToThird();
            //}
            //else
            //{
            //    GameManager.Instance.isThird = false;
            //    GameManager.Instance.ChangeTypeToUp();
            //}
            
        }
       
        
        if (!Player.gameObject.activeInHierarchy)
            return;
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool isJump = Input.GetButtonDown("Jump");
        Animator animator = this.gameObject.GetComponent<Animator>();

        if (CharacterInputSystem.Instance.playerMovement.x == 0 && CharacterInputSystem.Instance.playerMovement.y == 0)
        {
            if (animator != null)
                animator.SetBool("IsWalk", false);
            else 
            {
                Debug.Log("animator=null");
            }
        }
        else 
        {
            animator.SetBool("IsWalk", true);
        }

        Player.Move(CharacterInputSystem.Instance.playerMovement, isJump,Player.isInwater);
        Player.InwaterMove(CharacterInputSystem.Instance.playerMovement, isJump,Player.isInwater);
    }
}
