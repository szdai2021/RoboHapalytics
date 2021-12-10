using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class UserStudyControl : MonoBehaviour
{
    // PUBLIC
    [Header("Assignment")]
    public GameObject sliderknob;
    public GameObject randomPoint;

    public GameObject sliderKnobReference;

    //public WirelessAxes wireless;
    public MadeAxisOscRecieve axisReciever;
    public UnityClient unity_client;

    public GameObject robot;
    public GameObject axis;
    public GameObject axisReference;

    public GameObject virtualFingerTouchPoint;
    public GameObject virtualEndEffector;

    public GameObject realSliderReference;

    [Header("Parameters")]
    public int sliderBufferOne;
    public int sliderBufferTwo;

    public float rangeOne;
    public float rangeTwo;
    public float rangeThree;

    public float randomPointOffset;

    public int fileIndex = 0;

    [HideInInspector] public float virtualKnobMax = 2.928f; //2.4f; //2.928
    [HideInInspector] public float virtualKnobMin = -2.918f; //-2.4f; //-2.918

    [HideInInspector] public float randomPointMax = 0.878f;
    [HideInInspector] public float randomPointMin = 0.302f;

    [Header("")]
    public int scenario = 0; // 0: default;  1: virtual slider with controller;  2: virtual slider with physical 1:1 slider;  3: the haptic slider system
    public bool startFlag = false;

    public bool testFlag = false;
    public int iterations = 10;

    // PRIVATE
    private bool pre_startFlag = false;
    private int prev_sliderOne;
    private bool sFlag = false;
    private int test_Num;
    private int rangeIndex = 3;

    // Saved data
    private List<DateTime> triggerTime = new List<DateTime>();
    private List<double> reactionTime = new List<double>();
    private List<double> distanceToTarget = new List<double>();
    private List<double> distanceToKnob = new List<double>();

    // Start is called before the first frame update
    void Start()
    {
        randomPoint.SetActive(false);

        test_Num = iterations;
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

            sliderKnobReference.transform.position = realSliderReference.transform.position;

            switch (scenario) // five different setups
            {
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    moveRobot(200, 55, 100, 150);
                    virtualKnobUpdateFromRobotAxis();
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

            if (testFlag)
            {
                if (Input.GetKeyDown("s")) // start a new set of experiement
                {
                    sFlag = true;

                    triggerTime = new List<DateTime>();

                    reactionTime = new List<double>();
                    distanceToTarget = new List<double>();
                    distanceToKnob = new List<double>();
                }

                if (sFlag & rangeIndex > 0)
                {
                    float rangeSelected;
                    switch (rangeIndex)
                    {
                        case 1:
                            rangeSelected = rangeOne;
                            break;
                        case 2:
                            rangeSelected = rangeTwo;
                            break;
                        case 3:
                            rangeSelected = rangeThree;
                            break;
                        default:
                            rangeSelected = rangeOne;
                            break;
                    }

                    if (Input.GetKeyDown("t")) // trigger random point
                    {
                        if (triggerTime.Count > 0)
                        {
                            distanceToTarget.Add(sliderknob.transform.localPosition.y - randomPoint.transform.localPosition.y);
                            reactionTime.Add((System.DateTime.Now - triggerTime[triggerTime.Count - 1]).TotalMilliseconds);
                            test_Num--;
                        }
                        generateRandomPoint(rangeSelected);
                        triggerTime.Add(System.DateTime.Now);
                    }

                    if (test_Num == 0)
                    {
                        rangeIndex--;
                    }

                    if (rangeIndex == 0)
                    {
                        sFlag = false;
                        test_Num = iterations;
                        rangeIndex = 3;
                        randomPoint.SetActive(false);
                        saveToLocal();
                    }
                }
            }
        }

        pre_startFlag = startFlag;
        prev_sliderOne = axisReciever.sliderOne;
    }

    private void saveToLocal()
    {
        string saveFileName = "data/test_result" + fileIndex.ToString() +".txt";

        while (File.Exists(saveFileName))
        {
            fileIndex++;
            saveFileName = "data/test_result" + fileIndex.ToString() + ".txt";
        }

        StreamWriter sw = new StreamWriter(saveFileName);

        sw.WriteLine("Distance to Knob");
        foreach (var value in distanceToKnob)
        {
            sw.WriteLine(value.ToString());
        }
        sw.WriteLine(" ");

        sw.WriteLine("Time Taken");
        foreach (var value in reactionTime)
        {
            sw.WriteLine(value.ToString());
        }
        sw.WriteLine(" ");

        sw.WriteLine("Accuracy");
        foreach (var value in distanceToTarget)
        {
            sw.WriteLine(value.ToString());
        }

        sw.Close();
    }

    private void generateRandomPoint(float rangeDifference)
    {
        randomPoint.SetActive(true);

        float randomRangMax = randomPoint.transform.localPosition.y + rangeDifference;
        float randomRangMin = randomPoint.transform.localPosition.y - rangeDifference;

        float randomValue = UnityEngine.Random.value;

        if (randomRangMax > virtualKnobMax)
        {
            randomValue = 0;
        }

        if (randomRangMin < virtualKnobMin)
        {
            randomValue = 1;
        }

        float randomY = 0;
        if (randomValue > 0.5)
        {
            randomY = UnityEngine.Random.Range(randomRangMax-randomPointOffset, randomRangMax+ randomPointOffset);
        }
        else
        {
            randomY = UnityEngine.Random.Range(randomRangMin - randomPointOffset, randomRangMin + randomPointOffset);
        }

        if (randomY > virtualKnobMax)
        {
            randomY = virtualKnobMax;
        }

        if (randomY < virtualKnobMin)
        {
            randomY = virtualKnobMin;
        }

        randomPoint.transform.localPosition = new Vector3(randomPoint.transform.localPosition.x, randomY, randomPoint.transform.localPosition.z);

        distanceToKnob.Add(sliderknob.transform.localPosition.y - randomPoint.transform.localPosition.y);

    }

    private void virtualKnobUpdateFromRobotAxis()
    {
        sliderknob.transform.localPosition = new Vector3(sliderknob.transform.localPosition.x, sliderKnobReference.transform.localPosition.y + 1 * (float)(axisReciever.sliderOne - 127) / 255 * (1.89245f - 0.86574f), sliderknob.transform.localPosition.z);
    }

    private void moveRobot(int bufferOne, int bufferTwo, int bufferThree, int bufferFour)
    {
        if (axisReciever.sliderOne > bufferOne & prev_sliderOne < bufferOne)
        {
            unity_client.customMove(0.0485766, 0.4551, 0.08486, -0.6, 1.5, 0.62, movementType: 1);
        }
        else if (axisReciever.sliderOne < bufferTwo & prev_sliderOne > bufferTwo)
        {
            unity_client.customMove(0.4575, 0.0462, 0.088944, -0.6, 1.5, 0.62, movementType: 1);
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

    public void HideRobot(bool hideFlag = false)
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

        Renderer[] Axis_rsf = axisReference.GetComponentsInChildren<Renderer>();

        foreach (Renderer arf in Axis_rsf)
        {
            arf.enabled = axisRender;
        }
        
    }
}
