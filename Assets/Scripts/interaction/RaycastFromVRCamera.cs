using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastFromVRCamera : MonoBehaviour
{
    public Transform vrCamera; // VR camera's Transform
    public float detectionRadius = 100.0f; // Detection radius
    public float fieldOfViewAngle = 30.0f; // Field of view angle
    public LayerMask zombieLayerMask; // Layer mask to filter only zombies
    public static ZombieScript currentTargetZombie = null; // The closest zombie target

    private GameObject lastHitObject = null; // Previously detected object
    private Color originalColor; // Original color of the previously detected object
    
    private Color pointedcolor;
    private Color talismancolor;
    private Color defaultcolor;

    void Start()
    {
        talismancolor = Color.yellow;
        pointedcolor = Color.yellow;
        defaultcolor = Color.gray;
        defaultcolor.a = 0;
        pointedcolor.a = 0.8f;
    }

    void Update()
    {
        // Get all colliders within the detection radius
        Collider[] hitColliders = Physics.OverlapSphere(vrCamera.position, detectionRadius, zombieLayerMask);

        float halfFOV = fieldOfViewAngle / 2f; // Half of the field of view angle
        float closestDistance = Mathf.Infinity; // Initialize with a very large distance
        ZombieScript closestZombie = null; // Placeholder for the closest zombie
        GameObject closestHitObject = null; // Placeholder for the closest hit object

        Vector3 origin = vrCamera.position; // Camera position
        Vector3 forward = vrCamera.forward; // Camera forward direction

        foreach (Collider collider in hitColliders)
        {
            // Get direction from the camera to the collider
            Vector3 directionToCollider = collider.transform.position - origin;

            // Calculate angle between the camera's forward direction and the object
            float angle = Vector3.Angle(forward, directionToCollider);

            // Check if the object is within the field of view
            if (angle < halfFOV)
            {
                float distance = directionToCollider.magnitude; // Distance to the collider

                // Check if this object is closer than the previously detected one
                ZombieScript zombieScript = collider.GetComponent<ZombieScript>();
                if (zombieScript != null && !zombieScript.isRemoved) // Skip removed zombies
                {
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestHitObject = collider.gameObject;
                        closestZombie = zombieScript; // Get the zombie script
                    }
                }
            }
        }

        // Handle the closest detected zombie
        if (closestHitObject != null)
        {
            if (closestHitObject != lastHitObject)
            {
                ResetLastHitObjectColor(); // Reset the previous object's color
                lastHitObject = closestHitObject;

                closestZombie.transform.Find("Plane").GetComponent<MeshRenderer>().material.color = pointedcolor;
                /*// Highlight the new closest object
                Renderer renderer = closestHitObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    originalColor = renderer.material.color;
                    renderer.material.color = Color.yellow; // Change the color to highlight it
                }*/
            }

            currentTargetZombie = closestZombie; // Update the current target
            Debug.Log("Closest Zombie Detected: " + currentTargetZombie);
        }
        else
        {
            ResetLastHitObjectColor(); // Reset if no zombies are detected
            lastHitObject = null;
            currentTargetZombie = null;
        }
    }

    // Reset the color of the previously highlighted object
    private void ResetLastHitObjectColor()
    {
        if (lastHitObject != null)
        {
            /*Renderer renderer = lastHitObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = originalColor; // Restore original color
            }*/
            lastHitObject.transform.Find("Plane").GetComponent<MeshRenderer>().material.color = defaultcolor;;
        }
    }
}
