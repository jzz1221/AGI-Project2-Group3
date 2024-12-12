using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using System;
using TMPro;
using Unity.VisualScripting;

public class CanvasFollowView : MonoBehaviour
{
    public Transform vrCamera;
    public HandGestureRecognizerWithPainting gestureRecognizer;
    public float distance = 0.3f;
    public Vector3 offset = Vector3.zero;
    public Material lineMaterial;
    public Material SucceedMaterial;
    public float lineWidth = 0.1f;
    public float minDistanceThreshold = 0.001f;
    public bool PaintingMode = false;

    private LineRenderer currentLine;
    private List<Vector3> drawingPoints = new List<Vector3>();
    private int drawingPointsNumber = 0;
    public DrawingReceiver drawingReceiver;
    private bool Detecting = false;

    private List<GameObject> LineRendererObjects = new List<GameObject>();

    private bool isDrawing = false;
    private bool canvasLocked = false;
    private Coroutine unlockCanvasCoroutine;

    public Plane paintingPlane;
    public Transform paintingPlaneTransform;
    public Transform reslutPlaneTransform;
    public float minMatchingScore = 0.5f;

    // Debug fields
    public TextMeshProUGUI PointTextforDebug;
    public TextMeshProUGUI ResultText;
    public TextMeshProUGUI IsDrawing;

    private bool isLine;

    // 新增的引用
    private GameObject beamObject;
    private List<GameObject> effectObjects = new List<GameObject>();

    public event Action<List<Vector3>, GameObject> OnDrawingFinished;
    public event Action<List<Vector3>, GameObject> OnSymbolMatchingRequested;

    void Start()
    {
        paintingPlane = new Plane(paintingPlaneTransform.up, paintingPlaneTransform.position);
        drawingReceiver.OnSymbolMatchingResult += HandleMatchingResult;
        StrokeManager.Instance.Initialize(lineMaterial, lineWidth);

        // 初始化beam和effect对象
        beamObject = GameObject.Find("Beam");
        var allEffectObjects = GameObject.FindGameObjectsWithTag("effect");
        foreach (var obj in allEffectObjects)
        {
            effectObjects.Add(obj);
            obj.SetActive(false); // 初始状态设为false
        }
        if (beamObject != null) beamObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (drawingReceiver != null) drawingReceiver.OnSymbolMatchingResult -= HandleMatchingResult;
    }

    void Update()
    {
        // 如果画布未锁定，实时更新位置
        if (!canvasLocked && vrCamera != null)
        {
            transform.position = vrCamera.position + vrCamera.forward * distance + offset;
            transform.rotation = Quaternion.LookRotation(transform.position - vrCamera.position, Vector3.up);
        }

        // 当手跟踪并在PaintingMode下手势识别成功时开始绘画
        if (gestureRecognizer.ovrHand.IsTracked)
        {
            if (PaintingMode && gestureRecognizer.IsGestureRecognized())
            {
                Debug.Log("Gesture recognized!");
                if (beamObject != null) beamObject.SetActive(true); // 手势识别成功，激活beam
                Draw();
            }
        }

        if (currentLine == null) isLine = false;
        else isLine = true;
    }

    private void StartDrawing()
    {
        if (!isDrawing)
        {
            isDrawing = true;
            canvasLocked = true;
            drawingPoints.Clear();
            currentLine = StrokeManager.Instance.StartStroke(transform, true, "DrawingLine");

            // 开始绘画时，将所有effect对象激活
            foreach (var effectObj in effectObjects)
            {
                effectObj.SetActive(true);
            }
        }
    }

    private void StopDrawing()
    {
        if (isDrawing)
        {
            isDrawing = false;

            if (drawingPoints.Count > 2)
            {
                OnDrawingFinished?.Invoke(drawingPoints, currentLine.gameObject);
            }
            else
            {
                StrokeManager.Instance.ChangeStrokeGroup(currentLine.gameObject, "DrawingLine Recognized");
            }

            // 绘画结束后将beam和effect对象全部关闭
            if (beamObject != null) beamObject.SetActive(false);
            foreach (var effectObj in effectObjects)
            {
                effectObj.SetActive(false);
            }

            if (currentLine != null) currentLine = null;
            drawingPointsNumber = 0;
            drawingPoints.Clear();
        }
    }

    private void Draw()
    {
        if (!isDrawing || currentLine == null)
        {
            StartDrawing();
        }

        Vector3 fingerTipPosition = gestureRecognizer.GetIndexFingerTipPosition();
        Debug.Log("Finger Tip Position: " + fingerTipPosition);
        drawingPoints.Add(fingerTipPosition);

        StrokeManager.Instance.SetStrokePoints(currentLine, drawingPoints);
        drawingPointsNumber++;

        if (drawingPointsNumber > 150 && Detecting == false)
        {
            StartCoroutine(TriggerSymbolMatchingAsync(new List<Vector3>(drawingPoints), currentLine.gameObject));
            Detecting = true;
        }
        else if (drawingPointsNumber > 90 && Detecting == false)
        {
            StartCoroutine(TriggerSymbolMatchingAsync(new List<Vector3>(drawingPoints), currentLine.gameObject));
            Detecting = true;
        }
        PointTextforDebug.text = $"Drawingpointsnumber: {drawingPointsNumber} total points number: {drawingPoints.Count}";
    }

    private IEnumerator TriggerSymbolMatchingAsync(List<Vector3> points, GameObject SymbolObject)
    {
        yield return null;
        OnSymbolMatchingRequested?.Invoke(points, SymbolObject);
    }

    private void HandleMatchingResult(string gestureClass, float score)
    {
        if (!isDrawing)
            StrokeManager.Instance.ChangeStrokeGroup(currentLine.gameObject, "DrawingLine Recognized");
        else if (score > minMatchingScore || drawingPoints.Count > 90)
        {
            StrokeManager.Instance.ChangeStrokeGroup(currentLine.gameObject, "DrawingLine Recognized");
            if (score > minMatchingScore) StrokeManager.Instance.ChangeMaterial(currentLine, SucceedMaterial);
            drawingPointsNumber = 0;
            currentLine = null;
            currentLine = StrokeManager.Instance.StartStroke(transform, true, "DrawingLine");
            drawingPoints.Clear();
        }
        Detecting = false;
        Debug.Log($"Gesture: {gestureClass}, Score: {score}");
    }

    public void UpdateResultText(string result)
    {
        ResultText.text = $"Result: {result}";
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Finger entered the canvas.");
        if (unlockCanvasCoroutine != null)
        {
            StopCoroutine(unlockCanvasCoroutine);
            unlockCanvasCoroutine = null;
        }
        PaintingMode = true;
    }

    private void OnTriggerExit(Collider other)
    {
        PaintingMode = false;
        if (unlockCanvasCoroutine != null)
        {
            StopCoroutine(unlockCanvasCoroutine);
        }
        unlockCanvasCoroutine = StartCoroutine(UnlockCanvasAfterDelay(1f));
    }

    private IEnumerator UnlockCanvasAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StopDrawing();
        canvasLocked = false;
        unlockCanvasCoroutine = null;
    }
}
