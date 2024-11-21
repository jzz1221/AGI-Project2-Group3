using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingReceiver : MonoBehaviour
{
    public CanvasFollowView canvasFollowView; // Must be assigned manually in the Inspector

    void Start()
    {
        if (canvasFollowView != null)
        {
            // Subscribe to the OnDrawingFinished event
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
            // Unsubscribe from the event to prevent potential memory leaks
            canvasFollowView.OnDrawingFinished -= HandleDrawingFinished;
        }
    }

    // Event handler method called when drawing is finished
    private void HandleDrawingFinished(List<Vector3> drawingPoints)
    {
        Debug.Log("Drawing finished with " + drawingPoints.Count + " points.");

        // Process the array of drawing points here, e.g., generate an image or perform other actions
        // Example: GenerateImageFromPoints(drawingPoints);
    }

    // Example method: Generate an image from drawing points (to be implemented as needed)
    private void GenerateImageFromPoints(List<Vector3> points)
    {
        // Implement the logic to convert 3D points into a 2D image
        // This implementation depends on your specific requirements and approach
    }
}
