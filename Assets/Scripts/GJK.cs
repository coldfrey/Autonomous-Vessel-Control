using UnityEngine;
using System.Collections.Generic;

public class GJKCollisionDetection
{ 

    Vector3 ORIGIN = Vector3.zero;
    public bool GJK(MeshFilter mf1, Vector3 mf1Pos, MeshFilter mf2, Vector3 mf2Pos)
    {
        Vector3[] vertices1 = mf1.mesh.vertices;
        Vector3[] vertices2 = mf2.mesh.vertices;
        Vector3 d = (mf2Pos - mf1Pos).normalized;
        d.y = 0;

        Vector3 simplexPoint = Support(vertices1, mf1.transform, vertices2, mf2.transform, d);
        List<Vector3> simplex = new List<Vector3> { simplexPoint };

        d = -simplexPoint;

        while (true)
        {
            Vector3 A = Support(vertices1, mf1.transform, vertices2, mf2.transform, d);
            A.y = 0;
            if (Vector3.Dot(A, d) < 0)
            {
                return false;
            }

            simplex.Add(A);
            if (HandleSimplex(simplex, ref d))
            {
                return true;
            }
        }
    }

    private Vector3 Support(Vector3[] vertices1, Transform t1, Vector3[] vertices2, Transform t2, Vector3 d)
    {
        // Implement the support function here, considering the object's transformation and 2D (x and z coordinates)

        Vector3 p1 = GetFarthestPointInDirection(vertices1, t1, d);
        Vector3 p2 = GetFarthestPointInDirection(vertices2, t2, -d);

        return p1 - p2;
    }

    private Vector3 GetFarthestPointInDirection(Vector3[] vertices, Transform transform, Vector3 d)
    {
        float highest = -Mathf.Infinity;
        Vector3 supportVertex = Vector3.zero;

        foreach (Vector3 vertex in vertices)
        {
            Vector3 vertexWorld = transform.TransformPoint(vertex);
            float dot = Vector3.Dot(vertexWorld, d);
            if (dot > highest)
            {
                highest = dot;
                supportVertex = vertexWorld;
            }
        }

        return supportVertex;
    }

    private bool HandleSimplex(List<Vector3> simplex, ref Vector3 d)
    {
        if (simplex.Count == 2)
        {
            return LineCase(simplex, ref d);
        }
        else
        {
            return TriangleCase(simplex, ref d);
        }
    }

    private bool LineCase(List<Vector3> simplex, ref Vector3 d)
    {
        Vector3 B = simplex[0];
        Vector3 A = simplex[1];
        Vector3 AB = B - A;
        Vector3 AO = ORIGIN - A;
        Vector3 ABperp = TripleProd(AB, AO, AB);
        d = ABperp;
        return false;
    }

    private bool TriangleCase(List<Vector3> simplex, ref Vector3 d)
    {
        Vector3 C = simplex[0];
        Vector3 B = simplex[1];
        Vector3 A = simplex[2];
        Vector3 AB = B - A;
        Vector3 AC = C - A;
        Vector3 AO = ORIGIN - A;

        Vector3 ABperp = TripleProd(AC, AB, AB);
        Vector3 ACperp = TripleProd(AB, AC, AC);

        if (Vector3.Dot(ABperp, AO) > 0)
        {
            simplex.RemoveAt(0);
            d = ABperp;
            return false;
        }
        else if (Vector3.Dot(ACperp, AO) > 0)
        {
            simplex.RemoveAt(1);
            d = ACperp;
            return false;
        }

        return true;
    }

    private Vector3 TripleProd(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 result = b * Vector3.Dot(c, a) - a * Vector3.Dot(c, b);
        result.y = 0;
        return result;
    }
}
