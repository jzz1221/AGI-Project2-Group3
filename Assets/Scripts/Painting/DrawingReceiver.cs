using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingReceiver : MonoBehaviour
{
    public CanvasFollowView canvasFollowView; // 需要在 Inspector 中手动分配

    void Start()
    {
        if (canvasFollowView != null)
        {
            // 订阅 OnDrawingFinished 事件
            canvasFollowView.OnDrawingFinished += HandleDrawingFinished;
        }
        else
        {
            Debug.LogError("CanvasFollowView is not assigned in DrawingReceiver.");
        }
    }

    void OnDestroy()
    {
        if (canvasFollowView != null)
        {
            // 取消订阅事件，避免潜在的内存泄漏
            canvasFollowView.OnDrawingFinished -= HandleDrawingFinished;
        }
    }

    // 事件处理方法，当绘画结束时调用
    private void HandleDrawingFinished(List<Vector3> drawingPoints)
    {
        Debug.Log("Drawing finished with " + drawingPoints.Count + " points.");

        // 在这里处理绘制的点数组，例如生成图片或其他操作
        // 例如：GenerateImageFromPoints(drawingPoints);
    }

    // 示例方法：根据绘制点生成图片（需要根据具体需求实现）
    private void GenerateImageFromPoints(List<Vector3> points)
    {
        // 实现将 3D 点转换为 2D 图片的逻辑
        // 这部分的实现取决于您的具体需求和方法
    }
}
