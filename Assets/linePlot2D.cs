using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class linePlot2D : MonoBehaviour
{
    public GameObject pointPrefb;
    public GameObject linePrefb;

    public GameObject highlighter;
    public GameObject detailPanel;
    public GameObject highlighParent;

    public GameObject plotParent;

    public List<string> plotRange = new List<string> { "United States", "Russia", "China", "Nigeria" };

    public string filename = "income_per_person_gdppercapita_ppp_inflation_adjusted.csv";

    private Dictionary<string, Dictionary<int, float>> data = new Dictionary<string, Dictionary<int, float>>();
    private int start_year = 1800;

    private int counter = 120;

    // Start is called before the first frame update
    void Start()
    {
        readFile();

        drewPlot();
    }

    private void drewPlot()
    {
        foreach (string country in plotRange)
        {
            GameObject newLine =  Instantiate(linePrefb, new Vector3(0, 0, 0), Quaternion.identity);
            newLine.transform.SetParent(plotParent.transform);

            var detail = data[country];

            foreach (int index in detail.Keys)
            {
                float x = (float)index/10; //year
                float y = detail[index]/7000; //value
                GameObject newPoint =  Instantiate(pointPrefb, new Vector3(x, y, 0), Quaternion.identity);
                newPoint.transform.SetParent(newLine.transform);
            }
        }
    }

    private void readFile()
    {
        StreamReader strReader = new StreamReader("Assets\\"+filename);
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
                if(count == 0)
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

            data.Add(country, detail);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("a"))
        {
            counter--;
            foreach (Transform child in highlighParent.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            float x = (float)counter / 10;
            float y = data["United States"][counter] / 7000; 
            GameObject h = Instantiate(highlighter, new Vector3(x, y, 0), Quaternion.identity);
            h.transform.SetParent(highlighParent.transform);
            GameObject hd = Instantiate(detailPanel, new Vector3(x, y+5, 0), Quaternion.Euler(-90,0,0));
            hd.transform.SetParent(highlighParent.transform);
        }

        if (Input.GetKeyDown("d"))
        {
            counter++;
            foreach (Transform child in highlighParent.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            float x = (float)counter / 10;
            float y = data["United States"][counter] / 7000;
            GameObject h = Instantiate(highlighter, new Vector3(x, y, 0), Quaternion.identity);
            h.transform.SetParent(highlighParent.transform);
            GameObject hd = Instantiate(detailPanel, new Vector3(x, y + 5, 0), Quaternion.Euler(-90, 0, 0));
            hd.transform.SetParent(highlighParent.transform);
        }

    }
}
