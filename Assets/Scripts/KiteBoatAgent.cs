using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using PathCreation.Examples;

public class KiteBoatAgent : Agent
{
    public Rudder rudder;

    public JointSim jointSim;

    public Rigidbody kiteRigidbody;

    public Rigidbody boatRigidbody;

    public WaypointManager waypointManager;

    private GJKCollisionDetection gjkCollisionDetection = new GJKCollisionDetection();

    private const int RudderActionSize = 3; // left, none, right for rudder
    private const int KiteControlActionSize = 3; // left, none, right for kite control

    private Vector3 previousPosition; // To store the previous position

    private float boatForwardSpeed;

    private float currentDistanceToWaypoint;
    private float previousDistanceToWaypoint;

    private float lastDistanceToWaypoint;
    private float checkInterval = 5f;
    private Coroutine checkPositionRoutine;

    public UnityEngine.UI.Text rewardText;

    public UnityEngine.UI.Text actionText;

    public UnityEngine.UI.Text completedWaypointsText;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private Vector3 startKitePosition;

    private Quaternion startKiteRotation;

    private bool hitFinish = false;

    private bool isFirstEpisode = true;

    public override void OnEpisodeBegin()
    {
        // jointSim.ResetKite();
        if (isFirstEpisode)
        {
            isFirstEpisode = false;
            startPosition = transform.position;
            startRotation = transform.rotation;
            startKitePosition = kiteRigidbody.transform.localPosition;
            startKiteRotation = kiteRigidbody.transform.localRotation;

        }
        previousPosition = transform.position;
        lastDistanceToWaypoint = waypointManager.distanceFromBoat;
        if (checkPositionRoutine != null)
        {
            StopCoroutine(checkPositionRoutine);
        }
        checkPositionRoutine = StartCoroutine(CheckPosition());
        // StartCoroutine(WaitAndRequestDecision());
        ResetEnvironment();
        Debug.Log("OnEpisodeBegin, post ResetEnvironment");
        RequestDecision();
    }

    // IEnumerator WaitAndRequestDecision()
    // {
    //     ResetEnvironment();
    //     yield return new WaitForSeconds(1.0f);  // Wait for 1 second
    //     RequestDecision();
    // }
    private void ResetEnvironment() 
    {
        rudder.angle = 0f;
        rudder.SetRudderTargetAngle(0f);
        jointSim.StopSteering();
        kiteRigidbody.velocity = Vector3.zero;
        kiteRigidbody.angularVelocity = Vector3.zero;
        boatRigidbody.velocity = Vector3.zero;
        boatRigidbody.angularVelocity = Vector3.zero;
        boatRigidbody.transform.GetChild(0).GetChild(0).GetComponent<Rigidbody>().velocity = Vector3.zero;
        boatRigidbody.transform.GetChild(0).GetChild(0).GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        transform.position = startPosition + new Vector3(0, 1, 0);
        transform.rotation = startRotation;

        // set the local position of the kite to the start position
        kiteRigidbody.transform.localPosition = startKitePosition;
        kiteRigidbody.transform.localRotation = startKiteRotation;


        if (hitFinish)
        {
            waypointManager.NewWaypoint();
            hitFinish = false;
            
        }
    }

    public void FixedUpdate()
    {
        boatForwardSpeed = rudder.GetBoatForwardSpeed();

        // reward: greater +ve velocity
        AddReward(boatForwardSpeed * 0.001f);

        // reward if the boat is moving towards the waypoint
        currentDistanceToWaypoint = Vector3.Distance(transform.position, waypointManager.waypoint.transform.position);
        previousDistanceToWaypoint = Vector3.Distance(previousPosition, waypointManager.waypoint.transform.position);        
        if (currentDistanceToWaypoint < previousDistanceToWaypoint)
        {
            // AddReward(0.001f);
            // make the reward inversely proportional to the distance
            AddReward(0.01f / currentDistanceToWaypoint);

        }
        previousPosition = transform.position;

        if (gjkCollisionDetection.GJK(gameObject.GetComponent<MeshFilter>(), transform.position, waypointManager.waypoint.GetComponent<MeshFilter>(), waypointManager.waypoint.transform.position))
        {
            Debug.Log("Finish");
            hitFinish = true;
            AddReward(10.0f);
            CompleteEpisode();
        }

        completedWaypointsText.text = "Completed Waypoints: " + waypointManager.GetCompletedWaypoints().ToString();

        AddReward(-0.001f);
    }

    private IEnumerator CheckPosition()
    {
        while (true)
        {
            // Wait for 5 seconds
            yield return new WaitForSeconds(checkInterval);

            // If the boat hasn't moved closer to the waypoint, end the episode
            if (currentDistanceToWaypoint >= lastDistanceToWaypoint)
            {               
                print("currentDistanceToWaypoint: " + currentDistanceToWaypoint + ", lastDistanceToWaypoint: " + lastDistanceToWaypoint);
                AddReward(-8.0f);
                CompleteEpisode();
            }

            // Update the last distance for the next check
            lastDistanceToWaypoint = currentDistanceToWaypoint;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Waypoint position in world space
        Vector3 waypointPosition = waypointManager.waypoint.transform.position;
        sensor.AddObservation(waypointPosition);

        // Boat position in world space
        Vector3 boatPosition = transform.position;
        sensor.AddObservation(boatPosition);

        // Boat speed
        sensor.AddObservation(boatForwardSpeed);

        // Distance to waypoint
        float distanceToWaypoint = Vector3.Distance(boatPosition, waypointPosition);
        sensor.AddObservation(distanceToWaypoint);

        // Kite position relative to boat, normalised relative position rotated to boat view and distance
        Vector3 kiteRelativePosition = kiteRigidbody.transform.position - transform.position;
        Vector3 kiteRelativePositionNormalised = kiteRelativePosition.normalized;
        sensor.AddObservation(Quaternion.Inverse(transform.rotation) * kiteRelativePositionNormalised);
        sensor.AddObservation(kiteRelativePosition.magnitude);

    }

    public override void OnActionReceived(ActionBuffers actions)
    {

        int rudderAction = actions.DiscreteActions[0];
        int kiteControlAction = actions.DiscreteActions[1];

        actionText.text = "Rudder: " + rudderAction + ", Kite Control: " + kiteControlAction;

        // discrete actions
        // Rudder control (left, right, none)
        if (rudderAction == 1)
        {
            rudder.RudderLeft();
        }
        else if (rudderAction == 2)
        {
            rudder.RudderRight();
        }

        // Kite control (left, right, none)
        if (kiteControlAction == 0)
        {
            jointSim.SteerLeft();
        }
        else if (kiteControlAction == 1)
        {
            jointSim.StopSteering();
        }
        else if (kiteControlAction == 2)
        {
            jointSim.SteerRight();
        }

        rewardText.text = "Reward: " + GetCumulativeReward().ToString("0.0000");

        RequestDecision();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Implementation of heuristic for manual control
        // It's useful for testing the agent with keyboard input
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut.Clear();

        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 1; // Rudder left
        }
        else if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 2; // Rudder right
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActionsOut[1] = 0; // Kite control left
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            discreteActionsOut[1] = 2; // Kite control right
        }
        else
        {
            discreteActionsOut[1] = 1; // Kite control none
        }
    }

    public void CompleteEpisode()
    {
        StopCoroutine(checkPositionRoutine);
        // StartCoroutine(WaitAndRequestDecision());
        // ResetEnvironment();
        EndEpisode();
    }

}