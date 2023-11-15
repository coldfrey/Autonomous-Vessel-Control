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

    private float checkInterval = 5f;
    private Coroutine checkPositionRoutine;

    public UnityEngine.UI.Text rewardText;

    public UnityEngine.UI.Text actionText;

    public UnityEngine.UI.Text completedWaypointsText;
    public UnityEngine.UI.Text lessonText;

    private Vector3 startPosition;
    private Quaternion startRotation;

    private Vector3 startKitePosition;

    private Quaternion startKiteRotation;

    private bool hitFinish = false;

    private bool isFirstEpisode = true;

    public int lessonNumber = 0;
    
    public float goneTheDistance = 0;

    public float lvl0Success = 5;
    private bool hitLessonTarget = false;

    private const float KiteStableHeight = 10f; // Example stable height for the kite
    private const float KiteHeightRewardMultiplier = 0.1f;

    private int[] lastActions = new int[2];
    private float currentRudderAngle = 0f;
    private float lastRudderAngle = 0f;

    public override void OnEpisodeBegin()
    {
        // jointSim.ResetKite();
        if (isFirstEpisode)
        {
            isFirstEpisode = false;
            startPosition = transform.position;
            // startRotation = transform.rotation;
            startKitePosition = kiteRigidbody.transform.position;
            startKiteRotation = kiteRigidbody.transform.rotation;

        }
        if (lessonNumber != 0 && waypointManager.GetWaypoint() == null)
        {
            waypointManager.CreateWaypoint();
        }
        if (lessonNumber == 3) checkInterval = 10f;
        if (lessonNumber == 4) checkInterval = 20f;
        // ResetEnvironment();
        StartCoroutine(ResetEnvironment());
        previousPosition = transform.position;
        if (checkPositionRoutine != null)
        {
            StopCoroutine(checkPositionRoutine);
        }
        checkPositionRoutine = StartCoroutine(CheckPosition());
        // StartCoroutine(WaitAndRequestDecision());
        SetReward(0f);
        Debug.Log("OnEpisodeBegin, post ResetEnvironment");
        RequestDecision();
    }
    private IEnumerator ResetEnvironment() 
    {

        yield return null;
        
        kiteRigidbody.velocity = Vector3.zero;
        kiteRigidbody.angularVelocity = Vector3.zero;
        boatRigidbody.velocity = Vector3.zero;
        boatRigidbody.angularVelocity = Vector3.zero;
        boatRigidbody.transform.GetChild(0).GetChild(0).GetComponent<Rigidbody>().velocity = Vector3.zero;
        boatRigidbody.transform.GetChild(0).GetChild(0).GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        
        rudder.angle = 0f;
        rudder.SetRudderTargetAngle(0f);
        jointSim.StopSteering();

        startRotation = Quaternion.Euler(0, Random.Range(90, 270), 0);
        transform.SetPositionAndRotation(startPosition + new Vector3(0, 2, 0), startRotation);
        kiteRigidbody.transform.SetPositionAndRotation(startKitePosition, startKiteRotation);

        if (hitFinish)
        {
            waypointManager.NewWaypoint(lessonNumber);
            hitFinish = false;
            
        }
        kiteRigidbody.velocity = Vector3.zero;
        kiteRigidbody.angularVelocity = Vector3.zero;
        boatRigidbody.velocity = Vector3.zero;
        boatRigidbody.angularVelocity = Vector3.zero;
        boatRigidbody.transform.GetChild(0).GetChild(0).GetComponent<Rigidbody>().velocity = Vector3.zero;
        boatRigidbody.transform.GetChild(0).GetChild(0).GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        // goneTheDistance = 0;
        hitLessonTarget = false;
        if (lessonNumber != 0 && waypointManager.GetWaypoint() == null)
        {
            waypointManager.CreateWaypoint();
        }

        yield return new WaitForSeconds(1.0f);
    }
    

    public void FixedUpdate()
    {
        // try catch edge cases
        if (GetCumulativeReward() < -100.0f || rudder.GetBoatForwardSpeed() > 100.0f || transform.position.y > 10.0f)
        {
            print("GetCumulativeReward: " + GetCumulativeReward() + ", rudder.GetBoatForwardSpeed(): " + rudder.GetBoatForwardSpeed() + ", transform.position.y: " + transform.position.y);
            CompleteEpisode();
        }

        boatForwardSpeed = rudder.GetBoatForwardSpeed();

        // reward: greater +ve velocity
        if (boatForwardSpeed > 1f) {
            AddReward(boatForwardSpeed * 0.005f);
            if (boatForwardSpeed > 4f)
            {
                AddReward(boatForwardSpeed * 0.01f);
            }
        }
        else if (boatForwardSpeed < 0.1f)
        {
            AddReward(-0.001f);
        }
        // reward if the boat is moving towards the waypoint
        if (lessonNumber != 0 && waypointManager.GetWaypoint() != null)
        {
            currentDistanceToWaypoint = Vector3.Distance(transform.position, waypointManager.GetWaypoint().transform.position);
            previousDistanceToWaypoint = Vector3.Distance(previousPosition, waypointManager.GetWaypoint().transform.position);        
            if (currentDistanceToWaypoint < previousDistanceToWaypoint)
            {
                // AddReward(0.001f);
                // make the reward inversely proportional to the distance
                AddReward(10f / currentDistanceToWaypoint);
            }
            else if (currentDistanceToWaypoint > previousDistanceToWaypoint)
            {
                AddReward(-0.5f * (currentDistanceToWaypoint- previousDistanceToWaypoint));
            }
            previousPosition = transform.position;

            if (gjkCollisionDetection.GJK(gameObject.GetComponent<MeshFilter>(), transform.position, waypointManager.GetWaypoint().GetComponent<MeshFilter>(), waypointManager.GetWaypoint().transform.position))
            {
                Debug.Log("Finish");
                hitFinish = true;
                AddReward(200.0f);
                // CompleteEpisode();
                waypointManager.NewWaypoint(lessonNumber);
            }

            currentRudderAngle = rudder.angle;
            if (Mathf.Abs(currentRudderAngle) == Mathf.Abs(lastRudderAngle) && Mathf.Abs(currentRudderAngle) > 45.0f)
            {
                AddReward(-0.001f * (Mathf.Abs(currentRudderAngle)));
            }
            lastRudderAngle = currentRudderAngle;
            
        }

        AddReward(-0.001f);
        CheckLessonSuccess();
        completedWaypointsText.text = "Completed Waypoints: " + waypointManager.GetCompletedWaypoints().ToString();
        if (lessonNumber == 0)
        {
            lessonText.text = "Lesson: 0, " + goneTheDistance.ToString() + "/" + lvl0Success.ToString() + ", " + (500f - Vector3.Distance(transform.position, startPosition)).ToString("0.0") + "m to go";
        }
        else 
        {
            lessonText.text = "Lesson: " + lessonNumber.ToString();
        }
    }

    private void CheckLessonSuccess()
    {
        // to graduate from lesson 0 to 1, the boat must travel 500m from the start position 5 times
        if (lessonNumber == 0 && Vector3.Distance(transform.position, startPosition) > 500f)
        {
            if (hitLessonTarget) return;
            goneTheDistance ++;
            hitLessonTarget = true;
            if (goneTheDistance >= lvl0Success)
            {
                lessonNumber ++;
                waypointManager.CreateWaypoint();
                goneTheDistance = 0;
            }
        }
        // to graduate from lesson 1 to 2, the boat must complete 10 waypoints in the 45 degree window
        if (lessonNumber == 1 && waypointManager.GetCompletedWaypoints() > 10f)
        {
            lessonNumber ++;
        }
        if (lessonNumber == 2 && waypointManager.GetCompletedWaypoints() > 50f)
        {
            lessonNumber ++;
        }
        if (lessonNumber == 3 && waypointManager.GetCompletedWaypoints() > 80f)
        {
            lessonNumber ++;
        }
        if (lessonNumber == 4 && waypointManager.GetCompletedWaypoints() > 150f)
        {
            lessonNumber ++;
        }

    }

    private IEnumerator CheckPosition()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            if (lessonNumber != 0)
            {
                float startDistanceToWaypoint = Vector3.Distance(startPosition, waypointManager.GetWaypoint().transform.position);
                // If the boat if further away now than it started end episode
                if (currentDistanceToWaypoint >= startDistanceToWaypoint + 10.0f)
                {               
                    print("currentDistanceToWaypoint: " + currentDistanceToWaypoint + ", started at position: " + startDistanceToWaypoint);
                    AddReward(-10.0f);
                    CompleteEpisode();
                }
            }
        }
    }

    private Vector3 PolarAngles(Vector3 kitePosition, Vector3 wind)
    {
        Vector3 basePosition = boatRigidbody.position + new Vector3(0, 1f, 0);
        // Debug.DrawLine(basePosition, basePosition + kitePosition, Color.green, 1.0f);
        // Debug.Log("kitePosition: " + kitePosition);
        // wind projected on floor
        Vector3 windOnFloor = new Vector3(wind.x, 0, wind.z);
        // kite position projected on floor
        Vector3 kitePositionOnFloor = Vector3.ProjectOnPlane(kitePosition, Vector3.up);
        // angle between wind and kite
        // Debug.DrawLine(basePosition, basePosition + windOnFloor, Color.red, 1.0f);
        // Debug.DrawLine(basePosition, basePosition + kitePositionOnFloor, Color.blue, 1.0f);

        float angleHorizontal = Vector3.Angle(windOnFloor, kitePositionOnFloor);
        bool angleHorizontalSign = Vector3.Cross(windOnFloor, kitePositionOnFloor).y < 0;
        if (angleHorizontalSign)
        {
            angleHorizontal = -angleHorizontal;
        }
        // wind projected on vertical plane as defined by direction of kitePositionOnFloor and up.
        // Vector3 windOnVerticalPlane = Vector3.ProjectOnPlane(wind, Vector3.Cross(kitePositionOnFloor, Vector3.up));
        // Debug.DrawLine(basePosition, basePosition + windOnVerticalPlane, Color.yellow, 1.0f);
        // angle between windOnVerticalPlane and kitePosition
        float angleVertical = Vector3.Angle(kitePositionOnFloor, kitePosition);
        // kite transform.forward projected on plane defined by kitePositionOnFloor
        Vector3 kiteForwardOnPlane = Vector3.ProjectOnPlane(kiteRigidbody.transform.forward, kitePositionOnFloor);
        // Debug.DrawLine(basePosition, basePosition + kiteForwardOnPlane, Color.magenta, 1.0f);
        // angle between kiteForwardOnPlane and up
        float angleKiteUp = Vector3.Angle(kiteForwardOnPlane, Vector3.up);

        return new Vector3(angleHorizontal, angleVertical, angleKiteUp);

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 boatPosition = transform.position;
        Vector3 boatForward = transform.forward;
        Vector3 wind = jointSim.windAtKite;
        float distanceToWaypoint;
        if (lessonNumber == 0)
        {
            distanceToWaypoint = 0;
        }
        else 
        {
            Vector3 waypointPosition = waypointManager.GetWaypoint().transform.position;
            distanceToWaypoint = Vector3.Distance(boatPosition, waypointPosition);
        }

        // Distance to waypoint
        sensor.AddObservation(distanceToWaypoint);
        // Boat speed
        sensor.AddObservation(boatForwardSpeed);
        // Wind
        sensor.AddObservation(wind);
        // kite polar angles
        sensor.AddObservation(PolarAngles(kiteRigidbody.transform.localPosition, wind));

        // Calculate the relative wind direction in the horizontal plane
        Vector3 windDirectionOnHorizontalPlane = Vector3.ProjectOnPlane(wind, Vector3.up).normalized;
        Vector3 boatForwardOnHorizontalPlane = Vector3.ProjectOnPlane(boatForward, Vector3.up).normalized;

        // Calculate the angle using atan2 for a smooth transition
        float angleToWind = Mathf.Atan2(
            Vector3.Dot(Vector3.Cross(boatForwardOnHorizontalPlane, windDirectionOnHorizontalPlane), Vector3.up),
            Vector3.Dot(boatForwardOnHorizontalPlane, windDirectionOnHorizontalPlane)
        );

        // Normalize the angle to be between 0 and 1
        float normalizedAngleToWind = (angleToWind + Mathf.PI) / (2 * Mathf.PI);

        sensor.AddObservation(normalizedAngleToWind);

        // Additional observations for kite control
        Vector3 kiteVelocityRelative = kiteRigidbody.velocity - boatRigidbody.velocity;
        sensor.AddObservation(kiteVelocityRelative); // Relative velocity of kite to boat

        float normalizedKiteHeight = Mathf.Clamp01((kiteRigidbody.transform.localPosition.y - 1f) / (KiteStableHeight - 1f));
        sensor.AddObservation(normalizedKiteHeight); // Kite height relative to water


        float normalizedRudderAngle = rudder.angle / 90.0f;
        sensor.AddObservation(normalizedRudderAngle);

        // Kite position relative to boat, normalised relative position rotated to boat view and distance
        // Vector3 kiteRelativePosition = kiteRigidbody.transform.position - transform.position;
        // Vector3 kiteRelativePositionNormalised = kiteRelativePosition.normalized;
        // sensor.AddObservation(Quaternion.Inverse(transform.rotation) * kiteRelativePositionNormalised);
        // sensor.AddObservation(kiteRelativePosition.magnitude);
        // print("angle of boat to wind: " + angleToWind * Mathf.Rad2Deg);

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

        if (lastActions[0] != rudderAction || lastActions[1] != kiteControlAction)
        {
            AddReward(-0.1f);
        }
        lastActions[0] = rudderAction;
        lastActions[1] = kiteControlAction;

        // if the kite is less that 1m above the water, end the episode
        if (kiteRigidbody.transform.localPosition.y < 1.0f)
        {
            AddReward(-10f);
            CompleteEpisode();
        }

        if (PolarAngles(kiteRigidbody.transform.localPosition, jointSim.windAtKite).y >15)
        {
            AddReward(0.01f);
            if (PolarAngles(kiteRigidbody.transform.localPosition, jointSim.windAtKite).y > 45.0f)
            {
                AddReward(0.05f);
            }
        }

        // negativly reward if the rudder is greater than 45 degrees by increasing the reward the further away from 45 degrees
        if (Mathf.Abs(rudder.angle) > 50.0f)
        {
            AddReward(-0.01f * (Mathf.Abs(rudder.angle) - 50.0f));
        }

        // float heightDifference = Mathf.Abs(kiteRigidbody.transform.localPosition.y - KiteStableHeight);
        // if (heightDifference < 2f) // Kite is within the desired height range
        // {
        //     AddReward((2f - heightDifference) * KiteHeightRewardMultiplier);
        // }
        // else
        // {
        //     AddReward(-heightDifference * KiteHeightRewardMultiplier);
        // }

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
        // ResetEnvironment();
        StartCoroutine(ResetEnvironment());
        StopCoroutine(checkPositionRoutine);
        // StartCoroutine(WaitAndRequestDecision());
        // ResetEnvironment();
        EndEpisode();
    }

}