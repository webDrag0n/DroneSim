using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TransformRecorder : MonoBehaviour
{
    StreamWriter writer;
    private void Start()
    {
        DateTime dt = DateTime.Now;
        string path = "Assets/Resources/Record/VTOL_Record/" + dt.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt";
        writer = new StreamWriter(path, true);

    }
    // Update is called once per frame
    void FixedUpdate()
    {
        writer.WriteLine(transform.position.ToString());
    }
}
