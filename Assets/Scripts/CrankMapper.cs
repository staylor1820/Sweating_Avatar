using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrankMapper : MonoBehaviour
{
    [Header("Tracker inputs (world-space tracked objects)")]
    public Transform leftTracker;   // physical left pedal tracker
    public Transform rightTracker;  // physical right pedal tracker (optional but recommended)

    [Header("Crank rig (virtual objects to drive)")]
    public Transform crankCenter;   // virtual bottom bracket center
    public Transform crankAxisRef;  // its forward (or chosen axis) = crank axis direction
    public Transform virtualCrank;  // rotate this about the axis to spin the drivetrain
    public Transform leftPedal;     // optional: rotate/position this
    public Transform rightPedal;    // optional

    [Header("Crank geometry")]
    public float crankLengthMeters = 0.1725f; // 172.5 mm typical; used if only one tracker or for clamping
    public bool inferMissingPedal = true;     // if one tracker is lost, mirror the other + 180°

    [Header("Smoothing")]
    [Range(0f, 1f)] public float angleSmoothing = 0.15f;  // 0=no smoothing, 1=heavy smoothing
    [Range(0f, 1f)] public float rpmSmoothing = 0.25f;

    [Header("Calibration")]
    public bool autoZeroOnStart = true;  // sets current left tracker angle as 0 at Start()
    public KeyCode reZeroKey = KeyCode.Z;

    // Output (read-only)
    [SerializeField, Tooltip("Current crank angle in degrees [0,360)")] private float crankAngleDeg;
    [SerializeField, Tooltip("Cadence in RPM")] private float rpm;

    // Internals
    private Vector3 axisDir;       // normalized world-space crank axis
    private Vector3 origin;        // world-space crank center
    private Vector3 refDirWorld;   // world-space 0° reference direction in crank plane
    private float lastAngleRad;    // unwrapped radians
    private float unwrappedAngle;  // continuous radians (not modulo 2π)
    private float lastTime;

    void Start()
    {
        if (!crankCenter || !crankAxisRef)
        {
            Debug.LogError("CrankMapper: Assign crankCenter and crankAxisRef.");
            enabled = false; return;
        }

        origin = crankCenter.position;
        axisDir = GetAxisDir();

        // Initial reference direction (x-axis of the crank plane).
        refDirWorld = GetInitialRefDir();

        // Initialize unwrapped angle using whichever tracker is valid
        float a = GetBestCrankAngleRad(out bool ok);
        if (!ok) a = 0f;
        lastAngleRad = a;
        unwrappedAngle = a;
        lastTime = Time.time;

        if (autoZeroOnStart && ok)
            ZeroHere();
    }

    void Update()
    {
        origin = crankCenter.position;
        axisDir = GetAxisDir();

        // Let user re-zero the angle at any time.
        if (Input.GetKeyDown(reZeroKey))
            ZeroHere();

        // Measure current angle
        float angleNow = GetBestCrankAngleRad(out bool validNow);

        // Handle loss gracefully: if no tracker is valid, just keep previous angle.
        if (!validNow)
        {
            ApplyTransforms(angleNow: unwrappedAngle); // apply last known
            return;
        }

        // Unwrap angle to keep it continuous
        float delta = Mathf.DeltaAngle(Mathf.Rad2Deg * lastAngleRad, Mathf.Rad2Deg * angleNow) * Mathf.Deg2Rad;
        unwrappedAngle += delta;

        // Smoothing on angle (exponential)
        float smoothed = Mathf.LerpAngle(lastAngleRad * Mathf.Rad2Deg, (unwrappedAngle % (2f * Mathf.PI)) * Mathf.Rad2Deg, 1f - angleSmoothing) * Mathf.Deg2Rad;
        // Keep unwrapped center consistent with smoothed (small correction, optional)
        float corr = Mathf.DeltaAngle(smoothed * Mathf.Rad2Deg, unwrappedAngle * Mathf.Rad2Deg) * Mathf.Deg2Rad;
        unwrappedAngle -= 0.15f * corr;

        // RPM estimate
        float t = Time.time;
        float dt = Mathf.Max(1e-4f, t - lastTime);
        float angVel = (unwrappedAngle - lastAngleRad) / dt; // rad/s
        float rpmInstant = angVel * 60f / (2f * Mathf.PI);
        rpm = Mathf.Lerp(rpm, rpmInstant, 1f - rpmSmoothing);

        lastAngleRad = unwrappedAngle;
        lastTime = t;

        ApplyTransforms(unwrappedAngle);
    }

    private void ApplyTransforms(float angleNow)
    {
        // Drive the virtual crank
        if (virtualCrank)
        {
            // Rotate around the axis passing through crankCenter
            Quaternion rot = Quaternion.AngleAxis(angleNow * Mathf.Rad2Deg, axisDir);
            // Compose rotation so that 0° is refDirWorld; we align virtualCrank's local with world via pivot trick:
            virtualCrank.rotation = rot * Quaternion.LookRotation(refDirWorld, axisDir);
            // Recenter at origin if needed
            Vector3 pivotDelta = origin - virtualCrank.position;
            virtualCrank.position += pivotDelta;
        }

        // Optionally position/rotate pedals along the circle
        if (leftPedal) PlacePedal(leftPedal, angleNow);
        if (rightPedal) PlacePedal(rightPedal, angleNow + Mathf.PI); // 180° out of phase

        // Public-friendly readouts
        crankAngleDeg = Mathf.Repeat(angleNow * Mathf.Rad2Deg, 360f);
    }

    private void PlacePedal(Transform pedal, float angleRad)
    {
        // Compute world-space position on the crank circle.
        // Basis vectors in the crank plane:
        Vector3 x = refDirWorld.normalized;
        Vector3 y = Vector3.Cross(axisDir, x).normalized;
        Vector3 pos = origin + (Mathf.Cos(angleRad) * x + Mathf.Sin(angleRad) * y) * crankLengthMeters;

        // Orient the pedal axle roughly perpendicular to the crank arm (optional).
        // Many rigs only need the position; if you need a local roll, add it here.
        pedal.position = pos;
        pedal.rotation = Quaternion.LookRotation(x * Mathf.Cos(angleRad) + y * Mathf.Sin(angleRad), axisDir);
    }

    private Vector3 GetAxisDir()
    {
        // Use crankAxisRef's forward as the crank axis (customize to suit your rig)
        Vector3 dir = crankAxisRef.forward;
        return dir.sqrMagnitude > 0f ? dir.normalized : Vector3.forward;
    }

    private Vector3 GetInitialRefDir()
    {
        // Try to derive a sane in-plane reference direction:
        // Use left tracker vector (to define 0°) if present, else crankAxisRef.right.
        if (leftTracker)
        {
            Vector3 v = Vector3.ProjectOnPlane(leftTracker.position - origin, axisDir);
            if (v.sqrMagnitude > 1e-6f) return v.normalized;
        }
        Vector3 alt = Vector3.ProjectOnPlane(crankAxisRef.right, axisDir);
        if (alt.sqrMagnitude < 1e-6f) alt = Vector3.right;
        return alt.normalized;
    }

    private void ZeroHere()
    {
        // Re-define refDirWorld so that the current best angle becomes 0°
        float a = GetBestCrankAngleRad(out bool ok);
        if (!ok) return;

        // Current in-plane vector becomes new x-axis
        Vector3 cur = Vector3.ProjectOnPlane(GetBestTrackerPos() - origin, axisDir).normalized;
        if (cur.sqrMagnitude > 1e-6f)
            refDirWorld = cur;

        // Reset unwrap refs
        lastAngleRad = 0f;
        unwrappedAngle = 0f;
    }

    private Vector3 GetBestTrackerPos()
    {
        // Prefer the tracker with a stronger signal; simple pick by availability.
        if (leftTracker && leftTracker.gameObject.activeInHierarchy) return leftTracker.position;
        if (rightTracker && rightTracker.gameObject.activeInHierarchy) return rightTracker.position;
        return leftTracker ? leftTracker.position : rightTracker ? rightTracker.position : origin;
    }

    private float GetBestCrankAngleRad(out bool valid)
    {
        valid = false;

        bool leftOk = leftTracker && leftTracker.gameObject.activeInHierarchy;
        bool rightOk = rightTracker && rightTracker.gameObject.activeInHierarchy;

        if (!leftOk && !rightOk)
        {
            if (inferMissingPedal)
            {
                // Neither is visible; keep previous
                valid = false;
                return lastAngleRad;
            }
            return 0f;
        }

        // Compute angle from whichever tracker is valid.
        if (leftOk && rightOk)
        {
            // Pick the one with the position closer to the crank circle (robust to temporary spikes)
            float aL = AngleFromTracker(leftTracker.position, out float errL);
            float aR = AngleFromTracker(rightTracker.position, out float errR);
            // Radian angles must be 180° out; if not, prefer lower error
            float a = (errL <= errR) ? aL : (aR - Mathf.PI); // subtract π so right maps to left frame
            valid = true;
            return WrapPi(a);
        }
        else if (leftOk)
        {
            float aL = AngleFromTracker(leftTracker.position, out _);
            valid = true; return WrapPi(aL);
        }
        else // right only
        {
            float aR = AngleFromTracker(rightTracker.position, out _);
            valid = true; return WrapPi(aR - Mathf.PI);
        }
    }

    private float AngleFromTracker(Vector3 trackerPos, out float inPlaneError)
    {
        // Vector from crank center to tracker, projected into crank plane
        Vector3 v = trackerPos - origin;
        Vector3 vProj = Vector3.ProjectOnPlane(v, axisDir);
        float mag = vProj.magnitude;

        // Estimate how far the tracker is from the ideal circle
        inPlaneError = Mathf.Abs(mag - crankLengthMeters);

        // Build an in-plane orthonormal basis (x along refDirWorld)
        Vector3 x = refDirWorld.normalized;
        Vector3 y = Vector3.Cross(axisDir, x).normalized;

        // Components in that basis
        float vx = Vector3.Dot(vProj, x);
        float vy = Vector3.Dot(vProj, y);

        // atan2 gives the signed angle CCW about +axisDir
        float angle = Mathf.Atan2(vy, vx);
        return angle;
    }

    private static float WrapPi(float a)
    {
        // Wrap to [-π, π)
        while (a >= Mathf.PI) a -= 2f * Mathf.PI;
        while (a < -Mathf.PI) a += 2f * Mathf.PI;
        return a;
    }

    // Public getters
    public float CrankAngleDegrees => crankAngleDeg;
    public float CadenceRPM => rpm;
}
