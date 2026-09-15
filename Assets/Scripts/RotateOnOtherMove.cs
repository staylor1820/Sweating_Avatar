using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOnOtherMove : MonoBehaviour
{

    [Header("Scene References")]
    public Transform tracker;         // Live tracker in world
    public Transform pedal;           // Pedal (child of this crank)

    [Header("Crank Geometry")]
    public Axis rotateAround = Axis.Z;       // Crank hinge axis in crank's LOCAL space
    public Vector3 referenceLocalDir = Vector3.right; // Local dir from crank pivot to pedal at 0°
    public float angleOffsetDegrees = 0f;    // Fine offset if your model's zero doesn't match

    [Tooltip("If zero, auto-detect from the pedal's initial local position projected to the plane.")]
    public float crankRadius = 0f;

    [Header("Behavior")]
    public bool smoothRotation = true;
    public float smoothSpeed = 12f;

    [Tooltip("If true, also place the pedal to the tracker-projected position on the circle (in crank local space).")]
    public bool snapPedalLocalToTracker = false;

    public enum Axis { X, Y, Z }

    Quaternion _initialLocalRot;
    Vector3 _axisLocal;          // Unit axis in LOCAL space
    Vector3 _refDirLocalUnit;    // Unit reference direction on the rotation plane

    void Awake()
    {
        _initialLocalRot = transform.localRotation;
        _axisLocal = (rotateAround == Axis.X ? Vector3.right :
                      rotateAround == Axis.Y ? Vector3.up :
                                               Vector3.forward).normalized;

        // Ensure reference direction lives on the rotation plane
        _refDirLocalUnit = Vector3.ProjectOnPlane(referenceLocalDir, _axisLocal).normalized;

        if (_refDirLocalUnit.sqrMagnitude < 1e-6f)
        {
            // Fallback: pick an orthogonal direction
            _refDirLocalUnit = Vector3.ProjectOnPlane(Vector3.right, _axisLocal).normalized;
            if (_refDirLocalUnit.sqrMagnitude < 1e-6f)
                _refDirLocalUnit = Vector3.ProjectOnPlane(Vector3.up, _axisLocal).normalized;
        }

        // Auto radius from pedal initial local position if needed
        if (crankRadius <= 0f && pedal)
        {
            Vector3 p = Vector3.ProjectOnPlane(pedal.localPosition, _axisLocal);
            crankRadius = p.magnitude;
        }
    }

    void LateUpdate()
    {
        if (!tracker) return;

        // --- 1) Get tracker direction in CRANK'S LOCAL SPACE, projected to rotation plane ---
        Vector3 toTrackerWorld = tracker.position - transform.position;

        // Convert world direction → crank local direction
        Vector3 toTrackerLocal = transform.InverseTransformDirection(toTrackerWorld);

        // Only keep the component that lies on the rotation plane (remove unwanted axes)
        Vector3 planeDirLocal = Vector3.ProjectOnPlane(toTrackerLocal, _axisLocal);
        float planeMag = planeDirLocal.magnitude;
        if (planeMag < 1e-6f) return; // too close to axis; skip this frame
        Vector3 planeDirLocalUnit = planeDirLocal / planeMag;

        // --- 2) Compute the signed angle on ONE axis (right-hand rule) ---
        float angle = Vector3.SignedAngle(_refDirLocalUnit, planeDirLocalUnit, _axisLocal);
        angle += angleOffsetDegrees;

        Quaternion targetCrankLocalRot = Quaternion.AngleAxis(angle, _axisLocal) * _initialLocalRot;

        // --- 3) Apply crank rotation (optionally smoothed) ---
        if (smoothRotation)
        {
            float t = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetCrankLocalRot, t);
        }
        else
        {
            transform.localRotation = targetCrankLocalRot;
        }

        // --- 4) (Optional) Place the pedal at the tracker’s local position on the circle ---
        if (snapPedalLocalToTracker && pedal)
        {
            // If radius is unknown, compute from current pedal (keeps its length)
            float r = (crankRadius > 0f) ? crankRadius :
                      Vector3.ProjectOnPlane(pedal.localPosition, _axisLocal).magnitude;

            // Desired point on the rotation circle in crank LOCAL
            Vector3 desiredLocal = planeDirLocalUnit * r;

            // Keep the pedal on the rotation plane (zero out the component along axis)
            pedal.localPosition = Vector3.ProjectOnPlane(desiredLocal, _axisLocal);

            // (Optional) Align pedal orientation so its “tread” stays world-up-ish or faces tracker, etc.
            // Example: keep pedal's local up aligned with world up, but only around the pedal’s spindle:
            // pedal.localRotation = Quaternion.identity; // or your own rule
        }
    }

    // Editor gizmos to visualize plane + radius
    void OnDrawGizmosSelected()
    {
        // Draw rotation plane circle in world
        Gizmos.color = Color.white;
        float r = crankRadius;
        if (r <= 0f && pedal)
            r = Vector3.ProjectOnPlane(pedal.localPosition, AxisVectorWorld()).magnitude;

        if (r > 0f)
        {
            const int steps = 64;
            Vector3 center = transform.position;
            Vector3 axisW = AxisVectorWorld();
            // Build 2 orthonormal directions on the plane
            Vector3 x = transform.TransformDirection(_refDirLocalUnit == Vector3.zero ? OrthoTo(axisW) : _refDirLocalUnit).normalized;
            Vector3 y = Vector3.Cross(axisW, x).normalized;
            Vector3 prev = center + x * r;
            for (int i = 1; i <= steps; i++)
            {
                float t = (i / (float)steps) * Mathf.PI * 2f;
                Vector3 p = center + (Mathf.Cos(t) * x + Mathf.Sin(t) * y) * r;
                Gizmos.DrawLine(prev, p);
                prev = p;
            }
        }
    }

    Vector3 AxisVectorWorld()
    {
        // Convert local axis to world axis
        return transform.TransformDirection(
            rotateAround == Axis.X ? Vector3.right :
            rotateAround == Axis.Y ? Vector3.up : Vector3.forward).normalized;
    }

    static Vector3 OrthoTo(Vector3 n)
    {
        // Any vector not parallel to n
        return Mathf.Abs(Vector3.Dot(n, Vector3.up)) < 0.9f ? Vector3.up : Vector3.right;
    }
}
