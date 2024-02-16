using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPosLogger : MonoBehaviour
{
    public float record_interval = 0.1f;
    public Transform ghost;
    private float timer = 0;


    // Update is called once per frame
    void FixedUpdate()
    {
        timer++;
        if (timer > record_interval * 1000)
        {
            Instantiate(ghost, transform.position, transform.rotation);
            timer = 0;
        }
    }
}
