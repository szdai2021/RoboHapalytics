using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FingerClick : MonoBehaviour
{
    public GameObject finger;
    public List<GameObject> AntiSelection;

    private bool distanceFlag = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
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
                this.GetComponent<Toggle>().isOn = !this.GetComponent<Toggle>().isOn;

                distanceFlag = false;
            }

            if (this.GetComponent<Toggle>().isOn)
            {
                foreach (GameObject g in AntiSelection)
                {
                    g.GetComponent<Toggle>().isOn = false;
                }
            }
        }

    }
}
