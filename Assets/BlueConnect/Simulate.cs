using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulate : MonoBehaviour
{
    public bool isMove;

    public BlueConnect blueConnect;
    public Rigidbody rb;

    // public float time = 0.02f;
    public float Vx, Vy, Vz;
    
    // Start is called before the first frame update
    void Start()
    {
        blueConnect = GetComponent<BlueConnect>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        SetRotate();
        Move();
    }

    public void SetRotate()
    {
        Quaternion quaternion = new Quaternion
        {
            x = blueConnect.QX,
            y = blueConnect.QZ,
            z = blueConnect.QY,
            w = blueConnect.QW
        };
        transform.rotation = quaternion;
    }

    public void Move()
    {
        if (isMove)
        {
            //Vx = Vx + blueConnect.asX * 9.8f * Time.deltaTime;
            //Vz = Vz + blueConnect.asZ * 9.8f * Time.deltaTime;
            //Vy = Vy + blueConnect.asY * 9.8f * Time.deltaTime;

            Vx = Vx + blueConnect.asX * 9.8f * 0.02f;
            Vy = Vy + blueConnect.asZ * 9.8f * 0.02f;
            Vz = Vz + blueConnect.asY * 9.8f * 0.02f;

            rb.velocity = new Vector3(Vx, Vy, Vz);
        }
        
    }
}
