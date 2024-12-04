using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using System; // Import System namespace to use Action
using TMPro;
using Unity.VisualScripting;

public class CanvasFollowView : MonoBehaviour
{
    public Transform vrCamera;
    public HandGestureRecognizerWithPainting gestureRecognizer;
    public float distance = 0.3f;
    public Vector3 offset = Vector3.zero; // Offset of the canvas relative to the camera

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

    private Coroutine unlockCanvasCoroutine; // Stores the coroutine for unlocking the canvas
    
    public Plane paintingPlane;
    public Transform paintingPlaneTransform;
    public Transform reslutPlaneTransform;

    public float minMatchingScore = 0.5f;

    //---------for debug----------
    public TextMeshProUGUI PointTextforDebug;
    public TextMeshProUGUI ResultText;
    public TextMeshProUGUI IsDrawing;

    private bool isLine;
    // Define an event that triggers when drawing finishes, passing the list of drawn points
    public event Action<List<Vector3>, GameObject> OnDrawingFinished;
    public event Action<List<Vector3>, GameObject> OnSymbolMatchingRequested;

    void Start()
    {
        paintingPlane = new Plane(paintingPlaneTransform.up, paintingPlaneTransform.position);
        drawingReceiver.OnSymbolMatchingResult += HandleMatchingResult;
        // Initialize the StrokeManager
        StrokeManager.Instance.Initialize(lineMaterial, lineWidth);
    }
    
    void OnDestroy()
    {
        if (drawingReceiver != null) drawingReceiver.OnSymbolMatchingResult -= HandleMatchingResult;
    }

    void Update()
    {
        // Update canvas position and orientation only if it's not locked
        if (!canvasLocked && vrCamera != null)
        {
            transform.position = vrCamera.position + vrCamera.forward * distance + offset;
            transform.rotation = Quaternion.LookRotation(transform.position - vrCamera.position, Vector3.up);
        }

        // Check if drawing should start
        if (gestureRecognizer.ovrHand.IsTracked)
        {
            if (PaintingMode)
            {
                if (gestureRecognizer.IsGestureRecognized())
                {
                    Debug.Log("Gesture recognized!");
                    Draw();
                }
            }
        }
        if(currentLine== null) isLine = false;
        else isLine = true;
        //IsDrawing.text = "paintingmode?" + PaintingMode + "IsDrawing?" + isDrawing + "line" + isLine;
    }

    private void StartDrawing()
    {
        if (!isDrawing)
        {
            isDrawing = true;
            canvasLocked = true; // Lock the canvas to prevent movement
            drawingPoints.Clear();
            
            //using new class to draw
            currentLine = StrokeManager.Instance.StartStroke(transform, true,"DrawingLine");
        }
    }

    private void StopDrawing()
    {
        if (isDrawing)
        {
            isDrawing = false;

            // Only proceed if points were drawn
            if (drawingPoints.Count > 2)
            {
                // Trigger the OnDrawingFinished event, passing the drawn points
                OnDrawingFinished?.Invoke(drawingPoints,currentLine.gameObject);
            }

            // Clear the current line
            if (currentLine != null) currentLine = null;
            drawingPointsNumber = 0;
            drawingPoints.Clear(); // Clear the drawing points list
        }
    }

    private void Draw()
    {
        // Initialize drawing if it hasn't started
        if (!isDrawing || currentLine == null)
        {
            StartDrawing();
        }

        Vector3 fingerTipPosition = gestureRecognizer.GetIndexFingerTipPosition();
        Debug.Log("Finger Tip Position: " + fingerTipPosition);
        drawingPoints.Add(fingerTipPosition);
        
        StrokeManager.Instance.SetStrokePoints(currentLine, drawingPoints);
        
        drawingPointsNumber++;
        if (drawingPointsNumber> 150 && Detecting == false)
        {
            //creat a new Stroke to cut Stroke and trigger symbolMatching
            StartCoroutine(TriggerSymbolMatchingAsync(new List<Vector3>(drawingPoints), currentLine.gameObject));
            Detecting = true;
        }
        else if (drawingPointsNumber> 90 && Detecting == false)
        {
            StartCoroutine(TriggerSymbolMatchingAsync(new List<Vector3>(drawingPoints), currentLine.gameObject));
            Detecting = true;
        }
        PointTextforDebug.text = $"Drawingpointsnumber"+drawingPointsNumber + "total points number" + drawingPoints.Count;
    }
    
    private IEnumerator TriggerSymbolMatchingAsync(List<Vector3> points, GameObject SymbolObject)
    {
        yield return null;

        OnSymbolMatchingRequested?.Invoke(points,SymbolObject);
    }
    
    private void HandleMatchingResult(string gestureClass, float score)
    {
        if (!isDrawing) StrokeManager.Instance.ChangeStrokeGroup
            (currentLine.gameObject, "DrawingLine Recognized"); // if that is the last stroke
        else if (score > minMatchingScore || drawingPoints.Count > 90)
        {
            // Change stroke group only when the score condition is met
            if (score > minMatchingScore){ 
                StrokeManager.Instance.ChangeStrokeGroup
                    (currentLine.gameObject, "DrawingLine Recognized");
                StrokeManager.Instance.ChangeMaterial(currentLine, SucceedMaterial);
                drawingPointsNumber = 0;
            }
            else if(drawingPoints.Count > 90) {
                StrokeManager.Instance.ChangeStrokeGroup
                    (currentLine.gameObject, "DrawingLine Recognized");
                drawingPointsNumber = 0;
            }
            
            // Reset the current line and related variables
            currentLine = null;
            currentLine = StrokeManager.Instance.StartStroke(transform, true, "DrawingLine");
            drawingPoints.Clear();
        }
        Detecting = false;
        Debug.Log($"Gesture: {gestureClass}, Score: {score}");
    }
    
    
    public void UpdateResultText(string result)
    {
        //PointTextforDebug.text = $"Points:{points}";
        ResultText.text = $"Result: {result}";
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Finger entered the canvas.");
        if (unlockCanvasCoroutine != null)
        {
            StopCoroutine(unlockCanvasCoroutine); // Stop any ongoing unlock coroutine
            unlockCanvasCoroutine = null;
        }
        PaintingMode = true;
        // StartDrawing is no longer called here
    }

    private void OnTriggerExit(Collider other)
    {
        PaintingMode = false;
        //StopDrawing();
        if (unlockCanvasCoroutine != null)
        {
            StopCoroutine(unlockCanvasCoroutine);
        }
        unlockCanvasCoroutine = StartCoroutine(UnlockCanvasAfterDelay(1f)); // Start a coroutine to unlock the canvas after 1 second
    }

    private IEnumerator UnlockCanvasAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StopDrawing();
        canvasLocked = false; // Unlock the canvas
        unlockCanvasCoroutine = null;
    }
}
