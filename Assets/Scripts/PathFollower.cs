using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathCreation.Examples
{
    // Moves along a path at constant speed.
    

    // Depending on the end of path instruction, will either loop, reverse, or stop at the end of the path.
    public class PathFollower : MonoBehaviour
    {
        public PathCreator pathCreator;
        public GenerateCourse course;
        public EndOfPathInstruction endOfPathInstruction;
        public float speed = 25;
        public GameObject objectToTrack;

        public GameObject startFinishBuoy;
        float distanceTravelled;

        public int currentWaypointIndex = 0;
        public int lapsCompleted = 0;
        private bool justStarted = true;

        private bool justMovedToNextWaypoint = false;

        float distanceToNextWaypoint = 0.0f;
        private bool crossedStartFinish = false;

        public UnityEngine.UI.Text lapCounterText;
        private float roundingRadius = 30.0f;

        private float buoyScale = 6.0f;

        bool followerHasRoundedBuoyPrevious = false;

        int buoysRounded = 0;


        private MeshCollider[] approachingBuoyColliders;
        private MeshCollider[] exitingBuoyColliders;

        private MeshCollider boatCollider;
        


        void Start() {
            if (pathCreator != null)
            {
                // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
                pathCreator.pathUpdated += OnPathChanged;
            }
            if (course != null)
            {
                // Set the position of the follower to the position of the first waypoint
                transform.position = course.waypoints[0].position;
            }
            currentWaypointIndex = 0;
            // wait for 1 second
            StartCoroutine(WaitForStart());

            buoyScale = course.raceBuoys[0].transform.localScale.x;

            
        }

        IEnumerator WaitForStart()
        {
            yield return new WaitForSeconds(0.5f);
            course.raceBuoys[1].transform.localScale = Vector3.one * buoyScale * 2;
            // get references to the colliders for the segments (children of the buoys)
            approachingBuoyColliders = new MeshCollider[course.raceBuoys.Count];
            exitingBuoyColliders = new MeshCollider[course.raceBuoys.Count];
            for (int i = 0; i < course.raceBuoys.Count; i++) {
                GameObject approachingBuoySegment = course.raceBuoys[i].transform.GetChild(0).gameObject;
                GameObject exitingBuoySegment = course.raceBuoys[i].transform.GetChild(1).gameObject;
                Mesh approachingBuoyMesh = approachingBuoySegment.GetComponent<MeshFilter>().mesh;
                Mesh exitingBuoyMesh = exitingBuoySegment.GetComponent<MeshFilter>().mesh;
                approachingBuoyColliders[i] = approachingBuoySegment.GetComponent<MeshCollider>();
                exitingBuoyColliders[i] = exitingBuoySegment.GetComponent<MeshCollider>();
                approachingBuoyColliders[i].sharedMesh = approachingBuoyMesh;
                exitingBuoyColliders[i].sharedMesh = exitingBuoyMesh;
            }

            // get reference to the boat collider
            boatCollider = objectToTrack.GetComponentInChildren<MeshCollider>();

            // make listeners for the colliders
            for (int i = 0; i < course.raceBuoys.Count; i++) {
                approachingBuoyColliders[i].gameObject.AddComponent<ApproachingBuoyColliderListener>();
                exitingBuoyColliders[i].gameObject.AddComponent<ExitingBuoyColliderListener>();
            }
        }

        IEnumerator WaitBeforeMove()
        {
            yield return new WaitForSeconds(.3f);
            justMovedToNextWaypoint = false;
        }

        void Update()
        {
            // CheckStartFinishCross();

            if (pathCreator != null)
            {
                // distanceTravelled += speed * Time.deltaTime;
                if (objectToTrack != null) {
                    // should check if the objectToTrack is within the current segment
                    distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(objectToTrack.transform.position);
                } else {
                    distanceTravelled += speed * Time.deltaTime;
                }
                // float distanceAlongPathAtClosestPoint = pathCreator.path.GetClosestDistanceAlongPath(objectToTrack.transform.position);
                // float distanceAtNextWaypoint = course.waypointDistances[currentWaypointIndex + 1];
                // if (currentWaypointIndex == course.waypointDistances.Length - 1) {
                //     distanceAtNextWaypoint = pathCreator.path.length;
                // }
                // if (distanceAlongPathAtClosestPoint > distanceAtNextWaypoint) {
                //     Debug.Log("Waypoint " + currentWaypointIndex + " is behind us (" + distanceAlongPathAtClosestPoint + " > " + distanceAtNextWaypoint + ")");
                //     // return;
                // }
                transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
                transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
            }

            float currentSegmentStart = course.waypointDistances[currentWaypointIndex];
            float currentSegmentEnd = (currentWaypointIndex == course.waypointDistances.Length - 1) ? pathCreator.path.length : course.waypointDistances[currentWaypointIndex + 1];

            bool starboardRounding = course.raceBuoyIsRoundingToStarboard[currentWaypointIndex];
            Vector3 raceBuoyToNextWaypoint = course.waypoints[currentWaypointIndex].position - course.raceBuoys[currentWaypointIndex].transform.position;
            if (starboardRounding) {
                raceBuoyToNextWaypoint = -raceBuoyToNextWaypoint;
            }
            Vector3 raceBuoyToFollower = transform.position - course.raceBuoys[currentWaypointIndex].transform.position;

            float dotProduct = Vector3.Dot(raceBuoyToNextWaypoint, raceBuoyToFollower);
            // float crossProduct = Vector3.Cross(raceBuoyToNextWaypoint, raceBuoyToFollower).y;

            // Debug.Log("Cross product: " + crossProduct + " Dot product: " + dotProduct);

            // this is reversed if the buoy is a right hand turn
            bool followerHasRoundedABuoy = starboardRounding ? dotProduct < 0 : dotProduct > 0;
            float distanceToNextWaypoint = Vector3.Distance(transform.position, course.waypoints[currentWaypointIndex].position);
            if (followerHasRoundedBuoyPrevious != followerHasRoundedABuoy && followerHasRoundedABuoy && distanceToNextWaypoint < roundingRadius) {   
                Debug.DrawRay(startFinishBuoy.transform.position, raceBuoyToNextWaypoint, Color.green, 1);
                buoysRounded++;
                followerHasRoundedBuoyPrevious = followerHasRoundedABuoy;
                UpdateLapDisplay();
            } else  {
                followerHasRoundedBuoyPrevious = followerHasRoundedABuoy;
            }

            if (currentWaypointIndex == course.waypointDistances.Length - 1)
            {
                // Check if we're near the end of the path
                if (distanceTravelled + speed * Time.deltaTime > currentSegmentEnd)
                {
                    // Near the end, handle lap completion and reset distance
                    HandleLapCompletion();
                }
            }
            else if (distanceTravelled > currentSegmentEnd)
            // else if (distanceTravelled > currentSegmentEnd && distanceToNextWaypoint < roundingRadius * 3)
            {
                // Debug.Log("Moving to waypoint " + (currentWaypointIndex + 1) + "");
                MoveToNextWaypoint();
            }
        }


        void MoveToNextWaypoint()
        {
            if (justMovedToNextWaypoint)
            {
                return;
            }
            // Determine the index of the buoy that needs to be reset to original scale
            int previousWaypointIndex = currentWaypointIndex;

            // Move to the next waypoint
            currentWaypointIndex = (currentWaypointIndex + 1) % course.waypoints.Length;

            // Reset the scale of the previous waypoint
            if (previousWaypointIndex == course.waypoints.Length - 1)
            {
                course.raceBuoys[0].transform.localScale = Vector3.one * buoyScale;
            }
            else
            {
                course.raceBuoys[previousWaypointIndex + 1].transform.localScale = Vector3.one * buoyScale;
            }

            // Scale up the next waypoint
            course.raceBuoys[(currentWaypointIndex+1)%course.waypoints.Length].transform.localScale = Vector3.one * buoyScale * 2; 

            justMovedToNextWaypoint = true;
            StartCoroutine(WaitBeforeMove());
        }


        void HandleLapCompletion()
        {
            if (justStarted)
            {
                justStarted = false;
            } else {
                lapsCompleted++;
            }
            UpdateLapDisplay();
            distanceTravelled = 0; // Reset the distance travelled
            currentWaypointIndex = 0; // Reset to the first waypoint

            // Reset the scale of the last buoy and enlarge the first one
            course.raceBuoys[0].transform.localScale = Vector3.one;
            course.raceBuoys[1].transform.localScale = Vector3.one * 3;
        }

        void OnDrawGizmos()
        {
            // add a line between startFinishBuoy and the first waypoint
            if (startFinishBuoy != null && course != null && course.raceBuoys != null && course.raceBuoys.Count > 0) {
                Gizmos.color = Color.yellow;
                Vector3 aboveFinishBuoy = new Vector3(startFinishBuoy.transform.position.x, startFinishBuoy.transform.position.y + 1, startFinishBuoy.transform.position.z);
                Vector3 aboveFirstWaypoint = course.raceBuoys[0].transform.position + new Vector3(0,1,0);
                Gizmos.DrawLine(aboveFinishBuoy, aboveFirstWaypoint);
            }

        }

        void OnPathChanged() {
            distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
        }

        void UpdateLapDisplay() {
            lapCounterText.text = "Laps: " + lapsCompleted + "                Buoys: " + buoysRounded.ToString();
        }
    }
}
    
        // void CheckStartFinishCross()
        // {
        //     Vector3 yellowToGreen = course.waypoints[0].position - startFinishBuoy.transform.position;
        //     Vector3 yellowToObject = objectToTrack != null ? objectToTrack.transform.position : this.transform.position - startFinishBuoy.transform.position;
            
        //     float crossProduct = Vector3.Cross(yellowToGreen, yellowToObject).y;

        //     if (crossedStartFinish && crossProduct > 0)
        //     {
        //         crossedStartFinish = false;
        //     }
        //     else if (!crossedStartFinish && crossProduct < 0)
        //     {
        //         if (!justStarted)
        //         {
        //             lapsCompleted++;
        //             UpdateLapDisplay();
        //         }
                
        //         crossedStartFinish = true;
        //         currentWaypointIndex = 0;
        //         justStarted = false;
        //     }
        // }

        // If the path changes during the game, update the distance travelled so that the follower's position on the new path
        // is as close as possible to its position on the old path