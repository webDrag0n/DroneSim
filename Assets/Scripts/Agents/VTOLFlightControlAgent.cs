using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using MBaske;

public enum AgentMode
{
    Naive,
    Advanced
}

public class VTOLFlightControlAgent : Agent
{
    public AgentMode Mode;

    Rigidbody rBody;
    VTOLController controller;
    public Transform Target;
    private Vector3 InitTargetPos;
    private Transform FirstTarget;

    public Transform FrontEngine;
    public Transform BackLeftEngine;
    public Transform BackRightEngine;

    private float DistanceToTarget;
    private float PrevDistanceToTarget;

    public int timer;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        controller = GetComponent<VTOLController>();
        InitTargetPos = Target.position;
        FirstTarget = Target;
        timer = 0;
        
        DistanceToTarget = 0;
        PrevDistanceToTarget = 0;
    }

    //public override void Heuristic(in ActionBuffers actionsOut)
    //{
    //    var continuousActionsOut = actionsOut.ContinuousActions;
    //    continuousActionsOut[0] = Input.GetAxis("Horizontal");
    //    continuousActionsOut[1] = Input.GetAxis("Vertical");
    //}

    public override void OnEpisodeBegin()
    {
        timer = 0;
        Target = FirstTarget;
        // If the Agent fell, zero its momentum
        if (DistanceToTarget > 20)
        {
            controller.Reset();
            Target.localPosition = InitTargetPos; Target.localRotation = Quaternion.identity;
        }
        //controller.Reset();

        // Move the target to a new spot
        Target.localPosition += new Vector3(Random.value * 20 - 10,
                                           Random.value * 20 - 10,
                                           Random.value * 20 - 10);

        if (Target.localPosition.magnitude > 1000)
        {
            Target.localPosition = InitTargetPos; Target.localRotation = Quaternion.identity;
        }

    }

    Vector3 manual_output = Vector3.zero;
    Vector3 lerp_manual_output = Vector3.zero;

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float thrust = Input.GetAxis("Vertical_R");
        
        manual_output = new Vector3(thrust - vertical + 0.5f, thrust - horizontal / 10 + vertical, thrust + horizontal / 10 + vertical);
        //Debug.Log(thrust);
        //if (Input.GetKey(KeyCode.Space))
        //{
        //    manual_output += new Vector3(0.018f, 0.01f, 0.01f);
        //}
        //else
        //{

        //    manual_output -= new Vector3(0.018f, 0.01f, 0.01f);
        //    for (int i = 0; i < 3; i++)
        //    {
        //        if (manual_output.x < 0)
        //        {
        //            manual_output.x = 0;
        //        }
        //        if (manual_output.y < 0)
        //        {
        //            manual_output.y = 0;
        //        }
        //        if (manual_output.z < 0)
        //        {
        //            manual_output.z = 0;
        //        }
        //    }
        //}
        lerp_manual_output = manual_output;
        continuousActionsOut[0] = lerp_manual_output[0];
        continuousActionsOut[1] = lerp_manual_output[1];
        continuousActionsOut[2] = lerp_manual_output[2];
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(controller.Inclination);
        sensor.AddObservation(Normalization.Sigmoid(
            controller.LocalizeVector(rBody.velocity), 0.25f));
        sensor.AddObservation(Normalization.Sigmoid(
            controller.LocalizeVector(rBody.angularVelocity)));

        // Target and Agent positions
        sensor.AddObservation(Target.localPosition - transform.localPosition);
        sensor.AddObservation(transform.rotation);

        sensor.AddObservation(controller.propellerForceFront);
        sensor.AddObservation(controller.propellerForceBL);
        sensor.AddObservation(controller.propellerForceBR);
    }

    public float forceMultiplier = 10;
    Vector3 controlSignal = Vector3.zero;
    Vector2 thrustDirection = Vector2.zero;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2
        controlSignal.x = actionBuffers.ContinuousActions[0] + 1;
        controlSignal.y = actionBuffers.ContinuousActions[1] + 1;
        controlSignal.z = actionBuffers.ContinuousActions[2] + 1;

        if (Mode == AgentMode.Advanced)
        {
            // +1 eliminate negative, *45 => 0 to 90, +91 => 91 to 181, 90 degrees has gimbal lock
            thrustDirection.x = (actionBuffers.ContinuousActions[3] + 1) * 45 + 91;
            thrustDirection.y = (actionBuffers.ContinuousActions[4] + 1) * 45 + 91;
        }else if (Mode == AgentMode.Naive)
        {
            thrustDirection.x = 90;
            thrustDirection.y = 90;
        }

        controller.propellerForceFront = 1.4f * controlSignal[0] * forceMultiplier;
        controller.propellerForceBL = controlSignal[1] * forceMultiplier;
        controller.propellerForceBR = controlSignal[2] * forceMultiplier;

        BackLeftEngine.transform.localEulerAngles = new Vector3(thrustDirection.x, -76.375f, 0);
        BackRightEngine.transform.localEulerAngles = new Vector3(thrustDirection.y, -103.625f, 0);


        // Rewards
        PrevDistanceToTarget = DistanceToTarget;
        DistanceToTarget = Vector3.Distance(transform.position, Target.localPosition);


        //Debug.Log(DistanceToTarget);
        // Reached target
        if (DistanceToTarget < 1f)
        {
            //Debug.Log("Reached target");
            if (GetCumulativeReward() < 1000000000)
            {

                AddReward(controller.Frame.up.y);
                AddReward(rBody.velocity.magnitude * -0.2f);
                AddReward(rBody.angularVelocity.magnitude * -0.1f);
            }

            // limit speed
            //float speed = rBody.velocity.magnitude;
            //if (speed >= 0.2f)
            //{
            //    position_award = 0.2f + 0.5f / speed;
            //}
            //else
            //{
            //    position_award = 2.7f;
            //}
            //position_award = 0.7f;
            //SetReward(position_award + rotation_award);
        }
        //else
        //{
        //    controller.Reset();
        //    EndEpisode();
        //}
        //else
        //{
        //    if (DistanceToTarget == PrevDistanceToTarget)
        //    {
        //        AddReward(100);
        //    }
        //    else
        //    {
        //        AddReward(-0.0000001f * (DistanceToTarget - PrevDistanceToTarget));
        //    }
        //}

        // Flipped over
        //else if (this_rotation.x > 150 || this_rotation.x < -150 || this_rotation.z > 150 || this_rotation.z < -150)
        //if (this_rotation.x > 100 || this_rotation.x < -100 || this_rotation.z > 100 || this_rotation.z < -100)
        //{
        //    //Debug.Log("Flipped over");
        //    //position_award = 0.5f / (transform.position - Target.position).magnitude;
        //    //SetReward(-0.5f + position_award);
        //    if (GetCumulativeReward() > -1000000)
        //    {
        //        AddReward(0.000002f);
        //    }
        //    else
        //    {
        //        EndEpisode();
        //    }
        //    //controller.Reset();
        //    //EndEpisode();
        //}
        //else
        //{
        //    if (GetCumulativeReward() < 1000000)
        //    {
        //        AddReward(-0.000002f);
        //    }
        //    else
        //    {
        //        EndEpisode();
        //    }
        //}


        // Drifted too far
        if (DistanceToTarget > 20)
        {
            EndEpisode();
        }

        //if (timer > 1000)
        //{
        //    controller.Reset();
        //    EndEpisode();
        //}
        //else
        //{
        //    timer += 1;
        //}

    }
}
