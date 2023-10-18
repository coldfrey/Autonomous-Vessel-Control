using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using PathCreation.Examples;

public class BoatPPOAgent : Agent
{
    public Rigidbody boatRigidbody;
    // public BoatController boatController;
    public PropellerBoats propellerBoats;
    public GenerateCourse pathGenerator; // Reference to the path generator

    private int currentWaypointIndex = 0;
    private float previousDistanceToWaypoint = Mathf.Infinity;

    private bool hasGoneThroughApproachSegment = false;

    // For velocity-based rewards
    private readonly float lowVelocityThreshold = 0.5f;
    private readonly float highVelocityThreshold = 2.0f;
    public override void OnEpisodeBegin()
    {
        boatRigidbody.velocity = Vector3.zero;
        boatRigidbody.angularVelocity = Vector3.zero;
        currentWaypointIndex = 0;
        Debug.Log("Calling ResetPath");
        pathGenerator.RegeneratePath();
        RequestDecision();
    }

    public void ApproachSegmentEntered(int buoyIndex)
    {
        if (buoyIndex == currentWaypointIndex) hasGoneThroughApproachSegment = true;
    }

    public void ExitSegmentEntered(int buoyIndex)
    {
        if (hasGoneThroughApproachSegment)
        {
            AddReward(10.0f);
            hasGoneThroughApproachSegment = false;
            currentWaypointIndex++;
        } else
        {
            AddReward(-10.0f);
            EndEpisode();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Relative position to the next waypoint
        Transform[] waypoints = pathGenerator.getWaypoints();
        Vector3 relativePosition = waypoints[currentWaypointIndex].position - transform.position;
        // direction
        sensor.AddObservation(relativePosition.normalized);
        // distance
        sensor.AddObservation(relativePosition.magnitude);
        Debug.Log("relativePosition: " + relativePosition);

        // throttle and ruder values
        sensor.AddObservation(propellerBoats.engine_rpm);
        sensor.AddObservation(propellerBoats.angle);

        // Boat's velocity
        sensor.AddObservation(boatRigidbody.velocity);
        // Boat's poise
        sensor.AddObservation(transform.rotation);
        RequestDecision();
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        float throttle = actions.ContinuousActions[0];
        float rudder = actions.ContinuousActions[1];
        print("throttle: " + throttle + ", rudder: " + rudder);
        // boatController.OnMove(throttle);

        if (throttle > -0.1f)
            propellerBoats.ThrottleUp();
        else if (throttle < -0.1f)
            propellerBoats.ThrottleDown();
        
        // Control the rudder based on the action
        if (rudder > 0.1f)
            propellerBoats.RudderRight();
        else if (rudder < -0.1f)
            propellerBoats.RudderLeft();

        if (rudder ==-1) AddReward(-0.5f);
        else if (rudder == 1) AddReward(-0.5f);
        // Reward based on the proximity to the next waypoint
        Transform[] waypoints = pathGenerator.getWaypoints();



        float distanceToWaypoint = Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position);
        if (distanceToWaypoint < previousDistanceToWaypoint)
        {
            AddReward(0.02f);
        }
        else
        {
            AddReward(-0.01f);
        }
        previousDistanceToWaypoint = distanceToWaypoint;
        // if (distanceToWaypoint < 5f) // Some threshold distance to consider reaching a waypoint
        // {
        //     AddReward(1.0f); // Reward for reaching the waypoint
        //     if (currentWaypointIndex >= pathGenerator.numberOfWaypoints)
        //     {
        //         AddReward(2.0f); // Bonus reward for completing the path
        //         EndEpisode();
        //     }
        // }
        // else
        // {
        //     AddReward(-0.01f); // Small penalty for every step to encourage faster completion
        // }

        Vector3 toWaypoint = waypoints[currentWaypointIndex].position - transform.position;
        distanceToWaypoint = toWaypoint.magnitude;
        float directionDotProduct = Vector3.Dot(transform.forward, toWaypoint.normalized);
        AddReward(0.01f * directionDotProduct);



        // Check the boat's velocity and assign rewards or penalties accordingly
        EvaluateAgentPerformanceByVelocity();
        float cumulativeReward = GetCumulativeReward();
        if(cumulativeReward < -5000 ) EndEpisode();
        Debug.Log("Reward: " + cumulativeReward);

        RequestDecision();
    }
    
    private void EvaluateAgentPerformanceByVelocity()
    {
        if (boatRigidbody.velocity.magnitude < lowVelocityThreshold)
        {
            // Penalize the agent for low velocity
            AddReward(-1f);
        }
        else if (boatRigidbody.velocity.magnitude > highVelocityThreshold)
        {
            // Reward the agent for maintaining a high velocity
            AddReward(1f);
        }
        RequestDecision();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
        RequestDecision();
    }
}
