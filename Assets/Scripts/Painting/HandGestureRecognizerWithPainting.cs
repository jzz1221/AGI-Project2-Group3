using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using System.Collections;
using TMPro;
using System; // 引入 System 命名空间以使用 Action

public class HandGestureRecognizerWithPainting : MonoBehaviour
{
    public OVRHand ovrHand;
    private OVRSkeleton ovrSkeleton;

    // Line Renderer for drawing
    public LineRenderer lineRenderer;
    public bool PaintingMode = false;

    // Gesture detection variables
    private bool isDrawing = false;
    private List<Vector3> drawingPoints = new List<Vector3>();
    private List<OVRBone> bones = new List<OVRBone>();

    private bool bonesInitialized = false; // 添加骨骼初始化的标志位

    public event Action OnBonesInitialized; // 添加事件

    //---------for debug----------
    public TextMeshProUGUI PointTextforDebug;

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
    }

    void Update()
    {
        // 只有在骨骼初始化后才进行以下操作
        if (!bonesInitialized)
        {
            return; // 如果骨骼未初始化，直接返回
        }

        if (ovrHand.IsTracked)
        {
            if (PaintingMode)
            {
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

            // Bones are initialized now
            bonesInitialized = true;

            // 调用事件，通知监听者骨骼已初始化
            OnBonesInitialized?.Invoke();

            // 调用 AddFingerTipCollider()，确保在骨骼初始化后执行
            AddFingerTipCollider();
        }
        else
        {
            Debug.LogError("ovrSkeleton.Bones is null");
        }
    }

// Method to check if the gesture is recognized
    public bool IsGestureRecognized()
    {
        if (!bonesInitialized)
        {
            return false; // 如果骨骼未初始化，无法识别手势
        }

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
        if (!bonesInitialized)
        {
            Debug.LogError("Bones are not initialized.");
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

                //// Add Rigidbody (如果需要)
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
