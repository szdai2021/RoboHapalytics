using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEditor.Recorder;
using UnityEditor;

public class UserStudyControl : MonoBehaviour
{
    // PUBLIC
    [Header("Assignment")]
    public GameObject sliderknob;
    public GameObject randomPoint;

    public GameObject sliderKnobReference;
    //public GameObject transSphere;

    //public WirelessAxes wireless;
    public SerialInOut ShortSliderInOut;
    public SerialInOut LongSliderInOut;
    public UnityClient unity_client;
    //public colliderCheck colliderCheck;

    public GameObject robot;
    public GameObject axis;
    public GameObject axisReference;

    public GameObject virtualFingerTouchPoint;
    public GameObject virtualEndEffector;
    public GameObject realSliderReference;
    public GameObject vrHandInteraction;

    public GameObject instructionPanel;
    public GameObject confirmationPanel;
    public GameObject finishPanel;

    public GameObject sliderModelReference1;
    public GameObject sliderModelReference2;

    [Header("Parameters")]
    public float longSliderMaxValue = 1764;
    public float shortSliderMaxValue = 415;
    public int sliderBufferOne;
    public int sliderBufferTwo;

    public float rangeOne;
    public float rangeTwo;
    public float rangeThree;

    public float randomPointOffset;

    //[HideInInspector] public float virtualKnobMax = 2.928f; //2.4f; //2.928
    //[HideInInspector] public float virtualKnobMin = -2.918f; //-2.4f; //-2.918

    //[HideInInspector] public float randomPointMax = 0.878f;
    //[HideInInspector] public float randomPointMin = 0.302f;

    private float virtualKnobMax = 2.928f; //2.4f; //2.928
    private float virtualKnobMin = -2.918f; //-2.4f; //-2.918

    private float randomPointMax = 2.928f;
    private float randomPointMin = -2.918f;

    [Header("")]
    public int scenario = 0; // 0: default;  1: virtual slider with controller;  2: virtual slider with physical 1:1 slider;  3: the haptic slider system
    public int participantID = 0; // =0 : testing // <0 : pilot testing // >0 : formal testing
    public bool startFlag = false;
    public bool experimentFlag = false;
    public bool startRecording = false;
    public int iterations = 10;

    // PRIVATE
    private int duplicateFileIndex = 0;
    private int prev_scenario = 0;
    private bool pre_startFlag = false;
    private int prev_sliderOne;
    private bool sFlag = false;
    private int test_Num;
    private int rangeIndex = 3;

    private int experimentStage = 1;

    private bool triggerFlag = false;
    private bool prev_triggerFlag = false;
    private bool trialFlag = false;

    private bool onControllerRaySelect = false;

    private Vector3 virtualSliderPos1;
    private Quaternion virtualSliderRot1;
    private Vector3 virtualSliderPos2;
    private Quaternion virtualSliderRot2;

    private RecorderWindow recorderWindow;

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

        virtualSliderPos1 = sliderModelReference1.transform.position;
        virtualSliderRot1 = sliderModelReference1.transform.rotation;

        virtualSliderPos2 = sliderModelReference2.transform.position;
        virtualSliderRot2 = sliderModelReference2.transform.rotation;

        instructionPanel.SetActive(false);
        confirmationPanel.SetActive(false);
        finishPanel.SetActive(false);

        recorderWindow = GetRecorderWindow();
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
                sliderknob.transform.localPosition = new Vector3(0, virtualKnobMax, 0);
            }

            if (sliderknob.transform.localPosition.y < virtualKnobMin)
            {
                sliderknob.transform.localPosition = new Vector3(0, virtualKnobMin, 0);
            }

            //sliderknob.transform.localPosition = new Vector3(0, sliderknob.transform.localPosition.y, 0);

            if (randomPoint.transform.localPosition.y > randomPointMax)
            {

                //randomPoint.transform.localPosition = new Vector3(randomPointMax, randomPoint.transform.localPosition.y, randomPoint.transform.localPosition.z);
                randomPoint.transform.localPosition = new Vector3(0.7416667f, randomPointMax, 0.0333334f);
            }

            if (randomPoint.transform.localPosition.y < randomPointMin)
            {
                //randomPoint.transform.localPosition = new Vector3(randomPointMin, randomPoint.transform.localPosition.y, randomPoint.transform.localPosition.z);
                randomPoint.transform.localPosition = new Vector3(0.7416667f, randomPointMin, 0.0333334f);
            }

            sliderKnobReference.transform.position = realSliderReference.transform.position;

            switch (scenario) // five different setups
            {
                case 1: // interacting with virtual slider with virtual hand only without hatpic feedback
                    //virtualKnobUpdateFromControllerPointing();
                    vrHandInteraction.SetActive(true);
                    virtualKnobUpdateFromVrHandControl();
                    break;
                case 2: // interacting with virtual slider which is aliged with a full size physical slider with haptic feedback
                    vrHandInteraction.SetActive(false);
                    //virtualKnobUpdateFromControllerGrabbing();
                    virtualKnobUpdateFromVrPhysicalSlider();
                    break;
                case 3: // interacting with virutal slider where a short physical slider is mounted on a robotic arm to cover the whole range
                    vrHandInteraction.SetActive(false);
                    //moveRobot(200, 55, 100, 150);
                    moveRobot(325, 89, 162, 244);
                    virtualKnobUpdateFromRobotAxis();
                    break;
                case 4:
                    vrHandInteraction.SetActive(false);
                    break;
                case 5:
                    vrHandInteraction.SetActive(false);
                    break;
                default:
                    break;
            }

            if (prev_scenario != scenario)
            {
                experimentStage = 1;
            }

            //if (Input.GetKeyDown("space")) // move robotic arm to position
            //{
            //    virtualFingerTouchPoint.transform.eulerAngles = new Vector3(0, 0, 90f);

            //    virtualFingerTouchPoint.transform.position = new Vector3(sliderknob.transform.position.x, sliderknob.transform.position.y, sliderknob.transform.position.z);

            //    Vector3 RobotCoord = convertUnityCoord2RobotCoord(virtualEndEffector.transform.position);

            //    Vector3 RobotRot = new Vector3(-0.6f, 1.47f, 0.62f);

            //    unity_client.customMove(RobotCoord.x, RobotCoord.y, RobotCoord.z, RobotRot.x, RobotRot.y, RobotRot.z, movementType: 1);
            //}

            triggerFlag = checkControllerTrigger();

            if (experimentFlag)
            {
                switch (experimentStage)
                {
                    case 1: // trial stage
                        if (!recorderWindow.IsRecording() && startRecording)
                        {
                            //print("not recording");
                            recorderWindow.StartRecording();
                        }

                        instructionPanel.SetActive(true);
                        trialFlag = true;

                        if (triggerFlag && sliderknob.transform.localPosition.y < -2.4f)
                        {
                            experimentStage += 1;
                        }

                        if (triggerFlag && !prev_triggerFlag)
                        {
                            generateRandomPoint(rangeOne);
                        }

                        break;
                    case 2: // comfirmation stage
                        instructionPanel.SetActive(false);
                        confirmationPanel.SetActive(true);
                        trialFlag = false;

                        if (triggerFlag && sliderknob.transform.localPosition.y < 0.17f && sliderknob.transform.localPosition.y > -0.37f)
                        {
                            experimentStage += 1;
                        }

                        break;
                    case 3: // recording stage
                        confirmationPanel.SetActive(false);

                        if (!sFlag)
                        {
                            sFlag = true;

                            triggerTime = new List<DateTime>();
                            reactionTime = new List<double>();
                            distanceToTarget = new List<double>();
                            distanceToKnob = new List<double>();

                            finishPanel.SetActive(false);
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

                            if (triggerFlag && !prev_triggerFlag) // trigger random point
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
                                test_Num = iterations;
                            }

                            if (rangeIndex == 0)
                            {
                                sFlag = false;
                                test_Num = iterations;
                                rangeIndex = 3;
                                randomPoint.SetActive(false);
                                saveToLocal();

                                finishPanel.SetActive(true);

                                experimentStage++;

                                if (recorderWindow.IsRecording() && startRecording)
                                {
                                    //print("recording");
                                    recorderWindow.StopRecording();
                                }
                            }

                            //print(test_Num.ToString() + " - " + rangeIndex.ToString() + " - " + rangeSelected.ToString());
                        }
                        break;
                    default:
                        instructionPanel.SetActive(false);
                        confirmationPanel.SetActive(false);
                        break;
                }
            }

            prev_triggerFlag = triggerFlag;
            prev_scenario = scenario;
        }

        pre_startFlag = startFlag;
        prev_sliderOne = ShortSliderInOut.value;

        //if (Input.GetKeyDown("space"))
        //{
        //    RecorderWindow recorderWindow = GetRecorderWindow();

        //    if (!recorderWindow.IsRecording())
        //    {
        //        print("not recording");
        //        recorderWindow.StartRecording();
        //    }

        //    if (recorderWindow.IsRecording())
        //    {
        //        print("recording");
        //        recorderWindow.StopRecording();
        //    }
        //}
    }

    private RecorderWindow GetRecorderWindow()
    {
        return (RecorderWindow)EditorWindow.GetWindow(typeof(RecorderWindow));
    }

    private bool checkControllerTrigger()
    {
        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(inputDevices);
        UnityEngine.XR.InputDevice device = inputDevices[0];
        for (int i = 0; i < inputDevices.Count; i++)
        {
            if (inputDevices[i].name == "Spatial Controller - Left")
            {
                device = inputDevices[i];
            }
        }

        bool triggerValue;
        return device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerValue) && triggerValue;
    }

    private void saveToLocal()
    {
        string fileName = "HapticSlider_P" + participantID.ToString() + "_S" + scenario.ToString();
        string saveFileName = "data/" + fileName +".txt";

        while (File.Exists(saveFileName))
        {
            duplicateFileIndex++;
            saveFileName = "data/" + fileName + "_D" + duplicateFileIndex.ToString() + ".txt";
        }

        StreamWriter sw = new StreamWriter(saveFileName);

        sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd\\THH:mm:ss\\Z"));
        sw.WriteLine(" ");

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

        if (trialFlag && randomRangMin < -2.35f)
        {
            randomRangMin = -2.2f;
        }

        float randomValue = UnityEngine.Random.value;

        if (randomRangMax > virtualKnobMax-0.1f)
        {
            randomValue = 0;
        }

        if (randomRangMin < virtualKnobMin+0.1f)
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

        //randomPoint.transform.localPosition = new Vector3(randomPoint.transform.localPosition.x, randomY, randomPoint.transform.localPosition.z);
        randomPoint.transform.localPosition = new Vector3(0.7416667f, randomY, 0.0333334f);

        distanceToKnob.Add(sliderknob.transform.localPosition.y - randomPoint.transform.localPosition.y);
    }

    //private void virtualKnobUpdateFromControllerGrabbing()
    //{
    //    var inputDevices = new List<UnityEngine.XR.InputDevice>();
    //    UnityEngine.XR.InputDevices.GetDevices(inputDevices);
    //    UnityEngine.XR.InputDevice device = inputDevices[0];
    //    for (int i = 0; i < inputDevices.Count; i++)
    //    {
    //        if (inputDevices[i].name == "Spatial Controller - Left")
    //        {
    //            device = inputDevices[i];
    //        }
    //    }

    //    bool triggerValue;
    //    triggerFlag = device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerValue) && triggerValue;
    //    if (triggerFlag && !prev_triggerFlag && colliderCheck.collisionCheck)
    //    {
    //        transSphere.GetComponent<MeshRenderer>().enabled = false;

    //        sliderknob.transform.position = transSphere.transform.position;
    //    }
    //    else
    //    {
    //        transSphere.GetComponent<MeshRenderer>().enabled = true;
    //    }
    //}

    private void virtualKnobUpdateFromControllerPointing()
    {
        if (onControllerRaySelect)
        {

        }
    }

    private void virtualKnobUpdateFromVrHandControl()
    {
        //unity_client.customMove(0f, 0.25f, 0.1f, -0.6f, 1.47f, 0.62f, movementType: 1);
    }

    private void virtualKnobUpdateFromVrPhysicalSlider()
    {
        // update virtual model position
        sliderModelReference1.transform.position = virtualSliderPos2;
        sliderModelReference2.transform.rotation = virtualSliderRot2;

        instructionPanel.transform.rotation = virtualSliderRot2 * Quaternion.Inverse(virtualSliderRot1) * virtualSliderRot1;
        confirmationPanel.transform.rotation = virtualSliderRot2 * Quaternion.Inverse(virtualSliderRot1) * virtualSliderRot1;
        finishPanel.transform.rotation = virtualSliderRot2 * Quaternion.Inverse(virtualSliderRot1) * virtualSliderRot1;

        if (prev_scenario != scenario)
        {
            instructionPanel.transform.position += virtualSliderPos2 - virtualSliderPos1;
            confirmationPanel.transform.position += virtualSliderPos2 - virtualSliderPos1;
            finishPanel.transform.position += virtualSliderPos2 - virtualSliderPos1;
        }
    }

    public void controllerRaySelect(bool select)
    {
        onControllerRaySelect = select;
    }

    private void virtualKnobUpdateFromRobotAxis()
    {
        // update virtual model position
        sliderModelReference1.transform.position = virtualSliderPos1;
        sliderModelReference2.transform.rotation = virtualSliderRot1;

        instructionPanel.transform.rotation = virtualSliderRot1 * Quaternion.Inverse(virtualSliderRot2) * virtualSliderRot2;
        confirmationPanel.transform.rotation = virtualSliderRot1 * Quaternion.Inverse(virtualSliderRot2) * virtualSliderRot2;
        finishPanel.transform.rotation = virtualSliderRot1 * Quaternion.Inverse(virtualSliderRot2) * virtualSliderRot2;

        if (prev_scenario != scenario)
        {
            instructionPanel.transform.position += virtualSliderPos1 - virtualSliderPos2;
            confirmationPanel.transform.position += virtualSliderPos1 - virtualSliderPos2;
            finishPanel.transform.position += virtualSliderPos1 - virtualSliderPos2;
        }
        
        // update slider
        sliderknob.transform.localPosition = new Vector3(sliderknob.transform.localPosition.x, sliderKnobReference.transform.localPosition.y + 1 * (float)(ShortSliderInOut.value - shortSliderMaxValue/2) / shortSliderMaxValue * (1.89245f - 0.86574f), sliderknob.transform.localPosition.z);
    }

    private void moveRobot(int bufferOne, int bufferTwo, int bufferThree, int bufferFour)
    {
        if (ShortSliderInOut.value > bufferOne & prev_sliderOne < bufferOne)
        {
            unity_client.customMove(0.0485766, 0.4551, 0.08486, -0.6, 1.5, 0.62, movementType: 1);
            //unity_client.customMove(0.4575, 0.0462, 0.088944, -0.6, 1.5, 0.62, movementType: 1);
        }
        else if (ShortSliderInOut.value < bufferTwo & prev_sliderOne > bufferTwo)
        {
            unity_client.customMove(0.4575, 0.0462, 0.088944, -0.6, 1.5, 0.62, movementType: 1);
            //unity_client.customMove(0.0485766, 0.4551, 0.08486, -0.6, 1.5, 0.62, movementType: 1);
        }
        else if ((ShortSliderInOut.value > bufferThree & ShortSliderInOut.value < bufferFour) & (prev_sliderOne < bufferThree | prev_sliderOne > bufferFour))
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
