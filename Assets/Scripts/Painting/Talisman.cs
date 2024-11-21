using System.Collections.Generic;
using UnityEngine;

public class Talisman : MonoBehaviour
{
    public HandGestureRecognizer gestureRecognizer; // ����ʶ����
    public LayerMask canvasLayerMask;           // ����� Layer��������ײ���
    public LineRenderer lineRenderer;              // LineRenderer Ԥ����
    //public Transform indexFingerTransform;      // ʳָλ�ã��� GestureRecognizer ��ȡ��

    private LineRenderer currentLine;           // ��ǰ���Ƶ� LineRenderer
    private List<Vector3> points = new List<Vector3>(); // ��¼���Ƶ���б�

    void Update()
    {
        

        //// ��������Ƿ�ʶ��
        //if (gestureRecognizer.IsGestureRecognized())
        //{
        //    // ʹ�����߼��ʳָ�Ƿ���������
        //    Vector3 fingerTipPosition = gestureRecognizer.GetIndexFingerTipPosition();
        //    if (fingerTipPosition != Vector3.zero)
        //    {
        //        // ʹ�����߼���Ƿ���������
        //        Ray ray = new Ray(fingerTipPosition, Vector3.forward);
        //        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, canvasLayerMask))
        //        {
        //            if (hit.collider.CompareTag("Canvas"))
        //            {
        //                Draw(hit.point); // �ڻ����ϻ���
        //                Debug.Log("Painting");
        //            }
        //        }
        //    }
        //}
        //else
        //{
        //    EndLine(); // ����ȡ��ʱ��������
        //}
    }


    void Draw(Vector3 position)
    {
        if (currentLine == null)
        {
            StartLine(position); // ��ʼһ������
        }

        // ֻ�е����λ�������ı�ʱ��������µ㣬��������
        if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], position) > 0.01f)
        {
            points.Add(position);
            currentLine.positionCount = points.Count;
            currentLine.SetPositions(points.ToArray());
        }
    }

    void StartLine(Vector3 startPosition)
    {
        // �����µ� LineRenderer ����
        currentLine = lineRenderer;
        points.Clear();
        points.Add(startPosition);

        // ��ʼ��������ʽ
        currentLine.positionCount = 1;
        currentLine.SetPosition(0, startPosition);
    }

    void EndLine()
    {
        currentLine = null; // ���õ�ǰ����
    }
}
