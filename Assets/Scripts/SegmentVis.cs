using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SegmentVis : MonoBehaviour
{
    public Vector3 vector1;
    public Vector3 vector2;

    private Vector3 previousVector1;
    private Vector3 previousVector2;
    public int arcResolution = 30; // Number of points to create the arc
    private Mesh mesh;

    private bool meshNeedsUpdate = false;


    private void Start()
    {
        previousVector1 = vector1;
        previousVector2 = vector2;
        CreateMeshBetweenVectorsAndArcPoints();
    }

    // private void OnDrawGizmos()
    // {
    //     // Draw the vectors
    //     Gizmos.color = Color.yellow;
    //     Gizmos.DrawLine(transform.position, transform.position + vector1);

    //     Gizmos.DrawLine(transform.position, transform.position + vector2);

    //     // Draw the arc between the vectors
    //     Vector3[] arcPoints = new Vector3[arcResolution + 1];
    //     for (int i = 0; i <= arcResolution; i++)
    //     {
    //         float t = i / (float)arcResolution;
    //         arcPoints[i] = Vector3.Slerp(vector1, vector2, t);
    //     }
    //     for (int i = 0; i < arcResolution; i++)
    //     {
    //         Gizmos.DrawLine(transform.position + arcPoints[i], transform.position + arcPoints[i + 1]);
    //     }
    // }

    private void OnValidate()
    {
        previousVector1 = vector1;
        previousVector2 = vector2;
        meshNeedsUpdate = true;
    }


    private void Update()
    {
        if (previousVector1 != vector1 || previousVector2 != vector2 || meshNeedsUpdate)
        {
            previousVector1 = vector1;
            previousVector2 = vector2;
            CreateMeshBetweenVectorsAndArcPoints();
            meshNeedsUpdate = false;
        }
    }

    private void CreateMeshBetweenVectorsAndArcPoints()
    {
        // Create the mesh
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Create the vertices
        Vector3[] vertices = new Vector3[arcResolution + 4];

        vertices[0] = Vector3.zero;
        vertices[1] = vector1;
        vertices[2] = vector2;

        for (int i = 0; i <= arcResolution; i++)
        {
            float t = i / (float)arcResolution;
            vertices[i + 3] = Vector3.Slerp(vector1, vector2, t);
        }

        // Create the triangles
        int[] triangles = new int[(arcResolution + 1) * 3];

        for (int i = 0; i < arcResolution; i++)
        {
            triangles[i * 3] = 0; // center point
            triangles[i * 3 + 1] = i + 4;
            triangles[i * 3 + 2] = i + 3;
        }

        // Add triangle for the end segment between arc end and vector2
        triangles[arcResolution * 3] = 0;
        triangles[arcResolution * 3 + 1] = 2; // vector2
        triangles[arcResolution * 3 + 2] = vertices.Length - 1;

        // Assign the vertices and triangles to the mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;


        mesh.RecalculateNormals();


        transform.GetComponent<MeshFilter>().mesh = mesh;
    }



}
