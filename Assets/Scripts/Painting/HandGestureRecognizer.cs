using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using System.Collections;

public class HandGestureRecognizer : MonoBehaviour
{
    // OVRHand
    public OVRHand ovrHand;

    private OVRSkeleton ovrSkeleton;
    private List<OVRBone> bones = new List<OVRBone>();
    //private SphereCollider indexFingerCollider;
    // Line Renderer for drawing
    //public LineRenderer lineRenderer;
    //public bool PaintingMode = false;


    // Painting Feature variables
    //public GameObject drawingBoard;
    //public Material lineMaterial;
    //public float lineWidth = 0.01f;
    //private bool isDrawing = false;
    //private bool isFingerTouchingBoard = false;
    //private Vector3 contactPoint;
    //private List<Vector3> drawingPoints = new List<Vector3>();

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
        //IsGestureRecognized();
        //if (ovrHand.IsTracked)
        //{
        //    if (IsGestureRecognized() && isFingerTouchingBoard)
        //    {
        //        Debug.Log("into this");

        //        StartDrawing();
        //        Debug.Log("Start Drawomg");
        //        DrawOnBoard();
        //    }
        //    else
        //    {
        //        StopDrawing();
        //    }
        //}
        //if (ovrHand.IsTracked)
        //{
        //    if (PaintingMode) {
        //        // Check if the gesture is recognized
        //        if (IsGestureRecognized())
        //        {
        //            Debug.Log("Gesture recognized!");
        //            StartDrawing();
        //            Draw();
        //        }
        //        else
        //        {
        //            StopDrawing();
        //        }
        //    }
        //}
    }
    //void OnTriggerEnter(Collider other)
    //{
    //    if (other.gameObject.CompareTag("Canvas"))
    //    {
    //        isFingerTouchingBoard = true;
    //        Debug.LogError("is Finger Touching Board");

    //    }
    //}

    //void OnTriggerExit(Collider other)
    //{
    //    if (other.gameObject.CompareTag("Canvas"))
    //    {
    //        isFingerTouchingBoard = false;
    //    }
    //}

    //void OnTriggerStay(Collider other)
    //{
    //    if (other.gameObject.CompareTag("Canvas"))
    //    {
    //        isFingerTouchingBoard = true;
    //        contactPoint = GetContactPoint(other);
    //    }
    //}
    //Vector3 GetContactPoint(Collider boardCollider)
    //{
    //    RaycastHit hit;
    //    Vector3 fingerTipPosition = GetIndexFingerTipPosition();
    //    Vector3 direction = -boardCollider.transform.forward;

    //    if (Physics.Raycast(fingerTipPosition, direction, out hit))
    //    {
    //        return hit.point;
    //    }
    //    return fingerTipPosition;
    //}

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
        float middleFingerCurl = ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);
        float ringFingerCurl = ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Ring);
        float pinkyFingerCurl = ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Pinky);
        float thumbCurl = ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Thumb);
        //Debug.Log($"Finger Curl Values -> Index: {indexFingerCurl}, Middle: {middleFingerCurl}, Ring: {ringFingerCurl}, Pinky: {pinkyFingerCurl}, Thumb: {thumbCurl}");

        // Define thresholds
        float extendedThreshold = 0.2f; // Lower values mean more extended
        float curledThreshold = 0.3f;   // Higher values mean more curled

        bool isIndexFingerExtended = indexFingerCurl < extendedThreshold;
        bool isMiddleFingerExtended = middleFingerCurl < extendedThreshold;

        bool isRingFingerCurled = ringFingerCurl > curledThreshold;
        bool isPinkyFingerCurled = pinkyFingerCurl > curledThreshold;
        bool isThumbCurled = thumbCurl > curledThreshold;


        if (isIndexFingerExtended && isMiddleFingerExtended)
        {
            //Debug.Log("Gesture Recognized");
        }
        //return isIndexFingerExtended && isMiddleFingerExtended && isRingFingerCurled && isPinkyFingerCurled && isThumbCurled;
        //return isIndexFingerExtended && isMiddleFingerExtended && isRingFingerCurled && isPinkyFingerCurled;
        return isIndexFingerExtended && isMiddleFingerExtended;
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
