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

    public GameObject sliderknob1;
    public GameObject randomPoint1;

    public GameObject sliderKnobReference;
    public GameObject panelPosReference;
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

    public GameObject sliderRight;
    public GameObject sliderLeft;

    public GameObject debug1;
    public GameObject debug2;

    public GameObject buttonRed;
    public GameObject redButton;

    public GameObject shortSliderTracker;

    [Header("Parameters")]
    public float longSliderMaxValue = 1764;
    public float shortSliderMaxValue = 415;
    public int sliderBufferOne;
    public int sliderBufferTwo;

    public float rangeOne;
    public float rangeTwo;
    public float rangeThree;

    public float randomPointOffset;

    public int sp1 = -250;
    public int sp2 = 250;

    //[HideInInspector] public float virtualKnobMax = 2.928f; //2.4f; //2.928
    //[HideInInspector] public float virtualKnobMin = -2.918f; //-2.4f; //-2.918

    //[HideInInspector] public float randomPointMax = 0.878f;
    //[HideInInspector] public float randomPointMin = 0.302f;

    private float virtualKnobMax = 2.928f; //2.4f; //2.928
    private float virtualKnobMin = -2.918f; //-2.4f; //-2.918

    private float randomPointMax = 2.928f;
    private float randomPointMin = -2.918f;

    private DateTime time_temp;
    private bool lastPress = false;

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
    private bool robotHomePos = true;

    private Vector3 panelReference1;
    private Quaternion panelReference2;

    private RecorderWindow recorderWindow;
    List<float> distanceOrder = new List<float>();

    private Vector3 buttonPos1 = new Vector3(1.7336f, -0.1341f, -0.0806f);
    private Vector3 buttonPos2 = new Vector3(0.5903f, -0.131f, -0.0709f);

    private Vector3 sliderLeftReference = new Vector3(-0.1942463f, 0.1035762f, 0.3072551f);
    private Vector3 sliderRightReference = new Vector3(-0.7358298f, 0.09867605f, 0.3014497f);

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

        instructionPanel.SetActive(false);
        confirmationPanel.SetActive(false);
        finishPanel.SetActive(false);

        recorderWindow = GetRecorderWindow();

        panelReference1 = instructionPanel.transform.position;
        panelReference2 = instructionPanel.transform.rotation;
        
        readOrder();
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

                HideObject(robot, false);
                HideObject(axis, false);
                HideObject(axisReference, false);
                HideObject(virtualFingerTouchPoint, false);

                ShortSliderInOut.SetSlider(0);
                LongSliderInOut.SetSlider(0);
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
                    if (!robotHomePos)
                    {
                        unity_client.customMove(-1.8765, -1.22337, 2.4, -1.19516, 2.06182, -7.85783, movementType: 3);
                        robotHomePos = true;
                    }
                    vrHandInteraction.SetActive(true);
                    redButton.transform.localPosition = buttonPos2;
                    virtualKnobUpdateFromVrHandControl();
                    break;
                case 2: // interacting with virtual slider which is aliged with a full size physical slider with haptic feedback
                    vrHandInteraction.SetActive(false);
                    redButton.transform.localPosition = buttonPos1;
                    virtualKnobUpdateFromVrPhysicalSlider();
                    break;
                case 3: // interacting with virutal slider where a short physical slider is mounted on a robotic arm to cover the whole range
                    if (robotHomePos)
                    {
                        unity_client.customMove(0.23, 0.17593, 0.045218, -0.6, 1.5, 0.62, movementType: 0);
                        robotHomePos = false;
                    }
                    vrHandInteraction.SetActive(false);
                    redButton.transform.localPosition = buttonPos2;
                    moveRobot(325, 89, 162, 244);
                    virtualKnobUpdateFromRobotAxis();
                    break;
                case 4:
                    if (robotHomePos)
                    {
                        unity_client.customMove(0.23, 0.17593, 0.045218, -0.6, 1.5, 0.62, movementType: 0);
                        robotHomePos = false;
                    }
                    vrHandInteraction.SetActive(false);
                    redButton.transform.localPosition = buttonPos2;
                    moveRobot(413, 2, 100, 300);
                    virtualKnobUpdateFromRobotAxis();
                    break;
                case 5:
                    if (robotHomePos)
                    {
                        unity_client.customMove(0.23, 0.17593, 0.045218, -0.6, 1.5, 0.62, movementType: 0);
                        robotHomePos = false;
                    }
                    vrHandInteraction.SetActive(false);
                    redButton.transform.localPosition = buttonPos2;
                    moveRobotDynamic(325, 89, 162, 244,355,385,59,29);
                    virtualKnobUpdateFromRobotAxis();
                    break;
                default:
                    break;
            }

            if (prev_scenario != scenario & scenario != 0)
            {
                experimentStage = 1;
                randomPoint.SetActive(true);
            }

            //if (Input.GetKeyDown("space")) // move robotic arm to position
            //{
            //    //virtualFingerTouchPoint.transform.eulerAngles = new Vector3(0, 0, 90f);

            //    virtualFingerTouchPoint.transform.position = new Vector3(sliderknob.transform.position.x, sliderknob.transform.position.y, sliderknob.transform.position.z);

            //    Vector3 RobotCoord = convertUnityCoord2RobotCoord(virtualEndEffector.transform.position);

            //    Vector3 RobotRot = new Vector3(-0.6f, 1.47f, 0.62f);

            //    unity_client.customMove(RobotCoord.x, RobotCoord.y, RobotCoord.z, RobotRot.x, RobotRot.y, RobotRot.z, movementType: 0);
            //}

            triggerFlag = checkControllerTrigger();

            /*
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
                            if (scenario != 2)
                            {
                                generateRandomPoint(sliderknob, randomPoint, rangeOne);
                            }
                            else
                            {
                                generateRandomPoint(sliderknob1, randomPoint1, rangeOne);
                            }
                        }

                        break;
                    case 2: // comfirmation stage
                        instructionPanel.SetActive(false);
                        confirmationPanel.SetActive(true);
                        trialFlag = false;

                        if (scenario != 2)
                        {
                            randomPoint.transform.localPosition = new Vector3(0.7416667f, 0, 0.0333334f);
                        }
                        else
                        {
                            randomPoint1.transform.localPosition = new Vector3(0.7416667f, 0, 0.0333334f);
                        }

                        if (triggerFlag && sliderknob.transform.localPosition.y < 0.17f && sliderknob.transform.localPosition.y > -0.37f)
                        {
                            experimentStage += 1;
                            time_temp = System.DateTime.Now;
                        }

                        break;
                    case 3: // recording stage
                        confirmationPanel.SetActive(false);

                        if (scenario == 2)
                        {
                            recordingStage(sliderknob1, randomPoint1);
                        }
                        else
                        {
                            recordingStage(sliderknob, randomPoint);
                        }

                        break;
                    default:
                        instructionPanel.SetActive(false);
                        confirmationPanel.SetActive(false);

                        if (Input.GetKeyDown("r"))
                        {
                            finishPanel.SetActive(false);
                            if (scenario != 1 & scenario != 2)
                            {
                                unity_client.customMove(0.23, 0.17593, 0.045218, -0.6, 1.5, 0.62, movementType: 1);
                            }
                            scenario = 0;
                        }

                        break;
                }
            }
            */
            if (scenario == 2)
            {
                experiment(sliderknob1);
            }
            else
            {
                experiment(sliderknob);
            }

            prev_triggerFlag = triggerFlag;
            prev_scenario = scenario;
        }

        pre_startFlag = startFlag;
        prev_sliderOne = ShortSliderInOut.value;

        if (triggerFlag)
        {
            //buttonRed.transform.localPosition = new Vector3(0, -0.3f, 0);
            buttonRed.transform.localPosition = Vector3.MoveTowards(buttonRed.transform.localPosition, new Vector3(0, -0.3f, 0), 1f);
        }
        else
        {
            //buttonRed.transform.localPosition = new Vector3(0, -0.2f, 0);
            buttonRed.transform.localPosition = Vector3.MoveTowards(buttonRed.transform.localPosition, new Vector3(0, -0.2f, 0), 1f);
        }

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

        if (Input.GetKeyDown("space"))
        {
            print(distanceOrder.Count);
        }
    }

    private void experiment(GameObject konb)
    {
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

                    if (triggerFlag && konb.transform.localPosition.y < -2.4f)
                    {
                        experimentStage += 1;
                    }

                    if (triggerFlag && !prev_triggerFlag)
                    {
                        if (scenario != 2)
                        {
                            generateRandomPoint(konb, randomPoint, rangeOne);
                        }
                        else
                        {
                            generateRandomPoint(konb, randomPoint1, rangeOne);
                        }
                    }

                    break;
                case 2: // comfirmation stage
                    instructionPanel.SetActive(false);
                    confirmationPanel.SetActive(true);
                    trialFlag = false;

                    if (scenario != 2)
                    {
                        randomPoint.transform.localPosition = new Vector3(0.7416667f, 0, 0.0333334f);
                    }
                    else
                    {
                        randomPoint1.transform.localPosition = new Vector3(0.7416667f, 0, 0.0333334f);
                    }

                    if (triggerFlag && konb.transform.localPosition.y < 0.17f && konb.transform.localPosition.y > -0.37f)
                    {
                        experimentStage += 1;
                        time_temp = System.DateTime.Now;
                    }

                    break;
                case 3: // recording stage
                    confirmationPanel.SetActive(false);

                    if (scenario == 2)
                    {
                        recordingStage(konb, randomPoint1);
                    }
                    else
                    {
                        recordingStage(konb, randomPoint);
                    }

                    break;
                default:
                    instructionPanel.SetActive(false);
                    confirmationPanel.SetActive(false);

                    if (Input.GetKeyDown("r"))
                    {
                        finishPanel.SetActive(false);
                        if (scenario != 1 & scenario != 2)
                        {
                            unity_client.customMove(0.23, 0.17593, 0.045218, -0.6, 1.5, 0.62, movementType: 1);
                        }
                        scenario = 0;
                    }

                    break;
            }
        }

    }

    private void recordingStage(GameObject konb, GameObject Rpoint)
    {
        if (!sFlag)
        {
            sFlag = true;

            triggerTime = new List<DateTime>();
            triggerTime.Add(time_temp);
            reactionTime = new List<double>();
            distanceToTarget = new List<double>();
            distanceToKnob = new List<double>();

            finishPanel.SetActive(false);
            prev_triggerFlag = true;
        }

        if (sFlag)
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
                if (test_Num != iterations)
                {
                    distanceToTarget.Add(konb.transform.localPosition.y - Rpoint.transform.localPosition.y);
                    reactionTime.Add((System.DateTime.Now - triggerTime[triggerTime.Count - 1]).TotalMilliseconds);
                }

                test_Num--;

                if (rangeIndex != 0)
                {
                    int index = (scenario - 1) * 30 + (iterations - test_Num - 1 + (3 - rangeIndex) * iterations);
                    generateRandomPoint(konb, Rpoint, rangeSelected, orderIndex: index);
                    debug1.GetComponent<TextMesh>().text = index.ToString();
                    debug2.GetComponent<TextMesh>().text = index.ToString();
                    triggerTime.Add(System.DateTime.Now);
                }
                else
                {
                    lastPress = true;
                }

                print(lastPress.ToString() + " " + rangeIndex.ToString());
            }

            if (test_Num == 0)
            {
                rangeIndex--;
                test_Num = iterations;
            }

            if (rangeIndex <0)
            {
                rangeIndex = 0;
            }

            if (rangeIndex == 0 & lastPress)
            {
                sFlag = false;
                lastPress = false;
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

        }
    }

    private RecorderWindow GetRecorderWindow()
    {
        return (RecorderWindow)EditorWindow.GetWindow(typeof(RecorderWindow));
    }

    private bool checkControllerTrigger()
    {
        //var inputDevices = new List<UnityEngine.XR.InputDevice>();
        //UnityEngine.XR.InputDevices.GetDevices(inputDevices);
        //UnityEngine.XR.InputDevice device = inputDevices[0];
        //for (int i = 0; i < inputDevices.Count; i++)
        //{
        //    if (inputDevices[i].name == "Spatial Controller - Left")
        //    {
        //        device = inputDevices[i];
        //    }
        //}

        //bool triggerValue;
        //return device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerValue) && triggerValue;

        return ShortSliderInOut.Button == 1;
    }

    private void saveToLocal()
    {
        string fileName = "HapticSlider" + DateTime.Now.ToString("yyyy-MM-dd") + "_P" + participantID.ToString() + "_S" + scenario.ToString();
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

    private void generateRandomPoint(GameObject knob, GameObject Rpoint, float rangeDifference, int orderIndex = -1)
    {
        Rpoint.SetActive(true);

        float randomRangMax = Rpoint.transform.localPosition.y + rangeDifference;
        float randomRangMin = Rpoint.transform.localPosition.y - rangeDifference;

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
        if (orderIndex < 0)
        {
            randomY = UnityEngine.Random.Range(-2.2f, 2.8f);
            Rpoint.transform.localPosition = new Vector3(0.7416667f, randomY, 0.0333334f);
        }
        else
        {
            Rpoint.transform.localPosition = new Vector3(0.7416667f, distanceOrder[orderIndex], 0.0333334f);
        }

        distanceToKnob.Add(knob.transform.localPosition.y - Rpoint.transform.localPosition.y);
    }

    private void virtualKnobUpdateFromVrHandControl()
    {
        // update virtual model position
        sliderRight.SetActive(true);
        sliderLeft.SetActive(false);

        //unity_client.customMove(0f, 0.25f, 0.1f, -0.6f, 1.47f, 0.62f, movementType: 1);
    }

    private void virtualKnobUpdateFromVrPhysicalSlider()
    {
        // update virtual model position
        sliderRight.SetActive(false);
        sliderLeft.SetActive(true);

        instructionPanel.transform.position = panelPosReference.transform.position;
        confirmationPanel.transform.position = panelPosReference.transform.position;
        finishPanel.transform.position = panelPosReference.transform.position;

        instructionPanel.transform.rotation = panelPosReference.transform.rotation;
        confirmationPanel.transform.rotation = panelPosReference.transform.rotation;
        finishPanel.transform.rotation = panelPosReference.transform.rotation;

        sliderknob1.transform.localPosition = new Vector3(sliderknob1.transform.localPosition.x, (float)(virtualKnobMax - ((float)LongSliderInOut.value / 1764.0) * (virtualKnobMax - virtualKnobMin)), sliderknob1.transform.localPosition.z);
    }

    public void controllerRaySelect(bool select)
    {
        onControllerRaySelect = select;
    }

    private void virtualKnobUpdateFromRobotAxis()
    {
        // update virtual model position
        sliderRight.SetActive(true);
        sliderLeft.SetActive(false);

        instructionPanel.transform.position = panelReference1;
        confirmationPanel.transform.position = panelReference1;
        finishPanel.transform.position = panelReference1;

        instructionPanel.transform.rotation = panelReference2;
        confirmationPanel.transform.rotation = panelReference2;
        finishPanel.transform.rotation = panelReference2;

        // update slider
        sliderknob.transform.localPosition = new Vector3(sliderknob.transform.localPosition.x, sliderKnobReference.transform.localPosition.y + -1 * (float)(ShortSliderInOut.value - shortSliderMaxValue) / shortSliderMaxValue * 1.37f, sliderknob.transform.localPosition.z);
    }

    private void moveRobot(int bufferOne, int bufferTwo, int bufferThree, int bufferFour)
    {
        if (ShortSliderInOut.value > bufferOne & prev_sliderOne <= bufferOne)
        {
            unity_client.customMove(0.41173, -0.00627, 0.0476, -0.6, 1.5, 0.62, movementType: 1);
            if (Vector3.Distance(shortSliderTracker.transform.position, sliderLeftReference) > 0.001)
            {
                ShortSliderInOut.SetSlider(sp1);
            }
        }
        else if (ShortSliderInOut.value < bufferTwo & prev_sliderOne >= bufferTwo)
        {
            unity_client.customMove(0.028987, 0.37663, 0.0430782, -0.6, 1.5, 0.62, movementType: 1);
            if (Vector3.Distance(shortSliderTracker.transform.position, sliderRightReference) > 0.001)
            {
                ShortSliderInOut.SetSlider(sp2);
            }
        }
        else if ((ShortSliderInOut.value > bufferThree & ShortSliderInOut.value < bufferFour) & (prev_sliderOne <= bufferThree | prev_sliderOne >= bufferFour))
        {
            unity_client.stopRobot();
            ShortSliderInOut.SetSlider(0);
        }
    }
    private void moveRobotDynamic(int bufferOne, int bufferTwo, int bufferThree, int bufferFour, int a1, int a2, int b1, int b2)
    {
        if ((ShortSliderInOut.value > bufferOne & ShortSliderInOut.value < a1) & (prev_sliderOne <= bufferOne | prev_sliderOne >= a1))
        {
            unity_client.customMove(0.4286, 0.015565, 0.059643, -0.6, 1.5, 0.62, acc: 0.3f, speed: 0.3f, movementType: 1);
        }
        else if ((ShortSliderInOut.value > a1 & ShortSliderInOut.value < a2) & (prev_sliderOne <= a1 | prev_sliderOne >= a2))
        {
            unity_client.customMove(0.4286, 0.015565, 0.059643, -0.6, 1.5, 0.62, acc: 0.3f*1.5, speed: 0.3f*1.5, movementType: 1);
        }
        else if (ShortSliderInOut.value > a2 & prev_sliderOne <= a2)
        {
            unity_client.customMove(0.4286, 0.015565, 0.059643, -0.6, 1.5, 0.62, acc: 0.3f*2, speed: 0.3f*2, movementType: 1);
        }
        else if ((ShortSliderInOut.value > bufferTwo & ShortSliderInOut.value < b1) & (prev_sliderOne <= bufferTwo | prev_sliderOne >= b1))
        {
            unity_client.customMove(0.0485547, 0.395625, 0.0558569, -0.6, 1.5, 0.62, acc: 0.3f, speed: 0.3f, movementType: 1);
        }
        else if ((ShortSliderInOut.value > b1 & ShortSliderInOut.value < b2) & (prev_sliderOne <= b1 | prev_sliderOne >= b2))
        {
            unity_client.customMove(0.0485547, 0.395625, 0.0558569, -0.6, 1.5, 0.62, acc: 0.3f*1.5, speed: 0.3f*1.5, movementType: 1);
        }
        else if (ShortSliderInOut.value < b2 & prev_sliderOne >= b2)
        {
            unity_client.customMove(0.0485547, 0.395625, 0.0558569, -0.6, 1.5, 0.62, acc: 0.3f*2, speed: 0.3f*2, movementType: 1);
        }
        else if ((ShortSliderInOut.value > bufferThree & ShortSliderInOut.value < bufferFour) & (prev_sliderOne <= bufferThree | prev_sliderOne >= bufferFour))
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
        p1.x -= -0.5606855f - -0.5606583f;
        p1.y -= -0.001490745f - -0.0005011343f;
        p1.z -= 0.3161324f - 0.3580751f;

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

    public void HideObject(GameObject obj, bool hideFlag = false)
    {
        Renderer[] objectR = obj.GetComponentsInChildren<Renderer>();

        foreach (Renderer rr in objectR)
        {
            rr.enabled = hideFlag;
        }

    }

    private void readOrder()
    {
        string file = "ExperimentOrder.csv";

        StreamReader reader = new StreamReader(file);

        while (!reader.EndOfStream)
        {
            distanceOrder.Add(float.Parse(reader.ReadLine()));
        }

        reader.Close();
    }
}
