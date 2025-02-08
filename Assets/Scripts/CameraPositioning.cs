using UnityEngine;

public class CameraPositioning : MonoBehaviour
{
    public Camera mainCamera; // Main player camera
    public Camera lensCamera; // Lens camera (renders magnified view)
    public bool isReverse;

    void Update()
    {
        if (mainCamera == null || lensCamera == null) return;

        // Align lens camera position to main camera, slightly offset by the lens
        Vector3 newPos = lensCamera.transform.position - mainCamera.transform.position;
        float distance = Vector3.Distance(lensCamera.transform.position, mainCamera.transform.position);
        distance /= 10;
        lensCamera.fieldOfView = Mathf.Clamp(distance, 10, 180);

        // Point lens camera in the same direction as the main camera
        lensCamera.transform.rotation = Quaternion.LookRotation(newPos.normalized);
    }
}