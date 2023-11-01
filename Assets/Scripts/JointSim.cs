using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;


public class JointSim : MonoBehaviour
{
    // Kite 3D Lift and Drag Simulation
    // Modeling a kite as two rigid bodies connected by a fixed length constraint can be a useful approach for understanding its behavior in the wind. The two rigid bodies can represent the kite and the tether connecting it to the ground. The kite is subject to lift and drag forces which can be calculated based on the fluid dynamics of air.

    // Lift is an aerodynamic force that acts perpendicular to the direction of motion and opposes the weight of the kite. It is generated by the flow of air over the surface of the kite and is proportional to the square of the air velocity. The lift force can be calculated using the lift coefficient, which depends on the shape of the kite and the angle of attack (the angle between the chord line of the kite and the direction of motion).

    // Drag is an aerodynamic force that acts in the direction opposite to the motion of the kite and is proportional to the square of the air velocity. It is caused by the friction and pressure differences between the air in front and behind the kite. The drag coefficient, which depends on the shape of the kite, can be used to calculate the drag force.

    // In order to calculate the total force acting on the kite, both lift and drag forces must be considered. The total force acting on the kite will determine the motion of the kite and the tension in the tether.

    // This model can be used to simulate the behavior of the kite in different wind conditions and to study its dynamics. By considering the forces acting on the kite, it is possible to make predictions about its motion and to design more effective kites for various applications.

    public StatsCollector statsCollector;

    public KiteJoyController joyController;

    public KitePPOAgent agent;

    public Vector3 currentResultantForce;

    // public float WindStrength = 20.0f; // 400

    public WindSim windSim = null;

    public float lineLength = 35.0f;

    public ConfigurableJoint jointPowerLeft;
    public ConfigurableJoint jointPowerRight;
    public ConfigurableJoint jointSteeringLeft;
    public ConfigurableJoint jointSteeringRight;

    public bool varyPhysics = false;
    public float liftCoefficient = 0.3f;
    public float dragCoefficient = 0.1f;
    private float _dragCoefficient = 0.1f;

    public float barPower = 0.15f;

    public SphereCollider kiteCollider;

    // public GameObject theFloor;
    public WaterSurface theSea;

    public float debugScale = 1.0f;
    public Rigidbody KiteRigidbody;
    public Rigidbody BaseRigidbody;

    public Transform windIndicator;

    public Vector3 windAtKite;
    private float time;
    private float magnitudeMin;

    public int counter = 0;

    public float score = 0.0f;

    public Vector2 BarPositionAsControlInput;
    
    private float magnitudeMax;
    private float oscillationSpeed;

    private float stepsPerSecond = 0f;

    public bool isResetting = false;
    private int resetCounter = 0;

    private bool isSteeringLeft = false;
    private bool isSteeringRight = false;
    public float steerSpeed = 0.1f;
    public float returnSpeed = 0.05f;

    private void Start() {
        // Set the wind direction
        time = 0;
        // UpdateWindAtKite(time, Vector3.zero);
        UpdateWindAtKite();
        magnitudeMin = 1.5f;
        magnitudeMax = 2f;
        oscillationSpeed = 0.01f;
        stepsPerSecond = (1 / Time.fixedDeltaTime);
        // start timer to simulate the wind (5 seconds)
        InvokeRepeating("UpdateWindAtKite", 0, 5);
        varyPhysics = PlayerPrefs.GetString("varyPhysics") == "true";
        // InvokeRepeating("Reset", 0, 1);
    }

    // private void UpdateWindAtKite(float time, Vector3 space) {
    private void UpdateWindAtKite() {
        // WindStrength = WindStrength + Random.Range(-WindStrength/50, WindStrength/50);
        // // set max and min wind strength
        // if (WindStrength > 40) {
        //     WindStrength = 40;
        // } else if (WindStrength < 25) {
        //     WindStrength = 25;
        // }
        windAtKite = windSim.wind;
        // Modify the following Unity function so as to model wind as a vector field full of 
    }

    private void Update() {
        time += Time.deltaTime;
        // constrainRotationOfKite();
    }

    private void OnDrawGizmos() {
        // Draw the wind direction
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(windIndicator.position, windIndicator.position - windAtKite);

        // Draw the resultant force from the boat's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(BaseRigidbody.position, BaseRigidbody.position + currentResultantForce * debugScale);

        
    }

    private float calculateAngleOfAttack(Vector3 apparentWind) {
        // Calculate the angle of attack
        float angleOfAttack = 180 - Vector3.Angle(apparentWind, KiteRigidbody.transform.forward);
        // Debug.Log("Angle of attack: " + angleOfAttack);
        return angleOfAttack;
    }

    private void startResetTimer() {
        StartCoroutine(resetTimer());
    }

    private IEnumerator resetTimer() {
        yield return new WaitForSeconds(1.0f);
        // reset the kite
        Reset();
        StartCoroutine(resetIsResettingTimer());
    }

    private IEnumerator resetIsResettingTimer() {
        yield return new WaitForSeconds(0.3f);
        isResetting = false;
    }

    // manual controls

    public void SteerLeft()
    {
        isSteeringLeft = true;
        isSteeringRight = false;
    }

    public void SteerRight()
    {
        isSteeringRight = true;
        isSteeringLeft = false;
    }

    private void UpdateBarPosition()
    {
        if (isSteeringLeft)
        {
            BarPositionAsControlInput.x -= steerSpeed;
            BarPositionAsControlInput.x = Mathf.Clamp(BarPositionAsControlInput.x, -1f, 1f);  // Assuming a max range of -1 to 1.
        }
        else if (isSteeringRight)
        {
            BarPositionAsControlInput.x += steerSpeed;
            BarPositionAsControlInput.x = Mathf.Clamp(BarPositionAsControlInput.x, -1f, 1f);
        }
        else
        {
            // Lerp back to center if neither steer is active
            BarPositionAsControlInput.x = Mathf.Lerp(BarPositionAsControlInput.x, 0, returnSpeed);
        }
    }

    public void Reset() {
        if (statsCollector != null) statsCollector.OnCrash();
        // reset the kite
        // resetCounter++;
        // random horizontal range -45 to 45
        // random vertical range 60 to 80
        // Vector3 pos = agent.cartesianAngles(new Vector3(Random.Range(-90, 0), Random.Range(10, 50), Random.Range(90,150)), lineLength);
        
        Vector3 polars = new Vector3(90 , Random.Range(45,135), 0);
        Vector3 pos = agent.cartesianAngles(polars, lineLength);
        Debug.Log("resetting to: " + polars);


        KiteRigidbody.transform.position = BaseRigidbody.transform.position + pos;
        // KiteRigidbody.transform.position = BaseRigidbody.transform.position + new Vector3(0, lineLength-3f, -3f);
        KiteRigidbody.transform.rotation = Quaternion.identity;
        KiteRigidbody.transform.Rotate(0, Random.Range(-1, 1), 0);
        KiteRigidbody.velocity = Vector3.zero;
        KiteRigidbody.angularVelocity = Vector3.zero;
        score = 0.0f;
        counter = 0;
    }

    private void FixedUpdate() {
        counter++;
        // constrainRotationOfKite();

        // Debug.Log("distance: " + Vector3.Distance(KiteRigidbody.transform.position, BaseRigidbody.transform.position));

        UpdateBarPosition();

        Vector3 apparentWind = windAtKite - KiteRigidbody.velocity;

        float angleOfAttack = calculateAngleOfAttack(apparentWind);

        float _liftCoefficient = liftCoefficient * Mathf.Cos((angleOfAttack-20f) * Mathf.Deg2Rad) * liftCoefficient;

        float apparentWindSpeedSquared = apparentWind.sqrMagnitude;
        
        Vector3 liftForce = Vector3.Cross(apparentWind, -KiteRigidbody.transform.right).normalized * apparentWindSpeedSquared / 2  * _liftCoefficient;
        
        // Calculate the drag force
        Vector3 dragForce = apparentWind.normalized * apparentWindSpeedSquared / 2 * _dragCoefficient;

        // Apply the forces to the kite
        KiteRigidbody.AddForce(liftForce + dragForce);

        currentResultantForce = liftForce + dragForce;
        float resultantForceMagnitude = currentResultantForce.magnitude;
        Debug.Log("Resultant Force Magnitude: " + resultantForceMagnitude);


        // Calculate the torque
        // Vector3 torque = Vector3.Cross(KiteRigidbody.velocity, (liftForce + dragForce));

        // Vector2 barPosition = joyController.barPosition;


        // Apply a torque to the kite relative to barPosition.x
        float torqueMultiplier = 2f * KiteRigidbody.angularVelocity.magnitude + 10f * KiteRigidbody.velocity.magnitude + 1.8f;
        KiteRigidbody.AddTorque(-BarPositionAsControlInput.x * torqueMultiplier * KiteRigidbody.transform.up);
        // cap the angular velocity
        KiteRigidbody.angularVelocity = Vector3.ClampMagnitude(KiteRigidbody.angularVelocity, 3f);

        // reduce the joint limit for the steering joints
        // float steeringLimitOffsetMultiplier = 0.5f;
        // float powerLimitOffsetMultiplier = 0.2f;
        // jointSteeringLeft.linearLimit = new SoftJointLimit { limit = lineLength + barPosition.x * steeringLimitOffsetMultiplier };
        // jointSteeringRight.linearLimit = new SoftJointLimit { limit = lineLength - barPosition.x * steeringLimitOffsetMultiplier };
        // jointPowerLeft.linearLimit = new SoftJointLimit { limit = lineLength - barPosition.y * powerLimitOffsetMultiplier + barPosition.x * steeringLimitOffsetMultiplier };
        // jointPowerRight.linearLimit = new SoftJointLimit { limit = lineLength - barPosition.y * powerLimitOffsetMultiplier - barPosition.x * steeringLimitOffsetMultiplier };

        // slow down horizontal velocity (local to kite)
        Vector3 localVelocity = KiteRigidbody.transform.InverseTransformDirection(KiteRigidbody.velocity);
        localVelocity.x *= 0.9f;
        KiteRigidbody.velocity = KiteRigidbody.transform.TransformDirection(localVelocity);

        // Increase drag force if the bar is pulled in
        if (joyController.upDownInput > 0.0f) {
            Debug.Log("Bar is pulled in");
            _dragCoefficient = barPower * joyController.upDownInput;
        } else {
            _dragCoefficient = dragCoefficient;
        }

        // check for collisions with the floor
        // if (kiteCollider.bounds.Intersects(theFloor.GetComponent<Collider>().bounds)) {
        //     Debug.Log("Kite is in the sea");
        //     if (!isResetting) {
        //         isResetting = true;
        //         startResetTimer();
        //     }

        //     // KiteRigidbody.AddForceAtPosition(-KiteRigidbody.velocity * 0.1f, KiteRigidbody.position);
        //     KiteRigidbody.velocity = Vector3.zero;
        //     KiteRigidbody.angularVelocity = Vector3.zero;
        // }

        if (kiteCollider.bounds.Intersects(theSea.GetComponent<MeshCollider>().bounds)) {
            Debug.Log("Kite is in the sea");
            if (!isResetting) {
                isResetting = true;
                startResetTimer();
            }

            // KiteRigidbody.AddForceAtPosition(-KiteRigidbody.velocity * 0.1f, KiteRigidbody.position);
            KiteRigidbody.velocity = Vector3.zero;
            KiteRigidbody.angularVelocity = Vector3.zero;
        }

        // Apply the torque to the kite
        // KiteRigidbody.AddTorque(torque);

        // // Calculate the distance between the kite and the base
        // float distance = Vector3.Distance(KiteRigidbody.position, BaseRigidbody.position);

        // // Calculate the spring force
        // float springForce = joint.linearLimit.limit - distance;

        // // Apply the spring force to the base
        // BaseRigidbody.AddForceAtPosition(springForce * Vector3.up, KiteRigidbody.position);
        // draw a debug line to show the spring force
        // Debug.DrawLine(KiteRigidbody.position, KiteRigidbody.position + KiteRigidbody.transform.up * 0.5f, Color.blue);
        Debug.DrawLine(KiteRigidbody.position, KiteRigidbody.position + liftForce * debugScale, Color.green);
        Debug.DrawLine(KiteRigidbody.position, KiteRigidbody.position + dragForce * debugScale, Color.red);
        
        // lines
        if (!joyController.isCut){
            Debug.DrawLine(KiteRigidbody.position, BaseRigidbody.position, Color.white);
        }

        if (counter < stepsPerSecond * 5) {
            score += 0.02f;
        }

        
        
        if (counter > stepsPerSecond * 10) {
            if (counter % 10 == 0) {
                if (varyPhysics)
                {
                    liftCoefficient = _liftCoefficient + Random.Range(-0.1f, 0.1f);
                    liftCoefficient = Mathf.Clamp(liftCoefficient, 0.1f, 0.5f);
                    dragCoefficient = _dragCoefficient + Random.Range(-0.1f, 0.1f);
                    dragCoefficient = Mathf.Clamp(dragCoefficient, 0.1f, 0.5f);
                }
                score += jointPowerLeft.currentForce.magnitude * 0.002f;
                score += jointPowerRight.currentForce.magnitude * 0.002f;
                score += jointSteeringLeft.currentForce.magnitude * 0.002f;
                score += jointSteeringRight.currentForce.magnitude * 0.002f;
            }
        }
        // if (counter > stepsPerSecond * 10) {
        //     if (counter % 10 == 0) {
                // add the forces together
                currentResultantForce = new Vector3(0, 0, 0);
                currentResultantForce += jointPowerLeft.currentForce;
                currentResultantForce += jointPowerRight.currentForce;
                currentResultantForce += jointSteeringLeft.currentForce;
                currentResultantForce += jointSteeringRight.currentForce;
        //     }
        // }

    }

    // void OnGUI() {
        // GUI.Label(new Rect(10, 10, 100, 20), "Score: " + score);
        // GUI.Label(new Rect(10, 30, 100, 20), "Lift coefficient: " + _liftCoefficient);
        // GUI.Label(new Rect(10, 50, 100, 20), "Drag coefficient: " + _dragCoefficient);
        // GUI.Label(new Rect(10, 70, 100, 20), "Apparent wind: " + apparentWind);
        // GUI.Label(new Rect(10, 90, 100, 20), "Apparent wind speed: " + apparentWind.magnitude);
        // GUI.Label(new Rect(10, 110, 100, 20), "Apparent wind speed squared: " + apparentWind.sqrMagnitude);
        // GUI.Label(new Rect(10, 130, 100, 20), "Lift force: " + liftForce);
        // GUI.Label(new Rect(10, 150, 100, 20), "Drag force: " + dragForce);
        // GUI.Label(new Rect(10, 170, 100, 20), "Torque: " + torque);
        // GUI.Label(new Rect(10, 190, 100, 20), "Kite velocity: " + KiteRigidbody.velocity);
        // GUI.Label(new Rect(10, 210, 100, 20), "Kite angular velocity: " + KiteRigidbody.angularVelocity);
        // GUI.Label(new Rect(10, 230, 100, 20), "Kite angular velocity magnitude: " + KiteRigidbody.angularVelocity.magnitude);
        // GUI.Label(new Rect(10, 250, 100, 20), "Kite angular velocity magnitude: " + KiteRigidbody.angularVelocity.magnitude);
        // GUI.Label(new Rect(10, 270, 100, 20), "Kite angular velocity magnitude: " + KiteRigidbody.angularVelocity.magnitude);
        // GUI.Label(new Rect(10, 290, 100, 20), "Kite angular velocity magnitude: " + KiteRigidbody.angularVelocity.magnitude);
        // GUI.Label(new Rect(10, 310, 100, 20), "Kite angular velocity magnitude: " + KiteRigidbody.angularVelocity.magnitude);
    // }
}
