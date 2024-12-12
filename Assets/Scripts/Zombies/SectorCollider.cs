using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class SectorCollider : MonoBehaviour
{
    public float radius = 5.0f; // 扇形半径
    public float angle = 90.0f; // 扇形角度
    public int segments = 10;  // 分段数，用于生成更精细的扇形
    public bool showSector = false; // 控制扇形是否显示

    private MeshRenderer meshRenderer; // 用于控制显示
    private MeshCollider meshCollider;

    void Start()
    {
        CreateSector();

        // 获取 MeshRenderer 和 MeshCollider
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        // 设置初始显示状态
        meshRenderer.enabled = showSector;
    }

    void Update()
    {
        // 动态控制显示状态
        if (meshRenderer != null)
        {
            meshRenderer.enabled = showSector;
        }
    }

    void CreateSector()
    {
        Mesh mesh = new Mesh();

        // 扇形顶点数组
        Vector3[] vertices = new Vector3[segments + 2];
        vertices[0] = Vector3.zero; // 扇形中心点

        float halfAngle = angle / 2.0f;
        float angleStep = angle / segments;

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = -halfAngle + angleStep * i;
            float radian = currentAngle * Mathf.Deg2Rad;
            vertices[i + 1] = new Vector3(Mathf.Sin(radian) * radius, 0, Mathf.Cos(radian) * radius);
        }

        // 扇形三角形索引
        int[] triangles = new int[segments * 3];
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0; // 中心点
            triangles[i * 3 + 1] = i + 1; // 当前点
            triangles[i * 3 + 2] = i + 2; // 下一点
        }

        // 配置 Mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        // 应用到组件
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshCollider meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        meshCollider.convex = true; // 碰撞需要设置为凸面
        meshCollider.isTrigger = true;
    }
}
