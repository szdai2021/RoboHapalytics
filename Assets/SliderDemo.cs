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
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject g in hiddenObject)
        {
            HideObject(g);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HideObject(GameObject obj, bool hideFlag = false)
    {
        Renderer[] objectR = obj.GetComponentsInChildren<Renderer>();

        foreach (Renderer rr in objectR)
        {
            rr.enabled = hideFlag;
        }

    }
}
