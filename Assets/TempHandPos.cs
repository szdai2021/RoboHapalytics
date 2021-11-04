using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;

namespace Leap
{
    public class TempHandPos : MonoBehaviour
    {
        // Start is called before the first frame update
        public GameObject leftHandReference;
        public GameObject rightHandReference;

        public GameObject tempLeft;
        public GameObject tempRight;

        // Update is called once per frame
        void Update()
        {
            tempLeft.transform.localPosition = leftHandReference.GetComponent<CapsuleHand>().tempPalm;
            tempRight.transform.localPosition = rightHandReference.GetComponent<CapsuleHand>().tempPalm;
        }
    }

}
