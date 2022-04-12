using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using TMPro;

public class lineGraphManager : MonoBehaviour
{
    public GameObject plotParent;
    public GameObject smallPoint;
    public GameObject linePrefb;
    public GameObject pointPrefb;
    public GameObject detailPanel;

    public GameObject dotLine;
    public GameObject knobReference;
    public GameObject dotLineReference;

    public GameObject virtualFingerTouchPoint;
    public GameObject virtualEndEffector;
    public GameObject controlller;

    public SerialInOut shortInOut;
    public UnityClient unity_client;

    public GameObject[] objectsToHide;

    public Collider leftChecker;
    public Collider rightChecker;

    public GameObject shortSliderTracker;

    public GameObject sliderKnobReference;
    public GameObject realSliderReference;

    private int dynamicCounter = 0;
    private bool dynamicLeftEnd = false;
    private bool dynamicRightEnd = false;
    private bool dynamicMoving = false;
    private float speedScale = 7.0f;

    private float ax;
    private float ay;
    private float az;

    private float norm;

    public float controllerMax;
    public float controllerMin;

    public string selectedCountry = "China";

    public string filename = "income_per_person_gdppercapita_ppp_inflation_adjusted.csv";
    public string filename2 = "life_expectancy_years.csv";

    private Dictionary<string, Dictionary<int, float>> data1 = new Dictionary<string, Dictionary<int, float>>(); // country - year - data
    private Dictionary<string, Dictionary<int, float>> data2 = new Dictionary<string, Dictionary<int, float>>(); // country - year - data

    private GameObject[] pointList;

    private int countryIndex = 0;

    public int indexMax = 11;
    public int indexMin = 0;

    public int counter = 100;

    private int yScale = 80000000;

    private float barGap = 0.8f;

    public bool flag = false;
    private bool prev_flag = false;

    public TMP_Text countryText;
    public TMP_Text yearText;
    public TMP_Text incomeText;
    public TMP_Text lifeExpText;

    public Collider Tcollider;
    public GameObject finger;

    private float localScale = 0.07f;
    private Vector3 newLocation = new Vector3(0.279f, 0.085f, 1.908f);

    private Vector3 prePos;
    private Vector3 newPos;
    private int preCountryIndex;

    private int frameCounter = 0;
    public GameObject centre;
    public GameObject centreReference;


    private void Start()
    {
        ax = 0.41173f - 0.028987f;
        ay = -0.00627f - 0.37663f;
        az = 0.0476f - 0.0430782f;

        norm = Mathf.Sqrt(ax * ax + ay * ay + az * az);
    }

    // Update is called once per frame
    void Update()
    {
        knobReference.transform.position = controlller.transform.position;
        dotLine.transform.position = dotLineReference.transform.position;

        centre.transform.position = centreReference.transform.position;

        sliderKnobReference.transform.position = realSliderReference.transform.position;

        if (flag)
        {
            if (!prev_flag)
            {
                pointList = GameObject.FindGameObjectsWithTag("DataPoint");
            }

            for (int i = 0; i < pointList.Length; i++)
            {
                if (Mathf.Abs(dotLine.transform.localPosition.x- pointList[i].transform.localPosition.x)<0.01)
                {
                    countryText.text = pointList[i].GetComponent<smallPointData>().country;
                    yearText.text = pointList[i].GetComponent<smallPointData>().year.ToString();
                    incomeText.text = pointList[i].GetComponent<smallPointData>().income;
                    lifeExpText.text = pointList[i].GetComponent<smallPointData>().lifeExp;

                    detailPanel.transform.localPosition = new Vector3(pointList[i].transform.localPosition.x, pointList[i].transform.localPosition.y+1.5f, -0.2f);
                }
            }

            moveRobotDynamic(150, 250);
            virtualKnobUpdateFromRobotAxis();
        }

        //if (flag)
        //{
        //    controlller.transform.localPosition = new Vector3(controlller.transform.localPosition.x, -(float)(shortInOut.value - 127) / 255 * (0.85f + 0.183f) + centre.transform.localPosition.y, controlller.transform.localPosition.z);

        //    foreach (Transform child in plotParent.transform)
        //    {
        //        if (child.GetComponent<barDetail>().country == selectedCountry)
        //        {
        //            detailPanel.transform.localPosition = new Vector3(child.GetComponent<barDetail>().pos.x, child.GetComponent<barDetail>().pos.y * 2 + 1.5f, detailPanel.transform.localPosition.z);

        //            countryText.text = child.GetComponent<barDetail>().country;
        //            yearText.text = child.GetComponent<barDetail>().year.ToString();
        //            populationText.text = child.GetComponent<barDetail>().data;
        //        }
        //    }
        //}


        if (Input.GetKeyDown("space"))
        {
            countryIndex = 0;

            //flag = true;

            unity_client.customMove(0.23, 0.17593, 0.045218, -0.6, 1.5, 0.62, movementType: 0);
        }

        if (Input.GetKeyDown("a")) // move to the previous one
        {
        }

        if (Input.GetKeyDown("d")) // move to the next one
        {
        }

        if (Input.GetKeyDown("r"))
        {
            readFile();
            reScalePlot();
            drewPlot();

            foreach (GameObject g in objectsToHide)
            {
                HideObject(g);
            }
        }

        preCountryIndex = countryIndex;
    }

    private void checkCounterRange()
    {
        if (countryIndex >= indexMax)
        {
            countryIndex = indexMax;
        }

        if (countryIndex <= indexMin)
        {
            countryIndex = indexMin;
        }
    }

    private Vector3 convertUnityCoord2RobotCoord(Vector3 p1)
    {
        p1.x -= -0.5606855f - -0.5606583f;
        p1.y -= -0.001490745f - -0.0005011343f;
        p1.z -= 0.3161324f - 0.3580751f;

        float new_x = 0.7098f * p1.x + -0.00617f * p1.y + 0.707f * p1.z + 0.345378f;
        float new_y = -0.7098f * p1.x + 0.00617f * p1.y + 0.7014f * p1.z - 0.338f;
        float new_z = 0.0071f * p1.x + 1f * p1.y + 0.000028f * p1.z + 0.0064f;

        return new Vector3(new_x, new_y, new_z);
    }

    private void drewPlot()
    {
        var detail = data1[selectedCountry];
        var detail2 = data2[selectedCountry];
        GameObject newline = Instantiate(linePrefb, new Vector3(0, 0, 0), Quaternion.identity);
        newline.transform.SetParent(plotParent.transform);
        newline.GetComponent<CurvedLineRenderer>().radius = 0.004f;

        for (int index = 100; index <= 200; index += 5)
        {
            if (detail.ContainsKey(index) & detail2.ContainsKey(index))
            {
                float y = detail2[index] / 10;
                float x = detail[index] / 100000000;
                GameObject newPoint = Instantiate(smallPoint, new Vector3(0, 0, 0), Quaternion.identity);
                newPoint.transform.SetParent(plotParent.transform);
                newPoint.transform.localPosition = new Vector3(x-5f, y-4f, 0);
                newPoint.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

                newPoint.GetComponent<smallPointData>().country = selectedCountry;
                newPoint.GetComponent<smallPointData>().year = index+1800;

                if (detail[index]>1000000000) newPoint.GetComponent<smallPointData>().income = (detail[index]/1000000000).ToString("F3")+"B";
                else if (detail[index] > 1000000) newPoint.GetComponent<smallPointData>().income = (detail[index] / 1000000).ToString("F3")+"M";
                else if (detail[index] > 1000) newPoint.GetComponent<smallPointData>().income = (detail[index] / 1000).ToString("F3")+"K";
                else newPoint.GetComponent<smallPointData>().income = (detail[index]).ToString("F3");

                if (detail2[index] > 1000000000) newPoint.GetComponent<smallPointData>().lifeExp = (detail2[index] / 1000000000).ToString("F3") + "B";
                else if (detail2[index] > 1000000) newPoint.GetComponent<smallPointData>().lifeExp = (detail2[index] / 1000000).ToString("F3") + "M";
                else if (detail2[index] > 1000) newPoint.GetComponent<smallPointData>().lifeExp = (detail2[index] / 1000).ToString("F3") + "K";
                else newPoint.GetComponent<smallPointData>().lifeExp = (detail2[index]).ToString("F3");

                GameObject newPoint1 = Instantiate(pointPrefb, new Vector3(0, 0, 0), Quaternion.identity);
                newPoint1.transform.SetParent(newline.transform);
                newPoint1.transform.position = newPoint.transform.position;
            }
        }

    }

    private void readFile()
    {
        StreamReader strReader = new StreamReader("Assets//" + filename);
        bool endOfFile = false;
        while (!endOfFile)
        {
            string data_string = strReader.ReadLine();
            if (data_string == null)
            {
                endOfFile = true;
                break;
            }

            int count = 0;
            string country = "";
            Dictionary<int, float> detail = new Dictionary<int, float>();
            foreach (string item in data_string.Split(','))
            {
                if (count == 0)
                {
                    country = item;
                }
                else
                {
                    float v = 0;
                    if (item.Substring(item.Length - 1) == "k")
                    {
                        v = float.Parse(item.Remove(item.Length - 1, 1)) * 1000;
                    }
                    else if (item.Substring(item.Length - 1) == "M")
                    {
                        v = float.Parse(item.Remove(item.Length - 1, 1)) * 1000000;
                    }
                    else if (item.Substring(item.Length - 1) == "B")
                    {
                        v = float.Parse(item.Remove(item.Length - 1, 1)) * 1000000000;
                    }
                    else
                    {
                        v = float.Parse(item);
                    }

                    detail.Add(count, v);
                }

                count += 1;
            }

            data1.Add(country, detail);
        }

        StreamReader strReader2 = new StreamReader("Assets//" + filename2);
        bool endOfFile2 = false;
        while (!endOfFile2)
        {
            string data_string2 = strReader2.ReadLine();
            if (data_string2 == null)
            {
                endOfFile2 = true;
                break;
            }

            int count = 0;
            string country = "";
            Dictionary<int, float> detail2 = new Dictionary<int, float>();
            foreach (string item in data_string2.Split(','))
            {
                if (count == 0)
                {
                    country = item;
                }
                else
                {
                    float v = 0;

                    v = float.Parse(item);


                    detail2.Add(count, v);
                }

                count += 1;
            }

            data2.Add(country, detail2);
        }
    }

    private void reScalePlot()
    {
        this.transform.localScale = new Vector3(0.07f, 0.07f, 0.07f);
        this.transform.localPosition = newLocation;
        this.transform.localEulerAngles = new Vector3(0, 180f, 0);
    }

    public void HideObject(GameObject obj, bool hideFlag = false)
    {
        Renderer[] objectR = obj.GetComponentsInChildren<Renderer>();

        foreach (Renderer rr in objectR)
        {
            rr.enabled = hideFlag;
        }

    }


    private void moveRobotDynamic(int bufferOne, int bufferTwo)
    {
        if (leftChecker.bounds.Contains(shortSliderTracker.transform.position))
        {
            if (!dynamicLeftEnd)
            {
                unity_client.stopRobot();
                shortInOut.SetSlider(0);
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
                shortInOut.SetSlider(0);
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
            if (shortInOut.value < bufferOne | shortInOut.value > bufferTwo)
            {
                dynamicMoving = true;
                float sp = (shortInOut.value - 415.0f / 2.0f) / (415.0f / 2.0f) * 0.0261f * speedScale * ((shortInOut.value - 415.0f / 2.0f) / (415.0f / 2.0f) * (shortInOut.value - 415.0f / 2.0f) / (415.0f / 2.0f) + 1);

                if ((shortInOut.value < bufferOne & !dynamicRightEnd) | (shortInOut.value > bufferTwo & !dynamicLeftEnd))
                {
                    unity_client.customMove(ax / (Mathf.Round(norm / sp * 100) / 100), ay / (Mathf.Round(norm / sp * 100) / 100), az / (Mathf.Round(norm / sp * 100) / 100), -0.6, 1.47, 0.62, speed: sp, acc: 1.5f, movementType: 4);

                    if (shortInOut.value < bufferOne & !dynamicRightEnd)
                    {
                        shortInOut.SetSlider((int)(Mathf.Abs(shortInOut.value - 415.0f / 2.0f) / (415.0f / 2.0f) * (120) + 190));
                    }

                    if (shortInOut.value > bufferTwo & !dynamicLeftEnd)
                    {
                        shortInOut.SetSlider((int)(Mathf.Abs(shortInOut.value - 415.0f / 2.0f) / (415.0f / 2.0f) * (-120) - 190));
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
                    shortInOut.SetSlider(0);
                }
            }
        }

        dynamicCounter++;
    }

    private void virtualKnobUpdateFromRobotAxis()
    {
        // update slider
        controlller.transform.localPosition = new Vector3(controlller.transform.localPosition.x, sliderKnobReference.transform.localPosition.y + (float)(shortInOut.value - 415) / 415 * 1.37f, controlller.transform.localPosition.z);
    }

}
