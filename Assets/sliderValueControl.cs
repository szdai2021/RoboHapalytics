using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public WirelessAxes wirelessAxes;
    public UnityClient unity_client;

    public GameObject slidingPlaneVisualizer;
    public GameObject slidingPlane;

    public GameObject finger;

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

    private int movementSign = 1;

    public bool sliderMoveFlag = true;
    public bool localCentreFlag = false;

    //public Text text;

    // Start is called before the first frame update
    void Start()
    {
        x = slidingPlaneVisualizer.transform.position.x;
        y = slidingPlaneVisualizer.transform.position.y;
        z = slidingPlaneVisualizer.transform.position.z;

        xl = slidingPlane.transform.localPosition.x;
        yl = slidingPlane.transform.localPosition.y;
        zl = slidingPlane.transform.localPosition.z;
    }

    // Update is called once per frame
    void Update()
    {
        onSliderValue = sliderNobe.transform.localPosition.y;
        currentValue = sliderNobe.transform.position;
        rangeValue = Vector3.Distance(sliderNobe.transform.position, zeroReference.transform.position)/4.1f*(rangeLimitMax-rangeLimitMin) + rangeLimitMin;

        switch (index)
        {
            case 1:
                xl += (x - currentValue.x) * ratio;
                x = currentValue.x;
                movementSign = 1;
                break;
            case 2:
                yl += -(y - currentValue.y) * ratio;
                y = currentValue.y;
                movementSign = -1;
                break;
            case 3:
                zl += (z - currentValue.z) * ratio;
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

        slidingPlane.transform.localPosition = new Vector3(xl-xOffset, yl-yOffset, zl-zOffset);

        if (isOn)
        {
            //text.text = sliderMoveFlag.ToString() + ", " + unity_client.receiveFlag.ToString() + ", " + Vector3.Distance(knobCentreReference.transform.position, finger.transform.GetChild(0).transform.position).ToString("f3");

            if (wirelessAxes.sliderOne > 220 | wirelessAxes.sliderOne < 30)
            {
                wirelessAxes.sendSlider(0, 127);
                sliderMoveFlag = true;
            }

            if (!sliderMoveFlag & unity_client.receiveFlag & Vector3.Distance(knobCentreReference.transform.position, finger.transform.GetChild(0).transform.position) < 0.1)
            {
                localCentreFlag = true;
            }
            else
            {
                localCentreFlag = false;
            }

            if (localCentreFlag)
            {
                knobCentre.transform.position = knobCentreReference.transform.position;
                knobCentre.transform.localPosition = new Vector3(0, knobCentre.transform.localPosition.y, 0);
            }

            if (!prev_isOn)
            {
                wirelessAxes.sendSlider(0, 127);
                /*
                if (Mathf.Abs(Mathf.Abs(rangeValue) - Mathf.Abs(rangeLimitMax)) < 0.001)
                {
                    wirelessAxes.sendSlider(0, 255);
                }

                if (Mathf.Abs(Mathf.Abs(rangeValue) - Mathf.Abs(rangeLimitMin)) < 0.001)
                {
                    wirelessAxes.sendSlider(0, 0);
                }*/
                //sliderMoveFlag = true;
            }

            if (localCentreFlag)
            {
                updateVirtualSlider();
            }
            
        }

        prev_isOn = isOn;

        currentValue = sliderNobe.transform.localPosition;

        pre_onSliderValue = onSliderValue;
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

        sliderNobe.transform.localPosition = new Vector3(sliderNobe.transform.localPosition.x, knobCentre.transform.localPosition.y + movementSign*(float)(wirelessAxes.sliderOne - 127) / 255 * (1.89245f - 0.86574f), sliderNobe.transform.localPosition.z);
    }

    public void isOnCheck()
    {
        foreach (GameObject g in anti_isOn)
        {
            g.GetComponent<sliderValueControl>().isOn = false;
        }
    }
}
