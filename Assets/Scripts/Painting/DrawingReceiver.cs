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
    private float minMatchingScore;
    
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
        
        minMatchingScore = canvasFollowView.minMatchingScore;
        
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
    
    public Dictionary<string, int> CountGestureOccurrences()
    {
        // 创建一个字典来存储每个 GestureClass 的出现次数
        Dictionary<string, int> gestureCounts = new Dictionary<string, int>();

        // 遍历 results 列表
        foreach (Result result in results)
        {
            string gestureClass = result.GestureClass;

            if (gestureCounts.ContainsKey(gestureClass))
            {
                // 如果字典中已有该手势，则增加计数
                gestureCounts[gestureClass]++;
            }
            else
            {
                // 如果字典中没有该手势，则添加新的键值对，计数设为1
                gestureCounts[gestureClass] = 1;
            }
        }

        // 返回统计结果
        return gestureCounts;
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
            if (result.Score > minMatchingScore)
            {
                results.Add(result);
            }
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
        //ZombieScript targetedZombie = RaycastFromVRCamera.currentTargetZombie;
        ZombieScript targetedZombie = FanDetection.currentTargetZombie;
        Debug.Log("get zombie in receiver");

        if (!GameManager.Instance.OnboardingEnd)
        {
            Dictionary<string, int> gestureCounts = CountGestureOccurrences();
            if ((gestureCounts.ContainsKey("circle") && gestureCounts["circle"] > 0) 
                || gestureCounts.ContainsKey("X") && gestureCounts["X"] > 0) 
                GameManager.Instance.DetectStartGesture();
            gestureCounts.Clear();
            results.Clear();
        }

        if (targetedZombie != null && targetedZombie.plane != null)
        {
            Debug.Log("zombie detected"+targetedZombie.plane.name);
            targetedZombie.ActivatePlane();
            GameObject targetedZombieGO = targetedZombie.plane.gameObject;
            GameObject ProjectedPointsGO = Render2DPointsOnPlane(projectedPoints, ResultPlaneTransform);
            RenderGestureToZombie(ProjectedPointsGO, targetedZombieGO);
            Dictionary<string, int> gestureCounts = CountGestureOccurrences();
            Debug.Log("GestureCounts" + gestureCounts);

            if (gestureCounts.ContainsKey("circle") && gestureCounts["circle"] > 0)
            {
                int circleCount = gestureCounts["circle"];
                Debug.Log("circle count" + circleCount);
                targetedZombie.PushZombie(circleCount); // 按 circle 的个数调用 PushZombie
                paintingPlaneTransform.Find("PushText").GetComponent<Animator>().SetTrigger("Push");
                Debug.Log($"Zombie pushed {circleCount} times.");
                GameManager.Instance.AddScore(5);
            }
            if (gestureCounts.ContainsKey("X") && gestureCounts["X"] > 0)
            {
                targetedZombie.RemoveZombie(); // 当 x 个数大于 0 时调用 RemoveZombie
                paintingPlaneTransform.Find("ExpelText").GetComponent<Animator>().SetTrigger("ExpelText");
                Debug.Log("Zombie removed due to 'x' gesture.");
                GameManager.Instance.AddScore(10);
            }
            gestureCounts.Clear();
            results.Clear();
            
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
        
        //Debug.Log($"Projected 2D Point: {projectedPoint}");
        
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
        
        lineObj.transform.localScale.Set(0.5f,0.5f,0.5f);
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

