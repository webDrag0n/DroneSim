using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InputReader : MonoBehaviour
{
    public bool use_manual_override;
    private Queue<Vector2> flat_input_sequence = new Queue<Vector2>();
    private Queue<Vector2> vert_input_sequence = new Queue<Vector2>();
    // Start is called before the first frame update
    void Start()
    {
        if (!use_manual_override)
        {
            read_in_control_sequence();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (use_manual_override)
        {
            flat_input_sequence.Clear();
            //Debug.Log(flat_input_sequence.Count);
            //Debug.Log("Override");
            flat_input_sequence.Enqueue(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));

            vert_input_sequence.Clear();
            Vector2 vert_vect = new Vector2();
            if (Input.GetKey(KeyCode.UpArrow))
            {
                vert_vect += new Vector2(0, 0.1f);
                
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                vert_vect += new Vector2(0, -0.1f);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                vert_vect += new Vector2(-1f, 0);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                vert_vect += new Vector2(1f, 0);
            }
            vert_input_sequence.Enqueue(vert_vect);
        }
    }

    public Vector2 get_next_flat_input()
    {
        if (flat_input_sequence.Count > 0)
        {
            return flat_input_sequence.Dequeue();
        }
        else
        {
            return Vector2.zero;
        }
    }

    public Vector2 get_next_vert_input()
    {
        if (vert_input_sequence.Count > 0)
        {
            return vert_input_sequence.Dequeue();
        }
        else
        {
            return Vector2.zero;
        }
    }

    public void read_in_control_sequence()
    {

    }
}
