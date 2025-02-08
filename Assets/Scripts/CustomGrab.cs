using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CustomGrab : MonoBehaviour
{
    private CustomGrab otherHand = null;
    private List<Transform> nearObjects = new List<Transform>();
    private Transform grabbedObject = null;
    private bool isPrimaryGrabber = false;
    public InputActionProperty grabAction;
    private bool grabbing = false;
    private Vector3 initialOffset;
    private Quaternion initialRotationOffset;
    private Rigidbody grabbedRigidbody;

    private void Start()
    {
        if (grabAction.action != null)
        {
            grabAction.action.Enable();
        }
        else
        {
            Debug.LogWarning("Grab action is not assigned or is null.");
        }

        // Find the other hand
        foreach (CustomGrab c in transform.parent.GetComponentsInChildren<CustomGrab>())
        {
            if (c != this)
            {
                otherHand = c;
            }
        }
    }

    void Update()
    {
        if (grabAction.action != null && grabAction.action.enabled)
        {
            grabbing = grabAction.action.ReadValue<float>() > 0.5f;
        }

        if (grabbing)
        {
            if (!grabbedObject)
            {
                // Grab the closest object
                if (nearObjects.Count > 0)
                {
                    grabbedObject = nearObjects[0];
                    grabbedRigidbody = grabbedObject.GetComponent<Rigidbody>();

                    if (grabbedRigidbody)
                    {
                        grabbedRigidbody.isKinematic = true;
                    }

                    initialOffset = grabbedObject.position - transform.position;
                    initialRotationOffset = Quaternion.Inverse(transform.rotation) * grabbedObject.rotation;
                    isPrimaryGrabber = true;
                }
                else if (otherHand?.grabbedObject)
                {
                    grabbedObject = otherHand.grabbedObject;
                    grabbedRigidbody = grabbedObject.GetComponent<Rigidbody>();
                    isPrimaryGrabber = false; // This hand is assisting
                }
            }

            if (grabbedObject)
            {
                Vector3 targetPosition;
                Quaternion targetRotation;

                if (otherHand?.grabbing == true)
                {
                    // Combine positions and rotations of both hands
                    Vector3 combinedPosition = (transform.position + otherHand.transform.position) / 2;
                    Quaternion combinedRotation = Quaternion.Lerp(transform.rotation, otherHand.transform.rotation, 0.5f);

                    targetPosition = combinedPosition + initialOffset;
                    targetRotation = combinedRotation * initialRotationOffset;
                }
                else
                {
                    // Single hand manipulation
                    targetPosition = transform.position + initialOffset;
                    targetRotation = transform.rotation * initialRotationOffset;
                }

                if (grabbedRigidbody)
                {
                    grabbedRigidbody.MovePosition(targetPosition);
                    grabbedRigidbody.MoveRotation(targetRotation);
                }
                else
                {
                    grabbedObject.position = targetPosition;
                    grabbedObject.rotation = targetRotation;
                }
            }
        }
        else if (grabbedObject && (isPrimaryGrabber || !otherHand.grabbing))
        {
            // Release the object
            if (grabbedRigidbody)
            {
                grabbedRigidbody.isKinematic = false;
            }

            grabbedObject = null;
            grabbedRigidbody = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("grabbable"))
        {
            nearObjects.Add(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("grabbable"))
        {
            nearObjects.Remove(other.transform);
        }
    }
}