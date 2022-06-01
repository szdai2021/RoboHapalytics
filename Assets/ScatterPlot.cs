using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using TMPro;

public class ScatterPlot : MonoBehaviour
{
    public GameObject smallPoint;
    public GameObject pointPrefb;
    public GameObject linePrefb;
    public GameObject highlightParent;

    public Material newM;
    public Material oldM;

    public GameObject scatterPoint;

    public GameObject plotParent;

    public List<string> plotRange = new List<string> { "United States", "Russia", "China", "Nigeria" };

    public string selectedCountry = "China";

    public string filename = "income_per_person_gdppercapita_ppp_inflation_adjusted.csv";
    public string filename2 = "life_expectancy_years.csv";

    private Dictionary<string, Dictionary<int, float>> data1 = new Dictionary<string, Dictionary<int, float>>(); // country - year - data
    private Dictionary<string, Dictionary<int, float>> data2 = new Dictionary<string, Dictionary<int, float>>(); // country - year - data
    //private int start_year = 1800;

    public int counter = 170;
    public int step = 5;
    private bool flag = false;

    public int counterMax = 200;
    public int counterMin = 150;

    public bool testFlag = false;

    public float xScale = 200;
    public float yScale = 1;

    public TMP_Text countryText;
    public TMP_Text yearText;
    public TMP_Text incomeText;
    public TMP_Text lifeExpactText;

    public GameObject emptyPoint1;
    public GameObject emptyPoint2;

    public GameObject gradientIndicator;

    public GameObject indicatorLine;
    public Material indicatorColor;

    private bool indicatorFlag = false;

    public int gradientRange = 5;

    private float localScale = 0.007f;
    private Vector3 newLocation = new Vector3(0.238f, -0.159f, 1.79f);

    public GameObject vrhand;
    public UnityClient unity_client;
    public SerialInOut ShortSliderInOut;
    public int shortSliderMaxValue = 415;

    public Collider chartCollider;
    private bool pre_inCollider = false;

    public GameObject virtualFingerTouchPoint;
    public GameObject virtualEndEffector;
    public GameObject sphere;

    private int frameCounter = 0;

    private Vector3 prePos = new Vector3(0, 0.25f, 0.1f);
    private Vector3 newPos = new Vector3(0, 0.25f, 0.1f);

    private float angle = 0;

    private string pre_country;
    public bool testFlag2 = false;

    private bool resetKnobPosFlag;

    // Start is called before the first frame update
    void Start()
    {
        readFile();

        drewPlot();

        reScalePlot();

        chartCollider.enabled = true;

        this.transform.parent.gameObject.SetActive(false);
        //showIndicationLine(emptyPoint1.transform);

        StartCoroutine(resetKnobPos());
    }

    private void drewPlot()
    {
        foreach (string country in plotRange)
        {
            var detail = data1[country];
            var detail2 = data2[country];
            
            if (detail.ContainsKey(counter) & detail2.ContainsKey(counter))
            {
                float y = detail2[counter] / yScale;
                float x = detail[counter] / xScale;
                GameObject newPoint = Instantiate(scatterPoint, new Vector3(x, y, 0), Quaternion.identity);
                newPoint.transform.SetParent(plotParent.transform);
                newPoint.GetComponent<pointDetail>().country = country;
                newPoint.GetComponent<pointDetail>().year = counter;
                newPoint.GetComponent<pointDetail>().data = new Vector2(x * xScale, y * yScale);
                newPoint.GetComponent<pointDetail>().updatePos();
            }
        }
    }

    private void drewTimeVariance(bool f)
    {
        if (f)
        {
            var detail = data1[selectedCountry];
            var detail2 = data2[selectedCountry];
            GameObject newline = Instantiate(linePrefb, new Vector3(0, 0, 0), Quaternion.identity);
            newline.transform.SetParent(highlightParent.transform);
            newline.GetComponent<CurvedLineRenderer>().radius = 0.003f;

            for (int index = counterMin; index <= counterMax; index += step)
            {
                if (detail.ContainsKey(index) & detail2.ContainsKey(index))
                {
                    float y = detail2[index] / 1;
                    float x = detail[index] / 200;
                    GameObject newPoint = Instantiate(smallPoint, new Vector3(0, 0, 0), Quaternion.identity);
                    newPoint.transform.SetParent(highlightParent.transform);
                    newPoint.transform.localPosition = new Vector3(x, y, 0);
                    newPoint.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

                    GameObject newPoint1 = Instantiate(pointPrefb, new Vector3(0, 0, 0), Quaternion.identity);
                    newPoint1.transform.SetParent(newline.transform);
                    newPoint1.transform.position = newPoint.transform.position;
                }
            }

            showIndicationLine(emptyPoint1.transform);
        }
        else
        {
            foreach (Transform child in highlightParent.transform)
            {
                GameObject.Destroy(child.gameObject);
            }

            showIndicationLine(emptyPoint1.transform, false);
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

    // Update is called once per frame
    void Update()
    {
        if (flag) // start highlight view
        {
            foreach(Transform child in plotParent.transform)
            {
                if (child.GetComponent<pointDetail>().country == selectedCountry)
                {
                    child.transform.localScale = new Vector3(4, 4, 4);
                    child.GetComponent<MeshRenderer>().material = newM;

                    countryText.text = child.GetComponent<pointDetail>().country;
                    yearText.text = (child.GetComponent<pointDetail>().year + 1800).ToString();
                    incomeText.text = child.GetComponent<pointDetail>().data[1].ToString();
                    lifeExpactText.text = child.GetComponent<pointDetail>().data[0].ToString();

                    if (indicatorLine.transform.childCount > 0)
                    {
                        indicatorLine.transform.GetChild(0).transform.GetChild(1).position = child.transform.position;
                    }

                    gradientIndicator.transform.position = child.transform.position;
                    updateGradientIndicator();

                    if (testFlag2)
                    {
                        virtualFingerTouchPoint.transform.eulerAngles = new Vector3(0, 0, 90f - gradientIndicator.transform.localEulerAngles.z);
                        virtualFingerTouchPoint.transform.position = new Vector3(child.transform.position.x, child.transform.position.y, child.transform.position.z);

                        if (frameCounter > 20)
                        {
                            //unity_client.circularMove(newPos[0], newPos[1], newPos[2], -0.6, 1.47, 0.62, 0, angle: gradientIndicator.transform.localEulerAngles.z*Mathf.Deg2Rad, jointIndex: 5);

                            newPos = convertUnityCoord2RobotCoord(virtualEndEffector.transform.position);

                            frameCounter = 0;

                            angle = gradientIndicator.transform.localEulerAngles.z * Mathf.Deg2Rad;
                        }

                        frameCounter++;
                    }
                }
                else
                {
                    child.transform.localScale = new Vector3(3, 3, 3);
                    child.GetComponent<MeshRenderer>().material = oldM;
                }
            }
            //plotParent.transform.GetChild(2).transform.localScale = new Vector3(10, 10, 10);
            //plotParent.transform.GetChild(2).GetComponent<MeshRenderer>().material = newM;
        }
        else
        {
            foreach (Transform child in plotParent.transform)
            {
                if (child.GetComponent<pointDetail>().country == selectedCountry)
                {
                    child.transform.localScale = new Vector3(3, 3, 3);
                    child.GetComponent<MeshRenderer>().material = oldM;
                }
            }
            //plotParent.transform.GetChild(2).transform.localScale = new Vector3(7, 7, 7);
            //plotParent.transform.GetChild(2).GetComponent<MeshRenderer>().material = oldM;
        }

        var p0 = vrhand.transform.position;

        if (chartCollider.bounds.Contains(p0) & !testFlag2)
        {
            centerKnob();

            GameObject closest = plotParent.transform.GetChild(0).gameObject;
            float distance = 1000;

            for(int i = 0; i < plotParent.transform.childCount; i++)
            {
                GameObject t = plotParent.transform.GetChild(i).gameObject;

                if(Vector3.Distance(p0, t.transform.position) < distance)
                {
                    closest = t;
                    distance = Vector3.Distance(p0, t.transform.position);
                }
            }

            virtualFingerTouchPoint.transform.eulerAngles = new Vector3(0, 0, 90f-gradientIndicator.transform.localEulerAngles.z);
            virtualFingerTouchPoint.transform.position = new Vector3(closest.transform.position.x, closest.transform.position.y, closest.transform.position.z);
            selectedCountry = closest.GetComponent<pointDetail>().country;

            flag = true;

            if (pre_country != selectedCountry)
            {
                drewTimeVariance(false);
                drewTimeVariance(flag);
            }

            if (frameCounter > 20)
            {
                //unity_client.circularMove(newPos[0], newPos[1], newPos[2], -0.6, 1.47, 0.62, 0, angle: gradientIndicator.transform.localEulerAngles.z*Mathf.Deg2Rad, jointIndex: 5);

                newPos = convertUnityCoord2RobotCoord(virtualEndEffector.transform.position);

                frameCounter = 0;

                angle = gradientIndicator.transform.localEulerAngles.z* Mathf.Deg2Rad;
            }

            frameCounter++;
        }


        if (testFlag)
        {
            testFlag2 = true;
        }

        if (!chartCollider.bounds.Contains(p0) & !testFlag2)
        {
            //unity_client.circularMove(0, 0.25, 0.1, -0.6, 1.47, 0.62, 0);
            newPos = new Vector3(0, 0.25f, 0.1f);
            angle = 0;
            flag = false;

            drewTimeVariance(flag);
        }

        if (prePos != newPos)
        {
            unity_client.customMove(newPos.x, newPos.y, newPos.z, -0.6, 1.47, 0.62, movementType: 1, angle6: angle);

            //sliderMoveFlag = false;

            prePos = newPos;
        }

        sphere.transform.localPosition = new Vector3((float)(ShortSliderInOut.value - shortSliderMaxValue) / shortSliderMaxValue * 1.37f, 0 , 0);

        
        if (Input.GetKeyDown("a")) // move to the previous one
        {
            centerKnob();

            counter -= step;
            checkCounterRange();

            updateMainPlot();
        }

        if (testFlag) // move to the next one
        {
            testFlag = false;

            centerKnob();

            counter += step;
            checkCounterRange();

            updateMainPlot();
        }
        
        if (Input.GetKeyDown("r"))
        {
            this.transform.localScale = new Vector3(localScale, localScale, localScale);
            this.transform.localPosition = newLocation;

            this.transform.localEulerAngles = new Vector3(0, 180f, 0);

            var line = Instantiate(linePrefb, new Vector3(0, 0, 0), Quaternion.identity);
            line.transform.SetParent(indicatorLine.transform);
            line.GetComponent<CurvedLineRenderer>().radius = 0.001f;
            line.GetComponent<MeshRenderer>().material = indicatorColor;

            GameObject newPoint1 = Instantiate(pointPrefb, new Vector3(0, 0, 0), Quaternion.identity);
            newPoint1.transform.SetParent(line.transform);
            newPoint1.transform.position = emptyPoint1.transform.position;

            GameObject newPoint2 = Instantiate(pointPrefb, new Vector3(0, 0, 0), Quaternion.identity);
            newPoint2.transform.SetParent(line.transform);
            newPoint2.transform.position = emptyPoint1.transform.position;
        }

        pre_inCollider = chartCollider.bounds.Contains(p0);

        pre_country = selectedCountry;
    }

    //private Vector3 convertUnityCoord2RobotCoord(Vector3 p1)
    //{
    //    /*
    //    float new_x = 0.702f * p1.x + 0.00522f * p1.y + 0.707f * p1.z + 0.476f;
    //    float new_y = -0.7023f * p1.x + -0.005f * p1.y + 0.6843f * p1.z - 0.4695f;
    //    float new_z = 0.09f * p1.x + 0.803f * p1.y + 0.4482f * p1.z - 0.047f;
    //    */

    //    float new_x = 0.7098f * p1.x + -0.00617f * p1.y + 0.707f * p1.z + 0.345378f;
    //    float new_y = -0.7098f * p1.x + 0.00617f * p1.y + 0.7014f * p1.z - 0.338f;
    //    float new_z = 0.0071f * p1.x + 1f * p1.y + 0.000028f * p1.z + 0.0064f;

    //    return new Vector3(new_x, new_y, new_z);
    //}

    private Vector3 convertUnityCoord2RobotCoord(Vector3 p1)
    {
        /*
        float new_x = 0.702f * p1.x + 0.00522f * p1.y + 0.707f * p1.z + 0.476f;
        float new_y = -0.7023f * p1.x + -0.005f * p1.y + 0.6843f * p1.z - 0.4695f;
        float new_z = 0.09f * p1.x + 0.803f * p1.y + 0.4482f * p1.z - 0.047f;
        */
        p1.x -= -0.5606855f - -0.5606583f;
        p1.y -= -0.001490745f - -0.0005011343f;
        p1.z -= 0.3161324f - 0.3580751f;

        float new_x = 0.7098f * p1.x + -0.00617f * p1.y + 0.707f * p1.z + 0.345378f;
        float new_y = -0.7098f * p1.x + 0.00617f * p1.y + 0.7014f * p1.z - 0.338f;
        float new_z = 0.0071f * p1.x + 1f * p1.y + 0.000028f * p1.z + 0.0064f;

        return new Vector3(new_x, new_y, new_z);
    }

    private void checkCounterRange()
    {
        if (counter >= counterMax)
        {
            counter = counterMax;
        }

        if (counter <= counterMin)
        {
            counter = counterMin;
        }
    }

    private void updateMainPlot()
    {
        foreach (Transform point in plotParent.transform)
        {
            string c = point.GetComponent<pointDetail>().country;
            var detail = data1[c];
            var detail2 = data2[c];

            float y = detail2[counter] / yScale;
            float x = detail[counter] / xScale;

            point.GetComponent<pointDetail>().AddQueue(new Vector2(detail2[counter], detail[counter]), new Vector3(x,y,0));
            point.GetComponent<pointDetail>().year = counter;
        }
    }

    private void showIndicationLine(Transform highlight, bool f = true)
    {
        /*
            var line = Instantiate(linePrefb, new Vector3(0, 0, 0), Quaternion.identity);
            line.transform.SetParent(indicatorLine.transform);
            line.GetComponent<CurvedLineRenderer>().radius = 0.001f;
            line.GetComponent<MeshRenderer>().material = indicatorColor;

            GameObject newPoint1 = Instantiate(pointPrefb, new Vector3(0, 0, 0), Quaternion.identity);
            newPoint1.transform.SetParent(line.transform);
            newPoint1.transform.position = emptyPoint1.transform.position;

            GameObject newPoint2 = Instantiate(pointPrefb, new Vector3(0, 0, 0), Quaternion.identity);
            newPoint2.transform.SetParent(line.transform);
            newPoint2.transform.position = highlight.transform.position;
        */

        if (indicatorLine.transform.childCount > 0)
        {
            indicatorLine.transform.GetChild(0).transform.GetChild(0).transform.position = highlight.transform.position;

            indicatorLine.transform.GetChild(0).transform.GetChild(1).transform.position = emptyPoint1.transform.position;
        }

    }

    private void updateGradientIndicator()
    {
        float angle;

        float[] a = new float[gradientRange]; // income - x
        float[] b = new float[gradientRange]; // life expectancy - y

        float[] c = new float[gradientRange];
        float[] d = new float[gradientRange];

        for (int i = 0; i < gradientRange; i++)
        {
            a[i] = data1[selectedCountry][counter - gradientRange / 2 + i] / xScale;
            b[i] = data2[selectedCountry][counter - gradientRange / 2 + i] / yScale;
            c[i] = a[i] * b[i];
            d[i] = a[i] * a[i];
        }

        float aa = ((a.Average()*b.Average()) - c.Average()) / (a.Average() * a.Average() - d.Average());

        angle = Mathf.Rad2Deg*Mathf.Atan(aa);

        gradientIndicator.transform.localEulerAngles = new Vector3(0, 0, angle);
    }

    private void reScalePlot() {
        this.transform.localScale = new Vector3(localScale, localScale, localScale);
        this.transform.localPosition = newLocation;

        this.transform.localEulerAngles = new Vector3(0, 180f, 0);

        var line = Instantiate(linePrefb, new Vector3(0, 0, 0), Quaternion.identity);
        line.transform.SetParent(indicatorLine.transform);
        line.GetComponent<CurvedLineRenderer>().radius = 0.001f;
        line.GetComponent<MeshRenderer>().material = indicatorColor;

        GameObject newPoint1 = Instantiate(pointPrefb, new Vector3(0, 0, 0), Quaternion.identity);
        newPoint1.transform.SetParent(line.transform);
        newPoint1.transform.position = emptyPoint1.transform.position;

        GameObject newPoint2 = Instantiate(pointPrefb, new Vector3(0, 0, 0), Quaternion.identity);
        newPoint2.transform.SetParent(line.transform);
        newPoint2.transform.position = emptyPoint1.transform.position;
    }

    private void centerKnob()
    {
        resetKnobPosFlag = true;
    }

    IEnumerator resetKnobPos()
    {
        while (resetKnobPosFlag)
        {
            if ((ShortSliderInOut.value - 415 / 2) > 20)
            {
                ShortSliderInOut.SetSlider(-250);
            }
            else if ((ShortSliderInOut.value - 415 / 2) < -20)
            {
                ShortSliderInOut.SetSlider(250);
            }
            yield return new WaitUntil(() => Mathf.Abs(ShortSliderInOut.value - 415 / 2) < 20);

            resetKnobPosFlag = false;
        }
    }
}
