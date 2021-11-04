using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class barChartManager : MonoBehaviour
{
    public GameObject plotParent;
    public GameObject barPref;
    public GameObject detailPanel;

    public GameObject virtualFingerTouchPoint;
    public GameObject virtualEndEffector;
    public GameObject controlller;

    public WirelessAxes wireless;
    public UnityClient unity_client;

    public float controllerMax;
    public float controllerMin;

    public string filename = "population_total.csv";

    private Dictionary<string, Dictionary<int, string>> data = new Dictionary<string, Dictionary<int, string>>(); // country - year - data

    private string selectedCountry;
    private int countryIndex = 0;

    public int indexMax = 11;
    public int indexMin = 0;

    public int counter = 100;

    public List<string> plotRange = new List<string> { "United States", "Russia", "China", "Nigeria" };

    private int yScale = 80000000;

    private float barGap = 0.8f;

    private bool flag = false;

    public GameObject countryText;
    public GameObject yearText;
    public GameObject populationText;

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

    // Start is called before the first frame update
    void Start()
    {
        readFile();

        drewPlot();

        unity_client.circularMove(0, 0.25f, 0.1f, -0.6, 1.47, 0.62, 0);
    }

    // Update is called once per frame
    void Update()
    {
        centre.transform.position = centreReference.transform.position;

        var p0 = finger.transform.GetChild(0).transform.position;

        selectedCountry = plotRange[countryIndex];

        if (flag)
        {
            controlller.transform.localPosition = new Vector3(controlller.transform.localPosition.x, -(float)(wireless.sliderOne - 127) / 255 * (0.85f + 0.183f) + centre.transform.localPosition.y, controlller.transform.localPosition.z);

            stepSlider();

            foreach (Transform child in plotParent.transform)
            {
                if (child.GetComponent<barDetail>().country == selectedCountry)
                {
                    detailPanel.transform.localPosition = new Vector3(child.GetComponent<barDetail>().pos.x, child.GetComponent<barDetail>().pos.y * 2 + 1.5f, detailPanel.transform.localPosition.z);

                    countryText.GetComponent<TextMesh>().text = child.GetComponent<barDetail>().country;
                    yearText.GetComponent<TextMesh>().text = child.GetComponent<barDetail>().year.ToString();
                    populationText.GetComponent<TextMesh>().text = child.GetComponent<barDetail>().data;
                }
            }
        }


        if (Input.GetKeyDown("space"))
        {
            countryIndex = 0;

            flag = true;
        }

        if (Input.GetKeyDown("a")) // move to the previous one
        {
            countryIndex--;

            wireless.sendSlider(0, 127);
            virtualFingerTouchPoint.transform.eulerAngles = new Vector3(0, 0, 90f);
            virtualFingerTouchPoint.transform.position = new Vector3(controlller.transform.position.x, controlller.transform.position.y, controlller.transform.position.z);

            newPos = convertUnityCoord2RobotCoord(virtualEndEffector.transform.position);

            unity_client.circularMove(newPos.x, newPos.y, newPos.z, -0.6, 1.47, 0.62, 0);

            checkCounterRange();
        }

        if (Input.GetKeyDown("d")) // move to the next one
        {
            countryIndex++;

            wireless.sendSlider(0, 127);
            virtualFingerTouchPoint.transform.eulerAngles = new Vector3(0, 0, 90f);
            virtualFingerTouchPoint.transform.position = new Vector3(controlller.transform.position.x, controlller.transform.position.y, controlller.transform.position.z);

            newPos = convertUnityCoord2RobotCoord(virtualEndEffector.transform.position);

            unity_client.circularMove(newPos.x, newPos.y, newPos.z, -0.6, 1.47, 0.62, 0);

            checkCounterRange();
        }

        if (Input.GetKeyDown("r")) // move to the next one
        {
            unity_client.circularMove(0, 0.25f, 0.1f, -0.6, 1.47, 0.62, 0);

            this.transform.localScale = new Vector3(0.07f, 0.07f, 0.07f);
            this.transform.localPosition = newLocation;
            this.transform.localEulerAngles = new Vector3(0, 180f, 0);
        }


        /*
        if (flag)
        {

            controlller.transform.localPosition = new Vector3(controlller.transform.localPosition.x, -(float)(wireless.sliderOne - 127) / 255 * (0.85f + 0.183f)+ centre.transform.localPosition.y, controlller.transform.localPosition.z);

            if (Tcollider.bounds.Contains(p0))
            {
                wireless.sendSlider(0, 127);
                virtualFingerTouchPoint.transform.eulerAngles = new Vector3(0, 0, 90f);
                virtualFingerTouchPoint.transform.position = new Vector3(controlller.transform.position.x, controlller.transform.position.y, controlller.transform.position.z);

                if (frameCounter > 20)
                {

                    newPos = convertUnityCoord2RobotCoord(virtualEndEffector.transform.position);

                    frameCounter = 0;
                }

                frameCounter++;
            }
            else
            {
                newPos = new Vector3(0, 0.25f, 0.1f);
            }


            if (prePos != newPos)
            {
                unity_client.circularMove(newPos.x, newPos.y, newPos.z, -0.6, 1.47, 0.62, 0);

                prePos = newPos;
            }
        }
        */
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

    private void drewPlot()
    {
        for (int i = 0; i < plotRange.Count; i++)
        {
            string country = plotRange[i];
            var detail = data[country];

            if (detail.ContainsKey(counter))
            {
                string item = detail[counter];

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

                float y = v / yScale;
                GameObject newPoint = Instantiate(barPref, new Vector3(0 + barGap*i, y/2, 0), Quaternion.identity);
                newPoint.transform.localScale = new Vector3(0.3f, y, 0.3f);
                newPoint.transform.SetParent(plotParent.transform);

                newPoint.GetComponent<Renderer>().material.SetColor("_Color", UnityEngine.Random.ColorHSV());

                newPoint.GetComponent<barDetail>().country = country;
                newPoint.GetComponent<barDetail>().year = counter+1800;
                newPoint.GetComponent<barDetail>().data = item;
                newPoint.GetComponent<barDetail>().pos = newPoint.transform.localPosition;
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
            Dictionary<int, string> detail = new Dictionary<int, string>();
            foreach (string item in data_string.Split(','))
            {
                if (count == 0)
                {
                    country = item;
                }
                else
                {
                    detail.Add(count, item);
                }

                count += 1;
            }

            data.Add(country, detail);
        }
    }

    private void stepSlider()
    {
        float step = (controllerMax - controllerMin) / indexMax;

        countryIndex = Mathf.CeilToInt((controlller.transform.localPosition.y-controllerMin)/step);

        checkCounterRange();
    }
}
