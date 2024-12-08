using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PDollarGestureRecognizer;
using System.IO;
using System;

public class DrawingReceiver : MonoBehaviour
{
    public CanvasFollowView canvasFollowView; // Must be assigned manually in the Inspector
    private Material defultMaterial;
    private float defaultLineWidth;

    private Plane paintingPlane;
    private Transform paintingPlaneTransform;
    private Transform ResultPlaneTransform;
    private List<Point> points = new List<Point>();
    private List<Gesture> trainingSet = new List<Gesture>();
    
    private Result result;
    private List<Result> results = new List<Result>();
    
    public event Action<string, float> OnSymbolMatchingResult;

    void Start()
    {
        defultMaterial = canvasFollowView.lineMaterial;
        defaultLineWidth = canvasFollowView.lineWidth;
        StrokeManager.Instance.Initialize(defultMaterial, defaultLineWidth);
        
        //Load pre-made gestures
        TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("GestureSet/10-stylus-MEDIUM/");
        foreach (TextAsset gestureXml in gesturesXml)
        {
            Debug.Log($"Loaded gesture: {gestureXml.name}");
            trainingSet.Add(GestureIO.ReadGestureFromXML(gestureXml.text));
        }

        //Load user custom gestures
        string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*.xml");
        foreach (string filePath in filePaths)
        {
            Debug.Log($"Loading file: {filePath}");
            trainingSet.Add(GestureIO.ReadGestureFromFile(filePath));
        }
        
        paintingPlane = canvasFollowView.paintingPlane;
        ResultPlaneTransform = canvasFollowView.reslutPlaneTransform;
        paintingPlaneTransform = canvasFollowView.paintingPlaneTransform;
        
        if (canvasFollowView != null)
        {
            // Subscribe to the OnDrawingFinished event
            canvasFollowView.OnDrawingFinished += HandleDrawingFinished;
            canvasFollowView.OnSymbolMatchingRequested += HandleMatchingRequestedl;
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
            canvasFollowView.OnSymbolMatchingRequested -= HandleMatchingRequestedl;
            canvasFollowView.OnDrawingFinished -= HandleDrawingFinished;
        }
    }

    // Event handler method called when drawing is finished
    private void HandleMatchingRequestedl(List<Vector3> drawingPoints, GameObject LineObject)
    {
        Debug.Log("Processing gesture matching...");
        if (drawingPoints.Count > 2)
        {
            Vector3 planeOrigin = paintingPlaneTransform.position;
            List<Vector2> projectedPoints = ProjectPointsToPlane(drawingPoints, planeOrigin);

            points.Clear();
            foreach (Vector2 p in projectedPoints)
            {
                points.Add(new Point(p.x, p.y, 1));
            }

            Gesture candidate = new Gesture(points.ToArray());
            Result gestureResult = PointCloudRecognizer.Classify(candidate, trainingSet.ToArray());
            result = gestureResult;
            results.Add(result);
            OnSymbolMatchingResult?.Invoke(gestureResult.GestureClass, gestureResult.Score);

            string resultOutput = gestureResult.GestureClass + " " + gestureResult.Score;
            canvasFollowView.UpdateResultText(resultOutput);
        }
        
    }
    
    //new function for drawing finished
    private void HandleDrawingFinished(List<Vector3> drawingPoints, GameObject LineObject)
    {
        HandleMatchingRequestedl(drawingPoints, LineObject);
        // Define the group ID to process
        string groupId = "DrawingLine Recognized"; 
        // Get all strokes in the group
        List<LineRenderer> succeedgroupStrokes = StrokeManager.Instance.GetLineRenderersInGroup(groupId);

        // Collect all 3D points from the strokes in the group
        List<Vector3> allGroupPoints = new List<Vector3>();
        foreach (LineRenderer lineRenderer in succeedgroupStrokes)
        {
            Vector3[] points = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(points);
            allGroupPoints.AddRange(points);
        }

        // Calculate the plane's origin
        Vector3 planeOrigin = paintingPlaneTransform.position;

        // Project all collected points onto the plane
        List<Vector2> projectedPoints = ProjectPointsToPlane(allGroupPoints, planeOrigin);

        // Handle the projected points (e.g., render, gesture detection, or zombie interaction)
        ZombieScript targetedZombie = RaycastFromVRCamera.currentTargetZombie;
        Debug.Log("get zombie in receiver");

        if (targetedZombie != null && targetedZombie.plane != null)
        {
            targetedZombie.ActivatePlane();
            GameObject targetedZombieGO = targetedZombie.plane.gameObject;
            GameObject ProjectedPointsGO = Render2DPointsOnPlane(projectedPoints, ResultPlaneTransform);
            RenderGestureToZombie(ProjectedPointsGO, targetedZombieGO);

            // Example condition: if gesture matches "D" and score is high enough
            if (result.GestureClass == "D" && result.Score >= 0.8)
            {
                targetedZombie.RemoveZombie();
                Debug.Log("Zombie removed.");
            }
        }
        else
        {
            Debug.Log("No targeted zombie. Handling projected points differently.");
            Render2DPointsOnPlane(projectedPoints, ResultPlaneTransform);
        }
        //clear all Recognized lines
        StrokeManager.Instance.ClearGroup(groupId, 1.5f);
    }

    // Example method: Generate an image from drawing points (to be implemented as needed)
    List<Vector2> ProjectPointsToPlane(List<Vector3> points, Vector3 planeOrigin)
    {
        List<Vector2> projectedPoints = new List<Vector2>();

        foreach (Vector3 point in points)
        {
            Vector2 projectedPoint = ProjectPointToPlane(point, planeOrigin);
            projectedPoints.Add(projectedPoint);
        }

        return projectedPoints;
    }
    
    Vector3 ProjectPointToPlane(Vector3 point, Vector3 planeOrigin)
    {
        // project the point on a plane
        Vector3 planePoint = paintingPlane.ClosestPointOnPlane(point);

        // generate the Relative coordinates of a relative plane 相对于平面中点转换为2D坐标，保留 x 和 y
        Vector2 projectedPoint = new Vector2(planePoint.x - planeOrigin.x, planePoint.y - planeOrigin.y);
        
        Debug.Log($"Projected 2D Point: {projectedPoint}");
        
        return projectedPoint;
    }
    
    private GameObject Render2DPointsOnPlane(List<Vector2> projectedPoints, Transform paintingPlane)
    {
        LineRenderer projectedline = StrokeManager.Instance.StartStroke(paintingPlane, false, "projected lines");
        GameObject lineObj = projectedline.gameObject;

        // 将 2D 点映射到 3D 世界坐标，并设置到 LineRenderer
        for (int i = 0; i < projectedPoints.Count; i++)
        {
            Vector2 point2D = projectedPoints[i];

            // 将 2D 点转换为 3D 点 (X, Y 平面)，并设置为平面 Transform 的本地坐标
            Vector3 point3D = new Vector3(point2D.x, point2D.y, 0);
            //lineRenderer.SetPosition(i, point3D);
            StrokeManager.Instance.SetStrokePoint(projectedline, point3D);
        }
        
        lineObj.transform.localScale.Set(3f,3f,3f);
        StrokeManager.Instance.ClearGroup("projected lines", 1f);
        
        return lineObj;
    }

    private void RenderGestureToZombie(GameObject originalObject, GameObject parentPlane)
    {
        if (originalObject == null || parentPlane == null)
        {
            Debug.LogError("Original object or parent plane is null. Please ensure both are assigned.");
            return;
        }

        // Step 1: Create a duplicate of the original object
        GameObject duplicatedObject = Instantiate(originalObject);

        // Step 2: Attach the duplicated object to the parentPlane as a child
        duplicatedObject.transform.SetParent(parentPlane.transform, true);

        // Step 3: Modify the properties of the duplicated object
        duplicatedObject.transform.localPosition = Vector3.zero; // Set position to zero
        duplicatedObject.transform.localRotation = Quaternion.Euler(-90, 90, -90); // Set rotation

        float objX = originalObject.transform.localScale.x;
        float objY = originalObject.transform.localScale.y;
        float objZ = originalObject.transform.localScale.z;
        duplicatedObject.transform.localScale.Set((objX*3), (objY*3), (objZ*3));

    }
}

