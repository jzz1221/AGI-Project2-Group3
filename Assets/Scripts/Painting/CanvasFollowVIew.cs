using UnityEngine;

public class CanvasFollowView : MonoBehaviour
{
    public Transform vrCamera;
    public HandGestureRecognizerWithPainting gestureRecognizer;
    public float distance = 0.3f;
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
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("_______enter the canvas_______");
        gestureRecognizer.PaintingMode = true;
    }

    private void OnTriggerExit(Collider other)
    {
        gestureRecognizer.PaintingMode = false;
    }

}
