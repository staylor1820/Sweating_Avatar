using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;

public class PedalController : MonoBehaviour
{
    public GameObject tracker;
    public GameObject helper;
    private Vector3 offset;
    float x;
    // Start is called before the first frame update
    public Vector3 rotationAxis = Vector3.up; // Example: Rotate around the Y-axis

    private void Start()
    {
        x = helper.transform.position.x;
        offset = helper.transform.position - tracker.transform.position;
        
        

        //helper.transform.position = tracker.transform.position
    }
    void Update()
    {
        if (tracker == null || helper == null)
            return;

        helper.transform.position = new Vector3(x,tracker.transform.position.y, tracker.transform.position.z) + helper.transform.TransformDirection(offset);
       // helper.transform.position += new Vector3(0, 0.2f, 0.2f);

        // 1️⃣ Compute target position relative to this object's coordinate system
        //Vector3 localTargetPos = transform.InverseTransformPoint(tracker.transform.position);

        // 2️⃣ Place helper at that same relative position, but in this object's local space
        //helper.transform.position = transform.TransformPoint(localTargetPos);

        // 3️⃣ Make this object look at the helper
        //transform.LookAt(helper.transform.position);

        // Get direction from this object to target
        //Vector3 direction = helper.transform.position - transform.position;

        // Zero out the X-axis to ignore pitch (if you only want YZ rotation)
        //direction.x = 0;

        // If you want it normalized (to prevent scaling issues)
        //direction.Normalize();

        // Apply the rotation
        //if (direction.sqrMagnitude > 0.0001f)
         //  transform.rotation = Quaternion.LookRotation(direction);
        
    }
}


/*
Vector3 targetPosition = tracker.transform.position;
targetPosition.z = transform.position.z;
targetPosition.x = transform.position.x;
//targetPosition.z = transform.position.z;

// Calculate the direction vector
Vector3 lookDirection = targetPosition - transform.position;

// if(lookDirection != Vector3.zero)
//{
Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1);
//}

// Optional: Zero out the Y-component if you want to explicitly prevent Y-axis rotation
//lookDirection.y = 0;
//Vector3 localDir = transform.InverseTransformVector(lookDirection);
// Create a rotation that looks in the calculated direction
// Quaternion targetRotation = Quaternion.LookRotation(lookDirection);

// Apply only the Y-axis rotation from the targetRotation
// while keeping the current X and Z rotations
//Debug.Log("My Direction: " + targetRotation.eulerAngles.x); 
// Create a new Vector3 for the target position, but keep the current object's Y-coordinate
//Vector3 targetPosition = new Vector3(tracker.transform.position.x,
//                                   tracker.transform.position.y,
//                                 tracker.transform.position.z);

//transform.rotation = targetRotation;
//transform.LookAt(new Vector3(tracker.transform.position.x,tracker.transform.position.y, tracker.transform.position.z));
//Vector3 eulerAngles = transform.rotation.eulerAngles;
//eulerAngles = new Vector3(eulerAngles.x, 0, 0);
//transform.rotation = Quaternion.Euler(eulerAngles);
// Make the object look at this modified target position
//this.transform.LookAt(targetPosition);
/*transform.rotation = Quaternion.Euler(
    targetRotation.eulerAngles.x,
    transform.rotation.eulerAngles.y,
    transform.rotation.eulerAngles.z
);
*/