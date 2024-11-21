using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingReceiver : MonoBehaviour
{
    public CanvasFollowView canvasFollowView; // ��Ҫ�� Inspector ���ֶ�����

    void Start()
    {
        if (canvasFollowView != null)
        {
            // ���� OnDrawingFinished �¼�
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
            // ȡ�������¼�������Ǳ�ڵ��ڴ�й©
            canvasFollowView.OnDrawingFinished -= HandleDrawingFinished;
        }
    }

    // �¼������������滭����ʱ����
    private void HandleDrawingFinished(List<Vector3> drawingPoints)
    {
        Debug.Log("Drawing finished with " + drawingPoints.Count + " points.");

        // �����ﴦ����Ƶĵ����飬��������ͼƬ����������
        // ���磺GenerateImageFromPoints(drawingPoints);
    }

    // ʾ�����������ݻ��Ƶ�����ͼƬ����Ҫ���ݾ�������ʵ�֣�
    private void GenerateImageFromPoints(List<Vector3> points)
    {
        // ʵ�ֽ� 3D ��ת��Ϊ 2D ͼƬ���߼�
        // �ⲿ�ֵ�ʵ��ȡ�������ľ�������ͷ���
    }
}
