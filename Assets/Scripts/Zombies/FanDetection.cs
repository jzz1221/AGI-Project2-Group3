using UnityEngine;

public class FanDetection : MonoBehaviour
{
    public float viewRadius = 5f; // 视野距离
    public int viewAngleStep = 20; // 视野分段
    [Range(0, 360)]
    public float viewAngle = 270f; // 视野角度
    public static ZombieScript currentTargetZombie = null;
    public LayerMask zombieLayerMask;

    private GameObject lastTarget = null; // 上一个高亮的目标
    private Color defaultColor = Color.gray; // 默认颜色
    private Color highlightColor = Color.yellow; // 高亮颜色

    void Start()
    {
                defaultColor.a = 0.5f;
                highlightColor.a = 0.8f;
    }

    void Update()
    {
        DetectAndHighlightClosestEnemy();
    }

    void DetectAndHighlightClosestEnemy()
    {
        Vector3 forwardLeft = Quaternion.Euler(0, -(viewAngle / 2f), 0) * transform.forward * viewRadius;
        GameObject closestZombieObject = null;
        ZombieScript closestZombieScript = null;
        float closestDistance = Mathf.Infinity;

        for (int i = 0; i <= viewAngleStep; i++)
        {
            // 计算当前方向
            Vector3 direction = Quaternion.Euler(0, (viewAngle / viewAngleStep) * i, 0) * forwardLeft;

            // 进行射线检测
            Ray ray = new Ray(transform.position, direction.normalized);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, viewRadius, zombieLayerMask))
            {
                // 检测到的物体是否有 ZombieScript
                ZombieScript zombieScript = hitInfo.collider.GetComponent<ZombieScript>();
                if (zombieScript != null && !zombieScript.isRemoved)
                {
                    float distance = hitInfo.distance;
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestZombieObject = hitInfo.collider.gameObject; // 更新最近目标
                        closestZombieScript = zombieScript; // 更新最近目标的 ZombieScript
                    }
                }
            }

            // Debug 可视化射线
            Debug.DrawLine(transform.position, transform.position + direction, Color.red);
        }

        // 更新 Zombie 高亮和当前目标
        UpdateZombieHighlight(closestZombieObject, closestZombieScript);
    }

    void UpdateZombieHighlight(GameObject currentTarget, ZombieScript currentZombieScript)
    {
        // 如果当前目标发生变化
        if (currentTarget != lastTarget)
        {
            // 重置上一个目标颜色
            if (lastTarget != null)
            {
                var planeRenderer = lastTarget.transform.Find("Plane")?.GetComponent<MeshRenderer>();
                if (planeRenderer != null)
                {
                    planeRenderer.material.color = defaultColor;
                }
            }

            // 设置当前目标为高亮颜色
            if (currentTarget != null)
            {
                var planeRenderer = currentTarget.transform.Find("Plane")?.GetComponent<MeshRenderer>();
                if (planeRenderer != null)
                {
                    planeRenderer.material.color = highlightColor;
                }
            }

            // 更新当前锁定的 ZombieScript
            currentTargetZombie = currentZombieScript;

            // 更新上一个目标
            lastTarget = currentTarget;
        }

        // 如果当前没有目标，重置全局状态
        if (currentTarget == null)
        {
            currentTargetZombie = null;
        }
    }
}
