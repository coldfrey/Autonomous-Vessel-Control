using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.IO;
using UnityEngine.SceneManagement;


public class StatsCollector : MonoBehaviour
{
    public JointSim[] jointSims;

    // public ModelLoader modelLoader;

    public WindSim windSim;

    public List<int> numberOfSteps = new List<int>(25);

    public float finalScore = 0.0f;

    public int numberOfCrashes = 0;

    public float forwardsResultantForce = 0.0f;

    public float timeElapsed = 0.0f;


    [System.Serializable]
    public class CheckpointData
    {
        public FlyingData Flying;
    }

    [System.Serializable]
    public class FlyingData
    {
        public OtherCheckpointData[] checkpoints;
        public FinalCheckpointData final_checkpoint;
    }

    [System.Serializable]
    public class FinalCheckpointData
    {
        public int steps;
    }
    
    [System.Serializable]
    public class OtherCheckpointData
    {
        public int steps;
    }

    private bool isEvaluating;
    private int currentModelIndex;

    void Start()
    {
        InitializeZeroList();
        // jointSims = FindObjectsOfType<JointSim>();
        // number of steps is found in string "Assets/ML_PPO/results/" + modelName + "/run_logs/training_status.json" under Flying.final_checkpoint.steps
        string model_name = PlayerPrefs.GetString("model_name");
        string training_status_path = "Assets/ML_PPO/results/" + model_name + "/run_logs/training_status.json";
        if (!System.IO.File.Exists(training_status_path))
        {
            Debug.Log("No training status file");
            Debug.Log(training_status_path);
            return;
        }
        string training_status_json = System.IO.File.ReadAllText(training_status_path);
        Debug.Log(training_status_json);
        CheckpointData runData = JsonUtility.FromJson<CheckpointData>(training_status_json);
        // Debug.Log(runData.Flying.final_checkpoint.steps);
        // Debug.Log(runData.Flying.checkpoints[0].steps);

        int i = 0;
        foreach (OtherCheckpointData checkpoint in runData.Flying.checkpoints)
        {
            Debug.Log("checkpoint " + i + " has " + checkpoint.steps + " steps");
            numberOfSteps[i] = checkpoint.steps;
            i++;
        }
        isEvaluating = false;
        currentModelIndex = 0;
    }

    void Update()
    {
        if (!isEvaluating)
        {
            ResetEvaluationStats();
            // TODO: FIX MODEL LOADER
            // if (modelLoader.LoadModelAtIndex(currentModelIndex))
            // {
            //     Debug.Log("Loaded model at index " + currentModelIndex);
            // } else
            // {
            //     Debug.Log("Finished Evaluation " + currentModelIndex);
            //     SceneManager.LoadScene("EvaluationMenuScene");
            // }
            isEvaluating = true;
        }
        forwardsResultantForce = 0.0f;
        foreach (JointSim jointSim in jointSims)
        {
            forwardsResultantForce -= jointSim.currentResultantForce.z;
        }
        finalScore += forwardsResultantForce * Time.deltaTime;
        timeElapsed += Time.deltaTime;

        // if (timeElapsed > 5.0f) // for testing
        if (timeElapsed > 30.0f)
        // if (timeElapsed > 60.0f)
        {
            SaveStats();
            currentModelIndex++;
            currentModelIndex++;
            // SceneManager.LoadScene("EvaluationMenuScene");
            isEvaluating = false;
        }
    }

    private void ResetEvaluationStats()
    {
        timeElapsed = 0.0f;
        finalScore = 0.0f;
        numberOfCrashes = 0;
    }

    public void OnCrash()
    {
        numberOfCrashes++;
        // finalScore -= 100.0f;
    }

    void InitializeZeroList()
    {
        numberOfSteps.Clear(); // Clear the list to make sure it's empty before initializing

        // Initialize the list of 20 integers with zeros
        for (int i = 0; i < 25; i++)
        {
            numberOfSteps.Add(0);
        }
    }

    private void SaveStats()
    {
        string modelName = PlayerPrefs.GetString("model_name");
        Debug.Log("Saving stat for " + modelName);
        string modelPath = "Assets/ML_PPO/results/" + modelName + "/evaluation.csv";
        if (!System.IO.File.Exists(modelPath))
        {
            Debug.Log("No stats file");
            // make new file
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(modelPath, true))
            {
                file.WriteLine("model_name,steps,time_elapsed,final_score,number_of_crashes,varying_wind, wind_strength");
            }
        }
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(modelPath, true))
        {
            file.WriteLine(modelName + "," + numberOfSteps[currentModelIndex] + "," + timeElapsed + "," + finalScore + "," + numberOfCrashes + "," + windSim.varyWind + "," + windSim.wind.magnitude);
        }

    }
}

