using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using PathCreation;
using System;
using Random = UnityEngine.Random;
using System.Collections.Specialized;



namespace PathCreation.Examples
{
    
    [RequireComponent(typeof(PathCreator))]
    public class GenerateCourse : MonoBehaviour
    {
        [SerializeField] private PathCreator pathCreator;
        [SerializeField] private bool closedLoop = true;
        [SerializeField] private GameObject waypointPrefab;
        [SerializeField] private GameObject buoyPrefab;
        public int numberOfWaypoints = 2;
        [SerializeField] private float distanceThreshold = 3.0f;
        [SerializeField] private float mapSize = 15.0f;
        public List<GameObject> raceBuoys = new List<GameObject>();
        public List<bool> raceBuoyIsRoundingToStarboard = new List<bool>();
        private VertexPath vertexPath;
        public Transform[] waypoints;
        public float[] waypointDistances;
        private Vector3[] previousWaypointPositions;
        // private List<GameObject> instantiatedGameObjects = new List<GameObject>();
        private List<GameObject> instantiatedWaypoints = new List<GameObject>();

        private bool shouldRegeneratePath = false;


        public int NumberOfWaypoints
        {
            get => numberOfWaypoints;
            set
            {
                numberOfWaypoints = value;
                Array.Resize(ref waypoints, numberOfWaypoints);
                Array.Resize(ref waypointDistances, numberOfWaypoints);
                Array.Resize(ref previousWaypointPositions, numberOfWaypoints);
                RegeneratePath();
            }
        }

        public float DistanceThreshold
        {
            get => distanceThreshold;
            set
            {
                distanceThreshold = value;
                RegeneratePath();
            }
        }

        void Start()
        {
            InitializeWaypoints();
            InitializePath();

        }

        void OnValidate() {
            shouldRegeneratePath = true;
        }


        void Update()
        {
            if (shouldRegeneratePath)
            {
                RegeneratePath();
                shouldRegeneratePath = false;
            }
            // ... 
            if (WaypointsHaveMoved()) shouldRegeneratePath = true;

            if (Input.GetKeyDown(KeyCode.UpArrow)) NumberOfWaypoints++;
            if (Input.GetKeyDown(KeyCode.DownArrow)) NumberOfWaypoints = Mathf.Max(1, NumberOfWaypoints - 1);
        }

        public Vector3 GetCurrentDirectionOfNearestPathFromPoint(Vector3 point) {
            float distance = vertexPath.GetClosestDistanceAlongPath(point);
            Vector3 direction = vertexPath.GetDirectionAtDistance(distance);
            // Debug.DrawRay(point, direction * 10, Color.red, 10);
            return direction;
        }

        public Transform[] getWaypoints() {
            return waypoints;
        }

        private void InitializeWaypoints()
        {
            if (waypoints == null || waypoints.Length != numberOfWaypoints) GenerateWaypoints();

            previousWaypointPositions = new Vector3[numberOfWaypoints];
            for (int i = 0; i < numberOfWaypoints; i++)
            {
                previousWaypointPositions[i] = waypoints[i].position;
            }
        }

        private void InitializePath()
        {
            BezierPath bezierPath = CreateBezierPathFromWaypoints();
            vertexPath = new VertexPath(bezierPath, transform);
            GetComponent<PathCreator>().bezierPath = bezierPath;
            CalculateWaypointDistances();
            PlaceBuoys(bezierPath);
        }

        private BezierPath CreateBezierPathFromWaypoints()
        {
            return new BezierPath(waypoints, closedLoop, PathSpace.xyz);
        }

        private void CalculateWaypointDistances()
        {
            waypointDistances = new float[numberOfWaypoints];
            for (int i = 0; i < numberOfWaypoints; i++)
            {
                waypointDistances[i] = vertexPath.GetClosestDistanceAlongPath(waypoints[i].position);
            }
        }

        private void GenerateWaypoints()
        {
            // if (waypoints != null) DeleteAllInstantiatedGameObjects();
            waypoints = new Transform[numberOfWaypoints];
            for (int i = 0; i < numberOfWaypoints; i++) {
                GameObject waypoint = Instantiate(waypointPrefab, GetRandomPositionInGrid(numberOfWaypoints, mapSize), Quaternion.identity);
                waypoints[i] = waypoint.transform;
                instantiatedWaypoints.Add(waypoint);

            }
        }

        Vector3 GetRandomPositionInGrid(int gridSize, float cellSize) {
            int x = Random.Range(0, gridSize);
            int z = Random.Range(0, gridSize);
            Vector3 basePosition = new Vector3(x * cellSize, 0, z * cellSize);
            return basePosition + new Vector3(Random.Range(-cellSize/2, cellSize/2), 0, Random.Range(-cellSize/2, cellSize/2));
        }


        private void PlaceBuoys(BezierPath bezierPath)
        {
            float delta = 1f;  // Small distance for approximating direction

            for (int i = 0; i < waypoints.Length; i++) {
                float distanceOfCurrent = vertexPath.GetClosestDistanceAlongPath(waypoints[i].position);
                
                Vector3 pointBehind = pathCreator.path.GetPointAtDistance(distanceOfCurrent - delta);
                Vector3 current = pathCreator.path.GetPointAtDistance(distanceOfCurrent);
                Vector3 pointAhead = pathCreator.path.GetPointAtDistance(distanceOfCurrent + delta);

                // Approximate the direction at the current point and the point ahead
                Vector3 directionAtCurrent = (current - pointBehind).normalized;

                Vector3 directionAtAhead = (pointAhead - current).normalized;

                // Cross product to determine the bend
                Vector3 crossProduct = Vector3.Cross(directionAtCurrent, directionAtAhead);

                // Perpendicular vector (2D)
                Vector3 perpendicularDirection = new Vector3(-directionAtCurrent.z, 0, directionAtCurrent.x);

                GameObject buoy = null;
                if (crossProduct.y > 0) {
                    // Right-hand bend
                    float angleToNorth = Vector3.SignedAngle(Vector3.forward, perpendicularDirection, Vector3.up);
                    buoy = Instantiate(buoyPrefab, current - perpendicularDirection * distanceThreshold, Quaternion.identity * Quaternion.Euler(0, angleToNorth -90, 0));
                    // buoy.transform.GetChild(2).GetComponent<Renderer>().material.color = Color.green;
                    
                    raceBuoys.Add(buoy);
                    raceBuoyIsRoundingToStarboard.Add(true);
                } else if (crossProduct.y < 0) {
                    // Left-hand bend
                    float angleToNorth = Vector3.SignedAngle(Vector3.forward, perpendicularDirection, Vector3.up);
                    buoy = Instantiate(buoyPrefab, current + perpendicularDirection * distanceThreshold, Quaternion.identity * Quaternion.Euler(0, angleToNorth + 90, 0));
                    buoy.transform.GetChild(2).GetComponent<Renderer>().material.color = Color.red;
                    raceBuoys.Add(buoy);
                    raceBuoyIsRoundingToStarboard.Add(false);
                }
                // Rounding lines
                // if (buoy != null) {
                //     // buoy.transform.forward = directionAtCurrent;
                //     // buoy.transform.localScale = Vector3.one * 0.5f;
                //     Debug.DrawRay(buoy.transform.position, directionAtCurrent * 10, Color.red, 10);
                // }

                // draw a line to show the direction from the buoy
            }
        }

        private bool WaypointsHaveMoved()
        {
            for (int i = 0; i < numberOfWaypoints; i++) {
                if (previousWaypointPositions[i] != waypoints[i].position) {
                    return true;
                }
            }
            return false;
        }

        public void RegeneratePath()
        {
            DeleteAllInstantiatedGameObjects();
            // GenerateWaypoints();
            InitializePath();
        }

        private void DeleteAllInstantiatedGameObjects()
        {
            foreach(GameObject go in instantiatedWaypoints) {
                Destroy(go);
            }
            instantiatedWaypoints.Clear();
            foreach(GameObject go in raceBuoys) {
                Destroy(go);
            }
            raceBuoys.Clear();
            raceBuoyIsRoundingToStarboard.Clear();
        }
    }
}