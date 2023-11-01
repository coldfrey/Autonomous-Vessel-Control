using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using UnityEngine.AI;
#if UNITY_EDITOR
    using UnityEditor;
#endif

public class KiteJoyController : MonoBehaviour
{
    public bool isEvaluation = false; // feels a bit volkswagen...

    private Vector2 moveInput;
    private Vector2 lookInput;
    public float upDownInput;
    public Vector2 barPosition;
    public Transform barTransform;
    public bool isCut;

    public ConfigurableJoint joint1;
    public ConfigurableJoint joint2;
    public ConfigurableJoint joint3;
    public ConfigurableJoint joint4;

    private Vector3 initialBarTransformPosition;

    public JointSim jointSim;

    public CarController carController;

    [SerializeField] private Unity.Barracuda.NNModel leftModel;
    [SerializeField] private Unity.Barracuda.NNModel rightModel;

    public BehaviorParameters behaviorParameters;

    public KitePPOAgent agent;

    void Start()
    {
        jointSim = GetComponent<JointSim>();
        
        // if (isEvaluation) {
        //     string modelName = PlayerPrefs.GetString("model_name");
        //     if (modelName == "")
        //     {
        //         Debug.Log("No model name");
        //         return;
        //     }
        //     // ? string modelDirectory = "Assets/ML_PPO/results/" + modelName + "/flying.onnx";
        //     string modelDirectory = "Assets/ML_PPO/results/" + modelName + "/Flying.onnx";

        //     if (!System.IO.File.Exists(modelDirectory))
        //     {
        //         Debug.Log("No model file");
        //         return;
        //     }
            
        //     #if UNITY_EDITOR
        //         Unity.Barracuda.NNModel nnModel = AssetDatabase.LoadAssetAtPath<Unity.Barracuda.NNModel>(modelDirectory);
        //     #endif
        //     if(nnModel == null)
        //     {
        //         Debug.Log("Failed to load model");
        //         return;
        //     }
        //     behaviorParameters.Model = nnModel;
        // }

        // get initial position of bar
        if (barTransform != null)
        {
            // Debug.Log("No bar transform");
            initialBarTransformPosition = barTransform.localPosition;
        }
    }

    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        if (moveInput.magnitude < 0.1)
        {
            moveInput = Vector2.zero;
        }
        if (carController != null) {
        carController.steering = moveInput.x;
        // carController.throttle = moveInput.y;
        }
    }
    private void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
        if (lookInput.magnitude < 0.1)
        {
            lookInput = Vector2.zero;
        }
    }

    private void OnBarPressure(InputValue value)
    {
        upDownInput = value.Get<float>();
    }

    public void CutKite()
    {
        if (joint1 == null)
        {
            Debug.Log("No joint");
            return;
        }
        joint1.breakForce = 0.1f;
        isCut = true;
        if (joint2 != null)
        {
            joint2.breakForce = 0.1f;
        }
        if (joint3 != null)
        {
            joint3.breakForce = 0.1f;
        }
        if (joint4 != null)
        {
            joint4.breakForce = 0.1f;
        }
    }

    private void OnCutKite(InputValue value)
    {
        Debug.Log("Cutting Kite");
        if (!isCut)
        {
            CutKite();
        }
    }

    // private void OnSwapModelLeft(InputValue value)
    // {
    //     Debug.Log("Swapping Model");
    //     behaviorParameters.Model = leftModel;
    //     if (agent != null)
    //     {
    //         agent.rewardMode1 = true;
    //         agent.rewardMode2 = false;
    //     }
    // }
    // private void OnSwapModelRight(InputValue value)
    // {
    //     Debug.Log("Swapping Model");
    //     behaviorParameters.Model = rightModel;
    //     if (agent != null)
    //     {
    //         agent.rewardMode1 = false;
    //         agent.rewardMode2 = true;
    //     }
    // }

    void Update()
    {
        barPosition = moveInput;
        // Debug.Log("Bar position: " + barPosition);
        if (barTransform != null)
        {
            barTransform.localPosition = initialBarTransformPosition + new Vector3(0, jointSim.BarPositionAsControlInput.y * 0.2f , 0);
            barTransform.localRotation = Quaternion.Euler(0, 0, -jointSim.BarPositionAsControlInput.x * 25f);
        }
    }
}