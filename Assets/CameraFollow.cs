using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Vector3 offset;

    public Transform target;
    
    void Start()
    {
        if (target != null)
        {
            offset = transform.position - target.position;
        }
        else
        {
            enabled = false;
        }
        
    }

    void Update()
    {
        transform.position = target.position + offset;
    }
}
