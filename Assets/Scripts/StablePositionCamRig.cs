using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;

public class StablePositionCamRig : MonoBehaviour
{
    [Header("Assign your XROrigin here (the XR Rig)")]
    public Transform xrOrigin;
    public Transform head;
    public bool startRecenter;
    int counter = 0;
    private bool CounterReset = false;
    //[Header("Assign the Main Camera (the HMD camera under Camera Offset)")]
    public Transform target;


    private void Update()
    {
        counter++;
        if (startRecenter == true && counter >= 20)
        {
            XROrigin XROrigin = GetComponent<XROrigin>();
            XROrigin.MoveCameraToWorldLocation(new Vector3(target.position.x, head.transform.position.y, target.position.z));
            XROrigin.MatchOriginUpCameraForward(target.up, target.forward);
            startRecenter = false;
            CounterReset = true;
            counter = 0;

        }
        if (CounterReset)
        {
            counter = 0;
        }


        
        //XRInputSubsystem.TryRecenter();
    }
}
