using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Text;
using UnityEngine.SceneManagement;

public class evaluatePower : MonoBehaviour
{
    // Start is called before the first frame update
    private Vector3 downwindStartingPosition;
    private Vector3 leftStartingPosition;
    private Vector3 rightStartingPosition;
    public GameObject downwindCarPrefab;
    public GameObject leftCarPrefab;
    public GameObject rightCarPrefab;

    private GameObject downwindCar;
    private GameObject leftCar;
    private GameObject rightCar;

    private Transform downwindCarTransform;
    private Transform leftCarTransform;
    private Transform rightCarTransform;

    public string csvFilePath = "scores.csv";
    public string experimentName = "test";
    private string _csvFilePath;
    private float _timer;

    public int maxResets = 10;

    public float resetTime = 20f;
    public int resetCounter = 0;
    void Start()
    {
        // get starting position
        // startingPosition = transform.position;
        _csvFilePath = Application.dataPath + "/scores/" + csvFilePath;
        _timer = 0f;
        if (!File.Exists(_csvFilePath))
        {
            File.WriteAllText(_csvFilePath, "Timestamp,Experiment,Car,Score\n");
        }
        reset();
    }


    void reset()
    {
        resetCounter++;
        if (downwindCar != null)
        {
            float distanceDown = Vector3.Distance(downwindCarTransform.position, downwindStartingPosition);
            WriteRowToCsv("downwind", distanceDown);
            Destroy(downwindCar);
        }
        if (leftCar != null)
        {
            float distanceLeft = Vector3.Distance(leftCarTransform.position, leftStartingPosition);
            bool distanceSign = leftCarTransform.position.x > leftStartingPosition.x;
            distanceLeft = distanceSign ? distanceLeft : -distanceLeft;
            WriteRowToCsv("left", distanceLeft);
            Destroy(leftCar);
        }
        if (rightCar != null)
        {
            float distanceRight = Vector3.Distance(rightCarTransform.position, rightStartingPosition);
            bool distanceSign = rightCarTransform.position.x < rightStartingPosition.x;
            distanceRight = distanceSign ? distanceRight : -distanceRight;
            WriteRowToCsv("right", distanceRight);
            Destroy(rightCar);
        }

        // if (resetCounter > maxResets)
        // {
        //     // exit play mode
        //     UnityEditor.EditorApplication.isPlaying = false;
        // }
        // instantiate cars
        downwindCar = Instantiate(downwindCarPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        // Debug.Log("Downwind car instantiated");
        leftCar = Instantiate(leftCarPrefab, new Vector3(30, 0, 0), Quaternion.identity);
        rightCar = Instantiate(rightCarPrefab, new Vector3(-30, 0, 0), Quaternion.identity);

        // record starting positions from 'base.transform' within children of each car
        foreach (Transform child in downwindCar.transform)
        {
            if (child.name == "Base")
            {
                downwindStartingPosition = child.transform.position;
                downwindCarTransform = child.transform;
            }
        }
        foreach (Transform child in leftCar.transform)
        {
            if (child.name == "Base")
            {
                leftStartingPosition = child.transform.position;
                leftCarTransform = child.transform;
            }
        }
        foreach (Transform child in rightCar.transform)
        {
            if (child.name == "Base")
            {
                rightStartingPosition = child.transform.position;
                rightCarTransform = child.transform;
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        // get distance from starting position
        // Debug.Log("Distance: " + distance);
        // check if more than 30 seconds have passed
        _timer += Time.deltaTime;

        if (_timer >= resetTime)
        {
            _timer = 0f;
            reset();
        }
    }

    private void WriteRowToCsv(string carName, float score)
    {
        using (StreamWriter streamWriter = File.AppendText(_csvFilePath))
        {
            StringBuilder row = new StringBuilder();
            row.Append(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            row.Append(",");
            row.Append(experimentName);
            row.Append(",");
            row.Append(carName); // car name
            row.Append(",");
            row.Append(score); // score
            row.Append("\n");

            streamWriter.Write(row);
        }

        Debug.Log("Row written to CSV");
    }
}
