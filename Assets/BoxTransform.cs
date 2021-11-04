using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxTransform : MonoBehaviour
{
    public GameObject slidingPlaneXLeft; // x
    public GameObject slidingPlaneXRight;
    public GameObject slidingPlaneYLeft; // z
    public GameObject slidingPlaneYRight;
    public GameObject slidingPlaneZTop; // y - 0.15
    public GameObject slidingPlaneZBottom;

    private float xGap;
    private float yGap;
    private float zGap;

    private float xPos = 0;
    private float yPos = 0;
    private float zPos = 0;

    public float xOffset;
    public float yOffset;
    public float zOffset;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        xGap = Mathf.Abs(slidingPlaneXLeft.transform.localPosition.x - slidingPlaneXRight.transform.localPosition.x);
        yGap = Mathf.Abs(slidingPlaneYLeft.transform.localPosition.z - slidingPlaneYRight.transform.localPosition.z);
        zGap = Mathf.Abs(slidingPlaneZTop.transform.localPosition.y - slidingPlaneZBottom.transform.localPosition.y);

        this.gameObject.transform.localScale = new Vector3(xGap, yGap, zGap);

        xPos = 0 + (slidingPlaneXLeft.transform.localPosition.x + 0.5f) / 2 - (0.5f - slidingPlaneXRight.transform.localPosition.x) / 2;
        zPos = 0 + (slidingPlaneYLeft.transform.localPosition.z + 0.5f) / 2 - (0.5f - slidingPlaneYRight.transform.localPosition.z) / 2;
        yPos = 0.15f - (0.65f - slidingPlaneZTop.transform.localPosition.y) / 2 + (slidingPlaneZBottom.transform.localPosition.y + 0.35f) / 2;

        this.gameObject.transform.localPosition = new Vector3(xPos, yPos, zPos);
    }
}
