using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserStudyControl : MonoBehaviour
{
    // PUBLIC
    [Header("Assignment")]
    public GameObject sliderknob;
    public GameObject randomPoint;

    //public WirelessAxes wireless;
    public MadeAxisOscRecieve axisReciever;
    public UnityClient unity_client;

    public GameObject robot;
    public GameObject axis;

    public GameObject virtualFingerTouchPoint;
    public GameObject virtualEndEffector;

    [Header("Parameters")]
    [HideInInspector] public float virtualKnobMax = 2.4f;
    [HideInInspector] public float virtualKnobMin = -2.4f;

    [HideInInspector] public float randomPointMax = 0.878f;
    [HideInInspector] public float randomPointMin = 0.302f;

    public int sliderBufferOne;
    public int sliderBufferTwo;

    public float rangeOne;
    public float rangeTwo;
    public float rangeThree;

    [Header("")]
    public int scenario = 0; // 0: default;  1: virtual slider with controller;  2: virtual slider with physical 1:1 slider;  3: the haptic slider system
    public bool startFlag = false;

    // PRIVATE
    private bool pre_startFlag = false;

    private int prev_sliderOne;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (startFlag)
        {
            if (!pre_startFlag)
            {
                HideShowTagGameObject("Slider", false);
                HideShowTagGameObject("Test", false);

                HideRobot();
            }

            if (sliderknob.transform.localPosition.y > virtualKnobMax)
            {
                sliderknob.transform.localPosition = new Vector3(0,virtualKnobMax,0);
            }

            if (sliderknob.transform.localPosition.y < virtualKnobMin)
            {
                sliderknob.transform.localPosition = new Vector3(0, virtualKnobMin, 0);
            }

            if (randomPoint.transform.localPosition.x > randomPointMax)
            {
                randomPoint.transform.localPosition = new Vector3(randomPointMax, randomPoint.transform.localPosition.y, randomPoint.transform.localPosition.z);
            }

            if (randomPoint.transform.localPosition.x < randomPointMin)
            {
                randomPoint.transform.localPosition = new Vector3(randomPointMin, randomPoint.transform.localPosition.y, randomPoint.transform.localPosition.z);
            }


            switch (scenario) // five different setups
            {
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    moveRobot(200, 55, 100, 150);
                    break;
                case 4:
                    break;
                case 5:
                    break;
                default:
                    break;
            }

            if (Input.GetKeyDown("space"))
            {
                virtualFingerTouchPoint.transform.eulerAngles = new Vector3(0, 0, 90f);

                virtualFingerTouchPoint.transform.position = new Vector3(sliderknob.transform.position.x, sliderknob.transform.position.y, sliderknob.transform.position.z);

                Vector3 RobotCoord = convertUnityCoord2RobotCoord(virtualEndEffector.transform.position);

                Vector3 RobotRot = new Vector3(-0.6f, 1.47f, 0.62f);

                unity_client.customMove(RobotCoord.x, RobotCoord.y, RobotCoord.z, RobotRot.x, RobotRot.y, RobotRot.z, movementType: 1);
            }
        }

        pre_startFlag = startFlag;
        prev_sliderOne = axisReciever.sliderOne;
    }

    private void virtualKnobUpdateFromRobotAxis()
    {
        sliderknob.transform.localPosition = new Vector3(sliderknob.transform.localPosition.x, 1 * (float)(axisReciever.sliderOne - 127) / 255 * (1.89245f - 0.86574f), sliderknob.transform.localPosition.z);
    }

    private void moveRobot(int bufferOne, int bufferTwo, int bufferThree, int bufferFour)
    {
        if (axisReciever.sliderOne > bufferOne & prev_sliderOne < bufferOne)
        {
            unity_client.customMove(0.05, 0.4, 0.1, -0.6, 1.5, 0.62, movementType: 1);
        }
        else if (axisReciever.sliderOne < bufferTwo & prev_sliderOne > bufferTwo)
        {
            unity_client.customMove(0.35, 0.1, 0.1, -0.6, 1.5, 0.62, movementType: 1);
        }
        else if ((axisReciever.sliderOne > bufferThree & axisReciever.sliderOne < bufferFour) & (prev_sliderOne < bufferThree | prev_sliderOne > bufferFour))
        {
            unity_client.stopRobot();
        }
    }

    private Vector3 convertUnityCoord2RobotCoord(Vector3 p1)
    {
        /*
        float new_x = 0.702f * p1.x + 0.00522f * p1.y + 0.707f * p1.z + 0.476f;
        float new_y = -0.7023f * p1.x + -0.005f * p1.y + 0.6843f * p1.z - 0.4695f;
        float new_z = 0.09f * p1.x + 0.803f * p1.y + 0.4482f * p1.z - 0.047f;
        */

        float new_x = 0.7098f * p1.x + -0.00617f * p1.y + 0.707f * p1.z + 0.345378f;
        float new_y = -0.7098f * p1.x + 0.00617f * p1.y + 0.7014f * p1.z - 0.338f;
        float new_z = 0.0071f * p1.x + 1f * p1.y + 0.000028f * p1.z + 0.0064f;

        return new Vector3(new_x, new_y, new_z);
    }

    private void HideShowTagGameObject(string tag, bool show)
    {
        GameObject[] gameObjectArray = GameObject.FindGameObjectsWithTag(tag);

        foreach (GameObject go in gameObjectArray)
        {
            go.SetActive(show);
        }
    }

    public void HideRobot(bool hideFlag = true)
    {
        bool robotRender = hideFlag;
        bool axisRender = hideFlag;

        Renderer[] Robot_rs = robot.GetComponentsInChildren<Renderer>();

        foreach (Renderer rr in Robot_rs)
        {
            rr.enabled = robotRender;
        }

        Renderer[] Axis_rs = axis.GetComponentsInChildren<Renderer>();

        foreach (Renderer ar in Axis_rs)
        {
            ar.enabled = axisRender;
        }
    }
}
