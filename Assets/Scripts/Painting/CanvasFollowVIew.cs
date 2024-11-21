using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using System; // 引入 System 命名空间以使用 Action

public class CanvasFollowView : MonoBehaviour
{
    public Transform vrCamera;
    public HandGestureRecognizerWithPainting gestureRecognizer;
    public float distance = 0.3f;
    public Vector3 offset = Vector3.zero; // 画板相对于相机的偏移量

    public Material lineMaterial;
    public float lineWidth = 0.1f;
    public float minDistanceThreshold = 0.001f;
    public bool PaintingMode = false;

    private LineRenderer currentLine;
    private List<Vector3> drawingPoints = new List<Vector3>();

    private bool isFingerTouchingBoard = false;
    private Vector3 contactPoint;

    private bool isDrawing = false;
    private bool canvasLocked = false; // 添加画板是否锁定的状态

    private Coroutine unlockCanvasCoroutine; // 用于存储解锁画板的协程

    // 定义一个事件，当绘画结束时触发，并传递绘制的点数组
    public event Action<List<Vector3>> OnDrawingFinished;

    void Update()
    {
        // 如果画板没有被锁定，才更新其位置和朝向
        if (!canvasLocked && vrCamera != null)
        {
            // 设置画板位置
            transform.position = vrCamera.position + vrCamera.forward * distance + offset;

            // 设置画板始终面朝玩家
            transform.rotation = Quaternion.LookRotation(transform.position - vrCamera.position, Vector3.up);
        }

        // 检查是否开始绘画
        if (gestureRecognizer.ovrHand.IsTracked)
        {
            if (PaintingMode)
            {
                // 检查手势是否被识别
                if (gestureRecognizer.IsGestureRecognized())
                {
                    Debug.Log("Gesture recognized!");
                    Draw();
                }
            }
        }
    }

    private void StartDrawing()
    {
        if (!isDrawing)
        {
            isDrawing = true;
            canvasLocked = true; // 锁定画板，禁止移动
            drawingPoints.Clear();

            GameObject lineObj = new GameObject("Line");
            lineObj.transform.SetParent(transform, false);

            currentLine = lineObj.AddComponent<LineRenderer>();
            currentLine.material = lineMaterial;
            currentLine.startWidth = lineWidth;
            currentLine.endWidth = lineWidth;
            currentLine.positionCount = 0;
        }
    }

    private void StopDrawing()
    {
        if (isDrawing)
        {
            isDrawing = false;
            currentLine = null;

            // 仅当实际绘制了点时，才进行后续处理
            if (drawingPoints.Count > 0)
            {
                // 触发 OnDrawingFinished 事件，传递当前绘制的点
                OnDrawingFinished?.Invoke(new List<Vector3>(drawingPoints));
            }

            drawingPoints.Clear();
        }
    }

    private void Draw()
    {
        // 如果还没有开始绘制，初始化绘制
        if (!isDrawing)
        {
            StartDrawing();
        }

        Vector3 fingerTipPosition = gestureRecognizer.GetIndexFingerTipPosition();
        Debug.Log("-----------" + fingerTipPosition + "----------");
        drawingPoints.Add(fingerTipPosition);
        currentLine.positionCount = drawingPoints.Count;
        currentLine.SetPositions(drawingPoints.ToArray());
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("_______enter the canvas_______");
        if (unlockCanvasCoroutine != null)
        {
            StopCoroutine(unlockCanvasCoroutine); // 如果存在解锁协程，停止它
            unlockCanvasCoroutine = null;
        }
        PaintingMode = true;
        // 不再在这里调用 StartDrawing()
    }

    private void OnTriggerExit(Collider other)
    {
        PaintingMode = false;
        StopDrawing();
        if (unlockCanvasCoroutine != null)
        {
            StopCoroutine(unlockCanvasCoroutine);
        }
        unlockCanvasCoroutine = StartCoroutine(UnlockCanvasAfterDelay(2f)); // 开始计时两秒后解锁
    }

    private IEnumerator UnlockCanvasAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canvasLocked = false; // 解锁画板
        unlockCanvasCoroutine = null;
    }
}
