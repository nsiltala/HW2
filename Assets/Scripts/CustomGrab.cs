using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CustomGrab : MonoBehaviour
{
    CustomGrab otherHand = null;
    public List<Transform> nearObjects = new List<Transform>();
    public Transform grabbedObject = null;
    public InputActionReference grabAction;
    public InputActionReference toggleDoubleRotationAction;
    bool grabbing = false;
    bool doubleRotationEnabled = false;

    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private void Start()
    {
        grabAction.action.Enable();
        toggleDoubleRotationAction.action.Enable();

        foreach (CustomGrab c in transform.parent.GetComponentsInChildren<CustomGrab>())
        {
            if (c != this)
                otherHand = c;
        }

        lastPosition = transform.position;
        lastRotation = transform.rotation;

        toggleDoubleRotationAction.action.performed += OnToggleDoubleRotation;
    }

    private void OnDestroy()
    {
        toggleDoubleRotationAction.action.performed -= OnToggleDoubleRotation;
    }

    void Update()
    {
        grabbing = grabAction.action.IsPressed();
        if (grabbing)
        {
            if (!grabbedObject)
                grabbedObject = nearObjects.Count > 0 ? nearObjects[0] : otherHand.grabbedObject;

            if (grabbedObject)
            {
                Vector3 deltaPosition = transform.position - lastPosition;
                Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(lastRotation);

                if (otherHand && otherHand.grabbedObject == grabbedObject)
                {
                    Vector3 otherDeltaPosition = otherHand.transform.position - otherHand.lastPosition;
                    Quaternion otherDeltaRotation = otherHand.transform.rotation * Quaternion.Inverse(otherHand.lastRotation);

                    deltaPosition += otherDeltaPosition;
                    deltaRotation *= otherDeltaRotation;
                }

                grabbedObject.position += deltaPosition;

                if (doubleRotationEnabled)
                {
                    grabbedObject.rotation = Quaternion.SlerpUnclamped(Quaternion.identity, deltaRotation, 2.0f) * grabbedObject.rotation;
                }
                else
                {
                    grabbedObject.rotation = deltaRotation * grabbedObject.rotation;
                }
            }
        }
        else if (grabbedObject)
        {
            grabbedObject = null;
        }

        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        Transform t = other.transform;
        if (t && t.tag.ToLower() == "grabbable")
            nearObjects.Add(t);
    }

    private void OnTriggerExit(Collider other)
    {
        Transform t = other.transform;
        if (t && t.tag.ToLower() == "grabbable")
            nearObjects.Remove(t);
    }

    private void OnToggleDoubleRotation(InputAction.CallbackContext context)
    {
        doubleRotationEnabled = !doubleRotationEnabled;
        Debug.Log("Double Rotation Enabled: " + doubleRotationEnabled);
    }
}