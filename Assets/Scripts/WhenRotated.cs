using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhenRotated : MonoBehaviour
{
    public Camera mainCamera; // Main player camera
    public GameObject targetObject;// Lens camera (renders magnified view)
    public GameObject lenseObject;

    void Update()
    {
        if (mainCamera == null || lenseObject == null || targetObject == null) return;

        // Get the forward vector of the camera and object (normalized direction they are facing)
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 objectForward = targetObject.transform.forward;

        // Calculate the dot product (cosine of the angle between vectors)
        float dot = Vector3.Dot(cameraForward, objectForward);

        // If the dot product is approximately -1, the object is facing 180 degrees away
        if (dot >= 0)
        {
            lenseObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
        } else {
            lenseObject.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
    }
}
