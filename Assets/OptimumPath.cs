using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptimumPath : MonoBehaviour
{
    public BuoyManager buoyManager;
    public float resolution = 1.0f;  // Spacing between points in the spline
    public Color lineColor = Color.green;

    private LineRenderer lineRenderer;
    
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            gameObject.AddComponent<LineRenderer>();
            lineRenderer = GetComponent<LineRenderer>();
        }
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.startWidth = 1.1f;
        lineRenderer.endWidth = 1.1f;
    }
    
    void Update()
    {
        DrawSpline();
    }
    
    void DrawSpline()
    {
        if (buoyManager != null)
        {
            Vector3[] buoyPositions = buoyManager.GetBuoyPositions();  // You need to implement this method in your BuoyManager
            // increase the y position of each buoy by 1.5f
            for (int i = 0; i < buoyPositions.Length; i++)
            {
                buoyPositions[i].y += 1.5f;
            }
            List<Vector3> splinePoints = new List<Vector3>();
            
            for (int i = 0; i < buoyPositions.Length - 1; i++)
            {
                Vector3 p0 = buoyPositions[LoopIndex(i-1, buoyPositions.Length)];  // The point before p1
                Vector3 p1 = buoyPositions[i];  // The point we're trying to find a smooth path around
                Vector3 p2 = buoyPositions[LoopIndex(i+1, buoyPositions.Length)];  // The point after p1
                Vector3 p3 = buoyPositions[LoopIndex(i+2, buoyPositions.Length)];  // The point after p2

                float t = 0f;
                while (t < 1)
                {
                    Vector3 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3);
                    splinePoints.Add(newPos);
                    t += resolution;
                }
            }
            lineRenderer.positionCount = splinePoints.Count;
            lineRenderer.SetPositions(splinePoints.ToArray());
        }
    }

    Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

        Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

        return pos;
    }

    int LoopIndex(int i, int n)
    {
        return (i + n) % n;
    }
}