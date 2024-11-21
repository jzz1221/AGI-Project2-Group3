using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using System; // ���� System �����ռ���ʹ�� Action

public class CanvasFollowView : MonoBehaviour
{
    public Transform vrCamera;
    public HandGestureRecognizerWithPainting gestureRecognizer;
    public float distance = 0.3f;
    public Vector3 offset = Vector3.zero; // ��������������ƫ����

    public Material lineMaterial;
    public float lineWidth = 0.1f;
    public float minDistanceThreshold = 0.001f;
    public bool PaintingMode = false;

    private LineRenderer currentLine;
    private List<Vector3> drawingPoints = new List<Vector3>();

    private bool isFingerTouchingBoard = false;
    private Vector3 contactPoint;

    private bool isDrawing = false;
    private bool canvasLocked = false; // ��ӻ����Ƿ�������״̬

    private Coroutine unlockCanvasCoroutine; // ���ڴ洢���������Э��

    // ����һ���¼������滭����ʱ�����������ݻ��Ƶĵ�����
    public event Action<List<Vector3>> OnDrawingFinished;

    void Update()
    {
        // �������û�б��������Ÿ�����λ�úͳ���
        if (!canvasLocked && vrCamera != null)
        {
            // ���û���λ��
            transform.position = vrCamera.position + vrCamera.forward * distance + offset;

            // ���û���ʼ���泯���
            transform.rotation = Quaternion.LookRotation(transform.position - vrCamera.position, Vector3.up);
        }

        // ����Ƿ�ʼ�滭
        if (gestureRecognizer.ovrHand.IsTracked)
        {
            if (PaintingMode)
            {
                // ��������Ƿ�ʶ��
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
            canvasLocked = true; // �������壬��ֹ�ƶ�
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

            // ����ʵ�ʻ����˵�ʱ���Ž��к�������
            if (drawingPoints.Count > 0)
            {
                // ���� OnDrawingFinished �¼������ݵ�ǰ���Ƶĵ�
                OnDrawingFinished?.Invoke(new List<Vector3>(drawingPoints));
            }

            drawingPoints.Clear();
        }
    }

    private void Draw()
    {
        // �����û�п�ʼ���ƣ���ʼ������
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
            StopCoroutine(unlockCanvasCoroutine); // ������ڽ���Э�̣�ֹͣ��
            unlockCanvasCoroutine = null;
        }
        PaintingMode = true;
        // ������������� StartDrawing()
    }

    private void OnTriggerExit(Collider other)
    {
        PaintingMode = false;
        StopDrawing();
        if (unlockCanvasCoroutine != null)
        {
            StopCoroutine(unlockCanvasCoroutine);
        }
        unlockCanvasCoroutine = StartCoroutine(UnlockCanvasAfterDelay(2f)); // ��ʼ��ʱ��������
    }

    private IEnumerator UnlockCanvasAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canvasLocked = false; // ��������
        unlockCanvasCoroutine = null;
    }
}
