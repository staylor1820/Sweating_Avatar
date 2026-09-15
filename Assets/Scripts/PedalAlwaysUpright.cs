using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class PedalAlwaysUpright : MonoBehaviour
{
    [Tooltip("Optional: what 'forward' should be when upright (e.g., your bike frame).")]
    public Transform forwardRef;     // leave null if you don't have one
    public Vector3 worldUp = Vector3.up;

    [Header("Axis Settings")]
    [Tooltip("Which local axis should be treated as 'forward' for this object.")]
    public Vector3 localForward = Vector3.forward;
    [Tooltip("Which local axis should be treated as 'up' for this object.")]
    public Vector3 localUp = Vector3.up;

    void LateUpdate()
    {
        // 1. Get a base forward direction lying in the horizontal plane
        Vector3 fwd = forwardRef ? forwardRef.forward
                                 : Vector3.ProjectOnPlane(transform.forward, worldUp);

        // 2. Fallback if projection is too small
        if (fwd.sqrMagnitude < 1e-6f)
            fwd = transform.parent ? transform.parent.right : Vector3.right;

        // 3. Build a "world upright" rotation
        Quaternion worldBasis = Quaternion.LookRotation(fwd.normalized, worldUp);

        // 4. Adjust for your mesh’s local orientation
        Quaternion modelBasis = Quaternion.LookRotation(localForward.normalized, localUp.normalized);

        // 5. Apply final orientation
        transform.rotation = worldBasis * Quaternion.Inverse(modelBasis);
    }
}
