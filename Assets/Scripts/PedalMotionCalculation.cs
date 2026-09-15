using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedalMotionCalculation : MonoBehaviour
{
    [Header("Refs")]
    public Transform movingObject;
    public Transform target;
    public Transform center; // or replace with public Vector3 centerPos;

    [Header("Noise guards")]
    public float movementEpsilon = 1e-4f;
    public float angleEpsilonDeg = 1e-3f;

    private bool havePrev;
    private Vector3 prevPos;
    private float prevAngleDeg;

    void Update()
    {
        var pos = movingObject.position;

        // Only react if it actually moved (any axis is fine; motion is in YZ)
        if (havePrev && (pos - prevPos).sqrMagnitude < movementEpsilon * movementEpsilon)
            return;

        // Vector from center to mover, projected to YZ plane
        Vector3 rel = pos - center.position; // or (pos - centerPos)
        rel.x = 0f;
        if (rel.sqrMagnitude < 1e-8f) return;

        // YZ-plane angle: atan2(y, z)
        float angleDeg = Mathf.Atan2(rel.y, rel.z) * Mathf.Rad2Deg;

        if (havePrev)
        {
            float deltaDeg = Mathf.DeltaAngle(prevAngleDeg, angleDeg);
            if (Mathf.Abs(deltaDeg) > angleEpsilonDeg)
            {
                // Rotate around X (right) by the orbital delta
                target.Rotate(Vector3.right, deltaDeg, Space.Self);
            }
        }

        havePrev = true;
        prevPos = pos;
        prevAngleDeg = angleDeg;
    }
}
