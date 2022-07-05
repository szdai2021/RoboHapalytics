using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SectionVewControlManager : MonoBehaviour
{
    public GameObject SlidingPlane;

    public GameObject xPosSlider;
    public GameObject xRotSlider;
    public GameObject yPosSlider;
    public GameObject yRotSlider;
    public GameObject zPosSlider;
    public GameObject zRotSlider;

    public GameObject xPosSliderCenter;
    public GameObject xRotSliderCenter;
    public GameObject yPosSliderCenter;
    public GameObject yRotSliderCenter;
    public GameObject zPosSliderCenter;
    public GameObject zRotSliderCenter;

    public Collider xCollider;
    public Collider yCollider;
    public Collider zCollider;

    public GameObject sliderPlaneVisualizerXLeft;
    public GameObject sliderPlaneVisualizerXRight;
    public GameObject sliderPlaneVisualizerYLeft;
    public GameObject sliderPlaneVisualizerYRight;
    public GameObject sliderPlaneVisualizerZTop;
    public GameObject sliderPlaneVisualizerZBottom;

    public GameObject xRotoryEncoder;
    public GameObject yRotoryEncoder;
    public GameObject zRotoryEncoder;

    public Collider xRotoryCollider;
    public Collider yRotoryCollider;
    public Collider zRotoryCollider;

    private List<float> test1 = new List<float> { 0.271818f, -0.198235f, 0.335298f, -0.61464f, 1.56437f, 0.602621f };
    private List<float> test2 = new List<float> { -0.183151f, -0.1211f, 0.372502f, -1.24571f, -2.85157f, 0.0393199f };
    private List<float> test3 = new List<float> { -0.00595115f, 0.0915175f, 0.686242f, -1.22094f, 1.14528f, -1.12578f };

    private List<float> test1_mid = new List<float> { 0.315f, 0.067f, 0.169f, -0.62f, 1.51f, 0.594f };
    private List<float> test2_mid = new List<float> { -0.22f, 0.182f, 0.44f, -1.06f, -1.31f, 1.665f };
    private List<float> test3_mid = new List<float> { -0.07f, 0.18f, 0.6f, -2.09f, 0.372f, 0.521f };

    private bool rotoryFlag = false;

    private Vector3 xRotPar = new Vector3(-0.6f, 1.47f, 0.62f);
    private Vector3 yRotPar = new Vector3(-1.746f, 0.7065f, 1.754f);
    private Vector3 zRotPar = new Vector3(0.741f, -1.748f, -1.7855f);

    private Vector3 xSliderNodeOffset = new Vector3(0,0,0);
    private Vector3 ySliderNodeOffset = new Vector3(0,0,0);
    private Vector3 zSliderNodeOffset = new Vector3(0,0,0);

    public GameObject finger;
    public GameObject virtualFingerTouchPoint;
    public GameObject virtualEndEffector;

    public UnityClient unity_client;

    public bool start = false;

    private int Pre_colliderArea = 5;
    private int Current_colliderArea = 0;

    private Vector3 pre_pos = new Vector3();
    private Vector3 pre_rot = new Vector3();

    public Text t1;
    public GameObject test;

    private Vector3 RobotCoord;
    private Vector3 RobotRot;

    private bool sliderMoveFlag = true;

    private bool toReset = true;
    private int onSliderIndex = 0;

    public bool fastMoveFlag = false;
    private int moveType = 3;
    private int interruptible = 1;

    public GameObject[] objectsToHide;

    public SerialInOut shortInOut;

    public Vector3 robotResetPos = new Vector3(-1.8765f, -1.22337f, 2.4f);
    private Vector3 robotResetRot = new Vector3(-1.19516f, 2.06182f, -7.85783f);

    private DateTime verticalAxisAddOnT1;
    private bool verticalAxisAddOnReady = false;
    public static bool verticalAxisAddOnDone = false;
    public static DateTime verticalAxisAddOnT2;

    public static DateTime actionStartTime;
    public static bool countTimeFlag = true;

    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject g in objectsToHide)
        {
            HideObject(g);
        }

        this.gameObject.SetActive(false);

        Vector3 RobotCoord = robotResetPos;
        Vector3 RobotRot = robotResetRot;
    }

    // Update is called once per frame
    void Update()
    {
        if (start)
        {
            var p0 = finger.transform.position;

            if (true)
            {
                interruptible = 1;
                if (xCollider.bounds.Contains(p0))
                {
                    getActionStartTime();
                    //moveType = 0;
                    Current_colliderArea = 1;
                    virtualFingerTouchPoint.transform.eulerAngles = new Vector3(0, 0, 90f);

                    var p1 = Vector3.Distance(p0, xPosSlider.transform.position);
                    var p2 = Vector3.Distance(p0, xRotSlider.transform.position);

                    var pmin = Mathf.Min(p1, p2);

                    if (pmin == p2)
                    {
                        onSliderIndex = 2;
                        xRotSlider.transform.parent.GetComponent<sliderValueControl>().isOnCheck();
                        xRotSlider.transform.parent.GetComponent<sliderValueControl>().isOn = true;

                        sliderMoveFlag = xRotSlider.transform.parent.GetComponent<sliderValueControl>().sliderMoveFlag;

                        if (sliderMoveFlag)
                        {
                            virtualFingerTouchPoint.transform.position = new Vector3(xRotSliderCenter.transform.position.x, xRotSliderCenter.transform.position.y, xRotSliderCenter.transform.position.z);
                        }
                    }
                    else if (pmin == p1)
                    {
                        onSliderIndex = 1;
                        xPosSlider.transform.parent.GetComponent<sliderValueControl>().isOnCheck();
                        xPosSlider.transform.parent.GetComponent<sliderValueControl>().isOn = true;

                        sliderMoveFlag = xPosSlider.transform.parent.GetComponent<sliderValueControl>().sliderMoveFlag;

                        if (sliderMoveFlag)
                        {
                            virtualFingerTouchPoint.transform.position = new Vector3(xPosSliderCenter.transform.position.x, xPosSliderCenter.transform.position.y, xPosSliderCenter.transform.position.z);
                        }
                    }

                    RobotCoord = convertUnityCoord2RobotCoord(virtualEndEffector.transform.position - xSliderNodeOffset);
                    RobotRot = xRotPar;
                }
                else if (yCollider.bounds.Contains(p0))
                {
                    getActionStartTime();
                    //moveType = 0;
                    Current_colliderArea = 2;
                    virtualFingerTouchPoint.transform.eulerAngles = new Vector3(0, -90f, 90f);

                    var p1 = Vector3.Distance(p0, yPosSlider.transform.position);
                    var p2 = Vector3.Distance(p0, yRotSlider.transform.position);

                    var pmin = Mathf.Min(p1, p2);

                    if (pmin == p2)
                    {
                        onSliderIndex = 4;
                        yRotSlider.transform.parent.GetComponent<sliderValueControl>().isOnCheck();
                        yRotSlider.transform.parent.GetComponent<sliderValueControl>().isOn = true;

                        sliderMoveFlag = yRotSlider.transform.parent.GetComponent<sliderValueControl>().sliderMoveFlag;

                        if (sliderMoveFlag)
                        {
                            virtualFingerTouchPoint.transform.position = new Vector3(yRotSliderCenter.transform.position.x, yRotSliderCenter.transform.position.y, yRotSliderCenter.transform.position.z);
                        }
                    }
                    else if (pmin == p1)
                    {
                        onSliderIndex = 3;
                        yPosSlider.transform.parent.GetComponent<sliderValueControl>().isOnCheck();
                        yPosSlider.transform.parent.GetComponent<sliderValueControl>().isOn = true;

                        sliderMoveFlag = yPosSlider.transform.parent.GetComponent<sliderValueControl>().sliderMoveFlag;

                        if (sliderMoveFlag)
                        {
                            virtualFingerTouchPoint.transform.position = new Vector3(yPosSliderCenter.transform.position.x, yPosSliderCenter.transform.position.y, yPosSliderCenter.transform.position.z);
                        }
                    }

                    RobotCoord = convertUnityCoord2RobotCoord(virtualEndEffector.transform.position - ySliderNodeOffset);

                    RobotRot = yRotPar;
                }
                else if (zCollider.bounds.Contains(p0))
                {
                    getActionStartTime();
                    Current_colliderArea = 3;
                    virtualFingerTouchPoint.transform.eulerAngles = new Vector3(0, -90f, 0);

                    var p1 = Vector3.Distance(p0, zPosSlider.transform.position);
                    var p2 = Vector3.Distance(p0, zRotSlider.transform.position);

                    var pmin = Mathf.Min(p1, p2);

                    if (pmin == p2)
                    {
                        onSliderIndex = 6;
                        zRotSlider.transform.parent.GetComponent<sliderValueControl>().isOnCheck();
                        zRotSlider.transform.parent.GetComponent<sliderValueControl>().isOn = true;

                        sliderMoveFlag = zRotSlider.transform.parent.GetComponent<sliderValueControl>().sliderMoveFlag;

                        if (sliderMoveFlag)
                        {
                            virtualFingerTouchPoint.transform.position = new Vector3(zRotSliderCenter.transform.position.x, zRotSliderCenter.transform.position.y, zRotSliderCenter.transform.position.z);
                        }
                    }
                    else if (pmin == p1)
                    {
                        onSliderIndex = 5;
                        zPosSlider.transform.parent.GetComponent<sliderValueControl>().isOnCheck();
                        zPosSlider.transform.parent.GetComponent<sliderValueControl>().isOn = true;

                        sliderMoveFlag = zPosSlider.transform.parent.GetComponent<sliderValueControl>().sliderMoveFlag;

                        if (sliderMoveFlag)
                        {
                            virtualFingerTouchPoint.transform.position = new Vector3(zPosSliderCenter.transform.position.x, zPosSliderCenter.transform.position.y, zPosSliderCenter.transform.position.z);
                        }
                    }

                    RobotCoord = convertUnityCoord2RobotCoord(virtualEndEffector.transform.position - zSliderNodeOffset);

                    RobotRot = zRotPar;

                    //interruptible = 0;
                    moveType = 1;
                }
                else if (xRotoryCollider.bounds.Contains(p0))
                {
                    getActionStartTime();
                    //moveType = 2;
                    rotoryFlag = true;

                    if (!xRotoryEncoder.GetComponent<RotationalEncoder>().isOn)
                    {
                        unity_client.customMove(-4.16445f, -1.18897f, 1.90225f, -2.1865f, 1.56376f, -9.35597f, movementType: 3);
                    }

                    xRotoryEncoder.GetComponent<RotationalEncoder>().isOnCheck();
                    xRotoryEncoder.GetComponent<RotationalEncoder>().isOn = true;

                }
                else if (yRotoryCollider.bounds.Contains(p0))
                {
                    getActionStartTime();
                    //moveType = 2;
                    rotoryFlag = true;

                    if (!yRotoryEncoder.GetComponent<RotationalEncoder>().isOn)
                    {
                        //unity_client.circularMove(test2_mid[0], test2_mid[1], test2_mid[2], test2_mid[3], test2_mid[4], test2_mid[5], 0);
                        //unity_client.circularMove(test2[0], test2[1], test2[2], test2[3], test2[4], test2[5], 0);

                        //unity_client.customMove(test2_mid[0], test2_mid[1], test2_mid[2], test2[0], test2[1], test2[2], movementType: 2);
                        //unity_client.customMove(test2[0], test2[1], test2[2], test2[3], test2[4], test2[5], movementType: 0);

                        //unity_client.customMove(test2R[0], test2R[1], test2R[2], test2R[3], test2R[4], test2R[5], movementType: 3);
                        unity_client.customMove(0.281548f, -0.827192f, 1.47676f, -2.17393f, 1.69551f, -8.9832f, movementType: 3);
                    }

                    //midPoint = test2_mid;

                    yRotoryEncoder.GetComponent<RotationalEncoder>().isOnCheck();
                    yRotoryEncoder.GetComponent<RotationalEncoder>().isOn = true;
                }
                else if (zRotoryCollider.bounds.Contains(p0))
                {
                    getActionStartTime();
                    //moveType = 2;
                    rotoryFlag = true;

                    if (!zRotoryEncoder.GetComponent<RotationalEncoder>().isOn)
                    {
                        //unity_client.circularMove(test3_mid[0], test3_mid[1], test3_mid[2], test3_mid[3], test3_mid[4], test3_mid[5], 0);
                        //unity_client.circularMove(test3[0], test3[1], test3[2], test3[3], test3[4], test3[5], 0);

                        //unity_client.customMove(test3_mid[0], test3_mid[1], test3_mid[2], test3[0], test3[1], test3[2], movementType: 2);
                        //unity_client.customMove(test3[0], test3[1], test3[2], test3[3], test3[4], test3[5], movementType: 0);

                        //unity_client.customMove(test3R[0], test3R[1], test3R[2], test3R[3], test3R[4], test3R[5], movementType: 3);
                        unity_client.customMove(-2.74635f, -2.30404f, 0.623497f, -1.38846f, -1.35079f, -3.09798f, movementType: 3);
                    }

                    //midPoint = test3_mid;

                    zRotoryEncoder.GetComponent<RotationalEncoder>().isOnCheck();
                    zRotoryEncoder.GetComponent<RotationalEncoder>().isOn = true;

                }
                else
                {
                    countTimeFlag = true;

                    verticalAxisAddOnDone = false;
                    sliderMoveFlag = true;
                    onSliderIndex = 0;

                    RobotCoord = robotResetPos;
                    RobotRot = robotResetRot;

                    Current_colliderArea = 0;

                    moveType = 3;

                    if (rotoryFlag)
                    {
                        rotoryFlag = false;

                        unity_client.customMove(RobotCoord.x, RobotCoord.y, RobotCoord.z, RobotRot.x, RobotRot.y, RobotRot.z, movementType: 3);
                    }

                    xRotoryEncoder.GetComponent<RotationalEncoder>().isOn = false;
                    yRotoryEncoder.GetComponent<RotationalEncoder>().isOn = false;
                    zRotoryEncoder.GetComponent<RotationalEncoder>().isOn = false;

                    xPosSlider.transform.parent.GetComponent<sliderValueControl>().isOn = false;
                    xRotSlider.transform.parent.GetComponent<sliderValueControl>().isOn = false;
                    yPosSlider.transform.parent.GetComponent<sliderValueControl>().isOn = false;
                    yRotSlider.transform.parent.GetComponent<sliderValueControl>().isOn = false;
                    zPosSlider.transform.parent.GetComponent<sliderValueControl>().isOn = false;
                    zRotSlider.transform.parent.GetComponent<sliderValueControl>().isOn = false;
                }

                if (((pre_pos != RobotCoord) | (pre_rot != RobotRot)) & sliderMoveFlag)
                {

                    if (Pre_colliderArea == 0 & Current_colliderArea != 0)
                    {
                        moveType = 0;
                    }

                    if (Current_colliderArea == 3)
                    {
                        if (!verticalAxisAddOnReady)
                        {
                            verticalAxisAddon();
                            verticalAxisAddOnT1 = DateTime.Now;
                            verticalAxisAddOnReady = true;
                        }
                    }
                    else
                    {
                        if (Pre_colliderArea == 3)
                        {
                            if (!verticalAxisAddOnReady)
                            {
                                verticalAxisAddon();
                                verticalAxisAddOnT1 = DateTime.Now;
                                verticalAxisAddOnReady = true;
                            }
                        }
                        else
                        {
                            unity_client.customMove(RobotCoord.x, RobotCoord.y, RobotCoord.z, RobotRot.x, RobotRot.y, RobotRot.z, movementType: moveType, interruptible: interruptible);
                        }
                    }
                }


                if (stateCheck() & verticalAxisAddOnReady)
                {
                    unity_client.customMove(RobotCoord.x, RobotCoord.y, RobotCoord.z, RobotRot.x, RobotRot.y, RobotRot.z, movementType: moveType, interruptible: interruptible);

                    verticalAxisAddOnReady = false;

                    verticalAxisAddOnDone = true;

                    verticalAxisAddOnT2 = DateTime.Now;
                }
                

                switch (onSliderIndex)
                {
                    case 1:
                        xPosSlider.transform.parent.GetComponent<sliderValueControl>().sliderMoveFlag = (Current_colliderArea == 0);
                        break;
                    case 2:
                        xRotSlider.transform.parent.GetComponent<sliderValueControl>().sliderMoveFlag = (Current_colliderArea == 0);
                        break;
                    case 3:
                        yPosSlider.transform.parent.GetComponent<sliderValueControl>().sliderMoveFlag = (Current_colliderArea == 0);
                        break;
                    case 4:
                        yRotSlider.transform.parent.GetComponent<sliderValueControl>().sliderMoveFlag = (Current_colliderArea == 0);
                        break;
                    case 5:
                        zPosSlider.transform.parent.GetComponent<sliderValueControl>().sliderMoveFlag = (Current_colliderArea == 0);
                        break;
                    case 6:
                        zRotSlider.transform.parent.GetComponent<sliderValueControl>().sliderMoveFlag = (Current_colliderArea == 0);
                        break;
                    default:
                        xPosSlider.transform.parent.GetComponent<sliderValueControl>().sliderMoveFlag = (Current_colliderArea == 0);
                        xRotSlider.transform.parent.GetComponent<sliderValueControl>().sliderMoveFlag = (Current_colliderArea == 0);
                        yPosSlider.transform.parent.GetComponent<sliderValueControl>().sliderMoveFlag = (Current_colliderArea == 0);
                        yRotSlider.transform.parent.GetComponent<sliderValueControl>().sliderMoveFlag = (Current_colliderArea == 0);
                        zPosSlider.transform.parent.GetComponent<sliderValueControl>().sliderMoveFlag = (Current_colliderArea == 0);
                        zRotSlider.transform.parent.GetComponent<sliderValueControl>().sliderMoveFlag = (Current_colliderArea == 0);
                        break;
                }
                
            }

            switch (Current_colliderArea)
            {
                case 0:
                    sliderPlaneVisualizerXLeft.GetComponent<MeshRenderer>().enabled = false;
                    sliderPlaneVisualizerXRight.GetComponent<MeshRenderer>().enabled = false;
                    sliderPlaneVisualizerYLeft.GetComponent<MeshRenderer>().enabled = false;
                    sliderPlaneVisualizerYRight.GetComponent<MeshRenderer>().enabled = false;
                    sliderPlaneVisualizerZTop.GetComponent<MeshRenderer>().enabled = false;
                    sliderPlaneVisualizerZBottom.GetComponent<MeshRenderer>().enabled = false;
                    break;
                case 1:
                    sliderPlaneVisualizerXLeft.GetComponent<MeshRenderer>().enabled = true;
                    sliderPlaneVisualizerXRight.GetComponent<MeshRenderer>().enabled = true;
                    sliderPlaneVisualizerYLeft.GetComponent<MeshRenderer>().enabled = false;
                    sliderPlaneVisualizerYRight.GetComponent<MeshRenderer>().enabled = false;
                    sliderPlaneVisualizerZTop.GetComponent<MeshRenderer>().enabled = false;
                    sliderPlaneVisualizerZBottom.GetComponent<MeshRenderer>().enabled = false;
                    break;
                case 2:
                    sliderPlaneVisualizerXLeft.GetComponent<MeshRenderer>().enabled = false;
                    sliderPlaneVisualizerXRight.GetComponent<MeshRenderer>().enabled = false;
                    sliderPlaneVisualizerYLeft.GetComponent<MeshRenderer>().enabled = true;
                    sliderPlaneVisualizerYRight.GetComponent<MeshRenderer>().enabled = true;
                    sliderPlaneVisualizerZTop.GetComponent<MeshRenderer>().enabled = false;
                    sliderPlaneVisualizerZBottom.GetComponent<MeshRenderer>().enabled = false;
                    break;
                case 3:
                    sliderPlaneVisualizerXLeft.GetComponent<MeshRenderer>().enabled = false;
                    sliderPlaneVisualizerXRight.GetComponent<MeshRenderer>().enabled = false;
                    sliderPlaneVisualizerYLeft.GetComponent<MeshRenderer>().enabled = false;
                    sliderPlaneVisualizerYRight.GetComponent<MeshRenderer>().enabled = false;
                    sliderPlaneVisualizerZTop.GetComponent<MeshRenderer>().enabled = true;
                    sliderPlaneVisualizerZBottom.GetComponent<MeshRenderer>().enabled = true;
                    break;
                default:
                    break;
            }

            Pre_colliderArea = Current_colliderArea;
            pre_pos = RobotCoord;
            pre_rot = RobotRot;
        }

        if (fastMoveFlag)
        {
            unity_client.circularMove(0, 0.25, 0.1, -0.6, 1.47, 0.62, 0);

            virtualFingerTouchPoint.transform.eulerAngles = new Vector3(0, 0, 90f);
            RobotRot = xRotPar;

            virtualFingerTouchPoint.transform.position = new Vector3(xRotSlider.transform.position.x, xRotSlider.transform.position.y, xRotSlider.transform.position.z);
            RobotCoord = convertUnityCoord2RobotCoord(virtualEndEffector.transform.position);
            unity_client.circularMove(RobotCoord.x, RobotCoord.y, RobotCoord.z, RobotRot.x, RobotRot.y, RobotRot.z, 0);

            virtualFingerTouchPoint.transform.position = new Vector3(xPosSlider.transform.position.x, xPosSlider.transform.position.y, xPosSlider.transform.position.z);
            RobotCoord = convertUnityCoord2RobotCoord(virtualEndEffector.transform.position);
            unity_client.circularMove(RobotCoord.x, RobotCoord.y, RobotCoord.z, RobotRot.x, RobotRot.y, RobotRot.z, 0);

            virtualFingerTouchPoint.transform.position = new Vector3(xRotSlider.transform.position.x, xRotSlider.transform.position.y, xRotSlider.transform.position.z);
            RobotCoord = convertUnityCoord2RobotCoord(virtualEndEffector.transform.position);
            unity_client.circularMove(RobotCoord.x, RobotCoord.y, RobotCoord.z, RobotRot.x, RobotRot.y, RobotRot.z, 0);

            unity_client.circularMove(0, 0.25, 0.1, -0.6, 1.47, 0.62, 0);

            virtualFingerTouchPoint.transform.eulerAngles = new Vector3(0, -90f, 90f);
            RobotRot = yRotPar;

            virtualFingerTouchPoint.transform.position = new Vector3(yRotSlider.transform.position.x, yRotSlider.transform.position.y, yRotSlider.transform.position.z);
            RobotCoord = convertUnityCoord2RobotCoord(virtualEndEffector.transform.position);
            unity_client.circularMove(RobotCoord.x, RobotCoord.y, RobotCoord.z, RobotRot.x, RobotRot.y, RobotRot.z, 0);

            virtualFingerTouchPoint.transform.position = new Vector3(yPosSlider.transform.position.x, yPosSlider.transform.position.y, yPosSlider.transform.position.z);
            RobotCoord = convertUnityCoord2RobotCoord(virtualEndEffector.transform.position);
            unity_client.circularMove(RobotCoord.x, RobotCoord.y, RobotCoord.z, RobotRot.x, RobotRot.y, RobotRot.z, 0);

            unity_client.circularMove(0, 0.25, 0.1, -0.6, 1.47, 0.62, 0);

            virtualFingerTouchPoint.transform.eulerAngles = new Vector3(0, -90f, 0);
            RobotRot = zRotPar;

            virtualFingerTouchPoint.transform.position = new Vector3(zPosSlider.transform.position.x, zPosSlider.transform.position.y, zPosSlider.transform.position.z);
            RobotCoord = convertUnityCoord2RobotCoord(virtualEndEffector.transform.position);
            unity_client.circularMove(RobotCoord.x, RobotCoord.y, RobotCoord.z, RobotRot.x, RobotRot.y, RobotRot.z, 0);

            virtualFingerTouchPoint.transform.position = new Vector3(zRotSlider.transform.position.x, zRotSlider.transform.position.y, zRotSlider.transform.position.z);
            RobotCoord = convertUnityCoord2RobotCoord(virtualEndEffector.transform.position);
            unity_client.circularMove(RobotCoord.x, RobotCoord.y, RobotCoord.z, RobotRot.x, RobotRot.y, RobotRot.z, 0);

            virtualFingerTouchPoint.transform.position = new Vector3(zPosSlider.transform.position.x, zPosSlider.transform.position.y, zPosSlider.transform.position.z);
            RobotCoord = convertUnityCoord2RobotCoord(virtualEndEffector.transform.position);
            unity_client.circularMove(RobotCoord.x, RobotCoord.y, RobotCoord.z, RobotRot.x, RobotRot.y, RobotRot.z, 0);

            unity_client.circularMove(0, 0.25, 0.1, -0.6, 1.47, 0.62, 0);

            unity_client.circularMove(test1_mid[0], test1_mid[1], test1_mid[2], test1_mid[3], test1_mid[4], test1_mid[5], 0);
            unity_client.circularMove(test1[0], test1[1], test1[2], test1[3], test1[4], test1[5], 0);

            unity_client.circularMove(test1_mid[0], test1_mid[1], test1_mid[2], test1_mid[3], test1_mid[4], test1_mid[5], 0);
            unity_client.circularMove(0, 0.25, 0.1, -0.6, 1.47, 0.62, 0);

            unity_client.circularMove(test2_mid[0], test2_mid[1], test2_mid[2], test2_mid[3], test2_mid[4], test2_mid[5], 0);
            unity_client.circularMove(test2[0], test2[1], test2[2], test2[3], test2[4], test2[5], 0);

            unity_client.circularMove(test2_mid[0], test2_mid[1], test2_mid[2], test2_mid[3], test2_mid[4], test2_mid[5], 0);
            unity_client.circularMove(0, 0.25, 0.1, -0.6, 1.47, 0.62, 0);

            unity_client.circularMove(test3_mid[0], test3_mid[1], test3_mid[2], test3_mid[3], test3_mid[4], test3_mid[5], 0);
            unity_client.circularMove(test3[0], test3[1], test3[2], test3[3], test3[4], test3[5], 0);

            unity_client.circularMove(test3_mid[0], test3_mid[1], test3_mid[2], test3_mid[3], test3_mid[4], test3_mid[5], 0);
            unity_client.circularMove(0, 0.25, 0.1, -0.6, 1.47, 0.62, 0);

            fastMoveFlag = false;
        }
    }

    private void getActionStartTime()
    {
        if (countTimeFlag)
        {
            actionStartTime = DateTime.Now;
            Debug.Log("Start Time: " + actionStartTime.ToString());

            countTimeFlag = false;
        }
    }

    IEnumerator centerSliderKnob()
    {
        while (true)
        {
            if (Current_colliderArea == 0)
            {
                if (shortInOut.value < 150)
                {
                    shortInOut.SetSlider(280);
                }
                else if (shortInOut.value > 250)
                {
                    shortInOut.SetSlider(-280);
                }
                else
                {
                    shortInOut.SetSlider(0);
                }
            }
            yield return null;
        }
    }

    private void verticalAxisAddon()
    {
        unity_client.customMove(0,0.35,0.2,-0.6,1.47,0.6,angle5: -1.57,angle6: 1.57, movementType: 0);
    }

    private bool stateCheck()
    {
        return ((DateTime.Now > verticalAxisAddOnT1.AddSeconds(0.1)) & unity_client.robotStopped);
    }

    private Vector3 convertUnityCoord2RobotCoord(Vector3 p1)
    {
        //p1.x -= -0.5606855f - -0.5606583f;
        //p1.y -= -0.001490745f - -0.0005011343f;
        //p1.z -= 0.3161324f - 0.3580751f;

        //float new_x = 0.7098f * p1.x + -0.00617f * p1.y + 0.707f * p1.z + 0.345378f;
        //float new_y = -0.7098f * p1.x + 0.00617f * p1.y + 0.7014f * p1.z - 0.338f;
        //float new_z = 0.0071f * p1.x + 1f * p1.y + 0.000028f * p1.z + 0.0064f;

        //return new Vector3(new_x, new_y, new_z);

        return unity_client.convertUnityCoord2RobotCoord(p1);
    }

    public void HideObject(GameObject obj, bool hideFlag = false)
    {
        Renderer[] objectR = obj.GetComponentsInChildren<Renderer>();

        foreach (Renderer rr in objectR)
        {
            rr.enabled = hideFlag;
        }

    }
}
