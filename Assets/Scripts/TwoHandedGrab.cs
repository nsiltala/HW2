using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic; // Required for List

public class TwoHandedGrab : MonoBehaviour
{
    public InputActionProperty grabAction;
    public Transform otherHand;
    public LayerMask grabbableLayer;
    public float grabRadius = 0.1f;

    private Transform grabbedObject;
    private Rigidbody grabbedRigidbody;
    private Vector3[] handOffsets = new Vector3[2];
    private Quaternion[] handRotations = new Quaternion[2];
    private bool isGrabbing;
    private List<Transform> nearObjects = new List<Transform>(); // List to store nearby objects

    private void Start()
    {
        grabAction.action?.Enable();

        if (otherHand == null)
        {
            Debug.LogError("Other hand not assigned in the Inspector for " + gameObject.name);
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        isGrabbing = grabAction.action?.ReadValue<float>() > 0.5f;

        if (isGrabbing)
        {
            if (!grabbedObject)
            {
                // Raycast grab (prioritizes objects in nearObjects)
                if (nearObjects.Count > 0)
                {
                    grabbedObject = nearObjects[0]; // Grab the first near object (you can implement more complex prioritization logic here)
                    grabbedRigidbody = grabbedObject.GetComponent<Rigidbody>();
                    if (grabbedRigidbody)
                    {
                        grabbedRigidbody.isKinematic = true;
                    }

                    handOffsets[0] = grabbedObject.position - transform.position;
                    handRotations[0] = Quaternion.Inverse(transform.rotation) * grabbedObject.rotation;

                    if (otherHand.GetComponent<TwoHandedGrab>().grabbedObject == null)
                    {
                        otherHand.GetComponent<TwoHandedGrab>().GrabObject(grabbedObject);
                        otherHand.GetComponent<TwoHandedGrab>().handOffsets[1] = grabbedObject.position - otherHand.transform.position;
                        otherHand.GetComponent<TwoHandedGrab>().handRotations[1] = Quaternion.Inverse(otherHand.transform.rotation) * grabbedObject.rotation;
                    }
                }
            }
        }
        else if (grabbedObject && (!otherHand.GetComponent<TwoHandedGrab>().isGrabbing || otherHand.GetComponent<TwoHandedGrab>().grabbedObject != grabbedObject))
        {
            DropObject();
        }

        if (grabbedObject)
        {
            // ... (rest of the grabbing logic - same as before)
            Vector3 combinedPosition;
            Quaternion combinedRotation;

            if (otherHand.GetComponent<TwoHandedGrab>().grabbedObject == grabbedObject && otherHand.GetComponent<TwoHandedGrab>().isGrabbing)
            {
                // Two-handed grab
                combinedPosition = (transform.position + otherHand.transform.position + handOffsets[0] + otherHand.GetComponent<TwoHandedGrab>().handOffsets[1]) / 2f;
                combinedRotation = Quaternion.Slerp(transform.rotation * handRotations[0], otherHand.transform.rotation * otherHand.GetComponent<TwoHandedGrab>().handRotations[1], 0.5f);
            }
            else
            {
                // One-handed grab
                combinedPosition = transform.position + handOffsets[0];
                combinedRotation = transform.rotation * handRotations[0];
            }


            if (grabbedRigidbody)
            {
                grabbedRigidbody.MovePosition(combinedPosition);
                grabbedRigidbody.MoveRotation(combinedRotation);
            }
            else
            {
                grabbedObject.position = combinedPosition;
                grabbedObject.rotation = combinedRotation;
            }
        }
    }

    public void GrabObject(Transform obj)
    {
        grabbedObject = obj;
        grabbedRigidbody = grabbedObject.GetComponent<Rigidbody>();
        if (grabbedRigidbody)
        {
            grabbedRigidbody.isKinematic = true;
        }
    }


    private void DropObject()
    {
        if (grabbedRigidbody)
        {
            grabbedRigidbody.isKinematic = false;
        }

        grabbedObject = null;
        grabbedRigidbody = null;
        handOffsets = new Vector3[2];
        handRotations = new Quaternion[2];
        otherHand.GetComponent<TwoHandedGrab>().grabbedObject = null;
        otherHand.GetComponent<TwoHandedGrab>().handOffsets = new Vector3[2];
        otherHand.GetComponent<TwoHandedGrab>().handRotations = new Quaternion[2];
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