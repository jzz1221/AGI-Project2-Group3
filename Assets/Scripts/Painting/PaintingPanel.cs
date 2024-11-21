using UnityEngine;

public class PaintingPanel : MonoBehaviour
{
    public Transform vrCamera;      // ��������ͨ���� VR Rig �е� Camera ����
    public float distance = 2.0f;  // �����������ľ���
    public Vector3 offset = Vector3.zero; // ��������������ƫ����

    void Update()
    {
        if (vrCamera != null)
        {
            // ���û���λ��
            transform.position = vrCamera.position + vrCamera.forward * distance + offset;

            // ���û���ʼ���泯���
            transform.rotation = Quaternion.LookRotation(transform.position - vrCamera.position, Vector3.up);
        }
    }
}
