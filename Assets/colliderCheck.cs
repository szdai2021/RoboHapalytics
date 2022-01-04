using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class colliderCheck : MonoBehaviour
{
    public bool collisionCheck = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "sphereKnob")
        {
            collisionCheck = true;
        }
        else
        {
            collisionCheck = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        collisionCheck = false;
    }
}
