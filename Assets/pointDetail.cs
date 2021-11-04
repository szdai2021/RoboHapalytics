using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pointDetail : MonoBehaviour
{
    public string country;
    public int year;
    public Vector2 data;

    private Vector3 location;

    private List<Vector3> queueList = new List<Vector3>();

    private Vector3 nextLocation;
    private bool movementFlag = false;

    private Vector3 step;

    private int counter = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (queueList.Count > 0)
        {
            if (!movementFlag)
            {
                nextLocation = queueList[0];
                step = (nextLocation - location) / 60;
                queueList.RemoveAt(0);

                movementFlag = true;
            }
        }

        if (movementFlag)
        {
            this.transform.localPosition += step;

            counter++;

            if (counter == 59)
            {
                location = nextLocation;
                movementFlag = false;

                counter = 0;
            }
        }
    }

    public void updatePos()
    {
        location = this.transform.localPosition;
    }

    public void AddQueue(Vector2 newData, Vector3 newLocation)
    {
        data = newData;
        queueList.Add(newLocation);
    }
}
