using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RotationalEncoder : MonoBehaviour
{
    //public GameObject virtualTouchPoint;
    //public GameObject testcube;

    //public Vector3 test;
    public GameObject Object;

    public int index;

    public SerialInOut shortInOut;
    public UnityClient unity_client;

    public Vector3 RotEncoderPos;// = new Vector3(-0.459f, 0.182f, 0.319f);
    // = new Vector3(-0.901f, 0.184f, 0.15f);
    // = new Vector3(-0.901f, 0.265f, 0.352f);

    public float currentValue = 0;
    public bool isOn = false;

    public List<GameObject> anti_isOn;

    private Vector3 rotStat;

    private bool pre_isOn;

    public GameObject rotationIndicator;

    private Quaternion startUpQuat;
    private Quaternion indicatorStartUpQuat;

    public Vector3 test;
    private DateTime t1;

    private bool temp = false;

    // Start is called before the first frame update
    void Start()
    {
        //test = virtualTouchPoint.transform.position;
        rotStat = Object.transform.eulerAngles;
        //testcube.transform.position = test;

        startUpQuat = Object.transform.rotation;
        indicatorStartUpQuat = rotationIndicator.transform.rotation;

        currentValue = shortInOut.rotaryValue;
    }

    // Update is called once per frame
    void Update()
    {
        if (isOn)
        {
            rotationIndicator.SetActive(true);

            if (!pre_isOn)
            {
                rotStat = Object.transform.eulerAngles;
                startUpQuat = Object.transform.rotation;
                indicatorStartUpQuat = rotationIndicator.transform.rotation;

                currentValue = shortInOut.rotaryValue;

                t1 = DateTime.Now;
            }

            if (stateCheck())
            {
                if (!temp)
                {
                    DateTime actionFinishTime = DateTime.Now;

                    Debug.Log("Finished Time: " + actionFinishTime.ToString());
                    Debug.Log("Difference: " + (actionFinishTime - SectionVewControlManager.actionStartTime).TotalMilliseconds.ToString("F6"));

                    SectionVewControlManager.countTimeFlag = true;
                }

                temp = true;
            }

            if (shortInOut.rotaryValue - currentValue != 0)
            {
                float v = (shortInOut.rotaryValue - currentValue) / 102.4f * 360f;

                switch (index)
                {
                    case 1:
                        Object.transform.rotation = Quaternion.Euler(v, 0, 0) * startUpQuat;
                        rotationIndicator.transform.rotation = Quaternion.Euler(v, 0, 0) * indicatorStartUpQuat;
                        break;
                    case 2:
                        Object.transform.rotation = Quaternion.Euler(0, v, 0) * startUpQuat;
                        rotationIndicator.transform.rotation = Quaternion.Euler(0, v, 0) * indicatorStartUpQuat;
                        break;
                    case 3:
                        Object.transform.rotation = Quaternion.Euler(0, 0, -v) * startUpQuat;
                        rotationIndicator.transform.rotation = Quaternion.Euler(0, 0, -v) * indicatorStartUpQuat;
                        break;
                    default:
                        break;
                }
            }

        }
        else
        {
            rotationIndicator.SetActive(false);

            temp = false;
        }

        pre_isOn = isOn;
    }

    public void isOnCheck()
    {
        foreach (GameObject g in anti_isOn)
        {
            g.GetComponent<RotationalEncoder>().isOn = false;
        }
    }

    private bool stateCheck()
    {
       return ((DateTime.Now > t1.AddSeconds(0.2)) & unity_client.robotStopped);
    }
}
