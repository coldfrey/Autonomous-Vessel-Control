using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    public GameObject boat;
    public float waypointRadius = 50f;
    public float distanceFromBoat =400f;
    public float waypointHeight = 2f;

    public GameObject waypointPrefab;
    private GameObject waypoint;

    private Vector3 previousPosition;
    public float completedWaypoints;

    private Vector3 startPosition = new Vector3(0f, 0f, -200f);
    private int currentLesson = 0;

    private void Start()
    {
        completedWaypoints = 0f;
    }

    public void CreateWaypoint()
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
        Vector3 spawnDirection = Vector3.back; // default direction (0,0,-1)

        // Calculate spawn direction based on current lesson
        if (currentLesson == 1)
        {
            // Randomly choose an angle within a 45-degree sector facing the negative z direction
            float angle = Random.Range(-22.5f, 22.5f); // Half of 45 degrees to either side
            spawnDirection = Quaternion.Euler(0, angle, 0) * spawnDirection;
        }
        else if (currentLesson == 2)
        {
            float angle = Random.Range(-50f, 50f); // Half of 80 degrees to either side
            spawnDirection = Quaternion.Euler(0, angle, 0) * spawnDirection;
        }
        else if (currentLesson == 3)
        {
            // Randomly choose between positive and negative x direction
            float xDirection = Random.Range(0, 2) * 2 - 1; // Will give either -1 or 1
            float zRange = Random.Range(-20f, 0f); // Random z value between -20 and 0
            spawnDirection = new Vector3(xDirection, 0f, zRange).normalized;
        }
        else if (currentLesson == 4)
        {
            // Randomly choose an angle within a 300-degree sector excluding the 80-degree sector facing the negative z direction
            float angle;
            float randomValue = Random.value; // Between 0.0 and 1.0
            if (randomValue < 0.5f)
            {
                // Choose angle from the negative side but exclude the -40 to 40 sector
                angle = Random.Range(-170f, -80f);
            }
            else
            {
                // Choose angle from the positive side but exclude the -40 to 40 sector
                angle = Random.Range(80f, 170f);
            }
            spawnDirection = Quaternion.Euler(0, angle, 0) * spawnDirection;
        }

        Vector3 waypointPosition = boat.transform.position + spawnDirection * distanceFromBoat;
        waypointPosition.y = waypointHeight;
        waypoint.transform.position = waypointPosition;
        previousPosition = waypointPosition;
        Debug.Log("New waypoint position: " + waypointPosition);
    }

    public void NewWaypoint(int lessonNumber)
    {
        currentLesson = lessonNumber;
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

    public GameObject GetWaypoint()
    { 
        if (waypoint != null)
         {
            return waypoint;
         }
         else 
         {
            return null;
         }
    }
}
