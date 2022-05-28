using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SliderDemo : MonoBehaviour
{
    public TMP_Text d1_text;
    public TMP_Text d2_text;

    public SerialInOut shortSliderInOut;
    public SerialInOut longSliderInOut;

    public UnityClient unity_client;

    public GameObject sliderKnob1;
    public GameObject sliderKnob2;

    public GameObject[] hiddenObject;
    public GameObject sliderKnobReference;
    public GameObject realSliderReference;

    public GameObject shortSliderTracker;

    public Collider leftChecker;
    public Collider rightChecker;

    private int dynamicCounter = 0;
    private bool dynamicLeftEnd = false;
    private bool dynamicRightEnd = false;
    private bool dynamicMoving = false;
    private float speedScale = 7.0f;

    private float ax;
    private float ay;
    private float az;

    private float norm;

    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject g in hiddenObject)
        {
            HideObject(g);
        }

        ax = 0.41173f - 0.028987f;
        ay = -0.00627f - 0.37663f;
        az = 0.0476f - 0.0430782f;

        norm = Mathf.Sqrt(ax * ax + ay * ay + az * az);

        unity_client.customMove(0.23, 0.17593, 0.045218, -0.6, 1.5, 0.62, movementType: 0);
    }

    // Update is called once per frame
    void Update()
    {
        d1_text.text = ((-longSliderInOut.value + 1764f) * 0.4).ToString() + " " + "mm";
        d2_text.text = ((sliderKnob2.transform.localPosition.y + 2.918f)*120.4379562f).ToString("F1") + " " + "mm";

        sliderKnobReference.transform.position = realSliderReference.transform.position;

        moveRobotDynamic(150, 250);
        virtualKnobUpdateFromRobotAxis();

        virtualKnobUpdateFromVrPhysicalSlider();
    }

    public void HideObject(GameObject obj, bool hideFlag = false)
    {
        Renderer[] objectR = obj.GetComponentsInChildren<Renderer>();

        foreach (Renderer rr in objectR)
        {
            rr.enabled = hideFlag;
        }

    }

    private void virtualKnobUpdateFromVrPhysicalSlider()
    {
        sliderKnob1.transform.localPosition = new Vector3(sliderKnob1.transform.localPosition.x, (float)(2.928f - ((float)longSliderInOut.value / 1764.0) * (2.928f - -2.918f)), sliderKnob1.transform.localPosition.z);
    }

    private void virtualKnobUpdateFromRobotAxis()
    {
        // update slider
        sliderKnob2.transform.localPosition = new Vector3(sliderKnob2.transform.localPosition.x, sliderKnobReference.transform.localPosition.y + -1 * (float)(shortSliderInOut.value - 418) / 418 * 1.37f, sliderKnob2.transform.localPosition.z);
    }

    private void moveRobotDynamic(int bufferOne, int bufferTwo)
    {
        //if (Vector3.Distance(shortSliderTracker.transform.position, sliderLeftReference) < leftCheck)
        if (leftChecker.bounds.Contains(shortSliderTracker.transform.position))
        {
            if (!dynamicLeftEnd)
            {
                unity_client.stopRobot();
                shortSliderInOut.SetSlider(0);
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
                shortSliderInOut.SetSlider(0);
            }
            dynamicRightEnd = true;
            dynamicMoving = false;
        }
        else
        {
            dynamicRightEnd = false;
        }

        if (dynamicCounter > 9)
        {
            if (shortSliderInOut.value < bufferOne | shortSliderInOut.value > bufferTwo)
            {
                dynamicMoving = true;
                float sp = (shortSliderInOut.value - 415.0f / 2.0f) / (415.0f / 2.0f) * 0.0261f * speedScale * ((shortSliderInOut.value - 415.0f / 2.0f) / (415.0f / 2.0f) * (shortSliderInOut.value - 415.0f / 2.0f) / (415.0f / 2.0f) + 1);

                if ((shortSliderInOut.value < bufferOne & !dynamicRightEnd) | (shortSliderInOut.value > bufferTwo & !dynamicLeftEnd))
                {
                    unity_client.customMove(ax / (Mathf.Round(norm / sp * 100) / 100), ay / (Mathf.Round(norm / sp * 100) / 100), az / (Mathf.Round(norm / sp * 100) / 100), -0.6, 1.47, 0.62, speed: sp, acc: 1.5f, movementType: 4);

                    if (shortSliderInOut.value < bufferOne & !dynamicRightEnd)
                    {
                        shortSliderInOut.SetSlider((int)(Mathf.Abs(shortSliderInOut.value - 415.0f / 2.0f) / (415.0f / 2.0f) * (120) + 190));
                    }

                    if (shortSliderInOut.value > bufferTwo & !dynamicLeftEnd)
                    {
                        shortSliderInOut.SetSlider((int)(Mathf.Abs(shortSliderInOut.value - 415.0f / 2.0f) / (415.0f / 2.0f) * (-120) - 190));
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
                    shortSliderInOut.SetSlider(0);
                }
            }
        }

        dynamicCounter++;
    }

}
