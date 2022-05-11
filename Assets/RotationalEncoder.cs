using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationalEncoder : MonoBehaviour
{
    //public GameObject virtualTouchPoint;
    //public GameObject testcube;

    //public Vector3 test;
    public GameObject Object;

    public int index;

    public SerialInOut shortInOut;

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

    public Vector3 test;

    // Start is called before the first frame update
    void Start()
    {
        //test = virtualTouchPoint.transform.position;
        rotStat = Object.transform.eulerAngles;
        //testcube.transform.position = test;

        startUpQuat = Object.transform.rotation;

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

                currentValue = shortInOut.rotaryValue;
            }

            if (shortInOut.rotaryValue - currentValue != 0)
            {
                float v = (shortInOut.rotaryValue - currentValue) / 102.4f * 360f;

                switch (index)
                {
                    case 1:
                        Object.transform.rotation = Quaternion.Euler(v, 0, 0) * startUpQuat;
                        break;
                    case 2:
                        Object.transform.rotation = Quaternion.Euler(0, v, 0) * startUpQuat;
                        break;
                    case 3:
                        Object.transform.rotation = Quaternion.Euler(0, 0, v) * startUpQuat;
                        break;
                    default:
                        break;
                }
            }

        }
        else
        {
            rotationIndicator.SetActive(false);
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
}
