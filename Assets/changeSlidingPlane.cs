using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class changeSlidingPlane : MonoBehaviour
{
    public GameObject PosSlider;
    public int index;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.gameObject.transform.position = new Vector3(PosSlider.GetComponent<sliderValueControl>().currentValue.x, PosSlider.GetComponent<sliderValueControl>().currentValue.y, PosSlider.GetComponent<sliderValueControl>().currentValue.z);
        //this.gameObject.transform.localEulerAngles = new Vector3(xRotSlider.GetComponent<sliderValueControl>().rangeValue, zRotSlider.GetComponent<sliderValueControl>().rangeValue, yRotSlider.GetComponent<sliderValueControl>().rangeValue);
    }
}
