using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LeftHandHeatController : MonoBehaviour
{
    int counter = 0;
    float timer = 0;
    public float timeInHeater = 5;
    public UnityEvent leftheaterTouched;
    public UnityEvent leftheaterHandRemoved;
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
        if (other.gameObject.tag == "lefthand")
        {
            leftheaterTouched.Invoke();
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "lefthand")
        {
            leftheaterHandRemoved.Invoke();
        }

    }
}
