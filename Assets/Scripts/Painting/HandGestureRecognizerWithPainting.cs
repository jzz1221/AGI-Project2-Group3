using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using System.Collections;

public class HandGestureRecognizerWithPainting : MonoBehaviour
{
    public OVRHand ovrHand;
    private OVRSkeleton ovrSkeleton;

    // Line Renderer for drawing
    public LineRenderer lineRenderer;
    //public bool isGestureRecognized = false;
    public bool PaintingMode = false;

    // Gesture detection variables
    private bool isDrawing = false;
    private List<Vector3> drawingPoints = new List<Vector3>();
    private List<OVRBone> bones = new List<OVRBone>();
    void Start()
    {
        // Assign ovrSkeleton before starting the coroutine
        ovrSkeleton = ovrHand.GetComponent<OVRSkeleton>();

        if (ovrSkeleton == null)
        {
            Debug.LogError("OVRSkeleton component not found on ovrHand.");
        }
        else
        {
            Debug.Log("OVRSkeleton component found.");
        }

        StartCoroutine(InitializeBones());
        AddFingerTipCollider();
    }



    void Update()
    {
        if (ovrHand.IsTracked)
        {
            if (PaintingMode) {
                // Check if the gesture is recognized
                if (IsGestureRecognized())
                {
                    Debug.Log("Gesture recognized!");
                    StartDrawing();
                    Draw();
                }
                else
                {
                    StopDrawing();
                }
            }
        }
    }

    IEnumerator InitializeBones()
    {
        while (!ovrSkeleton.IsInitialized)
        {
            yield return null;
        }

        bones.Clear();

        if (ovrSkeleton.Bones != null)
        {
            bones.AddRange(ovrSkeleton.Bones);
            Debug.Log($"Bones initialized. Total bones: {bones.Count}");
            foreach (var bone in bones)
            {
                Debug.Log($"Bone ID: {bone.Id}, Bone Name: {bone.Transform.name}");
            }

        }
        else
        {
            Debug.LogError("ovrSkeleton.Bones is null");
        }
    }


    // Method to check if the gesture is recognized
    public bool IsGestureRecognized()
    {
        // Use GetFingerPinchStrength to estimate finger curl
        float indexFingerCurl = ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        Debug.Log("-----------indexFingerCurl:" + indexFingerCurl + "----------");

        float middleFingerCurl = ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);
        float ringFingerCurl = ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Ring);
        float pinkyFingerCurl = ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Pinky);
        float thumbCurl = ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Thumb);

        // Define thresholds
        float extendedThreshold = 0.2f; // Lower values mean more extended
        float curledThreshold = 0.6f;   // Higher values mean more curled

        // Check if index and middle fingers are extended
        bool isIndexFingerExtended = indexFingerCurl < extendedThreshold;
        bool isMiddleFingerExtended = middleFingerCurl < extendedThreshold;

        // Check if other fingers are curled
        bool isRingFingerCurled = ringFingerCurl > curledThreshold;
        bool isPinkyFingerCurled = pinkyFingerCurl > curledThreshold;
        //bool isThumbCurled = thumbCurl > curledThreshold;

        // Return true if the gesture matches
        //return isIndexFingerExtended && isMiddleFingerExtended && isRingFingerCurled && isPinkyFingerCurled && isThumbCurled;
        //return isIndexFingerExtended && isMiddleFingerExtended && isRingFingerCurled && isPinkyFingerCurled;
        return isIndexFingerExtended && isMiddleFingerExtended;
    }

    private void StartDrawing()
    {
        if (!isDrawing)
        {
            isDrawing = true;
            drawingPoints.Clear();
            lineRenderer.positionCount = 0;
        }
    }

    private void StopDrawing()
    {
        if (isDrawing)
        {
            isDrawing = false;
        }
    }

    private void Draw()
    {
        Vector3 fingerTipPosition = GetIndexFingerTipPosition();
        //Debug.Log("-----------"+fingerTipPosition+"----------");
        drawingPoints.Add(fingerTipPosition);
        lineRenderer.positionCount = drawingPoints.Count;
        lineRenderer.SetPositions(drawingPoints.ToArray());
    }

    public Vector3 GetIndexFingerTipPosition()
    {
        if (bones == null || bones.Count == 0)
        {
            Debug.LogError("Bones are not initialized or empty.");
            return Vector3.zero;
        }

        foreach (var bone in bones)
        {
            if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip)
            {
                Debug.Log($"Found index fingertip bone: {bone.Transform.name}, Position: {bone.Transform.position}");
                return bone.Transform.position;
            }
        }

        Debug.LogError("Index fingertip bone not found.");
        return Vector3.zero;
    }
    void AddFingerTipCollider()
    {
        foreach (var bone in bones)
        {
            if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip)
            {
                var fingerTip = bone.Transform.gameObject;

                // Add SphereCollider
                var collider = fingerTip.GetComponent<SphereCollider>();
                if (collider == null)
                {
                    collider = fingerTip.gameObject.AddComponent<SphereCollider>();
                    collider.isTrigger = true;
                    collider.radius = 0.01f;
                }

                //// Add Rigidbody
                //var rigidbody = fingerTip.GetComponent<Rigidbody>();
                //if (rigidbody == null)
                //{
                //    rigidbody = fingerTip.gameObject.AddComponent<Rigidbody>();
                //    rigidbody.isKinematic = true;
                //    rigidbody.useGravity = false;
                //}

                // Set tip
                fingerTip.tag = "FingerTip";

                break;
            }
        }
    }

}
