using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using PathCreation.Examples;

using static GJKCollisionDetection;


public class PathAgent : Agent
{
    private GJKCollisionDetection gjkCollisionDetection = new GJKCollisionDetection();
    private Vector3 startPosition;
    private Quaternion startRotation;

    // private MeshFilter[] approachSegmentMeshFilters = new MeshFilter[3];
    // private Vector3[] approachSegmentMeshCenters = new Vector3[3];
    // private MeshFilter[] exitSegmentMeshFilters = new MeshFilter[3];
    // private Vector3[] exitSegmentMeshCenters = new Vector3[3];
    private GameObject[] approachSegmentMeshFilters = new GameObject[3];
    private GameObject[] exitSegmentMeshFilters = new GameObject[3];

    private MeshFilter navigatorMeshFilter;

    private float previousPolicedDistanceTravelled = 0.0f;

    public GenerateCourse pathGenerator; // Reference to the path generator
    
    public PathFollower pathFollower;

    private bool outOfBoundsEpisodeTimerStarted = false;

    private int currentWaypointIndex = 0;

    private int directionActionSpaceSize = 6;


    public float velocity = 10.0f;

    private Vector3 direction = Vector3.zero;

    public float episodeDuration = 0.0f;

    public UnityEngine.UI.Text actionText; // Reference to the UI text element to display the action

    public UnityEngine.UI.Text currentBuoyIdxText; // Reference to the UI text element to display the current buoy index

    public UnityEngine.UI.Text rewardText; // Reference to the UI text element to display the cumulative reward

    public UnityEngine.UI.Text countdownText; // Reference to the UI text element to display the countdown
    private float cumulativeReward = 0.0f;
    private bool hasGoneThroughApproachSegment = false;

    private float originalDistanceToWaypoint = 0.0f;

    private Vector3 previousPosition; // To store the previous position

    public float maxEpisodeDuration = 100.0f;


    private void Start()
    {
        navigatorMeshFilter = GetComponent<MeshFilter>();
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    public override void OnEpisodeBegin()
    {
        print("OnEpisodeBegin");
        transform.position = startPosition;
        transform.rotation = startRotation;
        currentWaypointIndex = 0;
        cumulativeReward = 0.0f;
        pathFollower.resetPosition();
        // for each buoy for each segment, get the mesh filter
        for (int i = 0; i < pathGenerator.raceBuoys.Count; i++)
        {
            approachSegmentMeshFilters[i] = pathGenerator.raceBuoys[i].transform.GetChild(0).gameObject;

            exitSegmentMeshFilters[i] = pathGenerator.raceBuoys[i].transform.GetChild(1).gameObject;
        }

        // StartCoroutine(CheckPosition()); // Start the coroutine

        RequestDecision();
        originalDistanceToWaypoint = 1000f;
        if (pathGenerator.waypoints == null) return;
        originalDistanceToWaypoint = (pathGenerator.getWaypoints()[currentWaypointIndex].position - transform.position).magnitude;   
    }

    void FixedUpdate()
    {
        try {
            for (int i = 0; i < pathGenerator.raceBuoys.Count; i++)
            {
                if (gjkCollisionDetection.GJK(gameObject.GetComponent<MeshFilter>(), transform.position, approachSegmentMeshFilters[i].GetComponent<MeshFilter>(), approachSegmentMeshFilters[i].transform.position))
                {
                    // print("Starting to Round Buoy" + i);
                    ApproachSegmentEntered(i);
                }
                if (gjkCollisionDetection.GJK(gameObject.GetComponent<MeshFilter>(), transform.position, exitSegmentMeshFilters[i].GetComponent<MeshFilter>(), exitSegmentMeshFilters[i].transform.position))
                {
                    // print("Exiting Buoy" + i);
                    ExitSegmentEntered(i);
                }
            }
        } catch (System.Exception e) {
            print("Exception thrown");
            print(e.Message);
            approachSegmentMeshFilters = new GameObject[3];
            exitSegmentMeshFilters = new GameObject[3];
            for (int i = 0; i < approachSegmentMeshFilters.Length; i++)
            {
                approachSegmentMeshFilters[i] = pathGenerator.raceBuoys[i].transform.GetChild(0).gameObject;
                exitSegmentMeshFilters[i] = pathGenerator.raceBuoys[i].transform.GetChild(1).gameObject;
            }
        }

        // apply a forwards lerp transform in direction
        transform.position = Vector3.Lerp(transform.position, transform.position + direction, velocity * Time.deltaTime);

        episodeDuration += Time.fixedDeltaTime;
        if (episodeDuration > maxEpisodeDuration)
        {
            CompleteEpisode();
        }

        // check distance to follower
        if(Vector3.Distance(transform.position, pathFollower.transform.position) < 10.0f)
        {
            AddReward(0.1f);
        }
        else
        {
            AddReward(-100f);
            CompleteEpisode();
        }

        currentBuoyIdxText.text = "Current Buoy Index: " + currentWaypointIndex;

        // AddReward(-0.01f);
    }

    public void ApproachSegmentEntered(int segmentIndex)
    {
        if (segmentIndex == currentWaypointIndex)
        {
            // AddReward(10.0f);
            hasGoneThroughApproachSegment = true;
        }
    }

    public void ExitSegmentEntered(int segmentIndex)
    {
        if (segmentIndex == currentWaypointIndex)
        {
            if (hasGoneThroughApproachSegment)
            {
                // Debug.Log("Exited segment " + segmentIndex);
                // AddReward(20.0f);
                currentWaypointIndex++;
                currentBuoyIdxText.text = "Current Buoy Index: " + currentWaypointIndex;
                originalDistanceToWaypoint = (pathGenerator.getWaypoints()[currentWaypointIndex].position - transform.position).magnitude;
                hasGoneThroughApproachSegment = false;
            }
            // else
            // {
            //     // Debug.Log("Entered exit segment without going through approach segment");
            //     AddReward(-100.0f);
            //     CompleteEpisode();
            // }
        }
        // else 
        // {
        //     AddReward(-100.0f);
        //     CompleteEpisode();
        // }
    }

    // private IEnumerator CheckPosition()
    // {
    //     while (true) // Run indefinitely
    //     {
    //         yield return new WaitForSeconds(1f); // Wait for 1 second

    //         float currentDistanceToWaypoint = Vector3.Distance(transform.position, pathGenerator.getWaypoints()[currentWaypointIndex].position);
    //         float previousDistanceToWaypoint = Vector3.Distance(previousPosition, pathGenerator.getWaypoints()[currentWaypointIndex].position);
            // If the current position is within 1 unit of the previous position
            // if (Vector3.Distance(previousPosition, transform.position) <= 1.0f)
            // {
                // AddReward(-1f); // Add a negative reward
                // CompleteEpisode(); // End the episode
            // }

            // If the current distance to the waypoint is greater than the previous distance
            // if (currentDistanceToWaypoint > previousDistanceToWaypoint)
            // {
            //     AddReward(-0.1f);
            //     // CompleteEpisode();
            // }
            // else
            // {
            //     AddReward(0.1f);
            // }

            // extra bonus for accuracy
            // if (Vector3.Distance(previousPosition, pathFollower.transform.position) <= 10.0f)
            // {
            //     AddReward(0.1f);
            // }
            // else {
            //     AddReward(-100f);
            //     CompleteEpisode();
            // }

            // previousPosition = transform.position; // Update the previous position
    //     }
    // }

    // private IEnumerator Countdown()
    // {
    //     outOfBoundsEpisodeTimerStarted = true;
    //     countdownText.gameObject.SetActive(true);

    //     for (int i = 3; i > 0; i--)
    //     {
    //         countdownText.text = i.ToString();

    //         yield return new WaitForSeconds(1f);
    //     }

    //     countdownText.text = "OOB!";


    //     yield return new WaitForSeconds(1f);
    //     countdownText.gameObject.SetActive(false);

    //     // if our boi is still not closer to the waypoint than we started then we are out of bounds.
    //     if (Vector3.Distance(transform.position, pathGenerator.getWaypoints()[currentWaypointIndex].position) > originalDistanceToWaypoint)
    //     {
    //         AddReward(-100.0f);
    //         CompleteEpisode();
    //     }
    // }

    public override void CollectObservations(VectorSensor sensor) 
    {
        // Transform[] waypoints = pathGenerator.getWaypoints();
        // Vector3 relativePosition = waypoints[currentWaypointIndex].position - transform.position;

        // sensor.AddObservation(relativePosition);
        // normalized relative position rotated to be relative to the agent's forward direction
        // sensor.AddObservation(Quaternion.Inverse(transform.rotation) * relativePosition.normalized);

        // magnaitude of the relative position 
        // sensor.AddObservation(relativePosition.magnitude);

        // 
        Vector3 relativeFollowerPosition = pathFollower.transform.position - transform.position;

        sensor.AddObservation(relativeFollowerPosition);
        // sensor.AddObservation(Quaternion.Inverse(transform.rotation) * relativeFollowerPosition.normalized);

        // sensor.AddObservation(relativeFollowerPosition.magnitude);

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int[] actionList = new int[directionActionSpaceSize];
        for (int i = 0; i < directionActionSpaceSize; i++)
        {
            actionList[i] = actions.DiscreteActions[i];
        }
        float bearing = BearingFromActions(actionList);
        direction = Quaternion.Euler(0, bearing, 0) * Vector3.forward;
        actionText.text = "Action: " + string.Join(",", actionList) + "\nBearing: " + bearing;

        // punish for time spent this will encourage the agent to move faster
        // AddReward(-0.001f);

        // punish for moving away from the next waypoint
        // if (Vector3.Dot(direction, (pathGenerator.getWaypoints()[currentWaypointIndex].position - transform.position).normalized) < 0)
        // {
        //     AddReward(-0.01f);
        // }

        Transform[] waypoints = pathGenerator.getWaypoints();
        float distanceToWaypoint = Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position);

        // if (distanceToWaypoint > originalDistanceToWaypoint)
        // {
        //     AddReward(Mathf.Pow((distanceToWaypoint - originalDistanceToWaypoint), 2) * -0.01f);
        //     if (!outOfBoundsEpisodeTimerStarted) StartCoroutine(Countdown());
        // }
        
        // float distanceDifference = previousPolicedDistanceTravelled - pathFollower.policedMaxDistanceTravelled;
        // previousPolicedDistanceTravelled = pathFollower.policedMaxDistanceTravelled;
        // AddReward(Mathf.Max(distanceDifference, 0.0f));


        // get cumulative reward
        cumulativeReward = GetCumulativeReward();
        if (cumulativeReward < -10000)
        {
            CompleteEpisode();
        }

        // AddReward(-0.001f);

        rewardText.text = "Reward: " + cumulativeReward.ToString("F2");
        
        RequestDecision();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = Input.GetKey(KeyCode.W) ? 1 : 0;
        discreteActions[1] = Input.GetKey(KeyCode.A) ? 1 : 0;
        discreteActions[2] = Input.GetKey(KeyCode.S) ? 1 : 0;
        discreteActions[3] = Input.GetKey(KeyCode.D) ? 1 : 0;

    }


    public static float BearingFromActions(int[] actions)
    {
        int directionActionSpaceSize = 6;
        float xSum = 0;
        float ySum = 0;

        for (int i = 0; i < directionActionSpaceSize; i++)
        {
            if (actions[i] == 1)
            {
                float angle = i * 360 / directionActionSpaceSize;
                xSum += Mathf.Cos(Mathf.PI * angle / 180);
                ySum += Mathf.Sin(Mathf.PI * angle / 180);
            }
        }

        // Calculate the angle of the resulting vector
        float bearing = Mathf.Atan2(ySum, xSum) * 180 / Mathf.PI;

        // Normalize the angle to be between 0 and 360
        return (float)(bearing >= 0 ? bearing : bearing + 360);
    }


    public void CompleteEpisode()
    {
        // StopCoroutine(CheckPosition()); // Stop the coroutine when the episode ends
        EndEpisode(); // End the episode
        episodeDuration = 0.0f;
        // RequestDecision();

    }
}
