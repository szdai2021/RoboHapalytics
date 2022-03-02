using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityVicon;

public class posReplay : MonoBehaviour
{
    // Public
    [Header("Recorder Object")]
    public GameObject VrHand;
    public GameObject VrSliderKnob;
    public GameObject VrYellowMarker;
    public UserStudyControl usc;

    [Header("Play Setting")]
    public bool startFlag = false;
    public TextAsset fileName;

    public bool pause = false;
    public int slowByFrame;

    [HideInInspector]
    private StreamReader sr;
    private bool prev_startFlag;

    private string stringLine;
    private int frameCounter = 0;

    // Update is called once per frame
    void Update()
    {
        if (startFlag)
        {
            usc.enabled = false;
            VrHand.GetComponent<RBScript>().enabled = false;

            if (!prev_startFlag)
            {
                sr = new StreamReader("Assets/Recordings/" + fileName.name + ".txt");

                stringLine = sr.ReadLine();
                frameCounter = 0;
            }

            if (sr != null)
            {
                if (!sr.EndOfStream)
                {
                    if (!pause & frameCounter > slowByFrame)
                    {
                        stringLine = sr.ReadLine();
                        string[] strings = stringLine.Split('\t');

                        VrHand.transform.localPosition = new Vector3(float.Parse(strings[1]), float.Parse(strings[2]), float.Parse(strings[3]));
                        VrSliderKnob.transform.localPosition = new Vector3(float.Parse(strings[4]), float.Parse(strings[5]), float.Parse(strings[6]));
                        VrYellowMarker.transform.localPosition = new Vector3(float.Parse(strings[7]), float.Parse(strings[8]), float.Parse(strings[9]));
                        
                        frameCounter = 0;
                    }
                }
                else
                {
                    startFlag = false;
                }
            }

            frameCounter++;
        }
        else
        {
            usc.enabled = true;
            VrHand.GetComponent<RBScript>().enabled = true;
        }

        prev_startFlag = startFlag;
    }

}
