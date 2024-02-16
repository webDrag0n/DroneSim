using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public enum ControllerState
{
    Manual,
    AutoTracking
}

public class VTOLController : MonoBehaviour
{
    public ControllerState state;
    public Transform track_target;
    public Transform Frame;
    public Vector3 Inclination => new Vector3(Frame.right.y, Frame.up.y, Frame.forward.y);


    public InputReader input_reader;

    public GameObject propeller_front;
    public GameObject propeller_back_left;
    public GameObject propeller_back_right;

    public GameObject propeller_front_render;
    public GameObject propeller_back_left_render;
    public GameObject propeller_back_right_render;

    //VTOL parameters
    [Header("Internal")]
    public float maxPropellerForce; //100
    public float maxTorque; //1
    public float throttle;
    public float lift; //5
    //PID
    public Vector3 PID_pitch_gains; //(2, 3, 2)
    public Vector3 PID_roll_gains; //(2, 0.2, 0.5)
    public Vector3 PID_yaw_gains; //(1, 0, 0)

    public Vector3 PID_speed_gains;
    public Vector3 PID_lift_gains;

    //External parameters
    [Header("External")]
    public float windForce;
    //0 -> 360
    public float forceDir;

    private Rigidbody rb;

    private Vector3 init_pos;
    private Quaternion init_rot;


    //The PID controllers
    private PIDController PID_pitch;
    private PIDController PID_roll;
    private PIDController PID_yaw;

    private PIDController PID_speed;
    private PIDController PID_lift;


    //Movement factors
    float moveForwardBack;
    float moveLeftRight;
    float yawDir;


    public float propellerForceFront;
    public float propellerForceBR;
    public float propellerForceBL;

    StreamWriter stream;

    private bool is_hover;

    // Start is called before the first frame update
    void Start()
    {
        input_reader = new InputReader();
        rb = GetComponent<Rigidbody>();
        init_pos = gameObject.transform.position;
        init_rot = gameObject.transform.rotation;


        PID_pitch = new PIDController();
        PID_roll = new PIDController();
        PID_yaw = new PIDController();

        PID_speed = new PIDController();
        PID_lift = new PIDController();

        // start recording
        //stream = new StreamWriter(Application.dataPath + "/record.csv", false, Encoding.UTF8);
        //Debug.Log(Application.dataPath + "/record.csv");

        is_hover = true;

        propellerForceFront = 0;
        propellerForceBR = 0;
        propellerForceBL = 0;
    }

    public void Reset()
    {
        propellerForceFront = 0;
        propellerForceBR = 0;
        propellerForceBL = 0;
        gameObject.transform.position = init_pos;
        gameObject.transform.rotation = init_rot;
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;
    }

    // Update is called once per frame
    public void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (state == ControllerState.Manual)
        {
            AddControls();
            ManipulateMotors();
            CalculateMotorForce();
        }
        AddMotorForce();
        //stream.WriteLine(transform.rotation.eulerAngles.ToString());
        
        //AddExternalForces();
    }


    void AddControls()
    {
        //Stop csv recording
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            stream.Close();
            stream.Dispose();
        }

        //Change throttle to move up or down
        if (Input.GetKey(KeyCode.UpArrow))
        {
            MoveUpDown(0.1f);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            MoveUpDown(-0.1f);
        }

        //Vector2 vert_vect = input_reader.get_next_vert_input();
        //throttle += vert_vect.y;
        //throttle = 50 + Input.GetAxis("Vertical_R") * 5;

        throttle = Mathf.Clamp(throttle, 0f, 200f);


        Yaw(Input.GetAxis("Horizontal_R"));
        //yawDir = vert_vect.x;

        //Steering
        //Vector2 flat_vect = input_reader.get_next_flat_input();
        //Debug.Log(flat_vect);
        MoveForwardBack(Input.GetAxis("Vertical"));
        //moveForwardBack = flat_vect.y;

        MoveLeftRight(Input.GetAxis("Horizontal"));
        //moveLeftRight = flat_vect.x;


        if (Input.GetKeyDown(KeyCode.R))
        {
            Reset();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SwitchHoverState();
        }
    }

    public void MoveUpDown(float input_perpendicular)
    {
        lift += input_perpendicular;
        //if (lift < 0f) lift = 0f;
    }

    public void MoveForwardBack(float input_vertical)
    {
        //Move forward or reverse
        //moveForwardBack = 0f;

        //moveForwardBack = input_vertical;
        throttle += input_vertical;
    }

    public void MoveLeftRight(float input_horizontal)
    {
        //Move left or right
        moveLeftRight = 0f;

        moveLeftRight = input_horizontal;
    }

    public void Yaw(float input_yaw)
    {
        //Rotate around the axis
        yawDir = 0f;
        yawDir = input_yaw;
    }

    public void SwitchHoverState()
    {
        is_hover = !is_hover;
    }
    

    void ManipulateMotors()
    {
        // change the direction of the motor
        if (is_hover)
        {
            propeller_back_left.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 140));
            propeller_back_right.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 140));
        }
        else
        {
            propeller_back_left.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
            propeller_back_right.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
        }
    }

    void CalculateMotorForce()
    {
        //Calculate the errors so we can use a PID controller to stabilize
        //Assume no error is if 0 degrees

        //Pitch
        //Returns positive if pitching forward
        float pitchError = GetPitchError();

        //Roll
        //Returns positive if rolling left
        float rollError = GetRollError() * -1f;


        //Yaw
        //Minimize the yaw error (which is already signed):
        float yawError = rb.angularVelocity.y;

        float speedError = GetSpeedError();
        float liftError = GetLiftError();

        //Debug.Log(pitchError + " " + rollError + " " + yawError);

        //Adapt the PID variables to the throttle
        //Vector3 PID_pitch_gains_adapted = throttle > 100f ? PID_pitch_gains * 2f : PID_pitch_gains;

        //Get the output from the PID controllers
        float PID_pitch_output = -PID_pitch.GetFactorFromPIDController(PID_pitch_gains, pitchError);
        float PID_roll_output = -PID_roll.GetFactorFromPIDController(PID_roll_gains, rollError);
        float PID_yaw_output = -PID_yaw.GetFactorFromPIDController(PID_yaw_gains, yawError);
        float PID_speed_output = -PID_speed.GetFactorFromPIDController(PID_speed_gains, speedError);
        float PID_lift_output = -PID_lift.GetFactorFromPIDController(PID_lift_gains, liftError);

        //throttle += PID_speed_output;

        //PID_lift_output = Mathf.Clamp(PID_lift_output, -1f, 1f);


        //Calculate the propeller forces
        //if (is_hover)
        if (true)
        {
            //Front
            //propellerForceFront = (lift + PID_lift_output - 2*PID_speed_output + (PID_pitch_output)) * 2f;
            propellerForceFront = Mathf.Lerp(
                propellerForceFront,
                lift + PID_lift_output + 2*PID_pitch_output,
                10 * Time.fixedDeltaTime
            );
            //Debug.Log(PID_lift_output + "|" + PID_speed_output + "|" + PID_pitch_output);

            if (GetSideSpeedError() != 0)
            {
                //moveLeftRight -= GetSideSpeedError();
            }
            //BR
            //propellerForceBR = (lift + PID_lift_output - PID_pitch_output + PID_roll_output + 30 * PID_yaw_output);
            propellerForceBR = Mathf.Lerp(
                propellerForceBR,
                lift + PID_lift_output - PID_pitch_output + 200 * PID_roll_output + 3000 * PID_yaw_output,
                10 * Time.fixedDeltaTime
            ); 

            //BL 
            //propellerForceBL = (lift + PID_lift_output - PID_pitch_output - PID_roll_output - 30 * PID_yaw_output);
            propellerForceBL = Mathf.Lerp(
                propellerForceBL,
                lift + PID_lift_output - PID_pitch_output - 200 * PID_roll_output - 3000 * PID_yaw_output,
                10 * Time.fixedDeltaTime
            );

            //if (rb.angularVelocity.y > 0)
            //{
            //    rb.angularVelocity = Vector3.Lerp(
            //        rb.angularVelocity,
            //        new Vector3(rb.angularVelocity.x, 0, rb.angularVelocity.z),
            //        10*Time.fixedDeltaTime); 
            //}

        }
        


        //Clamp
        propellerForceFront = Mathf.Clamp(propellerForceFront, 0f, maxPropellerForce);
        propellerForceBR = Mathf.Clamp(propellerForceBR, 0f, maxPropellerForce);
        propellerForceBL = Mathf.Clamp(propellerForceBL, 0f, maxPropellerForce);
        //Debug.Log(propellerForceFront + " | " + propellerForceBL + " | " + propellerForceBR);

        //Then we need to minimize the error
        rb.AddTorque(transform.up * throttle * PID_yaw_output * -1f);
    }

    void AddMotorForce()
    {

        //Add the force to the propellers
        AddForceToPropeller(propeller_front, 1.05f * propellerForceFront);
        //Debug.Log(propeller_front.transform.position);

        AddForceToPropeller(propeller_back_right, propellerForceBR);
        AddForceToPropeller(propeller_back_left, propellerForceBL);

        //First we need to add a force (if any)
        //rb.AddTorque(transform.up * yawDir * maxTorque * throttle);

        propeller_front_render.transform.Rotate(1.05f * propellerForceFront * propeller_front_render.transform.forward);
        propeller_back_right_render.transform.Rotate(propellerForceBR * propeller_back_right_render.transform.forward);
        propeller_back_left_render.transform.Rotate(propellerForceBL * propeller_back_left_render.transform.forward);
    }

    void AddForceToPropeller(GameObject propellerObj, float propellerForce)
    {
        // Model exported from blender all takes z as up direction,
        // however in unity z is forward, therefore we use forward here.
        // Please change this if different propeller config is used.
        Vector3 propellerUp = propellerObj.transform.up;

        Vector3 propellerPos = propellerObj.transform.position;

        rb.AddForceAtPosition(-propellerUp * propellerForce, propellerPos);

        //Debug
        Debug.DrawRay(propellerPos, propellerUp * propellerForce * 1f / 100f, Color.red);
    }

    //Pitch is rotation around x-axis
    //Returns positive if pitching forward
    private float GetPitchError()
    {
        //float xAngle = transform.eulerAngles.z - 19f;
        float xAngle = transform.localEulerAngles.z - 10*moveForwardBack;

        //Make sure the angle is between 0 and 360
        xAngle = WrapAngle(xAngle);

        //This angle going from 0 -> 360 when pitching forward
        //So if angle is > 180 then it should move from 0 to 180 if pitching back
        if (xAngle > 180f && xAngle < 360f)
        {
            xAngle = 360f - xAngle;

            //-1 so we know if we are pitching back or forward
            xAngle *= -1f;
        }

        return xAngle;
    }

    //Roll is rotation around z-axis
    //Returns positive if rolling left
    private float GetRollError()
    {
        float zAngle = transform.localEulerAngles.x;

        //Make sure the angle is between 0 and 360
        zAngle = WrapAngle(zAngle);

        //This angle going from 0-> 360 when rolling left
        //So if angle is > 180 then it should move from 0 to 180 if rolling right
        if (zAngle > 180f && zAngle < 360f)
        {
            zAngle = 360f - zAngle;

            //-1 so we know if we are rolling left or right
            zAngle *= -1f;
        }

        return zAngle;
    }


    private float GetSpeedError()
    {
        float speed = Vector3.Dot(rb.velocity, transform.right);
        return speed;
    }

    private float GetSideSpeedError()
    {
        float speed = Vector3.Dot(rb.velocity, transform.forward);
        return speed;
    }

    private float GetLiftError()
    {
        return Vector3.Dot(rb.velocity, transform.up);
    }

    //Wrap between 0 and 360 degrees
    float WrapAngle(float inputAngle)
    {
        //The inner % 360 restricts everything to +/- 360
        //+360 moves negative values to the positive range, and positive ones to > 360
        //the final % 360 caps everything to 0...360
        return ((inputAngle % 360f) + 360f) % 360f;
    }

    //Add external forces to the quadcopter, such as wind
    private void AddExternalForces()
    {
        //Important to not use the quadcopters forward
        Vector3 windDir = -Vector3.forward;

        //Rotate it 
        windDir = Quaternion.Euler(0, forceDir, 0) * windDir;

        rb.AddForce(windDir * windForce);

        //Debug
        //Is showing in which direction the wind is coming from
        //center of quadcopter is where it ends and is blowing in the direction of the line
        Debug.DrawRay(transform.position, -windDir * 3f, Color.red);
    }

    public Vector3 LocalizeVector(Vector3 v)
    {
        return Frame.InverseTransformVector(v);
    }
}
