using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEditor;
using TMPro;

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

    public SerialInOut ShortSliderInOut;
    public SerialInOut LongSliderInOut;
    public UnityClient unity_client;
    public PosRecorder posRecorder;

    public GameObject robot;
    public GameObject axis;
    public GameObject axisReference;

    public GameObject virtualFingerTouchPoint;
    public GameObject virtualEndEffector;
    public GameObject realSliderReference;
    public GameObject vrHandInteraction;

    public GameObject instructionPanel;
    public GameObject confirmationPanel;
    public GameObject experimentPanel;
    public GameObject finishPanel;

    public GameObject sliderRight;
    public GameObject sliderLeft;

    public GameObject debug1;
    public GameObject debug2;

    public GameObject buttonRed;
    public GameObject redButton;

    public GameObject shortSliderTracker;

    public Collider leftChecker;
    public Collider rightChecker;

    public TMP_Text trainingCounter;
    public TMP_Text experimentCounter;

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

    List<float> distanceOrder = new List<float>();

    private Vector3 buttonPos1 = new Vector3(1.7336f, -0.1341f, -0.0806f);
    private Vector3 buttonPos2 = new Vector3(0.5903f, -0.131f, -0.0709f);

    private Vector3 sliderLeftReference = new Vector3(-0.1942463f, 0.1035762f, 0.3072551f);
    private Vector3 sliderRightReference = new Vector3(-0.7358298f, 0.09867605f, 0.3014497f);

    private int dynamicCounter = 0;
    private bool dynamicLeftEnd = false;
    private bool dynamicRightEnd = false;
    private bool dynamicMoving = false;
    private float speedScale = 7.0f;

    private Vector3 sliderEndRobotPos1 = new Vector3(0.41173f, -0.00627f, 0.0476f); // left
    private Vector3 sliderEndRobotPos2 = new Vector3(0.028987f, 0.37663f, 0.0430782f); // right

    private int trialNum = 10;

    private float ax;
    private float ay;
    private float az;

    private float norm;

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
        experimentPanel.SetActive(false);
        finishPanel.SetActive(false);

        panelReference1 = instructionPanel.transform.position;
        panelReference2 = instructionPanel.transform.rotation;
        
        readOrder();

        ax = 0.41173f - 0.028987f;
        ay = -0.00627f - 0.37663f;
        az = 0.0476f - 0.0430782f;

        norm = Mathf.Sqrt(ax * ax + ay * ay + az * az);
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

            //if (sliderknob.transform.localPosition.y > virtualKnobMax)
            //{
            //    sliderknob.transform.localPosition = new Vector3(0, virtualKnobMax, 0);
            //}

            //if (sliderknob.transform.localPosition.y < virtualKnobMin)
            //{
            //    sliderknob.transform.localPosition = new Vector3(0, virtualKnobMin, 0);
            //}

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
                    moveRobotDynamic(150, 250);
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
                    if (startRecording)
                    {
                        posRecorder.startFlag = true;
                        posRecorder.participantID = participantID;
                        posRecorder.scenario = scenario;
                    }

                    instructionPanel.SetActive(true);
                    trialFlag = true;

                    trainingCounter.text = (10 - trialNum).ToString() + "/10";

                    if (triggerFlag && !prev_triggerFlag && LongSliderInOut.speed == 0) //&& (!lockoutThisFunction) // and then start a coroutine - turn lockouthisfunction true for 0.5 second then flase
                    {
                        if (trialNum > 0)
                        {
                            if (scenario != 2)
                            {
                                generateRandomPoint(konb, randomPoint, rangeOne);
                            }
                            else
                            {
                                generateRandomPoint(konb, randomPoint1, rangeOne);
                            }

                            trialNum--;
                        }
                        else
                        {
                            if (scenario != 2)
                            {
                                randomPoint.SetActive(false);
                            }
                            else
                            {
                                randomPoint1.SetActive(false);
                            }

                            experimentStage += 1;
                        }
                    }

                    break;
                case 2: // comfirmation stage
                    instructionPanel.SetActive(false);
                    confirmationPanel.SetActive(true);
                    trialFlag = false;

                    if (scenario != 2)
                    {
                        randomPoint.transform.localPosition = new Vector3(0.7416667f, distanceOrder[(scenario-1)*30+scenario-1], 0.0333334f);
                    }
                    else
                    {
                        randomPoint1.transform.localPosition = new Vector3(0.7416667f, distanceOrder[(scenario - 1) * 30 + scenario - 1], 0.0333334f);
                    }

                    if (triggerFlag && konb.transform.localPosition.y < 0.17f && konb.transform.localPosition.y > -0.37f)
                    {
                        experimentStage += 1;
                        if (scenario != 2)
                        {
                            randomPoint.SetActive(true);
                        }
                        else
                        {
                            randomPoint1.SetActive(true);
                        }
                        time_temp = System.DateTime.Now;
                    }

                    break;
                case 3: // recording stage
                    confirmationPanel.SetActive(false);
                    experimentPanel.SetActive(true);

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
                    experimentPanel.SetActive(false);

                    if (Input.GetKeyDown("r"))
                    {
                        finishPanel.SetActive(false);
                        if (scenario != 1 & scenario != 2)
                        {
                            unity_client.customMove(0.23, 0.17593, 0.045218, -0.6, 1.5, 0.62, movementType: 1);
                        }
                        scenario = 0;
                        trialNum = 10;
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

            experimentCounter.text = ((iterations - test_Num + (3 - rangeIndex) * iterations)).ToString() + "/30";

            if (triggerFlag && !prev_triggerFlag && LongSliderInOut.speed == 0) // trigger random point
            {
                distanceToTarget.Add(konb.transform.localPosition.y - Rpoint.transform.localPosition.y);
                reactionTime.Add((System.DateTime.Now - triggerTime[triggerTime.Count - 1]).TotalMilliseconds);

                test_Num--;

                if (rangeIndex != 0)
                {
                    int index = (scenario - 1) * 30 + (iterations - test_Num - 1 + (3 - rangeIndex) * iterations) + scenario;
                    generateRandomPoint(konb, Rpoint, rangeSelected, orderIndex: index);
                    debug1.GetComponent<TextMesh>().text = index.ToString();
                    debug2.GetComponent<TextMesh>().text = index.ToString();
                    triggerTime.Add(System.DateTime.Now);
                }
                else
                {
                    lastPress = true;
                }
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

                if (startRecording)
                {
                    posRecorder.startFlag = false;
                }
            }

        }
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
        bool pressed;
        if (scenario != 2) 
        {
            pressed = ShortSliderInOut.Button == 1;
        } 
        else 
        {
            pressed = LongSliderInOut.Button == 1;
        }

        return pressed;
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
            Rpoint.transform.localPosition = new Vector3(0.7416667f, Rpoint.transform.localPosition.y + distanceOrder[orderIndex], 0.0333334f);
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
        experimentPanel.transform.position = panelPosReference.transform.position;
        finishPanel.transform.position = panelPosReference.transform.position;

        instructionPanel.transform.rotation = panelPosReference.transform.rotation;
        confirmationPanel.transform.rotation = panelPosReference.transform.rotation;
        experimentPanel.transform.rotation = panelPosReference.transform.rotation;
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
        experimentPanel.transform.position = panelReference1;
        finishPanel.transform.position = panelReference1;

        instructionPanel.transform.rotation = panelReference2;
        confirmationPanel.transform.rotation = panelReference2;
        experimentPanel.transform.rotation = panelReference2;
        finishPanel.transform.rotation = panelReference2;

        // update slider
        sliderknob.transform.localPosition = new Vector3(sliderknob.transform.localPosition.x, sliderKnobReference.transform.localPosition.y + -1 * (float)(ShortSliderInOut.value - shortSliderMaxValue) / shortSliderMaxValue * 1.37f, sliderknob.transform.localPosition.z);
    }

    private void moveRobot(int bufferOne, int bufferTwo, int bufferThree, int bufferFour)
    {
        if (ShortSliderInOut.value > bufferOne & prev_sliderOne <= bufferOne)
        {
            unity_client.customMove(0.41173, -0.00627, 0.0476, -0.6, 1.5, 0.62, speed:0.4, movementType: 1);
            if (!leftChecker.bounds.Contains(shortSliderTracker.transform.position))
            {
                ShortSliderInOut.SetSlider(sp1);
            }
        }
        else if (ShortSliderInOut.value < bufferTwo & prev_sliderOne >= bufferTwo)
        {
            unity_client.customMove(0.028987, 0.37663, 0.0430782, -0.6, 1.5, 0.62, speed: 0.4, movementType: 1);
            if (!rightChecker.bounds.Contains(shortSliderTracker.transform.position))
            {
                ShortSliderInOut.SetSlider(sp2);
            }
        }
        else if ((ShortSliderInOut.value > bufferThree & ShortSliderInOut.value < bufferFour) & (prev_sliderOne <= bufferThree | prev_sliderOne >= bufferFour))
        {
            unity_client.stopRobot();
            ShortSliderInOut.SetSlider(0);
        }

        if (leftChecker.bounds.Contains(shortSliderTracker.transform.position) & ShortSliderInOut.SendVal < 0)
        {
            ShortSliderInOut.SetSlider(0);
        }

        if (rightChecker.bounds.Contains(shortSliderTracker.transform.position) & ShortSliderInOut.SendVal > 0)
        {
            ShortSliderInOut.SetSlider(0);
        }

        if (ShortSliderInOut.SendVal != 0)
        {
            if (ShortSliderInOut.value > bufferOne)
            {
                ShortSliderInOut.SetSlider(sp1 + ShortSliderInOut.speed * 20);
            }
            else if(ShortSliderInOut.value < bufferTwo)
            {
                ShortSliderInOut.SetSlider(sp2 + ShortSliderInOut.speed * 20);
            }
            
        }

        //debug1.GetComponent<TextMesh>().text = Vector3.Distance(shortSliderTracker.transform.position, sliderLeftReference).ToString() + " " + Vector3.Distance(shortSliderTracker.transform.position, sliderRightReference).ToString();
    }

    private void moveRobotDynamic(int bufferOne, int bufferTwo)
    {
        //if (Vector3.Distance(shortSliderTracker.transform.position, sliderLeftReference) < leftCheck)
        if (leftChecker.bounds.Contains(shortSliderTracker.transform.position))
        {
            if (!dynamicLeftEnd)
            {
                unity_client.stopRobot();
                ShortSliderInOut.SetSlider(0);
            }
            dynamicLeftEnd = true;
            dynamicMoving = false;
        }
        else
        {
            dynamicLeftEnd = false;
        }

        //if(Vector3.Distance(shortSliderTracker.transform.position, sliderRightReference) < rightCheck)
        if (rightChecker.bounds.Contains(shortSliderTracker.transform.position))
        {
            if (!dynamicRightEnd)
            {
                unity_client.stopRobot();
                ShortSliderInOut.SetSlider(0);
            }
            dynamicRightEnd = true;
            dynamicMoving = false;
        }
        else
        {
            dynamicRightEnd = false;
        }

        debug1.GetComponent<TextMesh>().text = Vector3.Distance(shortSliderTracker.transform.position, sliderLeftReference).ToString() + " " + dynamicLeftEnd.ToString() + " " + Vector3.Distance(shortSliderTracker.transform.position, sliderRightReference).ToString() + " " + dynamicRightEnd.ToString();

        if (dynamicCounter > 9)
        {
            if (ShortSliderInOut.value < bufferOne | ShortSliderInOut.value > bufferTwo)
            {
                dynamicMoving = true;
                float sp = (ShortSliderInOut.value - 415.0f / 2.0f) / (415.0f / 2.0f) * 0.0261f * speedScale * ((ShortSliderInOut.value - 415.0f / 2.0f) / (415.0f / 2.0f) * (ShortSliderInOut.value - 415.0f / 2.0f) / (415.0f / 2.0f) + 1);

                if ((ShortSliderInOut.value < bufferOne & !dynamicRightEnd) | (ShortSliderInOut.value > bufferTwo & !dynamicLeftEnd))
                {
                    unity_client.customMove(ax / (Mathf.Round(norm/sp * 100) / 100), ay / (Mathf.Round(norm / sp * 100) / 100), az /(Mathf.Round(norm / sp * 100) / 100), -0.6, 1.47, 0.62, speed: sp, acc: 1.5f, movementType: 4);

                    if (ShortSliderInOut.value < bufferOne & !dynamicRightEnd)
                    {
                        ShortSliderInOut.SetSlider((int)(Mathf.Abs(ShortSliderInOut.value - 415.0f / 2.0f) / (415.0f / 2.0f) * (120) + 190));
                    }

                    if (ShortSliderInOut.value > bufferTwo & !dynamicLeftEnd)
                    {
                        ShortSliderInOut.SetSlider((int)(Mathf.Abs(ShortSliderInOut.value - 415.0f / 2.0f) / (415.0f / 2.0f) * (-120) - 190));
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
                    ShortSliderInOut.SetSlider(0);
                }
            }
        }

        dynamicCounter++;
    }

    private Vector3 convertUnityCoord2RobotCoord(Vector3 p1)
    {
        //    p1.x -= -0.5606855f - -0.5606583f;
        //    p1.y -= -0.001490745f - -0.0005011343f;
        //    p1.z -= 0.3161324f - 0.3580751f;

        //    float new_x = 0.7098f * p1.x + -0.00617f * p1.y + 0.707f * p1.z + 0.345378f;
        //    float new_y = -0.7098f * p1.x + 0.00617f * p1.y + 0.7014f * p1.z - 0.338f;
        //    float new_z = 0.0071f * p1.x + 1f * p1.y + 0.000028f * p1.z + 0.0064f;

        //    return new Vector3(new_x, new_y, new_z);

        return unity_client.convertUnityCoord2RobotCoord(p1);
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
        string file = "ExperimentOrderNew.csv";

        StreamReader reader = new StreamReader(file);

        while (!reader.EndOfStream)
        {
            distanceOrder.Add(float.Parse(reader.ReadLine()));
        }

        reader.Close();
    }
}
