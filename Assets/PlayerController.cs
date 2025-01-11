using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    public PlayerCharacter Player;


    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(GameManager.Instance.isThird);
        if (Input.GetKeyDown(KeyCode.Q))
        {
            GameManager.Instance.isThird = false;
            GameManager.Instance.ChangeRotationType(true);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            GameManager.Instance.isThird = false;
            GameManager.Instance.ChangeRotationType(false);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            GameManager.Instance.isThird = false;
            GameManager.Instance.ChangeTypeToUp();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameManager.Instance.isThird = true;
            Camera.main.orthographic = false;
            Camera.main.GetComponent<CameraController>().enabled = true;
        }
        if (!Player.gameObject.activeInHierarchy)
            return;
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool isJump = Input.GetButtonDown("Jump");
        ;
        Player.Move(CharacterInputSystem.Instance.playerMovement, isJump);
    }
}
