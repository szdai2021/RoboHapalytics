using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserStudyControl : MonoBehaviour
{
    public GameObject sliderknob;

    public WirelessAxes wireless;
    public UnityClient unity_client;

    public int scenario = 0; // 0: default;  1: virtual slider with controller;  2: virtual slider with physical 1:1 slider;  3: the haptic slider system
    public bool startFlag = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
