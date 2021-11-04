using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Control : MonoBehaviour
{
    public GameObject dropdownBtn;
    public Text instructions;
    public GameObject HideRobotButton;

    public void HideRobot()
    {
        if (HideRobotButton.GetComponentInChildren<Text>().text == "Hide Robot")
        {
            HideRobotButton.GetComponentInChildren<Text>().text = "Show Robot";
        }
        else
        {
            HideRobotButton.GetComponentInChildren<Text>().text = "Hide Robot";
        }
        
    }

    public void dropDownOnChange()
    {
        if (dropdownBtn.GetComponentInChildren<Dropdown>().value == 0)
        {
            instructions.text = "Slider";
        }
        else
        {
            instructions.text = "circular Movement";
        }
    }
}
