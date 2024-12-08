using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrokeManager : MonoBehaviour
{
    private static StrokeManager _instance;

    public static StrokeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Search for an existing instance in the scene
                _instance = FindObjectOfType<StrokeManager>();

                // Create a new GameObject if none exists
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("StrokeManager");
                    _instance = singletonObject.AddComponent<StrokeManager>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        // Ensure there's only one instance
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        // Optional: Persist this object across scenes
        DontDestroyOnLoad(gameObject);
    }
    
    // Private fields for storing strokes and groups
    private List<GameObject> strokes = new List<GameObject>(); // All strokes
    private Dictionary<string, List<GameObject>> strokeGroups = new Dictionary<string, List<GameObject>>(); // Strokes grouped by name
    private Material defaultMaterial; // Default material for LineRenderer
    private float lineWidth = 0.1f; // Default line width

    private StrokeManager() { } // Private constructor to prevent instantiation
    
    public void Initialize(Material material, float lineWidth)
    {
        this.defaultMaterial = material;
        this.lineWidth = lineWidth;
    }
    
    /// <summary>
    /// Creates a new stroke as a LineRenderer.
    /// </summary>
    /// <param name="parent">The parent Transform to attach the stroke to.</param>
    /// <param name="groupName">Optional group name to group the stroke.</param>
    /// <returns>The created LineRenderer.</returns>
    public LineRenderer StartStroke(Transform parent, bool WorldSpace, string groupName = null)
    {
        // Create a new GameObject for the stroke
        GameObject strokeObj = new GameObject($"Stroke_{strokes.Count}");
        strokeObj.transform.SetParent(parent, false);

        // Add LineRenderer and configure it
        LineRenderer lineRenderer = strokeObj.AddComponent<LineRenderer>();
        lineRenderer.material = defaultMaterial;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = WorldSpace;

        // Add the stroke to the global list
        strokes.Add(strokeObj);

        // Add the stroke to the group if a group name is provided
        if (!string.IsNullOrEmpty(groupName))
        {
            if (!strokeGroups.ContainsKey(groupName))
            {
                strokeGroups[groupName] = new List<GameObject>();
            }
            strokeGroups[groupName].Add(strokeObj);
        }

        return lineRenderer;
    }
    
    /// <summary>
    /// Adds a single point to an existing stroke.
    /// </summary>
    public void SetStrokePoint(LineRenderer lineRenderer, Vector3 point)
    {
        if (lineRenderer == null) return;

        int positionCount = lineRenderer.positionCount;
        lineRenderer.positionCount = positionCount + 1;
        lineRenderer.SetPosition(positionCount, point);
    }
    
    /// <summary>
    /// Adds multiple points to a stroke using SetPositions.
    /// </summary>
    /// <param name="lineRenderer">The LineRenderer to add points to.</param>
    /// <param name="points">The list of points to add.</param>
    public void SetStrokePoints(LineRenderer lineRenderer, List<Vector3> points)
    {
        if (lineRenderer == null || points == null || points.Count == 0) return;

        // Set all points in one operation
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    /// <summary>
    /// Changes the group of a specific stroke.
    /// </summary>
    /// <param name="stroke">The stroke GameObject to change the group for.</param>
    /// <param name="newGroupName">The name of the new group.</param>
    public void ChangeStrokeGroup(GameObject stroke, string newGroupName)
    {
        if (stroke == null || string.IsNullOrEmpty(newGroupName)) return;

        // Remove from existing group
        foreach (var group in strokeGroups.Values)
        {
            group.Remove(stroke);
        }

        // Add to the new group
        if (!strokeGroups.ContainsKey(newGroupName))
        {
            strokeGroups[newGroupName] = new List<GameObject>();
        }
        strokeGroups[newGroupName].Add(stroke);
    }
    
    /// <summary>
    /// Retrieves all LineRenderers from a specific group.
    /// </summary>
    /// <param name="groupId">The ID of the group.</param>
    /// <returns>A list of LineRenderers belonging to the specified group.</returns>
    public List<LineRenderer> GetLineRenderersInGroup(string groupId)
    {
        List<LineRenderer> lineRenderersInGroup = new List<LineRenderer>();

        if (strokeGroups.ContainsKey(groupId))
        {
            foreach (GameObject strokeObj in strokeGroups[groupId])
            {
                LineRenderer lineRenderer = strokeObj.GetComponent<LineRenderer>();
                if (lineRenderer != null)
                {
                    lineRenderersInGroup.Add(lineRenderer);
                }
            }
        }
        else
        {
            Debug.LogWarning($"Group ID '{groupId}' does not exist.");
        }

        return lineRenderersInGroup;
    }

    /// <summary>
    /// Clears all strokes within a specific group, with a fade-out effect.
    /// </summary>
    /// <param name="groupName">The name of the group to clear.</param>
    /// <param name="fadeDuration">The duration of the fade-out effect.</param>
    public void ClearGroup(string groupName, float fadeDuration = 1.0f)
    {
        if (strokeGroups.ContainsKey(groupName))
        {
            List<GameObject> groupStrokes = strokeGroups[groupName];

            foreach (GameObject stroke in groupStrokes)
            {
                LineRenderer lineRenderer = stroke.GetComponent<LineRenderer>();
                if (lineRenderer != null)
                {
                    Debug.Log("Clearing line:" + lineRenderer.gameObject.name);
                    StartCoroutine(FadeAndDestroyCoroutine(lineRenderer, fadeDuration));
                }
                else Debug.LogError("No LineRenderer could found in clear group");
            }

            // After fade-out, remove the group
            StartCoroutine(RemoveGroupAfterFade(groupName, fadeDuration));
        }
    }

    /// <summary>
    /// Coroutine to remove the group after the fade-out is complete.
    /// </summary>
    /// <param name="groupName">The name of the group to remove.</param>
    /// <param name="fadeDuration">The duration of the fade-out effect.</param>
    private IEnumerator RemoveGroupAfterFade(string groupName, float fadeDuration)
    {
        yield return new WaitForSeconds(fadeDuration);

        if (strokeGroups.ContainsKey(groupName))
        {
            foreach (GameObject stroke in strokeGroups[groupName])
            {
                strokes.Remove(stroke);
                Destroy(stroke); // Ensure it's removed from the main list
            }
            strokeGroups.Remove(groupName); // Remove the group
        }
    }

    /// <summary>
    /// Coroutine to fade out and destroy a LineRenderer's GameObject.
    /// </summary>
    /// <param name="line">The LineRenderer to fade and destroy.</param>
    /// <param name="fadeDuration">The duration of the fade-out effect.</param>
    private IEnumerator FadeAndDestroyCoroutine(LineRenderer line, float fadeDuration)
    {
        if (line == null) yield break;

        Material lineMaterial = line.material;
        if (lineMaterial == null)
        {
            Debug.LogWarning("LineRenderer material is null. Skipping fade.");
            yield break;
        }

        Color startColor = lineMaterial.color;
        float elapsedTime = 0f;

        // Gradually reduce the Alpha value
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration); // Interpolate Alpha from 1 to 0
            lineMaterial.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        // Ensure the line is fully invisible
        lineMaterial.color = new Color(startColor.r, startColor.g, startColor.b, 0);
    }

    /// <summary>
    /// Changes the material of a specific LineRenderer.
    /// </summary>
    /// <param name="lineRenderer">The LineRenderer whose material needs to be changed.</param>
    /// <param name="newMaterial">The new material to apply.</param>
    public void ChangeMaterial(LineRenderer lineRenderer, Material newMaterial)
    {
        if (lineRenderer == null || newMaterial == null)
        {
            Debug.LogWarning("LineRenderer or Material is null. Material change aborted.");
            return;
        }

        lineRenderer.material = newMaterial;
    }

    /// <summary>
    /// Toggles the visibility of all strokes or strokes in a specific group.
    /// </summary>
    /// <param name="visible">Whether the strokes should be visible.</param>
    /// <param name="groupName">Optional group name to limit the visibility change.</param>
    public void ToggleVisibility(bool visible, string groupName = null)
    {
        List<GameObject> targetStrokes = string.IsNullOrEmpty(groupName) ? strokes : strokeGroups.GetValueOrDefault(groupName);

        if (targetStrokes == null) return;

        foreach (GameObject stroke in targetStrokes)
        {
            LineRenderer lineRenderer = stroke.GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                lineRenderer.enabled = visible;
            }
        }
    }

    /// <summary>
    /// Gets all LineRenderers globally or within a specific group.
    /// </summary>
    /// <param name="groupName">Optional group name to limit the search.</param>
    /// <returns>A list of LineRenderers.</returns>
    public List<LineRenderer> GetAllLineRenderers(string groupName = null)
    {
        List<GameObject> targetStrokes = string.IsNullOrEmpty(groupName) ? strokes : strokeGroups.GetValueOrDefault(groupName);

        List<LineRenderer> lineRenderers = new List<LineRenderer>();
        if (targetStrokes != null)
        {
            foreach (GameObject stroke in targetStrokes)
            {
                LineRenderer lr = stroke.GetComponent<LineRenderer>();
                if (lr != null)
                {
                    lineRenderers.Add(lr);
                }
            }
        }
        return lineRenderers;
    }
    
    /// <summary>
    /// Clears all points in a specific stroke.
    /// </summary>
    /// <param name="lineRenderer">The LineRenderer to clear.</param>
    public void ClearStroke(LineRenderer lineRenderer)
    {
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }
    }

    /// <summary>
    /// Deletes a specific stroke by index.
    /// </summary>
    /// <param name="index">The index of the stroke to delete.</param>
    public void DeleteStroke(int index)
    {
        if (index < 0 || index >= strokes.Count) return;

        GameObject strokeObj = strokes[index];
        strokes.RemoveAt(index);
        GameObject.Destroy(strokeObj);
    }

    /// <summary>
    /// Clears all strokes globally.
    /// </summary>
    public void ClearAllStrokes()
    {
        foreach (GameObject stroke in strokes)
        {
            GameObject.Destroy(stroke);
        }
        strokes.Clear();
        strokeGroups.Clear();
    }
}