using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitializePosition : MonoBehaviour
{
    public GameObject trackerObject;
    public GameObject endEffector;
    public GameObject UI;

    private bool positionTrack = true;

    // Start is called before the first frame updat

    private void Update()
    {
        if (positionTrack)
        {
            UI.transform.position = endEffector.transform.position;
            UI.transform.rotation = endEffector.transform.rotation;
        }

    }

    public void LockPosition()
    {
        Destroy(trackerObject.GetComponent("SteamVR_TrackedObject"));

        positionTrack = false;
    }
}
