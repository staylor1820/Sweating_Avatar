using RootMotion.Demos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelativeMotionPedalTracker : MonoBehaviour
{
    public Transform source;        // The object you move
    public Transform target;        // The object that should mimic the motion
    [Header("What to copy")]
    public bool copyPosition = true;
    public bool copyRotation = false;   // optional

    [Header("How to interpret & apply translation")]
    // If true: interpret the source's translation in its own local axes,
    // then apply those same numeric deltas to the target's local axes.
    // If false: take the world-space delta and re-express it in the target's local axes.
    public bool mapSourceLocalToTargetLocal = false;

    [Header("Multipliers & axis masks")]
    public float positionMultiplier = 1f;
    public Vector3 axisMask = Vector3.one;   // e.g., (1,0,1) to ignore Y
    public float rotationMultiplier = 1f;    // 1 = full, 0.5 = half, etc.

    Vector3 _prevSourcePos;
    Quaternion _prevSourceRot;

    void OnEnable()
    {
        if (source == null || target == null)
        {
            Debug.LogError($"{nameof(RelativeMotionPedalTracker)}: Assign source and target.");
            enabled = false;
            return;
        }
        _prevSourcePos = source.position;
        _prevSourceRot = source.rotation;
    }

    // Use LateUpdate so you read the final source pose for this frame.
    void LateUpdate()
    {
        if (copyPosition)
        {
            // 1) Position delta of the source this frame (in world space)
            Vector3 deltaWorld = source.position - _prevSourcePos;

            Vector3 deltaToApplyLocal;

            if (mapSourceLocalToTargetLocal)
            {
                // Express the source's world delta in the source's local axes...
                Vector3 deltaSourceLocal = source.InverseTransformDirection(deltaWorld);
                // ...then apply those *same numbers* on the target's local axes.
                deltaToApplyLocal = deltaSourceLocal;
            }
            else
            {
                // Take the source's world delta and re-express it in the target's local axes.
                deltaToApplyLocal = target.InverseTransformDirection(deltaWorld);
            }

            // Axis mask + multiplier
            deltaToApplyLocal = Vector3.Scale(deltaToApplyLocal, axisMask) * positionMultiplier;

            // Apply in target local space
            target.localPosition += deltaToApplyLocal;
        }

        if (copyRotation)
        {
            // Source delta rotation this frame (world)
            Quaternion deltaRot = source.rotation * Quaternion.Inverse(_prevSourceRot);

            // Apply that relative rotation to the target's *local* rotation.
            // (This composes the same incremental rotation onto the target.)
            if (rotationMultiplier != 1f)
            {
                // Interpolate the delta if you want a scaled effect
                deltaRot = Quaternion.Slerp(Quaternion.identity, deltaRot, rotationMultiplier);
            }
            target.localRotation = deltaRot * target.localRotation;
        }

        _prevSourcePos = source.position;
        _prevSourceRot = source.rotation;
    }
}
