using System;
using System.Collections;
using System.IO.Ports;
using System.Threading;
using UnityEngine;
using UnityVicon;
public class WirelessAxes : MonoBehaviour
{
    public string COM = "COM6";
   
    public int sliderOne;
    public int sliderTwo;
    public int rotary;
    public int rotarypress;
    public int buttonPress;
    private int oldSliderOne;
    private int oldSliderTwo;
    bool connected;
    bool firstRead;   // Added these - to reset rotary to zero on if axes not repowered.
    int rotaryDifference;
    
    int modeSelect = 0;
   
    public bool joystickMode = true;
    bool followMode;
    public int followDistanceOverride;
    //////////// these are only needed for testing with the keypresses. feel free to delete in order to clean things up. 
    [Range(0, 256)]
    public int LEDValue = 0;
    [Range(0, 600)]
    public int followDist;
    [Range(0, 256)]
    public int sendingSliderOne;
    [Range(0, 256)]
    public int sendingSliderTwo;
    [Range(0, 256)]
    public int hapticsValue1 = 200; 
    [Range(0, 256)]
    public int hapticsValue2 = 200;

    private int sliderTemp = 127;
    private int buttonTemp = 0;
    private float virtualSliderTemp;

    public UnityClient unity_client;
    public GameObject sliderNode;
    public GameObject endEffector;

    private Vector3 tempEndEffector = new Vector3(0, 0, 0);
    private int directionFlag = 0;

    private float slidervalue = 0;

    private int counter = 0;
    public int frame_threshold = 20;
    private bool resetFlag = true;
    private bool startflag = false;
    private bool extraFlag = true;

    private int sliderBufferOne = 55;
    private int sliderBufferTwo = 200;
    private int sliderBufferThree = 10;
    private int sliderBufferFour = 250;
    private int checkerFrameRate = 30;
    private int sliderSpeed;

    private float virtualSliderLimitLeft = 2.5f;
    private float virtualSliderLimitRight = -2.5f;

    private bool sliderMoveFlag = false;

    private Vector3 tempEndEffectorPos = new Vector3(0, 0, 0);

    public GameObject sliderKnobLeft;
    public GameObject sliderKnobRight;

    public GameObject theAxisModel;
    public GameObject UI_Parent;
     
    public float palmThreshold = 0.01f;
    /// ////////////////////////////////////////////


    SerialPort sp;
    Thread ReadThread;
    Thread CheckPortThread;
    void Start()
    {
        //ReadThread = new Thread(new ThreadStart(ReadSerial));
        //ReadThread.Start();
        sp = new SerialPort(COM, 115200);
        sp.ReadTimeout = 2000;
        sp.WriteTimeout = 2000;
        sp.WriteTimeout = 2000;
        sp.Parity = Parity.None;
        sp.DataBits = 8;
        sp.StopBits = StopBits.One;
        sp.RtsEnable = true;
        sp.Handshake = Handshake.None;
        sp.NewLine = "\n";  // Need this or ReadLine() fails

        try
        {
            sp.Open();
        }
        catch (SystemException f)
        {
            print("FAILED TO OPEN PORT");
            
        }
        if (sp.IsOpen)
        {
            print("SerialOpen!");

            ReadThread = new Thread(new ThreadStart(ReadSerial));
            ReadThread.Start();
            setSteppedMode(0);
            //  SetJoystickMode(6);
            setLEDValue(255);
        }
        else
        {
           // StartCoroutine(CheckPort());
        }
        //sendSlider(1, 255);
        // sendSlider(2, 0);

        sendSlider(2, 127);
    }

    void TryPort()
    {
        print("CAlled");
        try
        {
            sp.Open();
        }
        catch (SystemException f)
        {
            print("FAILED TO OPEN PORT");

        }
        if (sp.IsOpen)
        {
            print("SerialOpen!");

            ReadThread = new Thread(new ThreadStart(ReadSerial));
            ReadThread.Start();
            setSteppedMode(0);
            //  SetJoystickMode(6);
            setLEDValue(255);
        }
        else
        {
                
            StartCoroutine(CheckPort());
        }
    }
    IEnumerator CheckPort()  // Ignore
    {
        yield return new WaitForSeconds(1f);
        CheckPortThread = new Thread(new ThreadStart(TryPort));
        CheckPortThread.Start();


    }
    public void newSendMsg(int mode, int value1)
    {
        string value1String = value1.ToString();
        

        if (value1 >= 0 && value1 <= 9)
        {
            value1String = "00" + value1String;
        }
        else if (value1 >= 10 && value1 <= 99)
        {
            value1String = "0" + value1String;
        }
        else if (value1 >= 100 && value1 <= 999)
        {
            value1String = "" + value1String;
        }

        

        string message = mode.ToString() + value1String;                                                                                                                      
        try
        {
            sp.WriteLine(message);
        }
        catch (SystemException f)
        {
            print("ERROR::A message failed to send");
        }


    }

    void ReadSerial()
    {
        while (ReadThread.IsAlive)
        {
            try
            {
                if (sp.BytesToRead > 1)
                {

                    string indata = sp.ReadLine();


                    string[] splits = indata.Split(' ');
                    rotarypress = int.Parse(splits[3]);
                    if (rotarypress == 1) rotarypress = 0;
                    else rotarypress = 1;
                    buttonPress = int.Parse(splits[4]);

                    if (rotarypress != 1 && buttonPress != 1)
                    {
                        sliderOne = int.Parse(splits[1]);
                        oldSliderOne = sliderOne;
                        if (!followMode)        ////////////////////dodgy FollowModeCheat!
                        {
                            sliderTwo = int.Parse(splits[0]);
                            oldSliderTwo = sliderTwo;
                        }
                        else
                        {
                            sliderTwo = sliderOne - followDistanceOverride;
                            oldSliderTwo = sliderTwo;
                        }
                    }
                    else
                    {
                        sliderOne = oldSliderOne; // Prevents slidervalues being changed on button press
                        sliderTwo = oldSliderTwo;
                    }
                    if (!firstRead)  // Makes rotary 0 on first run regardless of resetting axes.
                    {
                        firstRead = true;
                        rotaryDifference = int.Parse(splits[2]);

                    }
                    rotary = int.Parse(splits[2]) - rotaryDifference;

                    
                  
                }
            }
            catch (SystemException f)
            {
                print(f);
                ReadThread.Abort();
            }
        }
    }

    public void speedTest(int sliderchange)
    {
        unity_client.sliderTest_OnChange((double)sliderchange, 0);
        sendSlider(0, sliderOne - sliderchange);
    }

    public void startSlider()
    {
        sendSlider(0, 127);
        sliderTemp = 127;
        slidervalue = 0;
        //startflag = !startflag;
        startflag = false;
        tempEndEffector = endEffector.transform.position;
        UI_Parent.GetComponent<RBScript>().enabled = false;
    }

    public void resetSlider()
    {
        sendSlider(0, 127);
        slidervalue = 0;
        sliderNode.transform.localPosition = new Vector3(sliderNode.transform.localPosition.x, 0, sliderNode.transform.localPosition.z);
        sliderTemp = sliderOne;
        unity_client.resetRobot();
    }

    void Update()  // For easy testing only! can be deleted if wishing to tidy things up. 
    {
        /*
        if (startflag)
        {
            slidervalue = sliderNode.transform.localPosition.y;
            if (counter >= frame_threshold && true)
            {
                if (Math.Abs(slidervalue - virtualSliderTemp) > 0.005 || buttonPress != buttonTemp)
                {
                    unity_client.sliderTest_OnChange((double)((slidervalue - virtualSliderTemp) * 200), buttonPress);
                    sendSlider(0, 127);
                    virtualSliderTemp = slidervalue;
                    counter = 0;
                }
            }
            counter++;
        }
        */
        
        if (startflag)
        {
            sliderSpeed = sliderTemp - sliderOne;
            //textHolder.GetComponent<TextMesh>().text = sliderSpeed.ToString();
            var virtualSliderChanges = 0;

            if ((sliderOne < sliderBufferOne || sliderOne > sliderBufferTwo) && resetFlag)
            {
                sliderMoveFlag = true;
                //virtualSliderChanges -= sliderOne - 127;
                //textHolder.GetComponent<TextMesh>().text = Math.Abs(sliderSpeed).ToString();
                if(unity_client.sliderTest_OnChange((double)(sliderOne - 127), buttonPress, sliderSpeed*2) == 0)
                {
                    sendSlider(0, 127);
                }
                resetFlag = false;
            }

            /*
            if (Vector3.Distance(tempEndEffectorPos, endEffector.transform.position) <= 0.001 && !resetFlag && extraFlag)
            {
                sendSlider(0, 127);
                unity_client.sliderTest_OnChange((double)(sliderOne - 127), buttonPress, sliderSpeed*2);
                extraFlag = false;
            }
            */
          
            if ((sliderOne < sliderBufferThree) & extraFlag)
            {
                sliderMoveFlag = true;
                if(unity_client.sliderTest_OnChange((double)(-10 - 127), buttonPress, 60) == 0)
                {
                    sendSlider(0, 127);
                }
                extraFlag = false;
            }
            else if ((sliderOne > sliderBufferFour) & extraFlag)
            {
                sliderMoveFlag = true;
                if(unity_client.sliderTest_OnChange((double)(265 - 127), buttonPress, -60) == 0)
                {
                    sendSlider(0, 127);
                }
                extraFlag = false;
            }

            /*
            if (counter >= frame_threshold && true)
            {
                if (Math.Abs(sliderOne - 127) > 30 || buttonPress != buttonTemp)
                {
                    unity_client.sliderTest_OnChange((double)(sliderOne - 127), buttonPress);
                    sendSlider(0, 127);

                    if (sliderOne - 127 > 0)
                    {
                        directionFlag = 1;
                    }
                    else if (sliderOne - 127 < 0)
                    {
                        directionFlag = 2;
                    }

                    //sliderTemp = sliderOne;
                    //buttonTemp = buttonPress;
                    virtualSliderChanges -= sliderOne - 127;
                    //changeVirtualSlider(virtualSliderChanges);
                    //virtualSliderChanges = 0;
                }

                if (Math.Abs(sliderOne - 127) < 30)
                {
                    directionFlag = 0;
                }
                counter = 0;
            }
            counter++;
            */

            /*
            switch (directionFlag)
            {
                case 1:
                    virtualSliderChanges += (int)Vector3.Distance(endEffector.transform.position, tempEndEffector);
                    break;
                case 2:
                    virtualSliderChanges -= (int)Vector3.Distance(endEffector.transform.position, tempEndEffector);
                    break;
                default:
                    break;
            }
            */

            if (sliderOne < sliderBufferTwo && sliderOne > sliderBufferOne)
            {
                resetFlag = true;
                sliderMoveFlag = false;
                extraFlag = true;
            }         

            virtualSliderChanges += sliderTemp - sliderOne;

            updateVirtualSlider();

            sliderTemp = sliderOne;
            buttonTemp = buttonPress;

            tempEndEffectorPos = endEffector.transform.position;

            tempEndEffector = endEffector.transform.position;
            //textHolder.GetComponent<TextMesh>().text = rightHand.GetComponent<CapsuleHand>().tempPalm.x.ToString();
        }
    }

    private void updateVirtualSlider()
    {
        float newY;

        newY = theAxisModel.transform.position.z * (-9f) - 3.4f
                + (sliderOne/255f)*0.5f + 0.1f;

        sliderNode.transform.localPosition = new Vector3(0f, newY, 0f);
    }

    public void setSteppedMode(int steppedRange) // between 10 and 128 else turns stepped off
    {
        newSendMsg(4, steppedRange);
    }

    public void setLEDValue(int value)  // 0 to 256
    {
        newSendMsg(8, value);
        LEDValue = value;
    }

    public void sendSlider(int slider, int targetValue) // 0 to 256, slider 1 or 2
    {
        if (slider == 1)
        {
           newSendMsg(0, targetValue); 
        }
        else
        {
            newSendMsg(1, targetValue);
        }
    }
    public void FollowModeChange(int distance) 
    {
        if (distance < 512)
        {
            followMode = true;
           // followDistanceOverride = 128 - distance;
        }
        else
        {
            followMode = false;
        }
           newSendMsg(2, distance);  //0-256 one follows 2. 257 - 512, two follows  one, > follow off.  0-128 is neg, 128 - 256 positive
    }
    public void TurnOffFollowMode()
    {
        newSendMsg(2, 999);
    }

    public void SetJoystickMode(int mode)  // 1 = on on. 2 = 2 on. 3 = both on. 4 = 1 off. 5 = 2 off. 6 = both off. 
    {
        newSendMsg(3, mode );
    }
    
    public void hapticPulse(int slider, int value) // 0 -256
    {
        if (slider == 1)
        {
            newSendMsg(6, value);
        }
        else
        {
            newSendMsg(7, value);
        }
    }

    void OnApplicationQuit()
    {
        ReadThread.Abort();
    }

    public void changeRealSlider(float changes)
    {
        sendSlider(0, (int)changes*200);
    }
}
