using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockAct: MonoBehaviour
{
    //tag为Block0是出生点方块，Block1红色方块，Block2紫色方块，Block3蓝色方块,Block4黑色，Block5终点方块
    public string thetag;
    private void Start()
    {
        thetag = gameObject.tag;
    }

    public void Act()
    {
        if (tag == "Block1")
        {
            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Block2");
            foreach (var obj in gameObjects)
            {
                if (Vector3.Distance(transform.position,obj.transform.position) < 1.3f)
                {
                    Boom(obj);
                    break;
                }
            }

            GameObject[] gameObjects2 = GameObject.FindGameObjectsWithTag("Block3");
            foreach (var obj in gameObjects2)
            {
                if (Vector3.Distance(transform.position, obj.transform.position) < 1.3f)
                {
                    Evaporation(obj);
                }
            }
        }
        if (tag == "Block2")
        {
            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Block1");
            foreach (var obj in gameObjects)
            {
                if (Vector3.Distance(transform.position, obj.transform.position) < 1.3f)
                {
                    Boom(obj);
                    break;
                }
            }
        }
        if (tag == "Block3")
        {
            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Block1");
            foreach (var obj in gameObjects)
            {
                if (Vector3.Distance(transform.position, obj.transform.position) < 1.3f)
                {
                    Boom(gameObject);
                    break;
                }
            }
        }


    }

    public void Boom(GameObject _Object)
    {
        Collider[] colliders = Physics.OverlapSphere(_Object.transform.position, 3f);
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.tag != "Block4")
            {
                //TODO：加入黑色方块
                Destroy(collider.gameObject);
            }

        }
    }

    public void Evaporation(GameObject _Object)
    {
        _Object.transform.position += new Vector3(0, 1,0);
    }
}
