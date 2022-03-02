using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class PosRecorder : MonoBehaviour
{
    // Public
    [Header("Recorder Object")]
    public GameObject VrHand;
    public GameObject VrSliderKnob;
    public GameObject VrYellowMarker;

    [Header("Save Setting")]
    public bool startFlag = false;
    public int participantID;
    public int scenario;

    [HideInInspector]
    private string fileName;
    private StreamWriter sw;

    private bool prev_startFlag;

    // Update is called once per frame
    void Update()
    {
        if (startFlag)
        {
            if (!prev_startFlag)
            {
                fileName = "HapticSlider_ObjectPosLog_" + DateTime.Now.ToString("yyyy-MM-dd") + "_P" + participantID.ToString() + "_S" + scenario.ToString();
                string saveFileName = "Assets/Recordings/" + fileName + ".txt";

                sw = new StreamWriter(saveFileName);

                sw.WriteLine("TimeStamp\tHandX\tHanY\tHandZ\tKonbX\tKonbY\tKonbZ\tMarkerX\tMarkerY\tMarkerZ");
            }

            if (sw != null)
            {
                sw.WriteLine(
                    DateTime.Now.ToString("yyyy-MM-dd\\THH:mm:ss\\Z") + "\t" +
                    VrHand.transform.localPosition.x + "\t" +
                    VrHand.transform.localPosition.y + "\t" +
                    VrHand.transform.localPosition.z + "\t" +
                    VrSliderKnob.transform.localPosition.x + "\t" +
                    VrSliderKnob.transform.localPosition.y + "\t" +
                    VrSliderKnob.transform.localPosition.z + "\t" +
                    VrYellowMarker.transform.localPosition.x + "\t" +
                    VrYellowMarker.transform.localPosition.y + "\t" +
                    VrYellowMarker.transform.localPosition.z + "\t"
                    );
            }
        }

        if (prev_startFlag & !startFlag)
        {
            sw.Close();
        }

        prev_startFlag = startFlag;
    }
}
