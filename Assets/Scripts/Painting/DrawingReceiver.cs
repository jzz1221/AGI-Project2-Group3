using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PDollarGestureRecognizer;
using System.IO;

public class DrawingReceiver : MonoBehaviour
{
    public CanvasFollowView canvasFollowView; // Must be assigned manually in the Inspector
    public Material lineMaterial;

    private Plane paintingPlane;
    private Transform paintingPlaneTransform;
    private Transform ResultPlaneTransform;
    private List<Point> points = new List<Point>();
    private List<Gesture> trainingSet = new List<Gesture>();

    void Start()
    {
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
    private void HandleMatchingRequestedl(List<Vector3> drawingPoints)
    {
        Debug.Log("Processing gesture matching...");
        Vector3 planeOrigin = paintingPlaneTransform.position;
        List<Vector2> projectedPoints = ProjectPointsToPlane(drawingPoints, planeOrigin);
        points.Clear();
        foreach (Vector2 p in projectedPoints)
        {
            points.Add(new Point(p.x, p.y, 1));
        }

        Gesture candidate = new Gesture(points.ToArray());
        Result gestureResult = PointCloudRecognizer.Classify(candidate, trainingSet.ToArray());

        string resultOutput = gestureResult.GestureClass + " " + gestureResult.Score;
        canvasFollowView.UpdateResultText(resultOutput);
    }

    // Event handler method called when drawing is finished
    private void HandleDrawingFinished(List<Vector3> drawingPoints)
    {
        //Render 2D points on Plane(Quad)
        Vector3 planeOrigin = paintingPlaneTransform.position;
        List<Vector2> projectedPoints = ProjectPointsToPlane(drawingPoints, planeOrigin);

        // Get the zombie currently being watched
        ZombieScript targetedZombie = RaycastFromVRCamera.currentTargetZombie;
        Debug.Log("get zombie in receiver");

        if (targetedZombie != null && targetedZombie.plane != null)
        {
            targetedZombie.ActivatePlane();
            targetedZombie.RemoveZombie();
            Debug.Log("set zombie plane active");
            Transform zombiePlaneTransform = targetedZombie.plane.transform;

            // Render the drawn shape on the zombie's plane
            Render2DPointsOnPlane(projectedPoints, zombiePlaneTransform);
        }
        else
        {
            // If there is no zombie being looked at, render to the default result plane
            Render2DPointsOnPlane(projectedPoints, ResultPlaneTransform);
        }
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
    
    Vector2 ProjectPointToPlane(Vector3 point, Vector3 planeOrigin)
    {
        // project the point on a plane
        Vector3 planePoint = paintingPlane.ClosestPointOnPlane(point);

        // generate the Relative coordinates of a relative plane 相对于平面中点转换为2D坐标，保留 x 和 y
        Vector2 projectedPoint = new Vector2(planePoint.x - planeOrigin.x, planePoint.y - planeOrigin.y);
        
        Debug.Log($"Projected 2D Point: {projectedPoint}");
        
        return projectedPoint;
    }
    
    
    private void Render2DPointsOnPlane(List<Vector2> projectedPoints, Transform paintingPlane)
    {
        // 创建一个新的 GameObject，用于存放 LineRenderer
        GameObject lineObj = new GameObject("ProjectedShape");
        lineObj.transform.SetParent(paintingPlane, false);

        // 添加 LineRenderer 组件
        LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.startWidth = 0.01f; // 线宽
        lineRenderer.endWidth = 0.01f;
        lineRenderer.positionCount = projectedPoints.Count; // 顶点数量与点的数量相同，不再加 1

        // 将 2D 点映射到 3D 世界坐标，并设置到 LineRenderer
        for (int i = 0; i < projectedPoints.Count; i++)
        {
            Vector2 point2D = projectedPoints[i];

            // 将 2D 点转换为 3D 点 (X, Y 平面)，并设置为平面 Transform 的本地坐标
            Vector3 point3D = paintingPlane.TransformPoint(new Vector3(point2D.x, point2D.y, 0));
            lineRenderer.SetPosition(i, point3D);
        }

        Debug.Log("2D Points Rendered on Plane without closure.");
    }
}

