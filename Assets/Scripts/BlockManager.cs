using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    public GameObject prefab;  // 预制件
    public GameObject remind; //提示方块
    public int x = 0;          // x 位置
    public int y = 0;          // y 位置
    public int z = 0;          // z 位置
    public float scale = 1f;   // 缩放
    public int xrow = 1;      //沿x方向生成
    public int yrow = 1;      //沿y方向生成
    public int zrow = 1;      //沿z方向生成

    private GameObject remindblock;
    public void RemindBlock()
    {
        if (remindblock ==null)
        {
            remindblock = Instantiate(remind, new Vector3(x, y, z ), Quaternion.identity);
            remindblock.transform.SetParent(gameObject.transform.parent);
            remindblock.transform.localScale = new Vector3(scale, scale, scale);
        }
        else
        {
            DestroyImmediate(remindblock);
            remindblock = null;
        }
    }

    public void RemindBlockUpdate(int xpoint,int ypoint ,int zpoint)
    {
        if (remind == null)
            return;
        x += xpoint;
        y += ypoint;
        z += zpoint;
        remindblock.transform.position = new Vector3(x, y, z);
    }
    public void GBlockFull(GameObject pref,int x,int y ,int z,int xrow ,int yrow,int zrow, float s = 1)
    {
        for (int i = 0; i < xrow; i++)
        {
            for (int j = 0; j <yrow; j++)
            {
                for (int k = 0; k < zrow; k++)
                {
                    GameObject _gameObject = Instantiate(pref, new Vector3(x + i, y + j, z + k), Quaternion.identity);
                    _gameObject.transform.SetParent(gameObject.transform.parent);
                    _gameObject.transform.localScale = new Vector3(s, s, s);
                }
            }
        }
    }

    public void GBlockQ(GameObject pref, int x, int y, int z, float s = 1)
    {
        GameObject _gameObject = Instantiate(pref, new Vector3(x, y, z), Quaternion.identity);
        _gameObject.transform.SetParent(gameObject.transform.parent);
        _gameObject.transform.localScale = new Vector3(s, s, s);
    }

}
