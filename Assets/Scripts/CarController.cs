using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CarController : MonoBehaviour
{
  // control globals
  public float lerpSpeed = 0.1f;
  private float horizontal = 0.0f;
  private float vertical = 0.0f;
  public float steering = 0.0f;
  
  // player number dropdown
  enum PlayerNumber {
    One = 0,
    Two = 1,
  }
  [SerializeField] PlayerNumber playerNumber = PlayerNumber.One;

  // Visual publics
  public float maxBarOffsetRotation = 25.0f;
  public float maxBarOffsetSpeed = 0.05f;

  // Physics publics
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

  // GameObject publics
  // public GameObject bar;
  public GameObject leftWheel;
  public GameObject rightWheel;

  // Other GameObjects and Components
  private GameObject line;
  private GameObject raider;
  private Rigidbody raiderRigidBody;
  public WheelCollider leftWheelCollider;
  public WheelCollider rightWheelCollider;
  private Transform leftWheelMesh;
  private Transform rightWheelMesh;

  // previous state
  private static int forceHistoryLength = 15;

  private float prevKiteAngularVelocityTheta;
  private float prevKiteAngularVelocityPhi;
  private Vector3 prevKitePosition;
  private Vector3 prevRaiderPosition;
  private Vector3[] prevForceGenerated = new Vector3[forceHistoryLength];
  private Quaternion prevRaiderRotation;
  private Quaternion prevBarRotation;

  
  // current state
  private float gameTime = 0.0f;

  void Start()
  {
    // inital setup and references
    // positioning
    // raider = transform.parent.gameObject.transform.parent.gameObject;
    // components
    // raiderRigidBody = raider.GetComponent<Rigidbody>();
    // leftWheelCollider = leftWheel.GetComponent<WheelCollider>();
    // rightWheelCollider = rightWheel.GetComponent<WheelCollider>();
    
    // raiderRigidBody.sleepThreshold = 0.0f; 

    // prevKiteAngularVelocityTheta = kiteAngularVelocityTheta;
    // prevKiteAngularVelocityPhi = kiteAngularVelocityPhi;
    // prevKiteAngularVelocityGamma = kiteAngularVelocityGamma;
    // prevRaiderRotation = raider.transform.rotation;
    // prevRaiderPosition = raider.transform.position;
  }

  // Update is called once per frame
  void Update()
  {
    gameTime += Time.deltaTime;

    // Draw/update all visual elements
    
    // Wheels
    leftWheel.transform.localRotation = Quaternion.Euler(0, steering * maxSteerAngle, 0);
    rightWheel.transform.localRotation = Quaternion.Euler(0, steering * maxSteerAngle, 0);

    // Bar
    // bar.transform.localRotation = Quaternion.Lerp(prevBarRotation, Quaternion.Euler(0, 0, -lerpSpeed*horizontal * 10 * maxBarOffsetRotation), maxBarOffsetSpeed);

    // This (kite + bar)
  }
  void FixedUpdate()
  {

    // Controls
    // if (playerNumber == PlayerNumber.One) {
    // //   horizontal = Input.GetAxis("P1Horizontal");
    // //   vertical = Input.GetAxis("P1Vertical");
    // //   steering = Input.GetAxis("P1Steering");
    // } else {
    // //   horizontal = Input.GetAxis("P2Horizontal");
    // //   vertical = -Input.GetAxis("P2Vertical");
    // //   steering = Input.GetAxis("P2Steering");
    // }
    
    // DEBUG Drive mechanics for car
    // raiderRigidBody.AddForce(raider.transform.forward + raider.transform.up *0.2f * vertical * 3000);
    // DEBUG resets raider
    // // if player 1
    // if (playerNumber == PlayerNumber.One) {
    //   // if player 1 pressing 'o'
    //   if (Input.GetKeyDown(KeyCode.C)) {
    //     raider.transform.position += new Vector3(0, 2,0);
    //     raider.transform.rotation = Quaternion.identity;
    //     systemRotation = Quaternion.Euler(0, 0, 0);
    //     prevKitePosition = kite.transform.position;
    //     for (int i = 0; i < forceHistoryLength; i++) {
    //       prevForceGenerated[i] = Vector3.zero;
    //     }
    //   }
    // } else {
    //   // if player 2 pressing 'p'
    //   if (Input.GetKeyDown(KeyCode.V)) {
    //     raider.transform.position += new Vector3(0, 2, 0);
    //     raider.transform.rotation = Quaternion.identity;
    //     systemRotation = Quaternion.Euler(0, 0, 0);
    //     prevKitePosition = kite.transform.position;
    //     for (int i = 0; i < forceHistoryLength; i++) {
    //       prevForceGenerated[i] = Vector3.zero;
    //     }
    //   }
    // }
    
    // Steering
    leftWheelCollider.steerAngle = steering * maxSteerAngle;
    rightWheelCollider.steerAngle = steering * maxSteerAngle;
    
    // Kite Simulation
    // DEBUG draw line above raider to show wind
    // Debug.DrawLine(raider.transform.position + Vector3.up * 2, raider.transform.position + Vector3.up * 2 + wind * 2.0f, Color.blue);
    // Vector3 bar2Kite = (kite.transform.position - bar.transform.position).normalized;
    // Vector3 kiteLookDirection = -kite.transform.forward;
    // Vector3 kiteVelocity = (kite.transform.position - prevKitePosition) / Time.fixedDeltaTime; // m/s
   
  }

  void OnGUI()
  {
    float screenWidth = Screen.width;
    
    // time
    if (playerNumber == PlayerNumber.One) {
      GUI.Label(new Rect(screenWidth * 0.5f, 10, 100, 20), "Time: " + gameTime.ToString("0.00"));
    }

    // Calculate the speed of the raider
    // string speed = raiderRigidBody.velocity.magnitude.ToString();
    // Debug.Log("speed: " + speed.ToString());

    // player stats
    if (playerNumber == PlayerNumber.One) {
      GUI.Label(new Rect(10, 10, 100, 20), "Player 1");
      // GUI.Label(new Rect(10, 30, 100, 20), "P1 Speed: " + speed + " m/s");
    } else {
      GUI.Label(new Rect(screenWidth - 110, 10, 100, 20), "Player 2");
      // GUI.Label(new Rect(screenWidth - 110, 30, 100, 20), "P2 Speed: " + speed + " m/s");
    }

    // prevRaiderPosition = raider.transform.position;
  }
}
