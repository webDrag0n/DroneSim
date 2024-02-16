using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class TransformPlayer : MonoBehaviour
{
    public string record_name;
    private string path;
    private StreamReader reader;

    private void Start()
    {
        path = "Assets/Resources/Record/VTOL_Record/" + record_name + ".txt";
        reader = new StreamReader(path, true);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        string line = reader.ReadLine();
        if (line.Equals("")) return;
        string[] line_arr = Regex.Split(line.Split('(')[1].Split(')')[0], ", ");
        float[] line_float_arr = new float[3];
        int i = 0;
        foreach (string element in line_arr)
        {
            if (string.IsNullOrEmpty(element)) continue;
            line_float_arr[i] = float.Parse(element);
            i++;
            if (i > 3) break;
        }

        transform.position = new Vector3(
            line_float_arr[0],
            line_float_arr[1],
            line_float_arr[2]
        );
    }
}
