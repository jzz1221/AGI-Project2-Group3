using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastFromVRCamera : MonoBehaviour
{
       public Transform vrCamera; // The Transform of the VR camera; should be assigned in the Inspector
    public float maxRayDistance = 100.0f; // The maximum distance for the raycast
    public LayerMask raycastLayerMask; // Layer mask to specify which layers the raycast should detect

    private GameObject lastHitObject = null; // Stores the last object hit by the ray
    private Color originalColor; // Stores the original color of the last hit object

    void Update()
    {
        // Get the position and forward direction of the VR camera
        Vector3 origin = vrCamera.position; // The starting position of the ray
        Vector3 direction = vrCamera.forward; // The forward direction of the VR camera

        // Optional: Draw the ray for debugging purposes
        Debug.DrawRay(origin, direction * maxRayDistance, Color.green);

        // Cast a ray and check if it hits an object
        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxRayDistance, raycastLayerMask))
        {
            GameObject hitObject = hit.collider.gameObject; // The object currently hit by the ray

            // If the ray hits a different object
            if (hitObject != lastHitObject)
            {
                // Restore the color of the last hit object
                ResetLastHitObjectColor();

                // Update the last hit object to the new one
                lastHitObject = hitObject;

                // Store the original color of the current hit object
                Renderer renderer = hitObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    originalColor = renderer.material.color; // Save the original color
                    renderer.material.color = Color.red; // Change the color to red
                }
            }
        }
        else
        {
            // If the ray does not hit any object, restore the last hit object's color
            ResetLastHitObjectColor();
            lastHitObject = null; // Clear the record of the last hit object
        }
    }

    // Restores the color of the last hit object to its original color
    private void ResetLastHitObjectColor()
    {
        if (lastHitObject != null)
        {
            Renderer renderer = lastHitObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = originalColor; // Restore the original color
            }
        }
    }
}
