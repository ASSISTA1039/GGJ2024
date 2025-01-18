using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionExplosion : MonoBehaviour
{
    public GameObject explosionEffect; // 爆炸特效的预制体
    public GameObject newObjectPrefab; // 生成的新物体的预制体（例如其他方块或物品）

    private void OnCollisionEnter(Collision collision)
    {
        // 检查碰撞的对象是否有特定标签（例如 "purple"）
        if (collision.gameObject.CompareTag("purple"))
        {
            // 获取碰撞点的位置
            ContactPoint contact = collision.contacts[0];
            Vector3 explosionPosition = contact.point;

            // 实例化爆炸特效
            GameObject explosion = Instantiate(explosionEffect, explosionPosition, Quaternion.identity);

            // 销毁爆炸特效（设定时间后自动销毁）
            Destroy(explosion, 2f);

            // 销毁碰撞的方块
            Destroy(collision.gameObject);
            Destroy(gameObject);

            // 取代被碰撞物体的位置生成新物体
            ReplaceWithNewObject(collision.gameObject);
        }
    }

    // 替换被碰撞物体的位置生成新物体
    void ReplaceWithNewObject(GameObject collidedObject)
    {
        // 获取被碰撞物体的位置
        Vector3 collidedPosition = collidedObject.transform.position;

        // 在被碰撞物体的位置生成新物体
        Instantiate(newObjectPrefab, collidedPosition, Quaternion.identity);
    }
}

