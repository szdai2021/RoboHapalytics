using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class controlmanager : MonoBehaviour
{
    public GameObject robot;
    public GameObject axis;

    public float x = 0.2f;
    public float y = 0.2f;
    public float z = 0.07f;
    public float rx = -0.6f;
    public float ry = 1.47f;
    public float rz = 0.62f;

    public double angleBias = 0;
    public int jointIndex = 0;

    public int moveType = 0;
    public float extra1 = 0;
    public float extra2 = 0;
    public float extra3 = 0;

    public int sp1 = -250;
    public int sp2 = 250;

    public float testSpeed = 0.0261f;
    public float testAcce = 1.0f;

    public UnityClient unity_client;
    public SerialInOut shortSliderInOut;

    public GameObject sliderModel;
    public GameObject torusModel;
    public GameObject plot;

    public GameObject tcp_pos;

    public GameObject sliderKnob;
    public GameObject OriginChild;
    public GameObject Origin;

    public GameObject TCPController;

    public GameObject endEffector;
    public GameObject target;
    public GameObject virtualConnection;
    public Collider rangeCollider;

    public GameObject virtualEndEffector;

    public bool testStartCheck;
    public float speedScale = 5.0f;

    private int force_flag = 0;
    private int counter = 0;

    private bool robotRender = true;
    private bool axisRender = true;

    public bool testFlag = false;

    private float angle = 0;
    private Vector3 xyz = Vector3.zero;

    private float prev_x = 0;
    private float prev_y = 0;
    private float prev_z = 0;

    private float TCPInitAngle = 0;
    private float prev_angle = 0;

    public GameObject angleReference;

    public int scenario = 0;
    public SectionVewControlManager svcm;
    public ViconMixedRealityCalibration vmrc;

    public GameObject sectionView;
    public GameObject scatterPlot;
    public GameObject barChart;

    private List<float> test1 = new List<float> { 0.229276f, -0.223641f, 0.396649f, -0.647695f, 1.54944f, 0.551866f };
    private List<float> test2 = new List<float> { -0.295316f, -0.147816f, 0.411297f, 0.584606f, 1.50867f, -0.56811f };
    private List<float> test3 = new List<float> { -0.018218f, 0.0830661f, 0.685242f, -0.779805f, -1.72632f, 1.75751f };

    private List<float> test1R = new List<float> { -4.51976f, 0.130553f, -1.32996f, -2.07984f, 0.573835f, -4.63f };
    private List<float> test2R = new List<float> { -0.0578583f, 0.0322039f, -1.07046f, -2.12128f, 0.894812f, -4.7f };
    private List<float> test3R = new List<float> { -2.7639f, -1.37618f, 0.006307f, -1.76509f, 1.94257f, -6.26f };

    private List<float> test1_mid = new List<float> { 0.315f, 0.067f, 0.169f, -0.62f, 1.51f, 0.594f };
    private List<float> test2_mid = new List<float> { -0.22f, 0.182f, 0.44f, -1.06f, -1.31f, 1.665f };
    private List<float> test3_mid = new List<float> { -0.07f, 0.18f, 0.6f, -2.09f, 0.372f, 0.521f };

    private List<float> midPoint = new List<float> { 0f, 0.25f, 0.1f, -0.63f, 1.47f, 0.62f };

    public Material transparent;

    private bool prototypeFlag = false;
    private int prev_sliderOne = 0;

    private bool prototypeFlag2 = false;
    private float speedRatio = 0.0261f;
    private int prototypeFlag2_counter = 0;
    private int prototypeFlag2_counter2 = 0;
    private bool tempTest = false;

    public int threshold = 60;

    private Vector3 prev_pos;
    private Vector3 new_pos;

    private float ax;
    private float ay;
    private float az;

    private float norm;

    public float sp = 0.01f;

    public void startScenario()
    {
        HideShowTagGameObject("Slider", false);
        HideShowTagGameObject("Test", false);

        if (scenario == 1) // section view
        {
            scatterPlot.SetActive(false);
            barChart.SetActive(false);
            sectionView.SetActive(true);

            svcm.start = true;
        }
        else if (scenario == 2) // scatter plot
        {
            scatterPlot.SetActive(true);
            barChart.SetActive(false);
            sectionView.SetActive(false);
        }
        else if (scenario == 3) // bar chart
        {
            scatterPlot.SetActive(false);
            barChart.SetActive(true);
            sectionView.SetActive(false);
        }
        else
        {
            scatterPlot.SetActive(false);
            barChart.SetActive(false);
            sectionView.SetActive(false);
        }
    }

    public void HideRobot(bool hideFlag)
    {
        robotRender = hideFlag;
        axisRender = hideFlag;

        Renderer[] Robot_rs = robot.GetComponentsInChildren<Renderer>();

        foreach (Renderer rr in Robot_rs)
        {
            //rr.enabled = robotRender;

            Material[] mats = rr.materials;

            for (int i = 0; i < rr.materials.Length; i++)
            {
                mats[i] = transparent;
            }

            rr.materials = mats;
        }

        Renderer[] Axis_rs = axis.GetComponentsInChildren<Renderer>();

        foreach (Renderer ar in Axis_rs)
        {
            //ar.enabled = axisRender;

            ar.material = transparent;
        }
    }

    private void HideShowTagGameObject(string tag, bool show)
    {
        GameObject[] gameObjectArray = GameObject.FindGameObjectsWithTag(tag);

        foreach (GameObject go in gameObjectArray)
        {
            go.SetActive(show);
        }
    }

    private void Start()
    {
        TCPInitAngle = TCPController.transform.rotation.eulerAngles.z;

        Renderer[] EE_Array = virtualEndEffector.transform.GetChild(0).GetComponentsInChildren<Renderer>();

        foreach (Renderer rr in EE_Array)
        {
            rr.enabled = false;
        }
    }

    private void Update()
    {
        //print(sliderKnob.transform.position);

        if (Input.GetKeyDown("space")) // position test
        {
            svcm.start = false;
            if (force_flag == 0)
            {
                force_flag = 1;
                //unity_client.circularMove(0.276741, 0.192015, 0.0699855, 0.412842, 0.050602, 0.280262, 0);
            }
            else
            {
                force_flag = 0;
                //unity_client.circularMove(0.085512, 0.397466, 0.316909, 0.701469, 1.82303, 1.77534, 0);
            }

            //unity_client.circularMove(x,y,z,rx,ry,rz,0,angle: angleBias, jointIndex: jointIndex);

            unity_client.customMove(x,y,z,rx,ry,rz, angle_bias: angleBias, joint_index: jointIndex, movementType: moveType, extra1: extra1, extra2: extra2, extra3: extra3);

            //Origin.transform.position = sliderKnob.transform.position;
            //Origin.transform.eulerAngles = new Vector3(0, 0, 0);

            //OriginChild.transform.position = sliderKnob.transform.position;
            //OriginChild.transform.rotation = sliderKnob.transform.rotation;

            /*
            debug.text = OriginChild.transform.localRotation.eulerAngles.ToString("f5");
            receivedText.text = OriginChild.transform.localRotation.ToString("f5");
            OriginChild.transform.localRotation.ToAngleAxis(out angle, out xyz);
            t2.text = angle.ToString("f5") + "+" + xyz.ToString("f5");
            t3.text = (Mathf.Deg2Rad * angle * xyz).ToString("f5");
            */


            /*
            debug.text = sliderKnob.transform.rotation.eulerAngles.ToString("f5");
            receivedText.text = sliderKnob.transform.rotation.ToString("f5");
            sliderKnob.transform.rotation.ToAngleAxis(out angle, out xyz);
            t2.text = angle.ToString("f5") + "+" + xyz.ToString("f5");
            t3.text = (Mathf.Deg2Rad*angle * xyz).ToString("f5");
            */
        }
        else if (Input.GetKeyDown("t")) // get current robot pose
        {
            /*
            svcm.start = false;
            unity_client.TCPAngleMove(10);
            */
            unity_client.getCurrentPos();
        } 
        else if (Input.GetKeyDown("r")) // reset camera and vr space
        {
            vmrc.ApplyOffset();
        }
        else if (Input.GetKeyDown("p")) // prototype test
        {
            prototypeFlag = !prototypeFlag;

            if (prototypeFlag)
            {
                unity_client.customMove(0.35, 0.1, 0.1, -0.6,1.5,0.62,movementType: 1);
            }
        }
        else if (Input.GetKeyDown("1"))
        {
            //unity_client.circularMove(test1_mid[0], test1_mid[1], test1_mid[2], test1_mid[3], test1_mid[4], test1_mid[5], 0);
            //unity_client.circularMove(test1[0], test1[1], test1[2], test1[3], test1[4], test1[5], 0);
            unity_client.customMove(test1R[0], test1R[1], test1R[2], test1R[3], test1R[4], test1R[5], movementType: moveType);

            midPoint = test1_mid;
        }
        else if (Input.GetKeyDown("2"))
        {
            //unity_client.circularMove(test2_mid[0], test2_mid[1], test2_mid[2], test2_mid[3], test2_mid[4], test2_mid[5], 0);
            //unity_client.circularMove(test2[0], test2[1], test2[2], test2[3], test2[4], test2[5], 0);
            unity_client.customMove(test2R[0], test2R[1], test2R[2], test2R[3], test2R[4], test2R[5], movementType: moveType);

            midPoint = test2_mid;
        }
        else if (Input.GetKeyDown("3"))
        {
            //unity_client.circularMove(test3_mid[0], test3_mid[1], test3_mid[2], test3_mid[3], test3_mid[4], test3_mid[5], 0);
            //unity_client.circularMove(test3[0], test3[1], test3[2], test3[3], test3[4], test3[5], 0);
            unity_client.customMove(test3R[0], test3R[1], test3R[2], test3R[3], test3R[4], test3R[5], movementType: moveType);

            midPoint = test3_mid;
        }
        else if (Input.GetKeyDown("0"))
        {
            //unity_client.circularMove(midPoint[0], midPoint[1], midPoint[2], midPoint[3], midPoint[4], midPoint[5], 0);
            unity_client.circularMove(0, 0.25, 0.1, -0.6, 1.47, 0.62, 0);
        }
        else if (Input.GetKeyDown("u"))
        {
            unity_client.customMove(0, 0.25, 0.1, -0.6, 1.47, 0.62, speed: testSpeed, acc: testAcce ,movementType: 4);
        }
        else if (Input.GetKeyDown("d"))
        {
            unity_client.customMove(0, 0.25, 0.1, -0.6, 1.47, 0.62, speed: -testSpeed, acc: testAcce, movementType: 4);
        }
        else if (Input.GetKeyDown("s"))
        {
            unity_client.stopRobot();
        }
        else if (Input.GetKeyDown("m"))
        {
            unity_client.customMove(0.23, 0.17593, 0.045218, -0.6, 1.5, 0.62, movementType: 0);
            prototypeFlag2 = true;
        }

        if (prototypeFlag2 & testStartCheck)
        {
            if (prototypeFlag2_counter > 9)
            {
                if (shortSliderInOut.value < 150 | shortSliderInOut.value > 250)
                {
                    tempTest = true;
                    float sp = (shortSliderInOut.value - 415.0f / 2.0f) / (415.0f / 2.0f) * speedRatio * speedScale;

                    float ax = 0.41173f - 0.028987f;
                    float ay = -0.00627f - 0.37663f;
                    float az = 0.0476f - 0.0430782f;

                    float norm = Mathf.Sqrt(ax * ax + ay * ay + az * az);

                    unity_client.customMove(ax / norm * sp, ay / norm * sp, az / norm * sp, -0.6, 1.47, 0.62, speed: sp, acc: 1.5f, movementType: 4);

                    prototypeFlag2_counter = 0;
                }
                else
                {
                    if (tempTest)
                    {
                        unity_client.stopRobot();
                        tempTest = false;
                    }
                }
            }

            if (prototypeFlag2_counter2 > 600)
            {
                unity_client.customMove(0, 0, 0, 0, 0, 0, movementType: 4, scenario: 3);

                prototypeFlag2_counter2 = 0;
            }
            prototypeFlag2_counter2++;

            prototypeFlag2_counter++;
            
        }

        if (prototypeFlag)
        {
            if (shortSliderInOut.value > 325 & prev_sliderOne <= 325)
            {
                unity_client.customMove(0.4286, 0.015565, 0.059643, -0.6, 1.5, 0.62, movementType: 1);
                shortSliderInOut.SetSlider(sp1);
                
            }
            else if (shortSliderInOut.value < 89 & prev_sliderOne >= 89)
            {
                unity_client.customMove(0.0485547, 0.395625, 0.0558569, -0.6, 1.5, 0.62, movementType: 1);
                shortSliderInOut.SetSlider(sp2);
            }
            else if ((shortSliderInOut.value > 162 & shortSliderInOut.value < 244) & (prev_sliderOne <= 162 | prev_sliderOne >= 244))
            {
                unity_client.stopRobot();
                shortSliderInOut.SetSlider(0);
            }

            prev_sliderOne = shortSliderInOut.value;
        }

        if (!testFlag)
        {
            virtualConnection.transform.position = sliderKnob.transform.position;
        }

        if (counter > threshold)
        {
            followUpTest();
            counter = 0;
        }
        counter++;
    }

    private void followUpTest()
    {
        svcm.start = false;
        //var p0 = target.transform.GetChild(0).transform.position;

        var p0 = TCPController.transform.position;

        //print(rangeCollider.bounds.Contains(p0));
        if (rangeCollider.bounds.Contains(p0))
        //if (rangeCollider.bounds.Contains(p0))
        {
            virtualConnection.transform.position = new Vector3(p0.x, p0.y, p0.z - 0.1655f);

            var p1 = virtualConnection.transform.GetChild(0).transform.position;

            p1.x -= -0.5606855f - -0.5606583f;
            p1.y -= -0.001490745f - -0.0005011343f;
            p1.z -= 0.3161324f - 0.3580751f;

            float new_x = 0.7098f * p1.x + -0.00617f * p1.y + 0.707f * p1.z + 0.345378f;
            float new_y = -0.7098f * p1.x + 0.00617f * p1.y + 0.7014f * p1.z - 0.338f;
            float new_z = 0.0071f * p1.x + 1f * p1.y + 0.000028f * p1.z + 0.0064f;

            new_pos = new Vector3(new_x, new_y, new_z);

            double a_bias = 0;
            var a = TCPController.transform.rotation.eulerAngles.z;

            if (a > 180)
            {
                a = a - 360;
            }

            if (Math.Abs(a - prev_angle) > 3)
            {
                a_bias = (a - prev_angle) / 180 * Math.PI;

                prev_angle = a;
            }

            if (Vector3.Distance(new Vector3(prev_x,prev_y,prev_z), new Vector3(p1.x, p1.y, p1.z)) > 0.001)
            {
                //unity_client.circularMove(new_x, new_y, new_z, rx, ry, rz, 0, true, a_bias);

                //unity_client.customMove(new_x, new_y, new_z, rx, ry, rz, angle_bias: -a_bias, movementType: 1);

                ax = new_pos.x - prev_pos.x;
                ay = new_pos.y - prev_pos.y;
                az = new_pos.z - prev_pos.z;

                norm = Mathf.Sqrt(ax * ax + ay * ay + az * az);

                // DANGEROUS !!!
                //unity_client.customMove(ax / (Mathf.Round(norm / sp * 100) / 100), ay / (Mathf.Round(norm / sp * 100) / 100), az / (Mathf.Round(norm / sp * 100) / 100), 0, 0, 0, speed: sp, acc: 1.5f, movementType: 4);
            }
            else
            {
                unity_client.stopRobot();
            }

            //var a = TCPController.transform.rotation.eulerAngles.z-angleReference.transform.rotation.eulerAngles.z+65;

            prev_x = p1.x;
            prev_y = p1.y;
            prev_z = p1.z;

            prev_pos = new_pos;
        }
    }

}
