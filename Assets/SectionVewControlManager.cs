using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SectionVewControlManager : MonoBehaviour
{
    public GameObject SlidingPlane;
    //public GameObject ViewObject;

    public GameObject xPosSlider;
    public GameObject xRotSlider;
    public GameObject yPosSlider;
    public GameObject yRotSlider;
    public GameObject zPosSlider;
    public GameObject zRotSlider;

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

    private List<float> test1R = new List<float> { -4.51976f, 0.130553f, -1.32996f, -2.07984f, 0.573835f, -4.63f };
    private List<float> test2R = new List<float> { -0.0578583f, 0.0322039f, -1.07046f, -2.12128f, 0.894812f, -4.7f };
    private List<float> test3R = new List<float> { -2.7639f, -1.37618f, 0.006307f, -1.76509f, 1.94257f, -6.26f };

    private List<float> test1_mid = new List<float> { 0.315f, 0.067f, 0.169f, -0.62f, 1.51f, 0.594f };
    private List<float> test2_mid = new List<float> { -0.22f, 0.182f, 0.44f, -1.06f, -1.31f, 1.665f };
    private List<float> test3_mid = new List<float> { -0.07f, 0.18f, 0.6f, -2.09f, 0.372f, 0.521f };

    //private List<float> midPoint = new List<float> { 0f, 0.25f, 0.1f, -0.63f, 1.47f, 0.62f };

    private bool rotoryFlag = false;

    public GameObject xRotoryEncoderPosRef;
    public GameObject yRotoryEncoderPosRef;
    public GameObject zRotoryEncoderPosRef;

    private Vector3 xRotPar = new Vector3(-0.6f, 1.47f, 0.62f);
    private Vector3 yRotPar = new Vector3(-1.746f, 0.7065f, 1.754f);
    private Vector3 zRotPar = new Vector3(0.741f, -1.748f, -1.7855f);

    private Vector3 resetPos = new Vector3(0f, 0.25f, 0.1f);

    private Vector3 xRotEncoderPos = new Vector3(-0.459f, 0.182f, 0.319f);
    private Vector3 yRotEncoderPos = new Vector3(-0.901f, 0.184f, 0.15f);
    private Vector3 zRotEncoderPos = new Vector3(-0.901f, 0.265f, 0.352f);

    private Vector3 xSliderOnePos = new Vector3();
    private Vector3 ySliderOnePos = new Vector3();
    private Vector3 zSliderOnePos = new Vector3();

    private Vector3 xSliderTwoPos = new Vector3();
    private Vector3 ySliderTwoPos = new Vector3();
    private Vector3 zSliderTwoPos = new Vector3();

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

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.SetActive(false);

        Vector3 RobotCoord = new Vector3(-1.8765f, -1.22337f, 2.4f);
        Vector3 RobotRot = new Vector3(-1.19516f, 2.06182f, -7.85783f);
    }

    // Update is called once per frame
    void Update()
    {
        if (start & finger.transform.childCount == 1)
        {
            var p0 = finger.transform.GetChild(0).transform.position;

            if (unity_client.receiveFlag)
            {
                if (xCollider.bounds.Contains(p0))
                {
                    //moveType = 0;
                    Current_colliderArea = 1;
                    virtualFingerTouchPoint.transform.eulerAngles = new Vector3(0, 0, 90f);

                    var p1 = Vector3.Distance(p0, xPosSlider.transform.position);
                    var p2 = Vector3.Distance(p0, xRotSlider.transform.position);
                    var p3 = Vector3.Distance(p0, xRotoryEncoderPosRef.transform.position);

                    //var pmin = Mathf.Min(Mathf.Min(p1, p2), p3);
                    var pmin = Mathf.Min(p1, p2);

                    if (pmin == p2)
                    {
                        onSliderIndex = 2;
                        xRotSlider.transform.parent.GetComponent<sliderValueControl>().isOnCheck();
                        xRotSlider.transform.parent.GetComponent<sliderValueControl>().isOn = true;

                        sliderMoveFlag = xRotSlider.transform.parent.GetComponent<sliderValueControl>().sliderMoveFlag;

                        if (sliderMoveFlag)
                        {
                            virtualFingerTouchPoint.transform.position = new Vector3(xRotSlider.transform.position.x, xRotSlider.transform.position.y, xRotSlider.transform.position.z);
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
                            virtualFingerTouchPoint.transform.position = new Vector3(xPosSlider.transform.position.x, xPosSlider.transform.position.y, xPosSlider.transform.position.z);
                        }
                    }
                    else if (pmin == p3)
                    {
                        virtualFingerTouchPoint.transform.position = xRotEncoderPos;
                        xRotoryEncoder.GetComponent<RotationalEncoder>().isOnCheck();
                        xRotoryEncoder.GetComponent<RotationalEncoder>().isOn = true;
                    }

                    RobotCoord = convertUnityCoord2RobotCoord(virtualEndEffector.transform.position - xSliderNodeOffset);
                    RobotRot = xRotPar;
                }
                else if (yCollider.bounds.Contains(p0))
                {
                    //moveType = 0;
                    Current_colliderArea = 2;
                    virtualFingerTouchPoint.transform.eulerAngles = new Vector3(0, -90f, 90f);

                    var p1 = Vector3.Distance(p0, yPosSlider.transform.position);
                    var p2 = Vector3.Distance(p0, yRotSlider.transform.position);
                    var p3 = Vector3.Distance(p0, yRotoryEncoderPosRef.transform.position);

                    //var pmin = Mathf.Min(Mathf.Min(p1, p2), p3);
                    var pmin = Mathf.Min(p1, p2);

                    if (pmin == p2)
                    {
                        onSliderIndex = 4;
                        yRotSlider.transform.parent.GetComponent<sliderValueControl>().isOnCheck();
                        yRotSlider.transform.parent.GetComponent<sliderValueControl>().isOn = true;

                        sliderMoveFlag = yRotSlider.transform.parent.GetComponent<sliderValueControl>().sliderMoveFlag;

                        if (sliderMoveFlag)
                        {
                            virtualFingerTouchPoint.transform.position = new Vector3(yRotSlider.transform.position.x, yRotSlider.transform.position.y, yRotSlider.transform.position.z);
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
                            virtualFingerTouchPoint.transform.position = new Vector3(yPosSlider.transform.position.x, yPosSlider.transform.position.y, yPosSlider.transform.position.z);
                        }
                    }
                    else if (pmin == p3)
                    {
                        virtualFingerTouchPoint.transform.position = yRotEncoderPos;
                    }

                    RobotCoord = convertUnityCoord2RobotCoord(virtualEndEffector.transform.position - ySliderNodeOffset);

                    //unity_client.circularMove(RobotCoord.x, RobotCoord.y, RobotCoord.z, yRotPar.x, yRotPar.y, yRotPar.z, 0);
                    RobotRot = yRotPar;
                }
                else if (zCollider.bounds.Contains(p0))
                {
                    //moveType = 0;
                    Current_colliderArea = 3;
                    virtualFingerTouchPoint.transform.eulerAngles = new Vector3(0, -90f, 0);

                    var p1 = Vector3.Distance(p0, zPosSlider.transform.position);
                    var p2 = Vector3.Distance(p0, zRotSlider.transform.position);
                    var p3 = Vector3.Distance(p0, zRotoryEncoderPosRef.transform.position);

                    //var pmin = Mathf.Min(Mathf.Min(p1, p2), p3);
                    var pmin = Mathf.Min(p1, p2);

                    if (pmin == p2)
                    {
                        onSliderIndex = 6;
                        zRotSlider.transform.parent.GetComponent<sliderValueControl>().isOnCheck();
                        zRotSlider.transform.parent.GetComponent<sliderValueControl>().isOn = true;

                        sliderMoveFlag = zRotSlider.transform.parent.GetComponent<sliderValueControl>().sliderMoveFlag;

                        if (sliderMoveFlag)
                        {
                            virtualFingerTouchPoint.transform.position = new Vector3(zRotSlider.transform.position.x, zRotSlider.transform.position.y, zRotSlider.transform.position.z);
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
                            virtualFingerTouchPoint.transform.position = new Vector3(zPosSlider.transform.position.x, zPosSlider.transform.position.y, zPosSlider.transform.position.z);
                        }
                    }
                    else if (pmin == p3)
                    {
                        virtualFingerTouchPoint.transform.position = zRotEncoderPos;
                    }

                    RobotCoord = convertUnityCoord2RobotCoord(virtualEndEffector.transform.position - zSliderNodeOffset);

                    //unity_client.circularMove(RobotCoord.x, RobotCoord.y, RobotCoord.z, zRotPar.x, zRotPar.y, zRotPar.z, 0);
                    RobotRot = zRotPar;
                }
                else if (xRotoryCollider.bounds.Contains(p0))
                {
                    //moveType = 2;
                    rotoryFlag = true;

                    if (!xRotoryEncoder.GetComponent<RotationalEncoder>().isOn)
                    {
                        //unity_client.circularMove(test1_mid[0], test1_mid[1], test1_mid[2], test1_mid[3], test1_mid[4], test1_mid[5], 0);
                        //unity_client.circularMove(test1[0], test1[1], test1[2], test1[3], test1[4], test1[5], 0);

                        //unity_client.customMove(test1_mid[0], test1_mid[1], test1_mid[2], test1[0], test1[1], test1[2], movementType: 2);
                        //unity_client.customMove(test1[0], test1[1], test1[2], test1[3], test1[4], test1[5], movementType: 0);

                        unity_client.customMove(-4.52, 0.13, -2.016, -1.1745, 0.62645, -4.7525, movementType: 3, radius: 0.1); // middle point to reduce the risk
                        unity_client.customMove(test1R[0], test1R[1], test1R[2], test1R[3], test1R[4], test1R[5], movementType: 3);
                    }

                    //midPoint = test1_mid;

                    xRotoryEncoder.GetComponent<RotationalEncoder>().isOnCheck();
                    xRotoryEncoder.GetComponent<RotationalEncoder>().isOn = true;

                }
                else if (yRotoryCollider.bounds.Contains(p0))
                {
                    //moveType = 2;
                    rotoryFlag = true;

                    if (!yRotoryEncoder.GetComponent<RotationalEncoder>().isOn)
                    {
                        //unity_client.circularMove(test2_mid[0], test2_mid[1], test2_mid[2], test2_mid[3], test2_mid[4], test2_mid[5], 0);
                        //unity_client.circularMove(test2[0], test2[1], test2[2], test2[3], test2[4], test2[5], 0);

                        //unity_client.customMove(test2_mid[0], test2_mid[1], test2_mid[2], test2[0], test2[1], test2[2], movementType: 2);
                        //unity_client.customMove(test2[0], test2[1], test2[2], test2[3], test2[4], test2[5], movementType: 0);

                        unity_client.customMove(test2R[0], test2R[1], test2R[2], test2R[3], test2R[4], test2R[5], movementType: 3);
                    }

                    //midPoint = test2_mid;

                    yRotoryEncoder.GetComponent<RotationalEncoder>().isOnCheck();
                    yRotoryEncoder.GetComponent<RotationalEncoder>().isOn = true;
                }
                else if (zRotoryCollider.bounds.Contains(p0))
                {
                    //moveType = 2;
                    rotoryFlag = true;

                    if (!zRotoryEncoder.GetComponent<RotationalEncoder>().isOn)
                    {
                        //unity_client.circularMove(test3_mid[0], test3_mid[1], test3_mid[2], test3_mid[3], test3_mid[4], test3_mid[5], 0);
                        //unity_client.circularMove(test3[0], test3[1], test3[2], test3[3], test3[4], test3[5], 0);

                        //unity_client.customMove(test3_mid[0], test3_mid[1], test3_mid[2], test3[0], test3[1], test3[2], movementType: 2);
                        //unity_client.customMove(test3[0], test3[1], test3[2], test3[3], test3[4], test3[5], movementType: 0);

                        unity_client.customMove(test3R[0], test3R[1], test3R[2], test3R[3], test3R[4], test3R[5], movementType: 3);
                    }

                    //midPoint = test3_mid;

                    zRotoryEncoder.GetComponent<RotationalEncoder>().isOnCheck();
                    zRotoryEncoder.GetComponent<RotationalEncoder>().isOn = true;

                }
                else
                {
                    /*
                    if (Pre_colliderArea == 2)
                    {
                        moveType = 1;
                    }
                    else
                    {
                        moveType = 0;
                    }
                    */

                    sliderMoveFlag = true;
                    onSliderIndex = 0;

                    RobotCoord = new Vector3(-1.8765f, -1.22337f, 2.4f);
                    RobotRot = new Vector3(-1.19516f, 2.06182f, -7.85783f);

                    Current_colliderArea = 0;

                    moveType = 3;

                    if (rotoryFlag)
                    {
                        rotoryFlag = false;

                        unity_client.customMove(-1.8765, -1.22337, 2.4, -1.19516, 2.06182, -7.85783, movementType: 3);
                    }

                    xRotoryEncoder.GetComponent<RotationalEncoder>().isOn = false;
                    yRotoryEncoder.GetComponent<RotationalEncoder>().isOn = false;
                    zRotoryEncoder.GetComponent<RotationalEncoder>().isOn = false;

                    /*
                    if (!rotoryFlag)
                    {
                        Current_colliderArea = 0;
                        //RobotCoord = resetPos;
                        //RobotRot = xRotPar;
                    }
                    else
                    {
                        Current_colliderArea = 0;

                        //unity_client.customMove(midPoint[0], midPoint[1], midPoint[2], 0, 0.25, 0.1, movementType: 2);
                        //unity_client.customMove(0, 0.25, 0.1, -0.6, 1.47, 0.62, movementType: 0);

                        unity_client.customMove(-1.8765, -1.22337, 2.4, -1.19516, 2.06182, -7.85783, movementType: 3);

                        //unity_client.circularMove(midPoint[0], midPoint[1], midPoint[2], midPoint[3], midPoint[4], midPoint[5], 0);
                        //unity_client.circularMove(0, 0.25, 0.1, -0.6, 1.47, 0.62, 0);

                        rotoryFlag = false;
                    }*/
                }

                if (((pre_pos != RobotCoord) | (pre_rot != RobotRot)) & sliderMoveFlag)
                {

                    if (Pre_colliderArea == 0 & Current_colliderArea != 0)
                    {
                        moveType = 0;
                    }

                    //unity_client.circularMove(RobotCoord.x, RobotCoord.y, RobotCoord.z, RobotRot.x, RobotRot.y, RobotRot.z, 0);
                    unity_client.customMove(RobotCoord.x, RobotCoord.y, RobotCoord.z, RobotRot.x, RobotRot.y, RobotRot.z, movementType: moveType);

                    //sliderMoveFlag = false;
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

            //t1.text = Current_colliderArea.ToString();
            //t1.text = Vector3.Distance(test.transform.position, finger.transform.GetChild(0).transform.position).ToString("f5");
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
}
