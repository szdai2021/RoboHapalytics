using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class follower : MonoBehaviour
{
    public bool flag;

    public GameObject target;
    public Vector3 pos;
    public Vector3 rot;

    private void Start()
    {
        pos = target.transform.position;
        rot = target.transform.eulerAngles;
    }
    // Update is called once per frame
    void Update()
    {
        if (flag)
        {
            this.transform.position = target.transform.position;
            this.transform.rotation = target.transform.rotation;
        }
    }
}
