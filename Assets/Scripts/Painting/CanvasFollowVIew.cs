using UnityEngine;

public class CanvasFollowView : MonoBehaviour
{
    public Transform vrCamera;
    public HandGestureRecognizerWithPainting gestureRecognizer;
    public float distance = 0.3f;
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
