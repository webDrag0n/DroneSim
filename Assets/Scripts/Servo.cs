using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Servo : MonoBehaviour
{
    public float target_angle;
    public Vector3 axis;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.Euler(axis * target_angle),
            Time.fixedDeltaTime
        );
    }
}
