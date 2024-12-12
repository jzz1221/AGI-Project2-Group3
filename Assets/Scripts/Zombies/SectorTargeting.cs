using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SectorCollider))]
public class SectorTargeting : MonoBehaviour
{
    public LayerMask zombieLayerMask; // 用于过滤 Zombie 层
    public Transform vrCamera; // 摄像机的 Transform，用于扇形的方向基准
    public static ZombieScript currentTargetZombie = null; // 当前锁定的目标

    private HashSet<ZombieScript> detectedZombies = new HashSet<ZombieScript>(); // 扇形范围内的目标集合
    private GameObject lastHighlightedZombie = null; // 上一个高亮的目标

    private Color defaultColor = Color.gray; // 默认颜色
    private Color highlightedColor = Color.yellow; // 高亮颜色

    void Start()
    {
        // 初始化默认颜色
        defaultColor.a = 0.5f;
        highlightedColor.a = 0.8f;
    }

    void Update()
    {
        UpdateClosestZombie();
    }

    private void UpdateClosestZombie()
    {
        float closestDistance = Mathf.Infinity; // 初始化为一个极大值
        ZombieScript closestZombie = null; // 最近的目标
        GameObject closestZombieObject = null;

        foreach (var zombie in detectedZombies)
        {
            if (zombie == null || zombie.isRemoved) continue; // 跳过无效或已移除的目标

            float distance = Vector3.Distance(vrCamera.position, zombie.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestZombie = zombie;
                closestZombieObject = zombie.gameObject;
            }
        }

        if (closestZombieObject != null)
        {
            if (closestZombieObject != lastHighlightedZombie)
            {
                ResetLastHighlightedZombie(); // 重置上一个目标的颜色
                HighlightZombie(closestZombieObject); // 高亮新的目标
                lastHighlightedZombie = closestZombieObject;
            }

            currentTargetZombie = closestZombie; // 更新锁定目标
        }
        else
        {
            ResetLastHighlightedZombie(); // 如果没有目标，重置颜色
            lastHighlightedZombie = null;
            currentTargetZombie = null;
        }
    }

    private void HighlightZombie(GameObject zombie)
    {
        var renderer = zombie.transform.Find("Plane").GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material.color = highlightedColor; // 设置高亮颜色
        }
    }

    private void ResetLastHighlightedZombie()
    {
        if (lastHighlightedZombie != null)
        {
            var renderer = lastHighlightedZombie.transform.Find("Plane").GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.color = defaultColor; // 恢复默认颜色
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter");
        
        // 检查是否是 Zombie 层的目标
        if (((1 << other.gameObject.layer) & zombieLayerMask) != 0)
        {
            Debug.Log("Trigger object layer:"+ other.gameObject.layer + "target layer:" +zombieLayerMask);
            ZombieScript zombie = other.GetComponent<ZombieScript>();
            Debug.Log("get zombie target");
            if (zombie != null && !zombie.isRemoved)
            {
                detectedZombies.Add(zombie); // 添加到检测集合
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 检查是否是 Zombie 层的目标
        if (((1 << other.gameObject.layer) & zombieLayerMask) != 0)
        {
            ZombieScript zombie = other.GetComponent<ZombieScript>();
            if (zombie != null)
            {
                detectedZombies.Remove(zombie); // 从检测集合移除
            }
        }
    }
}
