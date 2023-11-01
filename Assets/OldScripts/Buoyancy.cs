using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Rendering.HighDefinition;

public class Buoyancy : MonoBehaviour
{
    public Mesh buoyancyMesh;
    public WaterSurface waterSurface = null;

    private Vector3[] vertices;
    private NativeArray<float3> vertexPositionBuffer;
    private NativeArray<float3> projectedPositionWSBuffer;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Set the buoyancy mesh to the MeshFilter mesh if it hasn't been set
        buoyancyMesh = buoyancyMesh ?? GetComponent<MeshFilter>().mesh;
        vertices = buoyancyMesh.vertices;

        vertexPositionBuffer = new NativeArray<float3>(vertices.Length, Allocator.Persistent);
        projectedPositionWSBuffer = new NativeArray<float3>(vertices.Length, Allocator.Persistent);
    }

    void Update()
    {
        if (waterSurface == null)
            return;

        // Get the simulation data
        WaterSimSearchData simData = new WaterSimSearchData();
        if (!waterSurface.FillWaterSearchData(ref simData))
            return;

        // Fill the input positions with the world-space positions of the mesh vertices
        for (int i = 0; i < vertices.Length; ++i)
            vertexPositionBuffer[i] = transform.TransformPoint(vertices[i]);

        // Create and schedule the job
        WaterSimulationSearchJob searchJob = new WaterSimulationSearchJob
        {
            simSearchData = simData,
            targetPositionWSBuffer = vertexPositionBuffer,
            startPositionWSBuffer = vertexPositionBuffer,
            maxIterations = 8,
            error = 0.01f,
            includeDeformation = true,
            excludeSimulation = false,
            projectedPositionWSBuffer = projectedPositionWSBuffer,
        };

        JobHandle handle = searchJob.Schedule(vertices.Length, 1);
        handle.Complete();

        // Here, projectedPositionWSBuffer contains the water surface positions for each vertex
        // Use this data to calculate the buoyancy forces to apply to your boat
        // ...
    }

    private void OnDestroy()
    {
        vertexPositionBuffer.Dispose();
        projectedPositionWSBuffer.Dispose();
    }
}
