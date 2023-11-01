using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

[System.Serializable]
public class CourseLayout
{
    public string layoutName;
    public Vector3[] buoyPositions;
}

public class BuoyManager : MonoBehaviour
{
    public GameObject buoyPrefab;
    public CourseLayout[] courseLayouts = 
    {
        new CourseLayout
        {
            layoutName = "test",
            buoyPositions = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(200, 0, 0),
            }
        },
        new CourseLayout
        {
            layoutName = "Box",
            buoyPositions = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(300, 0, 30),
                new Vector3(300, 0, 200),
                new Vector3(50, 0, 220),
            }
        },
        new CourseLayout
        {
            layoutName = "WL",
            buoyPositions = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(0, 0, 100),
                new Vector3(50, 0, 20),
                new Vector3(50, 0, 50),
                new Vector3(300, 0, 30)
            }
        },
    };
    private GameObject[] buoys;
    public GameObject circlePrefab; // Assign the circle prefab in Inspector
    private GameObject[] circles;

    [Header("Layout Management")]
    // public string[] layoutNames;
    public int currentLayoutIndex = 0;


    void Start()
    {
        // print the number of course layouts
        print("Number of course layouts: " + courseLayouts.Length);
        // Initialize buoys based on the maximum number needed for all layouts
        int maxBuoys = GetMaxBuoys();
        buoys = new GameObject[maxBuoys];
        for (int i = 0; i < maxBuoys; i++)
        {
            buoys[i] = Instantiate(buoyPrefab, Vector3.zero, Quaternion.identity);
            buoys[i].SetActive(false); // Hide buoys until a course is set
        }

        // set the com of the rigidbody in each buoy prefab
        foreach (var buoy in buoys)
        {
            buoy.GetComponent<Rigidbody>().centerOfMass = new Vector3(0, 0, -3f);
        }
        // Initialize circles
        circles = new GameObject[GetMaxBuoys()];
        for (int i = 0; i < circles.Length; i++)
        {
            circles[i] = Instantiate(circlePrefab, Vector3.zero, Quaternion.identity);
            circles[i].SetActive(false); // Hide circles until a course is set
        }


        // Set initial layout
        SetCourseLayout(currentLayoutIndex);
    }

    int GetMaxBuoys()
    {
        int max = 0;
        if (courseLayouts.Length == 0)
        {
            print("No course layouts found!");
            return max;
        }
        foreach (var layout in courseLayouts)
        {
            print("Layout: " + layout.layoutName + " has " + layout.buoyPositions.Length + " buoys");
            if (layout.buoyPositions.Length > max)
            {
                max = layout.buoyPositions.Length;
            }
        }
        return max;
    }

    public void SetCourseLayout(int layoutIndex)
    {
        // Hide all buoys and circles
        foreach (var buoy in buoys)
        {
            buoy.SetActive(false);
        }
        foreach (var circle in circles)
        {
            circle.SetActive(false);
        }

        // Activate the specified layout
        var layout = courseLayouts[layoutIndex];
        // var layout = courseLayouts[2];
        for (int i = 0; i < layout.buoyPositions.Length; i++)
        {
            // Activate and position buoys
            buoys[i].transform.position = layout.buoyPositions[i];
            buoys[i].SetActive(true);

            // Activate and position circles
            if (circles[i] != null)
            {
                circles[i].transform.position = layout.buoyPositions[i]; // You might want to adjust the y-axis value to position circles correctly above water
                circles[i].SetActive(true);
                
                // Update circle position if the CircleDrawer script redraws the circle at every frame (if applicable)
                CircleDrawer circleDrawer = circles[i].GetComponent<CircleDrawer>();
                if (circleDrawer != null)
                {
                    circleDrawer.UpdateCirclePosition(layout.buoyPositions[i]);
                }
            }
        }
    }

    
    public Vector3[] GetBuoyPositions()
    {
        Vector3[] positions = new Vector3[buoys.Length];
        for (int i = 0; i < buoys.Length; i++)
        {
            positions[i] = buoys[i].transform.position;
        }
        return positions;
    }

    void OnDrawGizmos()
    {
        if (courseLayouts != null && courseLayouts.Length > currentLayoutIndex)
        {
            foreach (var pos in courseLayouts[currentLayoutIndex].buoyPositions)
            {
                Gizmos.DrawSphere(pos, 5f);
            }
        }
    }


#if UNITY_EDITOR
    // Code to draw buttons in the Unity Inspector
    [UnityEditor.CustomEditor(typeof(BuoyManager))]
    [UnityEditor.CanEditMultipleObjects]
    public class BuoyManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            BuoyManager myScript = (BuoyManager)target;
            if (GUILayout.Button("Set Course Layout"))
            {
                myScript.SetCourseLayout(myScript.currentLayoutIndex);
            }
        }
    }
#endif
}
