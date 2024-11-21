using System.Collections.Generic;
using UnityEngine;

public class Talisman : MonoBehaviour
{
    public HandGestureRecognizer gestureRecognizer; // 手势识别器
    public LayerMask canvasLayerMask;           // 画板的 Layer，用于碰撞检测
    public LineRenderer lineRenderer;              // LineRenderer 预制体
    //public Transform indexFingerTransform;      // 食指位置（从 GestureRecognizer 获取）

    private LineRenderer currentLine;           // 当前绘制的 LineRenderer
    private List<Vector3> points = new List<Vector3>(); // 记录绘制点的列表

    void Update()
    {
        

        //// 检查手势是否被识别
        //if (gestureRecognizer.IsGestureRecognized())
        //{
        //    // 使用射线检测食指是否触碰到画板
        //    Vector3 fingerTipPosition = gestureRecognizer.GetIndexFingerTipPosition();
        //    if (fingerTipPosition != Vector3.zero)
        //    {
        //        // 使用射线检测是否触碰到画板
        //        Ray ray = new Ray(fingerTipPosition, Vector3.forward);
        //        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, canvasLayerMask))
        //        {
        //            if (hit.collider.CompareTag("Canvas"))
        //            {
        //                Draw(hit.point); // 在画板上绘制
        //                Debug.Log("Painting");
        //            }
        //        }
        //    }
        //}
        //else
        //{
        //    EndLine(); // 手势取消时结束线条
        //}
    }


    void Draw(Vector3 position)
    {
        if (currentLine == null)
        {
            StartLine(position); // 开始一条新线
        }

        // 只有当点的位置显著改变时，才添加新点，避免冗余
        if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], position) > 0.01f)
        {
            points.Add(position);
            currentLine.positionCount = points.Count;
            currentLine.SetPositions(points.ToArray());
        }
    }

    void StartLine(Vector3 startPosition)
    {
        // 创建新的 LineRenderer 对象
        currentLine = lineRenderer;
        points.Clear();
        points.Add(startPosition);

        // 初始化线条样式
        currentLine.positionCount = 1;
        currentLine.SetPosition(0, startPosition);
    }

    void EndLine()
    {
        currentLine = null; // 重置当前线条
    }
}
