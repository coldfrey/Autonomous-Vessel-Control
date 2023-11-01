using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

using UnityEngine.InputSystem;




public class KitePPOAgent : Agent
{

    public bool isResetting = true;
    public Rigidbody kiteRigidbody;

    public Rigidbody baseRigidbody;
    public JointSim jointSim;
    public KiteJoyController joyController;

    // reward mode observations
    public string rewardMode;

    private float cumulativeReward = 0.0f;

    // public BoxCollider floorCollider;
    public SphereCollider kiteCollider;

    public override void Initialize()
    {
        //Initialize agent

        // if no floorCollider, get it from the scene
        // if (floorCollider == null) {
        //     floorCollider = GameObject.Find("ship").GetComponent<BoxCollider>();
        // }

        // Debug.Log("KitePPOAgent initialized");
        // random reward mode.
        // rewardMode0 = Random.value > 0.5f;
        // rewardMode1 = Random.value > 0.5f;
        // rewardMode2 = Random.value > 0.5f;
        // rewardMode3 = Random.value > 0.5f;
        // // start a timer to shuffle reward modes every 10 seconds.
        // InvokeRepeating("shuffleRewardModes", 10.0f, 10.0f);
        // start a timer to tweak wind (gusts) every 10 seconds.
        // InvokeRepeating("tweakWind", 10.0f, 10.0f);
        rewardMode = PlayerPrefs.GetString("rewardMode", "rewardMode0");
    }

    public override void OnEpisodeBegin()
    {
        //Reset agent state
        // Debug.Log("KitePPOAgent episode begin");
    }
    private void tweakWind() {
        // Debug.Log("tweakWind");
        // tweak wind
        // jointSim.wind = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * Random.Range(0.5f, 1.5f);
        // Debug.Log("jointSim.wind: " + jointSim.wind);
    }

    // private void shuffleRewardModes() {
    //     rewardMode0 = Random.value > 0.5f;
    //     rewardMode1 = Random.value > 0.5f;
    //     rewardMode2 = Random.value > 0.5f;
    //     rewardMode3 = Random.value > 0.5f;
    // }

    private Vector3 polarAngles(Vector3 kitePosition, Vector3 wind) {
        Vector3 basePosition = baseRigidbody.position + new Vector3(0, 1f, 0);
        // Debug.DrawLine(basePosition, basePosition + kitePosition, Color.green, 1.0f);
        // Debug.Log("kitePosition: " + kitePosition);
        // wind projected on floor
        Vector3 windOnFloor = new Vector3(wind.x, 0, wind.z);
        // kite position projected on floor
        Vector3 kitePositionOnFloor = Vector3.ProjectOnPlane(kitePosition, Vector3.up);
        // angle between wind and kite
        // Debug.DrawLine(basePosition, basePosition + windOnFloor, Color.red, 1.0f);
        // Debug.DrawLine(basePosition, basePosition + kitePositionOnFloor, Color.blue, 1.0f);

        float angleHorizontal = Vector3.Angle(windOnFloor, kitePositionOnFloor);
        bool angleHorizontalSign = Vector3.Cross(windOnFloor, kitePositionOnFloor).y < 0;
        if (angleHorizontalSign) {
            angleHorizontal = -angleHorizontal;
        }
        // wind projected on vertical plane as defined by direction of kitePositionOnFloor and up.
        // Vector3 windOnVerticalPlane = Vector3.ProjectOnPlane(wind, Vector3.Cross(kitePositionOnFloor, Vector3.up));
        // Debug.DrawLine(basePosition, basePosition + windOnVerticalPlane, Color.yellow, 1.0f);
        // angle between windOnVerticalPlane and kitePosition
        float angleVertical = Vector3.Angle(kitePositionOnFloor, kitePosition);
        // kite transform.forward projected on plane defined by kitePositionOnFloor
        Vector3 kiteForwardOnPlane = Vector3.ProjectOnPlane(kiteRigidbody.transform.forward, kitePositionOnFloor);
        // Debug.DrawLine(basePosition, basePosition + kiteForwardOnPlane, Color.magenta, 1.0f);
        // angle between kiteForwardOnPlane and up
        float angleKiteUp = Vector3.Angle(kiteForwardOnPlane, Vector3.up);

        return new Vector3(angleHorizontal, angleVertical, angleKiteUp);

    }

    public Vector3 cartesianAngles(Vector3 PolarAngles, float lineLength) {
       // convert polar angles to cartesian angles
        float angleHorizontal = PolarAngles.x;
        float angleVertical = PolarAngles.y;
        float angleKiteUp = PolarAngles.z;
        float angleHorizontalRad = angleHorizontal * Mathf.Deg2Rad;
        float angleVerticalRad = angleVertical * Mathf.Deg2Rad;
        float angleKiteUpRad = angleKiteUp * Mathf.Deg2Rad;
        float x = Mathf.Sin(angleHorizontalRad) * Mathf.Cos(angleVerticalRad);
        float y = Mathf.Sin(angleVerticalRad);
        float z = Mathf.Cos(angleHorizontalRad) * Mathf.Cos(angleVerticalRad);
        // line length
        x *= lineLength;
        y *= lineLength;
        z *= lineLength;
        return new Vector3(x, y, z);
    }
     public override void CollectObservations(VectorSensor sensor)
    {
        // Debug.Log("Collecting observations");
        sensor.AddObservation(jointSim.windAtKite); // Vector3

        // sensor.AddObservation(jointSim.BarPositionAsControlInput); // Vector2
        // Debug.Log(jointSim.BarPositionAsControlInput);

        // sensor.AddObservation(kiteRigidbody.position); // Vector3

        sensor.AddObservation(polarAngles(kiteRigidbody.transform.localPosition, jointSim.windAtKite)); // Vector3
        // Debug.Log(polarAngles(kiteLocalPosition, jointSim.windAtKite));
        // sensor.AddObservation(jointSim.windAtKite); // Vector3

        sensor.AddObservation(kiteRigidbody.velocity.magnitude); // float


        sensor.AddObservation(kiteRigidbody.angularVelocity.magnitude); // float

      
        // sensor.AddObservation(rewardMode0); // bool
        // sensor.AddObservation(rewardMode1); // bool
        // sensor.AddObservation(rewardMode2); // bool
        // sensor.AddObservation(rewardMode3); // bool

        // sensor.AddObservation(kiteRigidbody.rotation); // Quaternion
    }
    //Action at each step
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Debug.Log("OnActionReceived");
        // use actions
        // use continuous actions as 'hand positions'
        float leftHand = actions.ContinuousActions[0];
        float rightHand = actions.ContinuousActions[1];
        // convert into horizontal and vertical bar position
        float horizontal;
        float vertical;
        if (PlayerPrefs.GetString("carOrPunching") == "car")
        {
            horizontal = actions.ContinuousActions[0];
            vertical = actions.ContinuousActions[1];
        }
        else
        {
            horizontal = (leftHand - rightHand) / 2.0f;
            vertical = (leftHand + rightHand) / 2.0f;
        }
        // get both continuous actions
        // float vertical = 0.0f;
        // if (horizontal < 0.02f) {
        //     horizontal = 0.0f;
        // }
        // if (vertical < 0.02f) {
        //     vertical = 0.0f;
        // }
        jointSim.BarPositionAsControlInput = new Vector2(horizontal, vertical);

        // set reward
        float reward = 0.0f;
        // bool[] rewardModes = {rewardMode0, rewardMode1, rewardMode2, rewardMode3};

        if (rewardMode == "rewardMode0") {
          // reward for kite being in the air close to the zenith.
          reward += polarAngles(kiteRigidbody.transform.localPosition, jointSim.windAtKite).y / 90.0f;
          reward += Mathf.Abs(jointSim.currentResultantForce.z) / 100.0f;
        }
        if (rewardMode == "rewardMode1") {
            // reward for kite being in the air on the Left side of the wind.
            reward += polarAngles(kiteRigidbody.transform.localPosition, jointSim.windAtKite).y / 90.0f;
            reward += Mathf.Max(-polarAngles(kiteRigidbody.transform.localPosition, jointSim.windAtKite).x / 90.0f, 0.0f);

        }
        if (rewardMode == "rewardMode2") {
            // reward for kite being in the air on the Right side of the wind.
            reward += polarAngles(kiteRigidbody.transform.localPosition, jointSim.windAtKite).y / 90.0f;
            reward += Mathf.Max(polarAngles(kiteRigidbody.transform.localPosition, jointSim.windAtKite).x / 90.0f, 0.0f);
            // ideally the resultant force to the right should be rewarded.
        }
        if (rewardMode == "rewardMode3") {
            // reward downwind force generated
            reward += polarAngles(kiteRigidbody.transform.localPosition, jointSim.windAtKite).y / 90.0f;
            reward += Vector3.Dot(kiteRigidbody.velocity, jointSim.windAtKite.normalized);
        }
        
        // reward += jointSim.score - cumulativeReward;
        // cumulativeReward = jointSim.score;

        // small reward for moving the bar.
        reward += jointSim.BarPositionAsControlInput.magnitude * 0.1f;
        // penalty for moving the bar too full left or right
        if (Mathf.Abs(jointSim.BarPositionAsControlInput.x)>0.99f) {
            reward -= 1f;
        }

        if(!isResetting) {
            isResetting = jointSim.isResetting;
        } else {
            reward -= 10f;
            EndEpisode();
            isResetting = false;
        }
        SetReward(reward);
        // if (isResetting){
        //   if (jointSim.counter > 2000) {
        //       reward += 10f;
        //       EndEpisode();
        //       jointSim.Reset();
        //   }
        // }

        // potentially end episode
        // if (kiteCollider.bounds.Intersects(floorCollider.bounds)) {
        //     Debug.Log("Kite is on the floor, resetting (score: " + jointSim.score + ")");
        //     Debug.Log("Kite position: " + polarAngles(kiteRigidbody.transform.localPosition, jointSim.windAtKite));
        //     Debug.Log("Counter: " + jointSim.counter);
        //     SetReward(-10f);
        //     // cumulativeReward = 0.0f;
        //     EndEpisode();
        //     jointSim.Reset();
        // }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        // Debug.Log("Heuristics: " + joyController.barPosition);
        jointSim.BarPositionAsControlInput = joyController.barPosition;
        // Debug.Log("Heuristics: " + joyController.barPosition);
        continuousActionsOut[0] = jointSim.BarPositionAsControlInput.x;
        continuousActionsOut[1] = jointSim.BarPositionAsControlInput.y;
    }

    // private void OnGUI() {
    //     // GUI.Label(new Rect(10, 10, 100, 20), "Score: " + jointSim.score);
    //     // polar angles
    //     GUI.Label(new Rect(10, 30, 100, 20), "Horizontal: " + polarAngles(kiteRigidbody.transform.localPosition, jointSim.windAtKite).x);
    //     GUI.Label(new Rect(10, 50, 100, 20), "Vertical: " + polarAngles(kiteRigidbody.transform.localPosition, jointSim.windAtKite).y);
    //     GUI.Label(new Rect(10, 70, 100, 20), "Kite Up: " + polarAngles(kiteRigidbody.transform.localPosition, jointSim.windAtKite).z);
    // }
}
