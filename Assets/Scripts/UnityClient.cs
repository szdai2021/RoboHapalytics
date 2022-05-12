using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;

public class UnityClient : MonoBehaviour
{
    public GameObject axis;

    private TcpClient client;
    private NetworkStream stream;

    private StreamReader inChannel;

    private StreamWriter outChannel;

    //private GameObject End_effector_virtual_plane;

    private List<Action> trajectoryQueue;
    //private List<Action> angularQueue;
    private List<float[]> posQueue;
    private Dictionary<int, char> link_index_to_axis_mapper;
    private Dictionary<int, float> link_offset;
    private Dictionary<int, int> link_direction;

    public string host_ip = "localhost";
    public int host_port = 27;

    public GameObject Virtual_end_effector;

    public GameObject UR3;
    public GameObject[] joint_links;

    public GameObject endEffector;

    public TMP_Text debugger_text; 

    //display zone;
    public float trackerPos_x = 0f;
    public float trackerPos_y = 0f;
    public float trackerPos_z = 0f;
    public Vector3 controller_endEffector_offset;
    public Vector3 desired_pos;
    public Quaternion desired_orientation_q;
    public Vector3 desired_orientation_e;
    public Vector3 controller_pos;

    public Vector3 previous_controller_pos;
    public Vector3 previous_tcp_pos;
    public Quaternion previous_ee_orientation;

    public float[] jointAngles = { 0, 0, 0, 0, 0, 0 };

    private float[] cur_tcp_pos = null;

    private double UR_x = 0.166533;

    public Vector3 currentRobotPos;
    public Vector3 currentRobotRot;
    
    private const double xLimitRight = 0.099;
    private const double yLimitRight = 0.37;

    private const double xLimitLeft = 0.45;
    private const double yLimitLeft = 0.0136;

    private double x = 0.2;
    private double y = 0.2;
    private double z = 0.15;
    private double rx = -0.6;
    private double ry = 1.47;
    private double rz = 0.62;

    private double a = 2;
    private double v = 1.5;

    private double a1, a2, a3, a4, b1, b2, b3, b4, c1, c2, c3, c4;

    private double prev_x, prev_y, prev_z, prev_rx, prev_ry, prev_rz;

    public bool receiveFlag = false;
    public float scale = 3f;

    public string fromRobot;

    private string angleCMD;

    private Thread getSpeedInfo;

    void Start()
    {
        trajectoryQueue = new List<Action>();
        //angularQueue = new List<Action>();

        posQueue = new List<float[]>();

        link_index_to_axis_mapper = new Dictionary<int, char>() {
            {0, 'y' },
            {1, 'x' },
            {2, 'y' },
            {3, 'x' },
            {4, 'y' },
            {5, 'z' }
        };

        link_offset = new Dictionary<int, float>()
        {
            {0, 462.078f},
            {1, 0.416f},
            {2, 89.922f},
            {3, -267.005f},
            {4, 182.724f},
            {5, 89.122f},
        };

        link_direction = new Dictionary<int, int>()
        {
            {0, -1}, //-1
            {1, 1},
            {2, -1},
            {3, -1},
            {4, 1},
            {5, 1},

        };
        //y, x, y, x, y, z

        client = new TcpClient(host_ip, host_port);
        Debug.Log("Connected to relay server");

        stream = client.GetStream();
        inChannel = new StreamReader(client.GetStream());
        outChannel = new StreamWriter(client.GetStream());

        //new Thread(new ThreadStart(recvJointStateLoop)).Start();
        //StartCoroutine(executeJointTrajectory());

        //getSpeedInfo = new Thread(getInfo);

        //getSpeedInfo.Start();

        initialPos();

        InvokeRepeating("getInfo", 0.5f, 0.5f);
    }

    private void initialPos()
    {
        customMove(-1.8765, -1.22337, 2.4, -1.19516, 2.06182, -7.85783, movementType: 3);
    }

    public void resetRobot()
    {
        string cmd = "(" + (xLimitLeft + xLimitRight) / 2 + "," + (yLimitLeft + yLimitRight)/2 + "," + z + ","
               + rx + "," + ry + "," + rz + ","
               + 1.2 + "," + 0.5 + "," + 0 + ")";
        outChannel.Write(cmd);
        outChannel.Flush();
        receiveFlag = false;
    }

    public int sliderTest_OnChange(double sliderChanges, int buttonPress, int sliderSpeed = 0, double acc = 0, double speed = 0)
    {
        double ratio = 1.3;

        if (acc == 0)
        {
            acc = a;
        }

        if (speed == 0)
        {
            speed = v;
        }

        if (Math.Abs(sliderSpeed)>=10)
        {
            acc *= 2;
            speed *= 2;
            ratio *= 1.2;
        }

        if (Math.Abs(sliderSpeed) >= 20)
        {
            acc *= 1.5;
            speed *= 1.5;
        }

        if (Math.Abs(sliderSpeed) >= 30)
        {
            acc *= 1.5;
            speed *= 1.5;
        }

        if (Math.Abs(sliderSpeed) >= 40)
        {
            acc *= 1.5;
            speed *= 1.5;
        }

        if (sliderSpeed < 0)
        {
            ratio *= 1;
        }

        //UR_x += -sliderChanges;
        x -= sliderChanges * (0.15 / (255 * 30.2 / 14.5)) * ratio;
        y += sliderChanges * (0.15 / (255 * 30.2 / 14.5)) * ratio;

        if (x < xLimitRight)
        {
            x = xLimitRight;
        }
        if (y > yLimitRight)
        {
            y = yLimitRight;
        }
        
        if (x > xLimitLeft)
        {
            x = xLimitLeft;
        }
        if (y < yLimitLeft)
        {
            y = yLimitLeft;
        }
        
        string cmd = packCMD(x, y, z, rx, ry, rz, acc, speed, buttonPress, 0);
        outChannel.Write(cmd);
        outChannel.Flush();

        receiveFlag = false;

        if (x == xLimitLeft | x == xLimitRight)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
    
    public void circularMove(double xi, double yi, double zi, double rxi, double ryi, double rzi, int btn, bool sA = false, double angle = 0, int jointIndex = 0)
    {
        string cmd = packCMD(xi, yi, zi, rxi, ryi, rzi, btn_press: btn, scenario: 0, speedAdopt: sA, angle_bias: angle, joint_index: jointIndex);
        outChannel.Write(cmd);
        outChannel.Flush();
        receiveFlag = false;
    }

    public void scatterMove(double xi, double yi, double zi)
    {
        string cmd = packCMD(xi, yi, zi, scenario: 1);
        outChannel.Write(cmd);
        outChannel.Flush();
        receiveFlag = false;
    }

    public void getCurrentPos()
    {
        string cmd = packCMD(scenario: 1);
        outChannel.Write(cmd);
        outChannel.Flush();
        receiveFlag = false;
    }

    public void stopRobot()
    {
        string cmd = packCMD(scenario: 2);
        outChannel.Write(cmd);
        outChannel.Flush();
        receiveFlag = false;
    }

    public void customMove(double xi, double yi, double zi, double rxi, double ryi, double rzi,
        double acc = 0.3, double speed = 0.3, double blend_r = 0, double btn_press = 0, double scenario = 0, bool speedAdopt = false, double angle_bias = 0, int joint_index = 5,
        int movementType = 0, double extra1 = 0, double extra2 = 0, double extra3 = 0, double radius = 0) // movementType 0: jointspace linear; Type 1: toolspace linear; Type 2: circular; Type 3: jointspace linear by joint pos; Type 4: speedl
    {
        string cmd = packCMD(xi, yi, zi, rxi, ryi, rzi, acc, speed, blend_r, btn_press, scenario, speedAdopt, angle_bias, joint_index, movementType, extra1, extra2, extra3, radius);
        outChannel.Write(cmd);
        outChannel.Flush();
        receiveFlag = false;
    }

    private string packCMD(double Pos_x = 0.2, double Pos_y = 0.2, double Pos_z = 0.07, double Rot_x = -0.6, double Rot_y = 1.47, double Rot_z = 0.62, 
        double acc = 0.3, double speed = 0.3, double blend_r = 0, double btn_press = 0, double scenario = 0, bool speedAdopt = false, double angle_bias = 0, int joint_index = 5, 
        int movementType = 0, double extra1 = 0, double extra2 = 0, double extra3 = 0, double radius = 0) // movementType 0: jointspace linear; Type 1: toolspace linear; Type 2: circular; Type 3: jointspace linear by joint pos
    {
        if (speedAdopt)
        {
            var distance = Vector3.Distance(new Vector3((float)prev_x, (float)prev_y, (float)prev_z), new Vector3((float)Pos_x, (float)Pos_y, (float)Pos_z));

            acc = Math.Log(1 + distance) * scale + 0.3;
            speed = Math.Log(1 + distance) * scale + 0.3;
        }

        string cmd = "(" + Pos_x + "," + Pos_y + "," + Pos_z + ","
               + Rot_x + "," + Rot_y + "," + Rot_z + ","
               + acc + "," + speed + "," + btn_press + "," + scenario + "," + angle_bias + "," + joint_index + "," + movementType + "," + extra1 + "," + extra2 + "," + extra3 + "," + radius + ")";

        prev_x = Pos_x;
        prev_y = Pos_y;
        prev_z = Pos_z;
        prev_rx = Rot_x;
        prev_ry = Rot_y;
        prev_rz = Rot_z;

        return cmd;
    }

    public void TCPAngleMove(double angle, double acc = 3, double speed = 3, double btn_press = 0)
    {
        double new_angle = (angle+jointAngles[5]) / 180 * Math.PI;

        string cmd = "(" + jointAngles[0] / 180 * Math.PI + "," + jointAngles[1] / 180 * Math.PI + "," + jointAngles[2] / 180 * Math.PI + ","
               + jointAngles[3] / 180 * Math.PI + "," + jointAngles[4] / 180 * Math.PI + "," + new_angle + ","
               + acc + "," + speed + "," + btn_press + "," + 1 + ")";

        outChannel.Write(cmd);
        outChannel.Flush();
        receiveFlag = false;
    }

    public void UR_test()
    {
        print(endEffector.transform.position);
        print(endEffector.transform.rotation.eulerAngles);
        sliderTest_OnChange(0, 0, 1, 0.5, 0.5);
        /*
        string cmd = "(" + x + "," + y + "," + z + ","
            + rx + "," + ry + "," + rz + ","
            + a + "," + v + "," + 0 + ")";
            //+ 0.2 + "," + 0.1 + "," + 0 + ")";
        outChannel.Write(cmd);
        outChannel.Flush();
        */
    }

    private void getInfo()
    {
        fromRobot = inChannel.ReadLine();
        debugger_text.text = fromRobot;

        Debug.Log(fromRobot);
    }

    private void Update()
    {

    }

    void OnDestroy()
    {
        inChannel.Close();
        outChannel.Close();
        stream.Close();
        client.Close();
        Debug.Log("Client close");
    }


}
