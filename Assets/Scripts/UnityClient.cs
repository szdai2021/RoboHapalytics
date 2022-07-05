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

    public string host_ip = "localhost";
    public int host_port = 27;

    public GameObject UR3;
    public GameObject[] joint_links;

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

    private double prev_x, prev_y, prev_z;

    public bool receiveFlag = false;
    public float scale = 3f;

    public string fromRobot;

    private Thread getSpeedInfo;

    public bool robotStopped = true;

    private Matrix4x4 transMatrix = Matrix4x4.zero;
    private Matrix4x4 unityCoordMatrix = Matrix4x4.zero;
    private Matrix4x4 robotCoordMatrix = Matrix4x4.zero;

    private Vector3 robotCoordTemp;

    public GameObject virtualEndEffector;

    void Start()
    {
        client = new TcpClient(host_ip, host_port);
        Debug.Log("Connected to relay server");

        stream = client.GetStream();
        inChannel = new StreamReader(client.GetStream());
        outChannel = new StreamWriter(client.GetStream());


        initialPos();

        getSpeedInfo = new Thread(getInfo);

        getSpeedInfo.Start(); 
        
        StartCoroutine(RobotCoordUnityCoordCalibration());
    }

    IEnumerator RobotCoordUnityCoordCalibration()
    {
        // wait for 15 seconds before the calibration
        yield return new WaitForSeconds(15f);

        // send command to robot for the first movement and wait for 5 seconds
        customMove(0.2f,0.2f,0.2f,-0.6f,1.47f,0.62f, movementType: 1);
        yield return new WaitForSeconds(5f);
        // record unity coord and robot coord in matrix
        robotCoordMatrix[0, 0] = robotCoordTemp.x;
        robotCoordMatrix[1, 0] = robotCoordTemp.y;
        robotCoordMatrix[2, 0] = robotCoordTemp.z;
        robotCoordMatrix[3, 0] = 1f;

        unityCoordMatrix[0, 0] = virtualEndEffector.transform.position.x;
        unityCoordMatrix[1, 0] = virtualEndEffector.transform.position.y;
        unityCoordMatrix[2, 0] = virtualEndEffector.transform.position.z;
        unityCoordMatrix[3, 0] = 1f;

        // send command to robot for the second movement and wait for 5 seconds
        customMove(-4.16445f, -1.18897f, 1.90225f, -2.1865f, 1.56376f, -9.35597f, movementType: 3);
        yield return new WaitForSeconds(5f);
        // record unity coord and robot coord in matrix
        robotCoordMatrix[0, 1] = robotCoordTemp.x;
        robotCoordMatrix[1, 1] = robotCoordTemp.y;
        robotCoordMatrix[2, 1] = robotCoordTemp.z;
        robotCoordMatrix[3, 1] = 1f;

        unityCoordMatrix[0, 1] = virtualEndEffector.transform.position.x;
        unityCoordMatrix[1, 1] = virtualEndEffector.transform.position.y;
        unityCoordMatrix[2, 1] = virtualEndEffector.transform.position.z;
        unityCoordMatrix[3, 1] = 1f;

        // send command to robot for the third movement and wait for 5 seconds
        customMove(0.281548f, -0.827192f, 1.47676f, -2.17393f, 1.69551f, -8.9832f, movementType: 3);
        yield return new WaitForSeconds(5f);
        // record unity coord and robot coord in matrix
        robotCoordMatrix[0, 2] = robotCoordTemp.x;
        robotCoordMatrix[1, 2] = robotCoordTemp.y;
        robotCoordMatrix[2, 2] = robotCoordTemp.z;
        robotCoordMatrix[3, 2] = 1f;

        unityCoordMatrix[0, 2] = virtualEndEffector.transform.position.x;
        unityCoordMatrix[1, 2] = virtualEndEffector.transform.position.y;
        unityCoordMatrix[2, 2] = virtualEndEffector.transform.position.z;
        unityCoordMatrix[3, 2] = 1f;

        // send command to robot for the fourth movement and wait for 5 seconds
        customMove(-2.74635f, -2.30404f, 0.623497f, -1.38846f, -1.35079f, -3.09798f, movementType: 3);
        yield return new WaitForSeconds(5f);
        // record unity coord and robot coord in matrix
        robotCoordMatrix[0, 3] = robotCoordTemp.x;
        robotCoordMatrix[1, 3] = robotCoordTemp.y;
        robotCoordMatrix[2, 3] = robotCoordTemp.z;
        robotCoordMatrix[3, 3] = 1f;

        unityCoordMatrix[0, 3] = virtualEndEffector.transform.position.x;
        unityCoordMatrix[1, 3] = virtualEndEffector.transform.position.y;
        unityCoordMatrix[2, 3] = virtualEndEffector.transform.position.z;
        unityCoordMatrix[3, 3] = 1f;

        transMatrix = robotCoordMatrix * unityCoordMatrix.inverse;

        Debug.Log(transMatrix);

        customMove(-1.8765f, -1.22337f, 2.4f, -1.19516f, 2.06182f, -7.85783f, movementType: 3);
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
        string cmd = packCMD(xi, yi, zi, rxi, ryi, rzi, btn_press: btn, scenario: 0, speedAdopt: sA, angle5: angle);
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
        double acc = 0.3, double speed = 0.3, double blend_r = 0, double btn_press = 0, double scenario = 0, bool speedAdopt = false,
        double angle1 = 0, double angle2 = 0, double angle3 = 0, double angle4 = 0, double angle5 = 0, double angle6 = 0,
        int movementType = 0, double extra1 = 0, double extra2 = 0, double extra3 = 0, double radius = 0, int interruptible = 1) // movementType 0: jointspace linear; Type 1: toolspace linear; Type 2: circular; Type 3: jointspace linear by joint pos; Type 4: speedl
    {
        string cmd = packCMD(xi, yi, zi, rxi, ryi, rzi, acc, speed, blend_r, btn_press, scenario, speedAdopt, angle1, angle2, angle3, angle4, angle5, angle6, movementType, extra1, extra2, extra3, radius, interruptible);
        outChannel.Write(cmd);
        outChannel.Flush();
        receiveFlag = false;
    }

    private string packCMD(double Pos_x = 0.2, double Pos_y = 0.2, double Pos_z = 0.07, double Rot_x = -0.6, double Rot_y = 1.47, double Rot_z = 0.62, 
        double acc = 0.3, double speed = 0.3, double blend_r = 0, double btn_press = 0, double scenario = 0, bool speedAdopt = false,
        double angle1 = 0, double angle2 = 0, double angle3 = 0, double angle4 = 0, double angle5 = 0, double angle6 = 0,
        int movementType = 0, double extra1 = 0, double extra2 = 0, double extra3 = 0, double radius = 0, int interruptible = 1) // movementType 0: jointspace linear; Type 1: toolspace linear; Type 2: circular; Type 3: jointspace linear by joint pos
    {
        if (speedAdopt)
        {
            var distance = Vector3.Distance(new Vector3((float)prev_x, (float)prev_y, (float)prev_z), new Vector3((float)Pos_x, (float)Pos_y, (float)Pos_z));

            acc = Math.Log(1 + distance) * scale + 0.3;
            speed = Math.Log(1 + distance) * scale + 0.3;
        }

        string cmd = "(" + Pos_x + "," + Pos_y + "," + Pos_z + ","
               + Rot_x + "," + Rot_y + "," + Rot_z + ","
               + acc + "," + speed + "," + btn_press + "," + scenario + "," + 
               angle1 + "," + angle2 + "," + angle3 + "," + angle4 + "," + angle5 + "," + angle6 + "," + 
               movementType + "," + extra1 + "," + extra2 + "," + extra3 + "," + radius + "," + interruptible + ")";

        prev_x = Pos_x;
        prev_y = Pos_y;
        prev_z = Pos_z;

        robotStopped = false;

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

    private void getInfo()
    {
       fromRobot = inChannel.ReadLine();
    }

    private void Update()
    {
        getSpeedInfo.Abort();
        getSpeedInfo = new Thread(getInfo);
        getSpeedInfo.Start();

        if (fromRobot.StartsWith("R"))
        {
            DateTime dt2 = DateTime.Now;
            //Debug.Log("Returned Time: " + dt2);
            //Debug.Log("Difference: " + (dt2 - controlmanager.dt).TotalMilliseconds.ToString("F6")+"ms");
        }
        else if (fromRobot.StartsWith("p"))
        {
            var items = fromRobot.Split(new string[] { "p", "[", "]", "," }, StringSplitOptions.RemoveEmptyEntries);
            robotCoordTemp[0] = float.Parse(items[0]);
            robotCoordTemp[1] = float.Parse(items[1]);
            robotCoordTemp[2] = float.Parse(items[2]);
        }
        else if (fromRobot.StartsWith("i"))
        {
            var items = fromRobot.Split(new string[] { "i", "p", "[", "]", "," }, StringSplitOptions.RemoveEmptyEntries);

            float sp = float.Parse(items[0]) * float.Parse(items[0]) + float.Parse(items[1]) * float.Parse(items[1]) + float.Parse(items[2]) * float.Parse(items[2]);

            robotStopped = sp < 0.0001;
        }
    }

    void OnDestroy()
    {
        inChannel.Close();
        outChannel.Close();
        stream.Close();
        client.Close();
        Debug.Log("Client close");
    }

    public Vector3 convertUnityCoord2RobotCoord(Vector3 p1)
    {
        Vector3 new_p = transMatrix.MultiplyPoint3x4(p1);

        return new_p;
    }
}
