using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class sliderValueControl : MonoBehaviour
{
    //public GameObject theAxisModel;
    public GameObject sliderNobe;
    public GameObject zeroReference;
    public GameObject knobCentre;
    public GameObject knobCentreReference;

    private float rangeLimitMin = 0f;
    private float rangeLimitMax = 1f;

    public Vector3 currentValue;
    public float rangeValue;
    private float onSliderValue;
    private float pre_onSliderValue;

    public bool isOn = false;
    private bool prev_isOn = false;
    public List<GameObject> anti_isOn;
    public SerialInOut shortInOut;
    public UnityClient unity_client;

    public GameObject slidingPlaneVisualizer;
    //public GameObject slidingPlane;

    public GameObject finger;
    public Collider t;

    public int index = 0;

    private float x;
    private float y;
    private float z;

    public float ratio = 1;
    public float bias = 0;

    private float xl;
    private float yl;
    private float zl;

    public float xOffsetA = 0;
    public float yOffsetA = 0;
    public float zOffsetA = 0;

    public float xOffsetB = 0;
    public float yOffsetB = 0;
    public float zOffsetB = 0;

    private float xOffset = 0;
    private float yOffset = 0;
    private float zOffset = 0;

    private float difference;

    private int AxisValue;
    private int pre_AxisValue;

    public int movementSign = 1;

    public bool sliderMoveFlag = true;
    public bool localCentreFlag = false;

    private float onSliderTCPMax = 1.95f;
    private float onSliderTCPMin = -1.6f;

    private float ax;
    private float ay;
    private float az;

    private float norm;

    private float sp = 1;

    public Vector3 p1;
    public Vector3 p2;

    public Vector3 rot;

    private int counter = 0;
    private int pre_sliderValue = 0;

    public GameObject refernce;

    private DateTime t1;

    //public Text text;

    // Start is called before the first frame update
    void Start()
    {
        x = slidingPlaneVisualizer.transform.position.x;
        y = slidingPlaneVisualizer.transform.position.y;
        z = slidingPlaneVisualizer.transform.position.z;

        //xl = slidingPlane.transform.localPosition.x;
        //yl = slidingPlane.transform.localPosition.y;
        //zl = slidingPlane.transform.localPosition.z;

        ax = p1.x - p2.x;
        ay = p1.y - p2.y;
        az = p1.z - p2.z;

        norm = Mathf.Sqrt(ax * ax + ay * ay + az * az);
    }

    // Update is called once per frame
    void Update()
    {
        knobCentre.transform.position = refernce.transform.position;

        onSliderValue = sliderNobe.transform.localPosition.y;
        currentValue = sliderNobe.transform.position;
        rangeValue = Vector3.Distance(sliderNobe.transform.position, zeroReference.transform.position)/4.1f*(rangeLimitMax-rangeLimitMin) + rangeLimitMin;

        switch (index)
        {
            case 1:
                //xl += (x - currentValue.x) * ratio;
                x = currentValue.x;
                movementSign = 1;
                break;
            case 2:
                //yl += -(y - currentValue.y) * ratio;
                y = currentValue.y;
                movementSign = -1;
                break;
            case 3:
                //zl += (z - currentValue.z) * ratio;
                z = currentValue.z;
                movementSign = -1;
                break;
            default:
                break;
        }

        slidingPlaneVisualizer.transform.position = new Vector3(x, y, z);

        xOffset = rangeValue * xOffsetA + xOffsetB;
        yOffset = rangeValue * yOffsetA + yOffsetB;
        zOffset = rangeValue * zOffsetA + zOffsetB;

        //slidingPlane.transform.localPosition = new Vector3(xl-xOffset, yl-yOffset, zl-zOffset);
        //slidingPlane.transform.localPosition = new Vector3(x, y, z);

        //if (isOn)
        //{
        //    //text.text = sliderMoveFlag.ToString() + ", " + unity_client.receiveFlag.ToString() + ", " + Vector3.Distance(knobCentreReference.transform.position, finger.transform.GetChild(0).transform.position).ToString("f3");

        //    if ((shortInOut.value > 360 | shortInOut.value < 50) | (sliderNobe.transform.localPosition.y < onSliderTCPMin | sliderNobe.transform.localPosition.y > onSliderTCPMax))
        //    {
        //        if (sliderNobe.transform.localPosition.y < onSliderTCPMin)
        //        {
        //            sliderNobe.transform.localPosition = new Vector3(0, onSliderTCPMin, 0);
        //        }

        //        if(sliderNobe.transform.localPosition.y > onSliderTCPMax)
        //        {
        //            sliderNobe.transform.localPosition = new Vector3(0, onSliderTCPMax, 0);
        //        }

        //        //center
        //        sliderMoveFlag = true;
        //    }

        //    if (!sliderMoveFlag & unity_client.receiveFlag & Vector3.Distance(knobCentreReference.transform.position, finger.transform.position) < 0.1)
        //    {
        //        localCentreFlag = true;
        //    }
        //    else
        //    {
        //        localCentreFlag = false;
        //    }

        //    if (localCentreFlag)
        //    {
        //        knobCentre.transform.position = knobCentreReference.transform.position;
        //        knobCentre.transform.localPosition = new Vector3(0, knobCentre.transform.localPosition.y, 0);
        //    }

        //    if (!prev_isOn)
        //    {
        //        //center 

        //        /*
        //        if (Mathf.Abs(Mathf.Abs(rangeValue) - Mathf.Abs(rangeLimitMax)) < 0.001)
        //        {
        //            wirelessAxes.sendSlider(0, 255);
        //        }

        //        if (Mathf.Abs(Mathf.Abs(rangeValue) - Mathf.Abs(rangeLimitMin)) < 0.001)
        //        {
        //            wirelessAxes.sendSlider(0, 0);
        //        }*/
        //        //sliderMoveFlag = true;
        //    }

        //    if (localCentreFlag)
        //    {
        //        updateVirtualSlider();
        //    }

        //}

        if (isOn)
        {
            if (!prev_isOn)
            {
                while (shortInOut.value > 240 | shortInOut.value < 170)
                {
                    if (shortInOut.value > 240)
                    {
                        shortInOut.SetSlider(-350);
                    }
                    else if (shortInOut.value < 170)
                    {
                        shortInOut.SetSlider(350);
                    }
                }

                if (shortInOut.value < 240 & shortInOut.value > 170)
                {
                    if (index == 2)
                    {
                        shortInOut.SetSlider(-250);
                    }
                    else
                    {
                        shortInOut.SetSlider(0);
                    }
                }

                t1 = DateTime.Now;
            }

            if (counter >=10 & pre_sliderValue!=shortInOut.value & DateTime.Now > t1.AddSeconds(2.5))
            {
                if (shortInOut.value < 80 | shortInOut.value > 320)
                {
                    if (shortInOut.value < 80)
                    {
                        //shortInOut.SetSlider(320);
                        sp = 0.1f * movementSign;
                    }

                    if (shortInOut.value > 320)
                    {
                        sp = -0.1f * movementSign;
                        //shortInOut.SetSlider(-320);
                    }

                    if (index == 3)
                    {
                        sp = sp * -1;
                    }

                    unity_client.customMove(ax / (Mathf.Round(norm / sp * 100) / 100), ay / (Mathf.Round(norm / sp * 100) / 100), az / (Mathf.Round(norm / sp * 100) / 100), rot.x, rot.y, rot.z, speed: sp, acc: 1.5f, movementType: 4);
                }
                else
                {
                    unity_client.stopRobot();
                    //shortInOut.SetSlider(0);
                }

                counter = 0;
            }
            counter++;

            //updateVirtualSlider();

        }

        prev_isOn = isOn;

        currentValue = sliderNobe.transform.localPosition;

        pre_onSliderValue = onSliderValue;

        pre_sliderValue = shortInOut.value;
    }

    private void moveSlider()
    {
        /*
        AxisValue = wirelessAxes.sliderOne;

        sliderNobe.transform.localPosition = new Vector3(sliderNobe.transform.localPosition.x, sliderNobe.transform.localPosition.y + (AxisValue-pre_AxisValue)/255*(1.89245f-0.86574f), sliderNobe.transform.localPosition.z);

        pre_AxisValue = AxisValue;
        */

        
    }

    private void updateVirtualSlider()
    {
        //float newY;

        //newY = theAxisModel.transform.position.z * (-9f) - 3.4f
        //        + (wirelessAxes.sliderOne / 255f) * 0.5f + 0.1f;

        //sliderNobe.transform.localPosition = new Vector3(sliderNobe.transform.localPosition.x, newY, sliderNobe.transform.localPosition.z);

        sliderNobe.transform.localPosition = new Vector3(sliderNobe.transform.localPosition.x, knobCentre.transform.localPosition.y + 0.3f- movementSign*(float)(shortInOut.value - 415/2) / 415 * (1.89245f - 0.86574f), sliderNobe.transform.localPosition.z);
    }

    public void isOnCheck()
    {
        foreach (GameObject g in anti_isOn)
        {
            g.GetComponent<sliderValueControl>().isOn = false;
        }
    }
}
