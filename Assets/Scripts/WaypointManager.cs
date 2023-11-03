using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    public GameObject boat;
    public float waypointRadius = 50f;
    public float distanceFromBoat = 150f;
    public float waypointHeight = 2f;

    public GameObject waypointPrefab;
    public GameObject waypoint;

    private Vector3 previousPosition;
    private float completedWaypoints;

    private Vector3 startPosition = new Vector3(0f, 0f, -200f);
    private void Start()
    {
        completedWaypoints = 0f;
        CreateWaypoint();
    }

    void CreateWaypoint()
    {
        waypoint = Instantiate(waypointPrefab);
        waypoint.name = "FinishWaypoint";

        SegmentVis segmentVis = waypoint.GetComponent<SegmentVis>();
        segmentVis.vector1 = new Vector3(waypointRadius, 0f, 0f);
        segmentVis.vector2 = new Vector3(waypointRadius, 0f, 0f);
        segmentVis.arcResolution = 120;
        
        MoveWaypoint();
    }

    void MoveWaypoint()
    {
        Vector3 spawnDirection;

        if (completedWaypoints < 20)
        {
            float zFactor = 1f - (completedWaypoints / 20f);  // Decreases from 1 to 0 as completedWaypoints increases from 0 to 20.
            spawnDirection = new Vector3(Random.Range(-0.5f, 0.5f), 0, -1 * zFactor).normalized;
        }
        else
        {
            spawnDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        }

        Vector3 waypointPosition = startPosition + spawnDirection * distanceFromBoat;
        waypointPosition.y = waypointHeight;
        waypoint.transform.position = waypointPosition;
        previousPosition = waypointPosition;
        Debug.Log("New waypoint position: " + waypointPosition);

    }

    public void NewWaypoint()
    {
        completedWaypoints++;
        MoveWaypoint();
        Debug.Log("Completed waypoints: " + completedWaypoints);
    }

    public void ResetWaypoint()
    {
        Debug.Log("Resetting waypoint");
        completedWaypoints = 0f;
        waypoint.transform.position = previousPosition;
    }

    public float GetCompletedWaypoints()
    {
        return completedWaypoints;
    }
}
