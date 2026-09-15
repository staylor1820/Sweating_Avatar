using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RightHandHeatController : MonoBehaviour
{
    int counter = 0;
    float timer = 0;
    public float timeInHeater = 5;
    public UnityEvent rightheaterTouched;
    public UnityEvent rightheaterHandRemoved;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "righthand")
        {
            rightheaterTouched.Invoke();
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "righthand")
        {
            rightheaterHandRemoved.Invoke();
        }

    }
}
