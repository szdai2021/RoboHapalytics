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

    public WirelessAxes wirelessAxis;

    public Vector3 RotEncoderPos;// = new Vector3(-0.459f, 0.182f, 0.319f);
    // = new Vector3(-0.901f, 0.184f, 0.15f);
    // = new Vector3(-0.901f, 0.265f, 0.352f);

    public float currentValue = 0;
    public bool isOn = false;

    public List<GameObject> anti_isOn;

    private Vector3 rotStat;

    private bool pre_isOn;

    private float xChangeTotal = 0;
    private float yChangeTotal = 0;
    private float zChangeTotal = 0;

    private float xStep = 0;
    private float yStep = 0;
    private float zStep = 0;

    private int stepNum = 60;
    // Start is called before the first frame update
    void Start()
    {
        //test = virtualTouchPoint.transform.position;
        rotStat = Object.transform.eulerAngles;
        //testcube.transform.position = test;
    }

    // Update is called once per frame
    void Update()
    {
        if (isOn)
        {
            if (!pre_isOn)
            {
                rotStat = Object.transform.eulerAngles;
                stepNum = 60;
            }

            if (wirelessAxis.rotary - currentValue != 0)
            {
                stepNum = 60;
            }

            switch (index)
            {
                case 1:
                    xChangeTotal += -(wirelessAxis.rotary - currentValue) * 20;
                    xStep = xChangeTotal / stepNum;
                    rotStat.x += xStep;
                    break;
                case 2:
                    yChangeTotal += (wirelessAxis.rotary - currentValue) * 20;
                    yStep = yChangeTotal / stepNum;
                    rotStat.y += yStep;
                    break;
                case 3:
                    zChangeTotal += (wirelessAxis.rotary - currentValue) * 20;
                    zStep = zChangeTotal / stepNum;
                    rotStat.z += zStep;
                    break;
                default:
                    break;
            }

            if (Mathf.Abs(xChangeTotal) > 0.01 | Mathf.Abs(yChangeTotal) > 0.01 | Mathf.Abs(zChangeTotal) > 0.01)
            {
                //Object.transform.eulerAngles = rotStat; //new Vector3(rotStat.x + xStep, rotStat.y + yStep, rotStat.z + zStep);

                xChangeTotal -= xStep;
                yChangeTotal -= yStep;
                zChangeTotal -= zStep;
            }

            stepNum--;
            currentValue = wirelessAxis.rotary;
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
