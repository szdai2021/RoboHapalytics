using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class knobMovement : MonoBehaviour
{
    public GameObject knob;
    public GameObject startPoint;
    public GameObject endPoint;

    public float moveRatio; // 0 - 1

    private Vector3 coeff;

    private void Start()
    {
        coeff.x = endPoint.transform.position.x - startPoint.transform.position.x;
        coeff.y = endPoint.transform.position.y - startPoint.transform.position.y;
        coeff.z = endPoint.transform.position.z - startPoint.transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        knob.transform.position = startPoint.transform.position + coeff*moveRatio;
    }
}
