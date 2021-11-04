using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class fingerClickBtn : MonoBehaviour
{
    public GameObject HideRobotToggle;
    public GameObject RecentreToggle;

    public GameObject SectionToggle;
    public GameObject ScatterToggle;
    public GameObject BarChartToggle;

    public GameObject finger;

    public controlmanager cm;

    private bool distanceFlag = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (SectionToggle.GetComponent<Toggle>().isOn)
        {
            cm.scenario = 1;
        }
        else if (ScatterToggle.GetComponent<Toggle>().isOn)
        {
            cm.scenario = 2;
        }
        else if (BarChartToggle.GetComponent<Toggle>().isOn)
        {
            cm.scenario = 3;
        }

        if (finger.transform.childCount > 0)
        {
            var p0 = finger.transform.GetChild(0).transform.position;
            //print(Vector3.Distance(this.GetComponent<RectTransform>().position, p0));
            if (Vector3.Distance(this.GetComponent<RectTransform>().position, p0) > 0.1)
            {
                distanceFlag = true;
            }

            if (Vector3.Distance(this.GetComponent<RectTransform>().position, p0) <= 0.05 & distanceFlag)
            {
                print("start");
                cm.startScenario();
                cm.HideRobot(!HideRobotToggle.GetComponent<Toggle>().isOn);

                //this.GetComponent<Button>().onClick.Invoke();

                distanceFlag = false;
            }
        }
    }
}
