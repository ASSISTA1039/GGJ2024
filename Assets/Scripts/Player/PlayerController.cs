using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    private PlayerCharacter Player;


    public void Awake()
    {
        Player= gameObject.GetComponent<PlayerCharacter>();
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
        if (Input.GetKeyDown(KeyCode.F))
        {
            //GameManager.Instance.isThird = false;
            //GameManager.Instance.ChangeTypeToUp();
            GameManager.Instance.isThird = !GameManager.Instance.isThird;  // 如果是第三人称，则切换为第一人称，反之亦然

            // 根据切换的状态，进行不同的操作
            if (GameManager.Instance.isThird)
            {
                GameManager.Instance.isThird = true;
                Camera.main.orthographic = false;
                Camera.main.GetComponent<CameraController>().enabled = true;
                GameManager.Instance.ChangeTypeToThird();
            }
            else
            {
                GameManager.Instance.isThird = false;
                GameManager.Instance.ChangeTypeToUp();
            }
        }
       
        
        if (!Player.gameObject.activeInHierarchy)
            return;
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool isJump = Input.GetButtonDown("Jump");
        
        Player.Move(CharacterInputSystem.Instance.playerMovement, isJump);
    }
}
