using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealisticDroneController : MonoBehaviour
{
    private GameData game_data;
    
    public GameObject propeller01;
    public GameObject propeller02;
    public GameObject propeller03;
    public GameObject propeller04;

    private Rigidbody drone_rig;

    public float power_multiplier;
    public float deceleration_multiplier;

    // 1: Front Left, 2: Back Left, 3: Front Right, 4: Back Right
    public float power01;
    public float power02;
    public float power03;
    public float power04;

    public float air_resistance;

    //public GameObject power01_bar;
    //public GameObject power02_bar;
    //public GameObject power03_bar;
    //public GameObject power04_bar;


    private bool isSlowMotion;

    private Vector3 init_pos;

    // separate direction and height to control using different parameters
    private Vector2 expected_direction;
    private float expected_height;


    // Start is called before the first frame update
    void Start()
    {
        game_data = GameObject.Find("GameData").GetComponent<GameData>();

        init_pos = transform.position;

        drone_rig = gameObject.GetComponent<Rigidbody>();

        isSlowMotion = false;

        expected_height = transform.position.y;
        expected_direction = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        //power01_bar.transform.localScale = new Vector3(1, power01, 1);
        //power02_bar.transform.localScale = new Vector3(1, power02, 1);
        //power03_bar.transform.localScale = new Vector3(1, power03, 1);
        //power04_bar.transform.localScale = new Vector3(1, power04, 1);

        if (Input.GetKey(KeyCode.R))
        {
            transform.rotation = new Quaternion();
            power01 = 0;
            power02 = 0;
            power03 = 0;
            power04 = 0;
            //propeller01.transform.rotation = new Quaternion();
            //propeller02.transform.rotation = new Quaternion();
            //propeller03.transform.rotation = new Quaternion();
            //propeller04.transform.rotation = new Quaternion();

            transform.position = game_data.Player_last_checkpoint.position;
            drone_rig.velocity = new Vector3();
        }

        if (Input.GetKey(KeyCode.Tab))
        {
            transform.rotation = new Quaternion();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isSlowMotion = !isSlowMotion;
            if (isSlowMotion)
            {
                Physics.gravity = Physics.gravity * 0.1f;
                drone_rig.velocity = drone_rig.velocity * 0.1f;
                drone_rig.angularVelocity = drone_rig.angularVelocity * 0.1f;
                power_multiplier *= 0.25f;
                deceleration_multiplier *= 0.25f;
            } else
            {
                Physics.gravity = Physics.gravity * 10f;
                drone_rig.velocity = drone_rig.velocity * 10f;
                drone_rig.angularVelocity = drone_rig.angularVelocity * 10f;
                power_multiplier *= 4;
                deceleration_multiplier *= 4;
            }
        }
    }

    private float power01_delta;
    private float power02_delta;
    private float power03_delta;
    private float power04_delta;

    public void RotateForward(float power = 0.1f)
    {
        IncreasePowerLB(power);
        IncreasePowerRB(power);
    }

    public void RotateBackward(float power = 0.1f)
    {
        IncreasePowerLF(power);
        IncreasePowerRF(power);
    }

    public void RotateRightward(float power = 0.1f)
    {
        IncreasePowerRF(power);
        IncreasePowerRB(power);
    }

    public void RotateLeftward(float power = 0.1f)
    {
        IncreasePowerLF(power);
        IncreasePowerLB(power);
    }

    public void Lift(float power = 0.1f)
    {
        IncreasePowerRF(power);
        IncreasePowerRB(power);
        IncreasePowerLF(power);
        IncreasePowerLB(power);
    }

    public void RotateClockwise()
    {
        transform.Rotate(new Vector3(0, 1, 0));
    }

    public void RotateAntiClockwise()
    {
        transform.Rotate(new Vector3(0, -1, 0));
    }

    public void IncreasePowerLF(float power = 0.1f)
    {
        if (power01 == 0) power01 = power;
        power01 += power01_delta;

        power01_delta = power_multiplier / power01;
    }
    public void IncreasePowerLB(float power = 0.1f)
    {
        if (power02 == 0) power02 = power;
        power02 += power02_delta;

        power02_delta = power_multiplier / power02;
    }

    public void IncreasePowerRF(float power = 0.1f)
    {
        if (power03 == 0) power03 = power;
        power03 += power03_delta;

        power03_delta = power_multiplier / power03;
    }

    public void IncreasePowerRB(float power = 0.1f)
    {
        if (power04 == 0) power04 = power;
        power04 += power04_delta;

        power04_delta = power_multiplier / power04;
    }

    public void NaiveFlightControl()
    {
        float r_x = transform.rotation.eulerAngles.x;
        float r_z = transform.rotation.eulerAngles.z;
        
        if (r_x > 1)
        {
            RotateBackward(0.02f);
        }
        if (r_x < -1)
        {
            RotateForward(0.02f);
        }

        if (r_z > 1)
        {
            RotateRightward(0.02f);
        }
        if (r_z < -1)
        {
            RotateLeftward(0.02f);
        }
    }

    void FixedUpdate()
    {
        // reset expected vector
        // if using keyboard
        //expected_direction = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
        // if using joystick
        expected_direction = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Debug.Log(expected_direction);



        if (expected_direction.y > 0)
        {

        }

        if (expected_direction.x > 0)
        {

        }

        //if (Input.GetKey(KeyCode.Q))
        //{
        //    IncreasePowerLF();
        //}

        //if (Input.GetKey(KeyCode.Z))
        //{
        //    IncreasePowerLB();
        //}

        //if (Input.GetKey(KeyCode.O))
        //{
        //    IncreasePowerRF();
        //}

        //if (Input.GetKey(KeyCode.M))
        //{
        //    IncreasePowerRB();
        //}
        //// horizontal control
        //if (Input.GetKey(KeyCode.W))
        //{
        //    if (transform.rotation.z < 1)
        //    {
        //        RotateForward();
        //    }
        //    expected_direction += new Vector2(1, 0);
        //}

        //if (Input.GetKey(KeyCode.S))
        //{
        //    if (transform.rotation.z > -1)
        //    {
        //        RotateBackward();
        //    }
        //    expected_direction += new Vector2(-1, 0);
        //}

        //if (Input.GetKey(KeyCode.A))
        //{
        //    if (transform.rotation.x > -1)
        //    {
        //        RotateRightward();
        //    }
        //    expected_direction += new Vector2(0, -1);
        //}

        //if (Input.GetKey(KeyCode.D))
        //{
        //    if (transform.rotation.x < 1)
        //    {
        //        RotateLeftward();
        //    }
        //    expected_direction += new Vector2(0, 1);
        //}



        // vertical control
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            RotateAntiClockwise();
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            RotateClockwise();
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            Lift();
        }

        //propeller01.GetComponent<Rigidbody>().AddTorque(new Vector3(0, power01_delta * 10000, 0));
        //propeller02.GetComponent<Rigidbody>().AddTorque(new Vector3(0, power02_delta * 10000 - propeller02.GetComponent<Rigidbody>().angularVelocity.z * 100, 0));
        //propeller03.GetComponent<Rigidbody>().AddTorque(new Vector3(0, power03_delta * 10000 - propeller03.GetComponent<Rigidbody>().angularVelocity.z * 100, 0));
        //propeller04.GetComponent<Rigidbody>().AddTorque(new Vector3(0, power04_delta * 10000 - propeller04.GetComponent<Rigidbody>().angularVelocity.z * 100, 0));

        propeller01.transform.Rotate(new Vector3(0, 0, power01 * 100f));
        propeller02.transform.Rotate(new Vector3(0, 0, power02 * 100f));
        propeller03.transform.Rotate(new Vector3(0, 0, power03 * 100f));
        propeller04.transform.Rotate(new Vector3(0, 0, power04 * 100f));

        // use delta rotation for better physics
        //drone_rig.AddForceAtPosition(propeller01.GetComponent<Rigidbody>().angularVelocity.z * propeller01.transform.forward, propeller01.transform.position);
        //drone_rig.AddForceAtPosition(propeller02.GetComponent<Rigidbody>().angularVelocity.z * propeller02.transform.forward, propeller02.transform.position);
        //drone_rig.AddForceAtPosition(propeller03.GetComponent<Rigidbody>().angularVelocity.z * propeller03.transform.forward, propeller03.transform.position);
        //drone_rig.AddForceAtPosition(propeller04.GetComponent<Rigidbody>().angularVelocity.z * propeller04.transform.forward, propeller04.transform.position);


        drone_rig.AddForceAtPosition(power01 * propeller01.transform.forward, propeller01.transform.position);
        drone_rig.AddForceAtPosition(power02 * propeller02.transform.forward, propeller02.transform.position);
        drone_rig.AddForceAtPosition(power03 * propeller03.transform.forward, propeller03.transform.position);
        drone_rig.AddForceAtPosition(power04 * propeller04.transform.forward, propeller04.transform.position);


        power01 *= deceleration_multiplier;
        if (power01 <= 0.01f) power01 = 0;
        power02 *= deceleration_multiplier;
        if (power02 <= 0.01f) power02 = 0;
        power03 *= deceleration_multiplier;
        if (power03 <= 0.01f) power03 = 0;
        power04 *= deceleration_multiplier;
        if (power04 <= 0.01f) power04 = 0;

        // air resistance
        drone_rig.AddForce(-drone_rig.velocity * air_resistance);

        // self correction
        NaiveFlightControl();
    }
}
