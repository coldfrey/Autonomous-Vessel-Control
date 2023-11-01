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

    public RewardPlotter rewardPlotter; // Reference to the reward plotter

    public UnityEngine.UI.Text rewardText; // Reference to the UI text element to display the cumulative reward

    public UnityEngine.UI.Text actionText; // Reference to the UI text element to display the action

    public UnityEngine.UI.Text currentBuoyIdxText; // Reference to the UI text element to display the current buoy index

    private int currentWaypointIndex = 0;
    private float previousDistanceToWaypoint = Mathf.Infinity;

    private bool hasGoneThroughApproachSegment = false;

    // For velocity-based rewards
    private readonly float lowVelocityThreshold = 0.5f;
    private readonly float highVelocityThreshold = 2.0f;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private float episodeDuration;

    private float originalDistanceToWaypoint = Mathf.Infinity;
    public override void OnEpisodeBegin()
    {
        boatRigidbody.velocity = Vector3.zero;
        boatRigidbody.angularVelocity = Vector3.zero;
        transform.position = startPosition;
        transform.rotation = startRotation;
        currentWaypointIndex = 0;
        // Debug.Log("Calling ResetPath");
        // pathGenerator.RegeneratePath();
        originalDistanceToWaypoint = Vector3.Distance(transform.position, pathGenerator.getWaypoints()[currentWaypointIndex].position);
        currentBuoyIdxText.text = "Current Buoy Index: " + currentWaypointIndex;
        episodeDuration = 0;
        RequestDecision();
    }

    void ResetEnvironment()
    {
        boatRigidbody.velocity = Vector3.zero;
        boatRigidbody.angularVelocity = Vector3.zero;
        transform.position = startPosition + new Vector3(0, 1, 0);
        transform.rotation = startRotation;
        currentWaypointIndex = 0;
        propellerBoats.throttle = 0.5f;
        propellerBoats.engine_rpm = 1000F;
        propellerBoats.angle = 0f;
        // pathGenerator.appr = 0;
        // pathGenerator.exit = 0;
        // Debug.Log("Calling ResetPath");
        hasGoneThroughApproachSegment = false;
    }
    public void ApproachSegmentEntered(int buoyIndex)
    {
        if (buoyIndex == currentWaypointIndex) hasGoneThroughApproachSegment = true;
        if (buoyIndex >= currentWaypointIndex) {
            AddReward(-50.0f);
            EndEpisode();
        }
    }

    public void ExitSegmentEntered(int buoyIndex)
    {
        if (hasGoneThroughApproachSegment)
        {
            AddReward(500.0f);
            hasGoneThroughApproachSegment = false;
            currentWaypointIndex++;
            currentBuoyIdxText.text = "Current Buoy Index: " + currentWaypointIndex;
            originalDistanceToWaypoint = Vector3.Distance(transform.position, pathGenerator.getWaypoints()[currentWaypointIndex].position);
        } else
        {
            print("Entered exit segment without going through approach segment");
            AddReward(-2000.0f);
            EndEpisode();
            ResetEnvironment();
        }
    }

    public void FixedUpdate()
    {
        episodeDuration += Time.fixedDeltaTime;
        if (episodeDuration > 300.0f) {
            EndEpisode();
            ResetEnvironment();
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
        // Debug.Log("relativePosition: " + relativePosition);

        // throttle and ruder values
        sensor.AddObservation(propellerBoats.engine_rpm);
        sensor.AddObservation(propellerBoats.angle);

        // Boat's velocity
        sensor.AddObservation(boatRigidbody.velocity);
        sensor.AddObservation(boatRigidbody.angularVelocity);
        // Boat's poise
        sensor.AddObservation(transform.rotation);
        // RequestDecision();

        // give it the time 
        sensor.AddObservation(episodeDuration);
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        float throttle = actions.DiscreteActions[0];
        float rudder = actions.DiscreteActions[1];
        // print("throttle: " + throttle + ", rudder: " + rudder);
        // boatController.OnMove(throttle);
        actionText.text = "Throttle: " + throttle.ToString("0.00") + ", Rudder: " + rudder.ToString("0.00");

        // if (throttle > -0.1f)
        //     propellerBoats.ThrottleUp();
        // else if (throttle < -0.1f)
        //     propellerBoats.ThrottleDown();
        
        // // Control the rudder based on the action
        // if (rudder > 0.1f)
        //     propellerBoats.RudderRight();
        // else if (rudder < -0.1f)
        //     propellerBoats.RudderLeft();

        if (throttle == 0) propellerBoats.ThrottleUp();
        else if (throttle == 2) propellerBoats.ThrottleDown();

        if (rudder == 0) propellerBoats.RudderRight();
        else if (rudder == 2) propellerBoats.RudderLeft();

        // Reward based on the rudder value
        AddReward(-0.001f * Mathf.Abs(propellerBoats.angle));

        // if (rudder ==-1) AddReward(-2f);
        // else if (rudder == 1) AddReward(-2f);
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

        if (distanceToWaypoint > originalDistanceToWaypoint) {
            AddReward(Mathf.Pow((distanceToWaypoint - originalDistanceToWaypoint), 2)* -0.01f);
        }
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

        // negative reward for angular velocity
        AddReward(-1f * boatRigidbody.angularVelocity.magnitude);

        // offset sigmoid reward for positive velocity
        AddReward(0.01f * (1.0f / (1.0f + Mathf.Exp(-boatRigidbody.velocity.magnitude)) - 0.5f));
        

        // Check the boat's velocity and assign rewards or penalties accordingly
        EvaluateAgentPerformanceByVelocity();
        float cumulativeReward = GetCumulativeReward();
        if(cumulativeReward < -5000 ) {
            EndEpisode(); 
            ResetEnvironment();
        }
        // Debug.Log("Reward: " + cumulativeReward);
        if (rewardPlotter != null) rewardPlotter.AddReward(cumulativeReward);
        rewardText.text = "Reward: " + cumulativeReward.ToString("0.00");
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
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // var continuousActionsOut = actionsOut.ContinuousActions;
        // continuousActionsOut[0] = Input.GetAxis("Vertical");
        // continuousActionsOut[1] = Input.GetAxis("Horizontal");
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 1;
        discreteActionsOut[1] = 0;
        RequestDecision();
    }
}
