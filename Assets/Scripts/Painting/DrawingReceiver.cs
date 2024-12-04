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
    
    private Result result;

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
        result = gestureResult;

        string resultOutput = gestureResult.GestureClass + " " + gestureResult.Score;
        canvasFollowView.UpdateResultText(resultOutput);
        
        if (gestureResult.Score >= 0.8f)
        {
            Debug.Log("High confidence detected! Updating material and clearing line.");
        
            // Change line material
            Material newMaterial = new Material(Shader.Find("Sprites/Default"));
            newMaterial.color = Color.yellow; // Example: Change the color to blue
            canvasFollowView.UpdateLineMaterial(newMaterial);
            // Clear previous line
            //canvasFollowView.ClearPreviousLine();
        }
        
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
            GameObject targetedZombieGO = targetedZombie.plane.gameObject;
            GameObject ProjectedPointsGO = Render2DPointsOnPlane(projectedPoints, ResultPlaneTransform);
            if(result.GestureClass == "D" && result.Score >= 0.5f)
            {
                RenderGestureToZombie(ProjectedPointsGO, targetedZombieGO);
                targetedZombie.RemoveZombie();
                Debug.Log("set zombie plane active");
                //Transform zombiePlaneTransform = targetedZombie.plane.transform;

                // Render the drawn shape on the zombie's plane
                //Render2DPointsOnPlane(projectedPoints, zombiePlaneTransform);
                
            }
        }
        else
        {
            // If there is no zombie being looked at, render to the default result plane
            //Render2DPointsOnPlane(projectedPoints, ResultPlaneTransform);
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
    
    
    private GameObject Render2DPointsOnPlane(List<Vector2> projectedPoints, Transform paintingPlane)
    {
        // 创建一个新的 GameObject，用于存放 LineRenderer
        GameObject lineObj = new GameObject("ProjectedShape");
        lineObj.transform.SetParent(paintingPlane, false);
        Mesh mesh = lineObj.AddComponent<MeshFilter>().mesh;

        // 添加 LineRenderer 组件
        LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.startWidth = 0.01f; // 线宽
        lineRenderer.endWidth = 0.01f;
        lineRenderer.material.color = Color.black;
        lineRenderer.positionCount = projectedPoints.Count; // 顶点数量与点的数量相同，不再加 1
        
        lineRenderer.useWorldSpace = false;

        // 将 2D 点映射到 3D 世界坐标，并设置到 LineRenderer
        for (int i = 0; i < projectedPoints.Count; i++)
        {
            Vector2 point2D = projectedPoints[i];

            // 将 2D 点转换为 3D 点 (X, Y 平面)，并设置为平面 Transform 的本地坐标
            Vector3 point3D = new Vector3(point2D.x, point2D.y, 0);
            lineRenderer.SetPosition(i, point3D);
        }
        
        /*Transform projectedPointsTransform = lineObj.transform;
        projectedPointsTransform = CreateTransformForScaledImage(projectedPoints, paintingPlane);
        
        lineObj.transform.position = projectedPointsTransform.position;
        lineObj.transform.localScale = projectedPointsTransform.localScale;*/
        
        //StartCoroutine(FadeAndDestroyLine(lineRenderer, 1f));
        
        Debug.Log("2D Points Rendered on Plane without closure.");
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
    
    private IEnumerator FadeAndDestroyLine(LineRenderer line, float fadeDuration)
    {
        if (line == null) yield break;

        Material lineMaterial = line.material;
        Color startColor = lineMaterial.color;

        float elapsedTime = 0f;
        // Gradually reduce the Alpha value
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration); // Linearly interpolate Alpha from 1 to 0
            lineMaterial.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        // Ensure the line is fully invisible
        lineMaterial.color = new Color(startColor.r, startColor.g, startColor.b, 0);

        // Destroy the line GameObject
        if(line.gameObject != null) Destroy(line.gameObject);
    }
    
}

