using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class sliderValueControl : MonoBehaviour
{
    public GameObject sliderKnobe;
    public GameObject zeroReference;
    public GameObject knobCentre;
    public GameObject knobCentreReference;
    public GameObject sliderCenter;

    public Collider rightChecker;
    public Collider leftChecker;
    public GameObject shortSliderTracker;

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

    public GameObject finger;
    public Collider t;

    public int index = 0;

    private float x;
    private float y;
    private float z;

    public float ratio = 1;
    public float bias = 0;

    public int movementSign = 1;

    public bool sliderMoveFlag = true;
    public bool localCentreFlag = false;

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

    private bool sliderControlIsOn = false;

    private bool dynamicLeftEnd = false;
    private bool dynamicRightEnd = false;
    private bool dynamicMoving = false;
    private int dynamicCounter = 0;
    private float speedScale = 7.0f;

    // Start is called before the first frame update
    void Start()
    {
        x = slidingPlaneVisualizer.transform.position.x;
        y = slidingPlaneVisualizer.transform.position.y;
        z = slidingPlaneVisualizer.transform.position.z;

        ax = p1.x - p2.x;
        ay = p1.y - p2.y;
        az = p1.z - p2.z;

        norm = Mathf.Sqrt(ax * ax + ay * ay + az * az);
    }

    // Update is called once per frame
    void Update()
    {
        knobCentre.transform.position = refernce.transform.position;

        onSliderValue = sliderKnobe.transform.localPosition.y;
        currentValue = sliderKnobe.transform.position;

        switch (index)
        {
            case 1:
                x = currentValue.x;
                movementSign = 1;
                break;
            case 2:
                y = currentValue.y;
                movementSign = -1;
                break;
            case 3:
                z = currentValue.z;
                movementSign = -1;
                break;
            default:
                break;
        }

        slidingPlaneVisualizer.transform.position = new Vector3(x, y, z);

        if (isOn)
        {
            if (!prev_isOn)
            {
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
                sliderControlIsOn = false;
            }

            if (stateCheck())
            {
                sliderControlIsOn = true;
            }

            if (sliderControlIsOn)
            {
                moveRobotDynamic(150, 250);

                updateVirtualSlider();
            }
        }
        else
        {
            sliderControlIsOn = false;
        }

        prev_isOn = isOn;

        currentValue = sliderKnobe.transform.localPosition;
        pre_onSliderValue = onSliderValue;
        pre_sliderValue = shortInOut.value;

        sliderCenter.transform.localPosition = new Vector3(sliderKnobe.transform.localPosition.x, sliderKnobe.transform.localPosition.y + movementSign * (shortInOut.value - 208f) / 415 * (1.89245f - 0.86574f), sliderKnobe.transform.localPosition.z);
    }

    private bool stateCheck()
    {
        if (index == 2)
        {
            return ((DateTime.Now > SectionVewControlManager.verticalAxisAddOnT2.AddSeconds(0.2)) & unity_client.robotStopped & SectionVewControlManager.verticalAxisAddOnDone);
        }
        else
        {
            return ((DateTime.Now > t1.AddSeconds(0.2)) & unity_client.robotStopped);
        }
        
    }

    private void moveRobotDynamic(int bufferOne, int bufferTwo)
    {
        if (leftChecker.bounds.Contains(shortSliderTracker.transform.position))
        {
            if (!dynamicLeftEnd)
            {
                unity_client.stopRobot();
                shortInOut.SetSlider(0);
            }
            dynamicLeftEnd = true;
            dynamicMoving = false;
        }
        else
        {
            dynamicLeftEnd = false;
        }

        if (rightChecker.bounds.Contains(shortSliderTracker.transform.position))
        {
            if (!dynamicRightEnd)
            {
                unity_client.stopRobot();
                shortInOut.SetSlider(0);
            }
            dynamicRightEnd = true;
            dynamicMoving = false;
        }
        else
        {
            dynamicRightEnd = false;
        }

        if (dynamicCounter > 9)
        {
            if (shortInOut.value < bufferOne | shortInOut.value > bufferTwo)
            {
                dynamicMoving = true;
                float sp = (shortInOut.value - 415.0f / 2.0f) / (415.0f / 2.0f) * 0.0261f * speedScale * ((shortInOut.value - 415.0f / 2.0f) / (415.0f / 2.0f) * (shortInOut.value - 415.0f / 2.0f) / (415.0f / 2.0f) + 1);

                if (index == 3 | index == 1)
                {
                    sp = sp * -1;
                }

                if ((shortInOut.value < bufferOne & !dynamicRightEnd) | (shortInOut.value > bufferTwo & !dynamicLeftEnd))
                {
                    unity_client.customMove(ax / (Mathf.Round(norm / sp * 100) / 100), ay / (Mathf.Round(norm / sp * 100) / 100), az / (Mathf.Round(norm / sp * 100) / 100), -0.6, 1.47, 0.62, speed: sp, acc: 1.5f, movementType: 4);

                    if (shortInOut.value < bufferOne & !dynamicRightEnd)
                    {
                        shortInOut.SetSlider((int)(Mathf.Abs(shortInOut.value - 415.0f / 2.0f) / (415.0f / 2.0f) * (120) + 190));
                    }

                    if (shortInOut.value > bufferTwo & !dynamicLeftEnd)
                    {
                        shortInOut.SetSlider((int)(Mathf.Abs(shortInOut.value - 415.0f / 2.0f) / (415.0f / 2.0f) * (-120) - 190));
                    }
                }

                dynamicCounter = 0;
            }
            else
            {
                if (dynamicMoving)
                {
                    unity_client.stopRobot();
                    dynamicMoving = false;
                    shortInOut.SetSlider(0);
                }
            }
        }

        dynamicCounter++;
    }

    private void updateVirtualSlider()
    {
        float offset;

        switch (index)
        {
            case 1: offset = 0.8f;
                break;
            case 2: offset = -1.4f;
                break;
            case 3: offset = -0.8f;
                break;
            default: offset = 0f;
                break;
        }

        sliderKnobe.transform.localPosition = new Vector3(sliderKnobe.transform.localPosition.x, knobCentre.transform.localPosition.y + offset - movementSign*(float)(shortInOut.value - 415/2) / 415 * (1.9f - 0.85f), sliderKnobe.transform.localPosition.z);
        //sliderKnobe.transform.localPosition = new Vector3(sliderKnobe.transform.localPosition.x, sliderKnobe.transform.localPosition.y - movementSign * (float)(shortInOut.value - pre_sliderValue) / 415 * (1.89245f - 0.86574f), sliderKnobe.transform.localPosition.z);
    }

    public void isOnCheck()
    {
        foreach (GameObject g in anti_isOn)
        {
            g.GetComponent<sliderValueControl>().isOn = false;
        }
    }
}
