using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.IO;
using System.Web;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UI;

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

    public GameObject VRController;
    public GameObject Virtual_end_effector;
    public GameObject RobotActualTCPIndicator;
    public GameObject Movement_indicator; //assigned a cube of different color to Virtual_end_effector


    public GameObject UR3;
    public GameObject[] joint_links;

    public GameObject endEffector;

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

    // Start is called before the first frame update

    
    private const double xLimitRight = 0.099;
    private const double yLimitRight = 0.37;

    private const double xLimitLeft = 0.45;
    private const double yLimitLeft = 0.0136;

    /*
    private double x = (xLimitLeft + xLimitRight)/2;
    private double y = (yLimitLeft + yLimitRight)/2;
    public double z = 0.07;
    public double rx = -0.6;
    public double ry = 1.47;
    public double rz = 0.62;
    */

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

    private string angleCMD;

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

        Debug.Log("Wait for host to response");

        client = new TcpClient(host_ip, host_port);
        Debug.Log("Connected to relay server");
        stream = client.GetStream();
        inChannel = new StreamReader(client.GetStream());
        outChannel = new StreamWriter(client.GetStream());

        new Thread(new ThreadStart(recvJointStateLoop)).Start();
        StartCoroutine(executeJointTrajectory());

        //circularMove(0.085512,0.397466,0.316909,0.701469,1.82303,1.77534,0);
        sliderTest_OnChange(0, 0);

        posCalibration();
    }

    private void posCalibration()
    {
        var A = 0;
    }

    public void activate(GameObject virtual_plane_on_tcp)
    {
        controller_endEffector_offset = Virtual_end_effector.transform.position - VRController.transform.position;
        previous_controller_pos = VRController.transform.position;
        //End_effector_virtual_plane = virtual_plane_on_tcp;
        previous_ee_orientation = virtual_plane_on_tcp.transform.rotation;

        desired_pos = virtual_plane_on_tcp.transform.position;
        Virtual_end_effector.transform.position = desired_pos;
        //Virtual_end_effector.transform.rotation = End_effector_virtual_plane.transform.rotation;

       /* desired_orientation_q =  Virtual_end_effector.transform.rotation * Quaternion.identity;
        Virtual_end_effector.transform.rotation = desired_orientation_q;*/
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

    public void customMove(double xi, double yi, double zi, double rxi, double ryi, double rzi,
        double acc = 0.3, double speed = 0.3, double blend_r = 0, double btn_press = 0, double scenario = 0, bool speedAdopt = false, double angle_bias = 0, int joint_index = 5,
        int movementType = 0, double extra1 = 0, double extra2 = 0, double extra3 = 0, double radius = 0) // movementType 0: jointspace linear; Type 1: toolspace linear; Type 2: circular; Type 3: jointspace linear by joint pos
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

    public void interact(GameObject virtual_plane_on_tcp)
    {
        
        if (posQueue.Count > 0)
        {
            cur_tcp_pos = posQueue[posQueue.Count - 1];
            posQueue.Clear();
            print("cur_tcp_pos is not null: " + cur_tcp_pos);
        }

        if (cur_tcp_pos == null)
        {
            cur_tcp_pos = new float[6] { 
                Virtual_end_effector.transform.position.x,
                Virtual_end_effector.transform.position.y,
                Virtual_end_effector.transform.position.z,
                Virtual_end_effector.transform.rotation.eulerAngles.x,
                Virtual_end_effector.transform.rotation.eulerAngles.y,
                Virtual_end_effector.transform.rotation.eulerAngles.z,
            };
        }
        
        Vector3 RobotPos = Robot2UnityCoor(new Vector3(cur_tcp_pos[0], cur_tcp_pos[1], cur_tcp_pos[2]));
        RobotActualTCPIndicator.transform.position = RobotPos;

        virtual_plane_on_tcp.transform.position = VRController.transform.position + controller_endEffector_offset;
        virtual_plane_on_tcp.transform.rotation = VRController.transform.rotation;
        controller_pos = VRController.transform.position;
        Vector3 actual_movement = controller_pos - previous_controller_pos;

        Quaternion rotation_diff = virtual_plane_on_tcp.transform.rotation * Quaternion.Inverse(previous_ee_orientation);
        desired_orientation_q = rotation_diff * Virtual_end_effector.transform.rotation;
        desired_orientation_e = virtual_plane_on_tcp.transform.rotation.eulerAngles;

        desired_pos = virtual_plane_on_tcp.transform.position;

        Virtual_end_effector.transform.position = desired_pos;
        Virtual_end_effector.transform.rotation = desired_orientation_q;

        //Virtual_end_effector.transform.rotation = desired_orientation_q;

        if (actual_movement.magnitude > 0.01) //0.01, 0.008
        {
            /*Vector3 actual_tcp_movement =  (VRtracker_TCP_offset + tracker_pos) - previous_tcp_pos;
            Vector3 wanted_tcp_movement = actual_tcp_movement / 10;
            desired_pos = previous_tcp_pos + wanted_tcp_movement;*/

            Movement_indicator.transform.position = desired_pos;
            Movement_indicator.transform.rotation = desired_orientation_q;

            string cmd = packCommand(desired_pos, desired_orientation_q, actual_movement.magnitude);
            //Debug.Log("pos sent to relay server: " + cmd);
            outChannel.Write(cmd);
            outChannel.Flush();

            previous_controller_pos = controller_pos;
            // previous_tcp_pos = desired_pos;

        }
        previous_ee_orientation = virtual_plane_on_tcp.transform.rotation;
        /*}*/
        


        //transform.position = desired_pos;

        diaplayControllerPosInfo();
    }

    private IEnumerator executeJointTrajectory()
    {
        while (true)
        {
            while (trajectoryQueue.Count > 0)
            {
                Action executeOneJointState = trajectoryQueue[trajectoryQueue.Count - 1];
                //trajectoryQueue.RemoveAt(0);
                trajectoryQueue.Clear();

                //Debug.Log("Trajectory Execute");
                executeOneJointState();
                //yield return null;

                //Action executeOneAngleMove = angularQueue[angularQueue.Count - 1];
                //angularQueue.Clear();

                //executeOneAngleMove();
            }
            yield return null;
        }

    }


    // Update is called once per frame
    private void recvJointStateLoop()
    {
        while (true)
        {
            float[] res;
            string temp = Recv6Tuple(inChannel, out res);

            if (temp.StartsWith("p"))
            {
                //print("recv pos info: " + temp);
                posQueue.Add(res);
                //print("pos result: " + res[0] + "," + res[1] + "," + res[2] + "," + res[3] + "," + res[4] + "," + res[5]);
            }
          /*  res[2] += 263.208458;
            res[3] += 83.968;*/
            //Debug.Log("JOINT STATE RECV");
           
            else
            {
                jointAngles = res;
                //print("joint result in rad: " + res[0] + "," + res[1] + "," + res[2] + "," + res[3] + "," + res[4] + "," + res[5]);
                Parallel.For(0, 6, i => {
                    res[i] = rad2deg(res[i]);
                }); 
                //print("recv joint state info: " + temp);
                //print("joint result in deg: " + res[0] + "," + res[1] + "," + res[2] + "," + res[3] + "," + res[4] + "," + res[5]);
                Action executeOneJointState = () =>
                {
                    for (int i = 0; i < res.Length; i++)
                    {
                        move(joint_links[i], i, res[i]);
                    }
                };

                trajectoryQueue.Add(executeOneJointState);
                receiveFlag = true;

                //Action executeOneAngleMove = () =>
                //{
                //    outChannel.Write(angleCMD);
                //    outChannel.Flush();
                //    receiveFlag = false;
                //};
                //angularQueue.Add(executeOneAngleMove);

            }

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

    private string Recv6Tuple(StreamReader inChannel, out float[] result)
    {
        string res = inChannel.ReadLine();
        //res = HttpUtility.UrlDecode(res, Encoding.UTF8);
        //print("Res was " + res);
       // Debug.Log(res);
        string temp = res.Trim(new char[] { '[', ']', 'p' });
        //Debug.Log(res);
        if (temp.Contains("p"))
        {
            //print("invalid data");
            int charLocation = temp.IndexOf("]", StringComparison.Ordinal);

            if (charLocation > 0)
            {
                temp = temp.Substring(0, charLocation);
            }
        }
        string[] each_double = temp.Split(',');
        result = new float[6];
        int i = 0;
        foreach (var each in each_double)
        {
            /*print("Parsing");*/
            //print("each was  "  + each);
            result[i] = float.Parse(each);
            //print("pose result was " +pose_result[i]);
           // no need if return 6 tuple has p[]
           // print("Pose result is " + pose_result[i]);
            //Debug.Log(pose_result[i]);
            i++;
        }
        return res;
    }

    private void move(GameObject joint_link, int of_index, float to)
    {
        char move_which_axis;
        float offset;
        int direction;
        if (link_index_to_axis_mapper.TryGetValue(of_index, out move_which_axis) && link_offset.TryGetValue(of_index, out offset) && link_direction.TryGetValue(of_index, out direction))
        {
            Vector3 rotationVector = new Vector3();

            //joint_link.transform.localEulerAngles;

            //print("rotating joint of index: " + of_index + " along: " + move_which_axis + " axis, to degree: " + to);
            if (move_which_axis == 'x')
            {
                rotationVector = new Vector3(((to * direction) + offset) % 360, joint_link.transform.localEulerAngles.y, joint_link.transform.localEulerAngles.z);
            }
            else if (move_which_axis == 'y')
            {
                rotationVector = new Vector3(joint_link.transform.localEulerAngles.x, ((to * direction) + offset) % 360, joint_link.transform.localEulerAngles.z);
            }
            else if (move_which_axis == 'z')
            {
                rotationVector = new Vector3(joint_link.transform.localEulerAngles.x, 0, ((to * direction) + offset) % 360);
                //print(joint_link.transform.localEulerAngles.y);
            }

            if (of_index == 3)
            {
                rotationVector.y = 0f;
                rotationVector.z = 0f;
            }

            //joint_link.transform.localRotation = Quaternion.Euler(rotationVector);
            joint_link.transform.localEulerAngles = rotationVector;
        }
        else
        {
            Debug.LogError("cannot find the joint to move !!!");
        }
    }

    private string packCommand(Vector3 desired_pos, Quaternion desired_orientation, float movement_length)
    {

        Vector3 FLU = Unity2RobotCoor(desired_pos);

        Vector3 axisAngle = Quaternion2axisAngle(desired_orientation);
        /*float angle;
        Vector3 axis;
        

        desired_orientation.ToAngleAxis(out angle, out axis);
        axisAngle = new Vector3(axis.z, -axis.x, axis.y) * angle;*/
        float t = timeBasedOnMove(movement_length); //0.07f;
        float lk_ahead_t = lkatBasedOnMove(movement_length); //0.05f; 

        if (t < 0f)
        {
            t = 0f;
        }
        else if(t > 0.02f)
        {
            t = 0.02f;
        }

        t = 0.2f;

        if (lk_ahead_t < 0.03f)
        {
            lk_ahead_t = 0.03f;
        }else if(lk_ahead_t > 0.2f)
        {
            lk_ahead_t = 0.2f;
        }

        lk_ahead_t = 0.1f;

        //Debug.Log("movement is " + movement_length + "t: " + t + "lk_ahead_t: " + lk_ahead_t);

         string pose_6_tuple = "(" + FLU.x + "," + FLU.y + "," + FLU.z + ","
            + axisAngle.x + "," + axisAngle.y + "," + axisAngle.z+ ","
            + 1 + "," + 0.5 + "," + 0 + ")";
            //+t + "," + lk_ahead_t + ")";

        return pose_6_tuple;
    }

    private float timeBasedOnMove(float movement)
    {
        return (float)movement * 6f+0.07f;
    }

    private float lkatBasedOnMove(float movement)
    {
        return (float)movement * -3.5f + 0.225f;
    }

    private float blocking_time_bound(float x)
    {
        return (float) Math.Log(3 * x + 1); //find the base of log?
    }

   /* private string packCommand(Vector3 desired_pos, Vector3 desired_orientation)
    {

        double x = desired_pos.z;
        double y = -desired_pos.x;
        double z = desired_pos.y;

        Vector3 axisAngle = Eular2axisAngle(desired_orientation.x
            , desired_orientation.y
            , desired_orientation.z);

        string pose_6_tuple = "(" + x + "," + y + "," + z + ","
            + axisAngle.x + "," + axisAngle.y + "," + axisAngle.z + ")"; //]n
        return pose_6_tuple;
    }*/
    private float rad2deg(double rad)
    {
    /*    Debug.Log("was: " + rad);
        Debug.Log("is " + (float)(180 * rad / Math.PI));*/
        return (float)(180 * rad / Math.PI);

    }


    private Vector3 Robot2UnityCoor(Vector3 FLU) //FLU 2 RUF
    {

        return new Vector3(-FLU.y, FLU.z ,FLU.x);
    }

    private Vector3 Unity2RobotCoor(Vector3 RUF) //RUF 2 FLU
    {
        return new Vector3(RUF.z, -RUF.x, RUF.y);
    }

    private Vector3 Eular2axisAngle(double theta, double phi, double psi) //RUF 2 FLU conversion is done
    {
        double c1 = Math.Cos(theta / 2);
        double s1 = Math.Sin(theta / 2);

        double c2 = Math.Cos(phi / 2);
        double s2 = Math.Sin(phi / 2);

        double c3 = Math.Cos(psi / 2);
        double s3 = Math.Sin(psi / 2);

        double w = c1 * c2 * c3 - s1 * s2 * s3;
        double x = c1 * c2 * s3 + s1 * s2 * c3;
        double y = s1 * c2 * c3 + c1 * s2 * s3;
        double z = c1 * s2 * c3 - s1 * c2 * s3;
        double angle = 2 * Math.Acos(w);
        double norm = x * x + y * y + z * z;
        if (norm < 0.001)
        {
            x = 1;
            y = z = 0;
        }
        else
        {
            norm = Math.Sqrt(norm);
            x /= norm;
            y /= norm;
            z /= norm;
        }

        return new Vector3((float)z, (float)-x, (float)y) * (float)angle;
    }

    private Vector3 Quaternion2axisAngle(Quaternion orientation) //RUF 2 FLU conversion is done
    {
        if (orientation.w > 1) orientation.Normalize();
        double angle = 2 * Math.Acos(orientation.w);
        double s = Math.Sqrt(1 - orientation.w * orientation.w);

        double x = orientation.x;
        double y = orientation.y;
        double z = orientation.z;

        if (s >= 0.001)
        {
            x /= s;
            y /= s;
            z /= s;
        }
        Vector3 axisAngle = new Vector3((float)z, (float)-x, (float)y) * (float)angle;
        //Debug.Log("orientation conversion result: " + axisAngle);
        //should this be converted to radian?
        return axisAngle;
    }


    private void diaplayControllerPosInfo()
    {
        trackerPos_x = controller_pos.x;
        trackerPos_y = controller_pos.y;
        trackerPos_z = controller_pos.z;
    }
}
