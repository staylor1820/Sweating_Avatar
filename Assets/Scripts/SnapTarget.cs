using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapTarget : MonoBehaviour
{
    public CCDIK ccd;                // Final IK CCDIK
    public Transform pivot;          // rotation center / first bone
    public Transform realTarget;     // moving target on the circle
    public Transform proxyTarget;    // empty GameObject assigned to ccd.solver.target
    public float fixedRadius = 1.0f; // constant radius around pivot
    public Vector3 upAxis = Vector3.up;

    void Update()
    {
        Vector3 to = realTarget.position - pivot.position;
        Vector3 flat = Vector3.ProjectOnPlane(to, upAxis);
        if (flat.sqrMagnitude < 1e-6f) return;

        Vector3 dir = flat.normalized;
        proxyTarget.position = pivot.position + dir * fixedRadius;

        // Ensure CCDIK uses the proxy
        ccd.solver.target = proxyTarget;
    }
}
