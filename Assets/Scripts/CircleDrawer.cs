using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CircleDrawer : MonoBehaviour
{
    public float radius = 5f;
    public int segments = 50;

    void Start()
    {
        DrawCircle();
    }

    void DrawCircle()
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segments + 1;

        Vector3[] positions = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            positions[i] = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius) + transform.position;
        }
        lineRenderer.SetPositions(positions);
    }

    public void UpdateCirclePosition(Vector3 newPosition)
    {
        transform.position = newPosition;
        DrawCircle();
    }


}
