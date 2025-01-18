using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningEffect : MonoBehaviour
{
    public GameObject lightningPrefab;        // 闪电粒子系统预制体
    public float minTime = 1f;                // 闪电最短时间间隔
    public float maxTime = 3f;                // 闪电最长时间间隔
    public float lightningDuration = 0.2f;    // 闪电持续时间
    public float lightningRadius = 5f;        // 闪电影响的半径（范围）

    private float nextLightningTime;

    void Start()
    {
        // 初始化下次闪电生成的时间
        SetNextLightningTime();
    }

    void Update()
    {
        // 每帧检查是否到了生成闪电的时间
        if (Time.time >= nextLightningTime)
        {
            // 在方块位置生成闪电
            CreateLightning();

            // 设置下次生成闪电的时间
            SetNextLightningTime();
        }
    }

    void SetNextLightningTime()
    {
        // 随机生成下一次闪电的时间间隔
        nextLightningTime = Time.time + Random.Range(minTime, maxTime);
    }

    void CreateLightning()
    {
        // 生成闪电的位置
        Vector3 lightningPosition = transform.position + Random.insideUnitSphere * lightningRadius;

        // 实例化闪电粒子系统
        GameObject lightning = Instantiate(lightningPrefab, lightningPosition, Quaternion.identity);

        // 设置闪电的持续时间（关闭闪电粒子效果）
        Destroy(lightning, lightningDuration);
    }
}
