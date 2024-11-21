using System.Collections.Generic;
using UnityEngine;

public class Canvas: MonoBehaviour
{
    public Transform vrCamera;
    public HandGestureRecognizer gestureRecognizer;
    public float distance = 0.3f;
    public Vector3 offset = Vector3.zero;

    public Material lineMaterial;
    public float lineWidth = 0.1f;
    public float minDistanceThreshold = 0.001f;

    private LineRenderer currentLine;
    private List<Vector3> drawingPoints = new List<Vector3>();

    private bool isFingerTouchingBoard = false;
    private Vector3 contactPoint;

    private bool isDrawing = false;

    void Update()
    {
        UpdateCanvasPositionAndRotation();

        // 检查手势识别和绘制状态
        if (gestureRecognizer.ovrHand != null && gestureRecognizer.ovrHand.IsTracked)
        {
            if (gestureRecognizer.IsGestureRecognized() && isFingerTouchingBoard)
            {
                StartDrawing();
                DrawOnBoard();
            }
            else
            {
                StopDrawing();
            }
        }
    }

    void UpdateCanvasPositionAndRotation()
    {
        //if (vrCamera != null)
        //{
        //    // 设置画板位置
        //    transform.position = vrCamera.position + vrCamera.forward * distance + offset;

        //    // 使画板始终面向玩家
        //    transform.rotation = Quaternion.LookRotation(transform.position - vrCamera.position, Vector3.up);
        //}
    }

    void OnTriggerEnter(Collider other)
    {
        isFingerTouchingBoard = true;
        Debug.Log("Finger started touching the canvas.");
    }

    void OnTriggerExit(Collider other)
    {
        isFingerTouchingBoard = false;
        Debug.Log("Finger stopped touching the canvas.");
    }

    void OnTriggerStay(Collider other)
    {
        isFingerTouchingBoard = true;
        contactPoint = GetContactPoint(other);
    }
    Vector3 GetContactPoint(Collider fingerCollider)
    {
        Vector3 fingerTipPosition = fingerCollider.transform.position;

        Vector3 closestPoint = GetComponent<Collider>().ClosestPoint(fingerTipPosition);

        return closestPoint;
    }
    void StartDrawing()
    {
        if (!isDrawing)
        {
            isDrawing = true;
            drawingPoints.Clear();

            // 创建新的线条对象
            GameObject lineObj = new GameObject("Line");
            lineObj.transform.SetParent(transform, false); // 将其设置为画板的子对象，不保持世界坐标

            currentLine = lineObj.AddComponent<LineRenderer>();
            currentLine.material = lineMaterial;
            currentLine.startWidth = lineWidth;
            currentLine.endWidth = lineWidth;
            currentLine.positionCount = 0;
            currentLine.useWorldSpace = false; // 使用本地坐标空间

            Debug.Log("StartDrawing: LineRenderer created.");
        }
    }

    void StopDrawing()
    {
        if (isDrawing)
        {
            isDrawing = false;
            currentLine = null;
        }
    }


    void DrawOnBoard()
    {
        if (currentLine == null)
        {
            Debug.LogError("DrawOnBoard: currentLine is null!");
            return;
        }

        Vector3 localPoint = transform.InverseTransformPoint(contactPoint);

        // 检查与上一个点的距离
        if (drawingPoints.Count == 0 || Vector3.Distance(drawingPoints[drawingPoints.Count - 1], localPoint) > minDistanceThreshold)
        {
            drawingPoints.Add(localPoint);
            currentLine.positionCount = drawingPoints.Count;
            currentLine.SetPosition(currentLine.positionCount - 1, localPoint);
            DrawSmoothedLine();
            Debug.Log($"DrawOnBoard: Drawing at local point {localPoint}, total points: {drawingPoints.Count}");
        }
    }


    void DrawSmoothedLine()
    {
        if (currentLine == null || drawingPoints.Count < 2)
        {
            return;
        }

        int smoothAmount = 10; // 每段曲线的插值点数量，根据需要调整
        List<Vector3> smoothedPoints = new List<Vector3>();

        for (int i = 0; i < drawingPoints.Count - 1; i++)
        {
            Vector3 p0 = i > 0 ? drawingPoints[i - 1] : drawingPoints[i];
            Vector3 p1 = drawingPoints[i];
            Vector3 p2 = drawingPoints[i + 1];
            Vector3 p3 = i < drawingPoints.Count - 2 ? drawingPoints[i + 2] : drawingPoints[i + 1];

            for (int j = 0; j < smoothAmount; j++)
            {
                float t = j / (float)smoothAmount;
                Vector3 point = GetCatmullRomPosition(t, p0, p1, p2, p3);
                smoothedPoints.Add(point);
            }
        }

        currentLine.positionCount = smoothedPoints.Count;
        currentLine.SetPositions(smoothedPoints.ToArray());
    }

    Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // Catmull-Rom spline formula
        float t2 = t * t;
        float t3 = t2 * t;

        float a0 = -0.5f * t3 + t2 - 0.5f * t;
        float a1 = 1.5f * t3 - 2.5f * t2 + 1.0f;
        float a2 = -1.5f * t3 + 2.0f * t2 + 0.5f * t;
        float a3 = 0.5f * t3 - 0.5f * t2;

        return a0 * p0 + a1 * p1 + a2 * p2 + a3 * p3;
    }



}
