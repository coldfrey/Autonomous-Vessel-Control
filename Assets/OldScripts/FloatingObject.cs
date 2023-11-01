using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;


    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class FloatingObject : MonoBehaviour
    {
        public WaterSurface water;
        public Mesh buoyancyMesh;
        WaterSearchParameters searchParameters = new WaterSearchParameters();
        WaterSearchResult searchResult = new WaterSearchResult();
      
        [SerializeField]
        private bool calculateDensity = false;

        [SerializeField]
        public float density = 0.75f;

        [SerializeField]
        [Range(0f, 1f)]
        private float normalizedVoxelSize = 0.5f;

        [SerializeField]
        private float dragInWater = 1f;

        [SerializeField]
        private float angularDragInWater = 1f;

        private new Collider collider;
        private new Rigidbody rigidbody;
        private float initialDrag;
        private float initialAngularDrag;
        private Vector3 voxelSize;
        private Vector3[] voxels;

        private void Awake()
        {
            this.collider = this.GetComponent<Collider>();
            this.rigidbody = this.GetComponent<Rigidbody>();

            this.initialDrag = this.rigidbody.drag;
            this.initialAngularDrag = this.rigidbody.angularDrag;

            // if (this.calculateDensity)
            // {
            //     // float objectVolume = Mathf.CalculateVolume_Mesh(this.GetComponent<Mesh>().mesh, this.transform);
            //     float objectVolume = VolumeOfMesh(this.GetComponent<Mesh>().mesh);
            //     this.density = this.rigidbody.mass / objectVolume;
            // }
            this.voxels = this.CutIntoVoxels();
        }

        // public float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
        // {
        //     float v321 = p3.x * p2.y * p1.z;
        //     float v231 = p2.x * p3.y * p1.z;
        //     float v312 = p3.x * p1.y * p2.z;
        //     float v132 = p1.x * p3.y * p2.z;
        //     float v213 = p2.x * p1.y * p3.z;
        //     float v123 = p1.x * p2.y * p3.z;

        //     return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
        // }
        // public float VolumeOfMesh(Mesh mesh)
        // {
        //     float volume = 0;

        //     Vector3[] vertices = mesh.vertices;
        //     int[] triangles = mesh.triangles;

        //     for (int i = 0; i < triangles.Length; i += 3)
        //     {
        //         Vector3 p1 = vertices[triangles[i + 0]];
        //         Vector3 p2 = vertices[triangles[i + 1]];
        //         Vector3 p3 = vertices[triangles[i + 2]];
        //         volume += SignedVolumeOfTriangle(p1, p2, p3);
        //     }
        //     return Mathf.Abs(volume);    
        // }

        private void FixedUpdate()
        {
            if (this.water == null)
            {
                Debug.Log("FixedUpdate, there is no water!!!");
                return;
            }
            if (this.water != null && this.voxels.Length > 0)
            {
                Debug.Log("FixedUpdate, there is water!!!");
                Vector3 forceAtSingleVoxel = this.CalculateMaxBuoyancyForce() / this.voxels.Length;
                Bounds bounds = this.collider.bounds;
                float voxelHeight = bounds.size.y * this.normalizedVoxelSize;

                float submergedVolume = 0f;
                for (int i = 0; i < this.voxels.Length; i++)
                {
                    Vector3 worldPoint = this.transform.TransformPoint(this.voxels[i]);
                    
                    searchParameters.startPositionWS = searchResult.candidateLocationWS;
                    searchParameters.targetPositionWS = worldPoint;
                    searchParameters.error = 0.01f;
                    searchParameters.maxIterations = 8;
                    // float waterLevel = this.water.GetWaterLevel(worldPoint);
                    if (water.ProjectPointOnWaterSurface(searchParameters, out searchResult))
                    {
                        Debug.Log(searchResult.projectedPositionWS);
                        gameObject.transform.position = searchResult.projectedPositionWS;
                    }
                    else Debug.LogError("Can't Find Projected Position");
                    float waterLevel = searchResult.projectedPositionWS.y;

                    float deepLevel = waterLevel - worldPoint.y + (voxelHeight / 2f); // How deep is the voxel                    
                    float submergedFactor = Mathf.Clamp(deepLevel / voxelHeight, 0f, 1f); // 0 - voxel is fully out of the water, 1 - voxel is fully submerged
                    submergedVolume += submergedFactor;

                    Vector3 surfaceNormal = GetSurfaceNormal(worldPoint);

                    Quaternion surfaceRotation = Quaternion.FromToRotation(this.water.transform.up, surfaceNormal);
                    surfaceRotation = Quaternion.Slerp(surfaceRotation, Quaternion.identity, submergedFactor);

                    Vector3 finalVoxelForce = surfaceRotation * (forceAtSingleVoxel * submergedFactor);
                    this.rigidbody.AddForceAtPosition(finalVoxelForce, worldPoint);

                    Debug.DrawLine(worldPoint, worldPoint + finalVoxelForce.normalized, Color.blue);
                }

                submergedVolume /= this.voxels.Length; // 0 - object is fully out of the water, 1 - object is fully submerged

                this.rigidbody.drag = Mathf.Lerp(this.initialDrag, this.dragInWater, submergedVolume);
                this.rigidbody.angularDrag = Mathf.Lerp(this.initialAngularDrag, this.angularDragInWater, submergedVolume);
            }
        }

        public Vector3 GetSurfaceNormal(Vector3 point)
        {
            return Vector3.up;
        }


        // protected virtual void OnTriggerEnter(Collider other)
        // {
        //     if (other.CompareTag(WaterVolume.TAG))
        //     {
        //         this.water = other.GetComponent<WaterVolume>();
        //         if (this.voxels == null)
        //         {
        //             this.voxels = this.CutIntoVoxels();
        //         }
        //     }
        // }

        // protected virtual void OnTriggerExit(Collider other)
        // {
        //     if (other.CompareTag(WaterVolume.TAG))
        //     {
        //         this.water = null;
        //     }
        // }

        protected virtual void OnDrawGizmos()
        {
            if (this.voxels != null)
            {
                for (int i = 0; i < this.voxels.Length; i++)
                {
                    Gizmos.color = Color.magenta - new Color(0f, 0f, 0f, 0.75f);
                    Gizmos.DrawCube(this.transform.TransformPoint(this.voxels[i]), this.voxelSize * 0.8f);
                }
            }
        }

        private Vector3 CalculateMaxBuoyancyForce()
        {
            float objectVolume = this.rigidbody.mass  / this.density;
            // density of water set to 1
            Vector3 maxBuoyancyForce = 1.0F * objectVolume * -Physics.gravity;

            return maxBuoyancyForce;
        }

        private Vector3[] CutIntoVoxels()
        {
            Quaternion initialRotation = this.transform.rotation;
            this.transform.rotation = Quaternion.identity;

            Bounds bounds = this.collider.bounds;
            this.voxelSize.x = bounds.size.x * this.normalizedVoxelSize;
            this.voxelSize.y = bounds.size.y * this.normalizedVoxelSize;
            this.voxelSize.z = bounds.size.z * this.normalizedVoxelSize;
            int voxelsCountForEachAxis = Mathf.RoundToInt(1f / this.normalizedVoxelSize);
            List<Vector3> voxels = new List<Vector3>(voxelsCountForEachAxis * voxelsCountForEachAxis * voxelsCountForEachAxis);

            for (int i = 0; i < voxelsCountForEachAxis; i++)
            {
                for (int j = 0; j < voxelsCountForEachAxis; j++)
                {
                    for (int k = 0; k < voxelsCountForEachAxis; k++)
                    {
                        float pX = bounds.min.x + this.voxelSize.x * (0.5f + i);
                        float pY = bounds.min.y + this.voxelSize.y * (0.5f + j);
                        float pZ = bounds.min.z + this.voxelSize.z * (0.5f + k);

                        Vector3 point = new Vector3(pX, pY, pZ);
                        if (PointInsideCollider(point))
                        {
                            voxels.Add(this.transform.InverseTransformPoint(point));
                        }
                    }
                }
            }

            this.transform.rotation = initialRotation;

            return voxels.ToArray();
        }
        private bool PointInsideCollider(Vector3 point)
        {
            // Find the closest point on the collider to the given point
            Vector3 closestPoint = this.collider.ClosestPoint(point);

            // Check if the closest point is approximately equal to the given point
            return Vector3.Distance(closestPoint, point) < 0.0001f;
        }


    }
