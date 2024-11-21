using UnityEngine;

public class PaintingPanel : MonoBehaviour
{
    public Transform vrCamera;      // 玩家相机（通常是 VR Rig 中的 Camera 对象）
    public float distance = 2.0f;  // 画板距离相机的距离
    public Vector3 offset = Vector3.zero; // 画板相对于相机的偏移量

    void Update()
    {
        if (vrCamera != null)
        {
            // 设置画板位置
            transform.position = vrCamera.position + vrCamera.forward * distance + offset;

            // 设置画板始终面朝玩家
            transform.rotation = Quaternion.LookRotation(transform.position - vrCamera.position, Vector3.up);
        }
    }
}
