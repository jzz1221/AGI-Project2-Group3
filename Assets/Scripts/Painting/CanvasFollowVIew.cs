using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using System; // Import System namespace to use Action
using TMPro;

public class CanvasFollowView : MonoBehaviour
{
    public Transform vrCamera;
    public HandGestureRecognizerWithPainting gestureRecognizer;
    public float distance = 0.3f;
    public Vector3 offset = Vector3.zero; // Offset of the canvas relative to the camera

    public Material lineMaterial;
    public float lineWidth = 0.1f;
    public float minDistanceThreshold = 0.001f;
    public bool PaintingMode = false;

    private LineRenderer currentLine;
    private List<Vector3> drawingPoints = new List<Vector3>();
    private int drawingPointsNumber = 0;

    private bool isDrawing = false;
    private bool canvasLocked = false;

    private Coroutine unlockCanvasCoroutine; // Stores the coroutine for unlocking the canvas
    
    public Plane paintingPlane;
    public Transform paintingPlaneTransform;
    public Transform reslutPlaneTransform;

    //---------for debug----------
    public TextMeshProUGUI PointTextforDebug;
    public TextMeshProUGUI ResultText;
    // Define an event that triggers when drawing finishes, passing the list of drawn points
    public event Action<List<Vector3>> OnDrawingFinished;
    public event Action<List<Vector3>> OnSymbolMatchingRequested;

    void Start()
    {
        paintingPlane = new Plane(paintingPlaneTransform.up, paintingPlaneTransform.position);
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
    }

    private void StartDrawing()
    {
        if (!isDrawing)
        {
            isDrawing = true;
            canvasLocked = true; // Lock the canvas to prevent movement
            drawingPoints.Clear();

            // Create a new line object
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

            // Only proceed if points were drawn
            if (drawingPoints.Count > 2)
            {
                // Trigger the OnDrawingFinished event, passing the drawn points
                OnDrawingFinished?.Invoke(new List<Vector3>(drawingPoints));
            }

            // Clear the current line
            if (currentLine != null)
            {
                Destroy(currentLine.gameObject); // Destroy the line's GameObject
                currentLine = null;
            }

            drawingPoints.Clear(); // Clear the drawing points list
        }
    }


    private void Draw()
    {
        // Initialize drawing if it hasn't started
        if (!isDrawing)
        {
            StartDrawing();
        }

        Vector3 fingerTipPosition = gestureRecognizer.GetIndexFingerTipPosition();
        Debug.Log("Finger Tip Position: " + fingerTipPosition);
        drawingPoints.Add(fingerTipPosition);
        drawingPointsNumber++;
        if (drawingPointsNumber > 120)
        {
            StartCoroutine(TriggerDrawingFinishedAsync(new List<Vector3>(drawingPoints)));
            drawingPointsNumber = 0;
        }
        currentLine.positionCount = drawingPoints.Count;
        currentLine.SetPositions(drawingPoints.ToArray());
        PointTextforDebug.text = $"Drawingpointsnumber"+drawingPointsNumber + "total points number" + drawingPoints.Count;

        if (drawingPoints.Count > 250)
        {
            //clean the first symbol
            //first 50points or?
        }
    }
    
    private IEnumerator TriggerDrawingFinishedAsync(List<Vector3> points)
    {
        yield return null;

        OnSymbolMatchingRequested?.Invoke(points);
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
