using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    public GameObject boat; // Drag and drop your boat GameObject here in the inspector
    public float waypointRadius = 50f; // Radius of the circular waypoint
    public float distanceFromBoat = 500f; // Distance from the boat to spawn the waypoint
    public float waypointHeight = 2f; // Height of the visible waypoint

    private GameObject waypoint; // The generated waypoint

    private void Start()
    {
        CreateWaypoint();
    }

    void CreateWaypoint()
    {
        // Create a Cylinder to represent the waypoint visually
        waypoint = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        waypoint.name = "FinishWaypoint";

        // Scale the waypoint based on our radius and height settings
        waypoint.transform.localScale = new Vector3(waypointRadius, waypointHeight / 2, waypointRadius); // Divide height by 2 because the cylinder's default height is 2

        // Position the waypoint
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        Vector3 waypointPosition = boat.transform.position + randomDirection * distanceFromBoat;
        waypointPosition.y = 0; // Assuming water surface is at y=0
        waypoint.transform.position = waypointPosition;

        // Add a circular collider (use the Cylinder's existing collider and adjust it)
        CapsuleCollider collider = waypoint.GetComponent<CapsuleCollider>();
        collider.radius = waypointRadius;
        collider.height = waypointHeight;
        collider.isTrigger = true;

        waypoint.AddComponent<WaypointTrigger>().boat = boat; // Adds the trigger behavior to the waypoint
    }
}

public class WaypointTrigger : MonoBehaviour
{
    public GameObject boat;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == boat) // Compares if the colliding object is our boat
        {
            Debug.Log("Boat has reached the finish waypoint!");

            // Optionally destroy the waypoint and/or spawn a new one
            // Destroy(gameObject);
            // Your code to create another waypoint, if desired
        }
    }
}