using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class StaticKiteController : MonoBehaviour
{


    // control globals
    public InputAction wasdAction;

    public float lerpSpeed = 0.1f;

    private float horizontal = 0.0f;
    private float vertical = 0.0f;

    private float steering = 0.0f;

    // player number dropdown
    enum PlayerNumber
    {
        One = 0,
        Two = 1
    }

    [SerializeField]
    PlayerNumber playerNumber = PlayerNumber.One;

    // Visual publics
    public float maxBarOffsetRotation = 25.0f;

    public float maxBarOffsetSpeed = 0.05f;

    // Physics public's
    public float maxTurnSpeed = 2f;

    public float lineLength = 15.0f;

    public float forceMultiplier = 0.05f;

    public float windSpeed = 1.0f;

    public float liftEfficiency = 0.5f;

    public float dragEfficiency = 0.5f;

    public float maximumKiteFallRate = 0.5f;

    public float maxSteerAngle = 30.0f;

    public float selfRightingSpeed = 0.1f;

    public float fuckItNumber = 0.1f;

    public float MaximumSpeed = 100f;

    // GameObject public's
    public GameObject kite;

    public GameObject bar;

    public GameObject leftWheel;

    public GameObject rightWheel;

    // Other GameObjects and Components
    private GameObject line;

    // private GameObject raider;
    // private Rigidbody raiderRigidBody;
    // private WheelCollider leftWheelCollider;
    // private WheelCollider rightWheelCollider;
    private Transform leftWheelMesh;

    private Transform rightWheelMesh;

    // previous state
    private static int forceHistoryLength = 15;

    private float prevKiteAngularVelocityTheta;

    private float prevKiteAngularVelocityPhi;

    private Vector3 prevKitePosition;

    // private Vector3 prevRaiderPosition;
    private Vector3[] prevForceGenerated = new Vector3[forceHistoryLength];

    // private Quaternion prevRaiderRotation;
    private Quaternion prevBarRotation;

    private Quaternion prevSystemRotation;

    // current state
    private float gameTime = 0.0f;

    private Vector3 sumOfForces = Vector3.zero;

    private Vector3 wind = Vector3.zero;

    // physics state
    private float kiteTheta;

    private float kitePhi;

    private float kiteGamma;

    private float kiteAngularVelocityTheta;

    private float kiteAngularVelocityPhi;

    private float kiteAngularVelocityGamma;

    private Vector3 kitePosition;

    private Vector3 apparentWind;

    private Quaternion systemRotation;

    public void OnEnable()
    {
        wasdAction.Enable();
        // set the action to KiteControls 'move'
        
    }

    public void OnDisable()
    {
        wasdAction.Disable();
    }

    public void OnMove(InputValue context)
    {
        Vector2 input = context.Get<Vector2>();
        horizontal = input.x;
        vertical = input.y;
        
    }

    // private Quaternion kiteRotation; // future use
    void Start()
    {
        // print GetJoystickNames();
        Debug.Log("Start");
        // Debug.Log(Input.GetJoystickNames());
        // initial setup and references
        // positioning
        kite.transform.Translate(0, lineLength, 0);

        // gameObjects
        // raider = transform.parent.gameObject.transform.parent.gameObject;
        // components
        // raiderRigidBody = raider.GetComponent<Rigidbody>();
        // leftWheelCollider = leftWheel.GetComponent<WheelCollider>();
        // rightWheelCollider = rightWheel.GetComponent<WheelCollider>();
        // raiderRigidBody.sleepThreshold = 0.0f;
        // initialize physics state
        kitePosition = kite.transform.position;
        kiteTheta = 90.0f;
        kitePhi = 0.0f;
        kiteGamma = 0.0f;
        kiteAngularVelocityTheta = 0.0f;
        kiteAngularVelocityPhi = 0.0f;
        kiteAngularVelocityGamma = 0.0f;
        for (int i = 0; i < forceHistoryLength; i++)
        {
            prevForceGenerated[i] = Vector3.zero;
        }
        wind = new Vector3(0, 0, windSpeed); // m/s (9 m/s = 20 mph)
        apparentWind = wind;

        // calculate system rotation
        systemRotation =
            Quaternion.Euler(kiteTheta, kitePhi, kiteGamma) *
            Quaternion.Euler(0, 0, 0);

        // initialize previous state
        prevKitePosition = kitePosition;
        prevSystemRotation = systemRotation;
        // prevKiteAngularVelocityTheta = kiteAngularVelocityTheta;
        // prevKiteAngularVelocityPhi = kiteAngularVelocityPhi;
        // prevKiteAngularVelocityGamma = kiteAngularVelocityGamma;
    }

    // Update is called once per frame
    void Update()
    {
        gameTime += Time.deltaTime;

        // Draw/update all visual elements
        // Wheels
        leftWheel.transform.localRotation =
            Quaternion.Euler(0, steering * maxSteerAngle, 0);
        rightWheel.transform.localRotation =
            Quaternion.Euler(0, steering * maxSteerAngle, 0);

        // Bar
        bar.transform.localRotation =
            Quaternion
                .Lerp(prevBarRotation,
                Quaternion
                    .Euler(0,
                    0,
                    -lerpSpeed * horizontal * 10 * maxBarOffsetRotation),
                maxBarOffsetSpeed);

        // This (kite + bar)
        transform.rotation =
            Quaternion.Lerp(transform.rotation, systemRotation, 1.0f);
    }

    void FixedUpdate()
    {
        // Controls
        if (playerNumber == PlayerNumber.One)
        {
            // horizontal = Input.GetAxis("P1Horizontal");
            // vertical = Input.GetAxis("P1Vertical");
            // steering = Input.GetAxis("P1Steering");
        }
        else
        {
            // horizontal = Input.GetAxis("P2Horizontal");
            // vertical = -Input.GetAxis("P2Vertical");
            // steering = Input.GetAxis("P2Steering");
        }

        // DEBUG Drive mechanics for car
        // raiderRigidBody.AddForce(raider.transform.forward + raider.transform.up *0.2f * vertical * 3000);
        // DEBUG resets raider
        // if player 1
        if (playerNumber == PlayerNumber.One)
        {
            // if player 1 pressing 'o'
            // if (Input.GetKeyDown(KeyCode.C))
            {
                // raider.transform.position += new Vector3(0, 2,0);
                // raider.transform.rotation = Quaternion.identity;
                systemRotation = Quaternion.Euler(0, 0, 0);
                prevKitePosition = kite.transform.position;
                for (int i = 0; i < forceHistoryLength; i++)
                {
                    prevForceGenerated[i] = Vector3.zero;
                }
            }
        }
        else
        {
            // if player 2 pressing 'p'
            // // if (Input.GetKeyDown(KeyCode.V))
            // {
            //     // raider.transform.position += new Vector3(0, 2, 0);
            //     // raider.transform.rotation = Quaternion.identity;
            //     systemRotation = Quaternion.Euler(0, 0, 0);
            //     prevKitePosition = kite.transform.position;
            //     for (int i = 0; i < forceHistoryLength; i++)
            //     {
            //         prevForceGenerated[i] = Vector3.zero;
            //     }
            // }
        }

        bool isKiteGrounded = false;

        // Check if raider is on the ground using raycast
        RaycastHit hit;

        // if (Physics.Raycast(raider.transform.position, -raider.transform.up, out hit, 1.0f)) {
        // if on ground
        //   isKiteGrounded = true;
        // } else {
        //   isKiteGrounded = false;
        // }
        // add self-righting force to raider
        // if (!(isKiteGrounded)) {
        //   // rotate to align with ground
        //   // Debug.Log("self righting");
        //   var rot = Quaternion.FromToRotation(raider.transform.up, Vector3.up);
        //   raiderRigidBody.AddTorque(new Vector3(rot.x, rot.y, rot.z)*selfRightingSpeed);
        // }
        // TODO: use vertical2 to apply breaks maybe?
        // Steering
        // leftWheelCollider.steerAngle = steering * maxSteerAngle;
        // rightWheelCollider.steerAngle = steering * maxSteerAngle;
        // Kite Simulation
        // DEBUG draw line above raider to show wind
        // Debug.DrawLine(raider.transform.position + Vector3.up * 2, raider.transform.position + Vector3.up * 2 + wind * 2.0f, Color.blue);
        Vector3 bar2Kite =
            (kite.transform.position - bar.transform.position).normalized;
        Vector3 kiteLookDirection = -kite.transform.forward;
        Vector3 kiteVelocity =
            (kite.transform.position - prevKitePosition) / Time.fixedDeltaTime; // m/s
        apparentWind = wind + kiteVelocity; // m/s
        float kiteAcceleration = Vector3.Dot(wind, kite.transform.forward);
        float airFlowMadeGood =
            Mathf.Max(0, Vector3.Dot(apparentWind, kite.transform.up)) +
            Mathf
                .Max(0,
                0.2f * Vector3.Dot(apparentWind, -kite.transform.forward));

        // Force generated by the kite
        Vector3 force2add =
            airFlowMadeGood * forceMultiplier * bar2Kite +
            fuckItNumber * bar2Kite;

        // Kite Dynamics
        float deltaKiteAngularVelocityTheta =
            airFlowMadeGood * liftEfficiency -
            apparentWind.magnitude * dragEfficiency;

        // // DEBUG draw line from kite proportional to force deltaKiteAngularVelocityTheta in the direction of the kite's nose
        Debug
            .DrawLine(kite.transform.position,
            kite.transform.position + kite.transform.up * 0.5f,
            Color.blue);
        Debug
            .DrawLine(kite.transform.position,
            kite.transform.position +
            kiteLookDirection * deltaKiteAngularVelocityTheta,
            Color.red);

        float kiteNewAngularVelocityTheta = 1.0f;
            // Mathf
            //     .Min(Mathf
            //         .Max(-maximumKiteFallRate,
            //         prevKiteAngularVelocityTheta +
            //         deltaKiteAngularVelocityTheta),
            //     MaximumSpeed);

        // kiteNewAngularVelocityTheta = Input
            // .GetAxis("Vertical") * MaximumSpeed;
        kiteNewAngularVelocityTheta = vertical * MaximumSpeed;
        // apply our found SPEEED!
        systemRotation *=
            Quaternion
                .Euler(-kiteNewAngularVelocityTheta * Time.fixedDeltaTime * 5.0f, 0, 0);

        // systemRotation *=
        //     Quaternion.Euler(-fuckItNumber * Time.fixedDeltaTime * 5.0f, 0, 0);
        systemRotation *=
            Quaternion
                .Euler(0, -maxTurnSpeed * horizontal * Time.fixedDeltaTime, 0);

        // sum previous forces
        sumOfForces = Vector3.zero;
        for (int i = 0; i < prevForceGenerated.Length; i++)
        {
            sumOfForces += prevForceGenerated[i];
        }

        // raiderRigidBody.AddForce(force2add + sumOfForces/forceHistoryLength);
        // store prevs
        for (int i = 1; i < prevForceGenerated.Length; i++)
        {
            prevForceGenerated[i] = prevForceGenerated[i - 1] * 0.7f;
        }
        prevForceGenerated[0] = force2add;
        prevBarRotation = bar.transform.localRotation;
        prevKitePosition = kite.transform.position;
    }

    void OnGUI()
    {
        float screenWidth = Screen.width;

        // time
        if (playerNumber == PlayerNumber.One)
        {
            GUI
                .Label(new Rect(screenWidth * 0.5f, 10, 100, 20),
                "Time: " + gameTime.ToString("0.00"));
        }

        // Calculate the speed of the raider
        // string speed = raiderRigidBody.velocity.magnitude.ToString();
        string speed = "0";

        // Debug.Log("speed: " + speed.ToString());
        // player stats
        if (playerNumber == PlayerNumber.One)
        {
            GUI.Label(new Rect(10, 10, 100, 20), "Player 1");
            GUI.Label(new Rect(10, 30, 100, 20), "P1 Speed: " + speed + " m/s");
        }
        else
        {
            GUI.Label(new Rect(screenWidth - 110, 10, 100, 20), "Player 2");
            GUI
                .Label(new Rect(screenWidth - 110, 30, 100, 20),
                "P2 Speed: " + speed + " m/s");
        }

        // player controls

        if (playerNumber == PlayerNumber.One)
        {
            GUI.Label(new Rect(10, 50, 100, 20), "Controls:");
            GUI
                .Label(new Rect(screenWidth - 110, 90, 100, 20),
                horizontal.ToString());
            GUI.Label(new Rect(10, 90, 100, 20), "Space to jump");
            GUI.Label(new Rect(10, 110, 100, 20), "Shift to sprint");
            GUI.Label(new Rect(10, 130, 100, 20), "Mouse to look");
        }
        else
        {
            GUI.Label(new Rect(screenWidth - 110, 50, 100, 20), "Controls:");
            GUI
                .Label(new Rect(screenWidth - 110, 70, 100, 20),
                "Horizontal:");
            

            GUI
                .Label(new Rect(screenWidth - 110, 90, 100, 20),
                "Enter to jump");
            GUI
                .Label(new Rect(screenWidth - 110, 110, 100, 20),
                "Right Shift to sprint");
            GUI
                .Label(new Rect(screenWidth - 110, 130, 100, 20),
                "Mouse to look");
        }
    }
}
