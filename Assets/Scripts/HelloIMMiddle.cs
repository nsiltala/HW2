using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelloIMMiddle : MonoBehaviour
{
    public Transform left;
    public Transform right;

    // Update is called once per frame
    void Update()
    {
        transform.position = (left.position + right.position) / 2;
    }
}
