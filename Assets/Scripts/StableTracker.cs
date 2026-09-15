using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StableTracker : MonoBehaviour
{
    public Transform tracker;
    public Transform target;
    public Transform origin;
    int counter = 0;
    public bool change;
    private bool CounterReset = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        counter++;
        if (change && counter >= 20) 
        { 
            Vector3 offset = tracker.position - origin.position;
            origin.position = target.position - offset;
            change = false;
            CounterReset = true;
            counter = 0;
        }
        if (CounterReset)
        {
            counter = 0;
        }

    }
}
