using UnityEngine;

public class Rudder : MonoBehaviour
{
    public float angle = 0;
    public float turnSensitivity = 1.0F;
    public float rudderLerpSpeed = 5.0F;
    public Transform[] rudder;

    public float keelEfficiency = 0.5F;
    private Rigidbody boatRb;
    private float targetAngle = 0;
    private bool inputReceived = false; // Flag to check if any rudder input was received

    [SerializeField] private float boatForwardSpeed = 0;

    public float rotationScale = 10.0F;
    private void Awake()
    {
        boatRb = GetComponentInParent<Rigidbody>();
    }

    void Update()
    {
        // Smoothly interpolate between the current angle and the target angle
        angle = Mathf.Lerp(angle, targetAngle, rudderLerpSpeed * Time.deltaTime);

        // Calculate the forward speed of the boat
        boatForwardSpeed = Vector3.Dot(boatRb.velocity, transform.forward);

        // Adjust the rudder angle based on boat's forward speed
        // float adjustedAngle = angle * turnSensitivity * boatForwardSpeed;
        float adjustedAngle = angle * turnSensitivity;

        for (int i = 0; i < rudder.Length; i++)
            rudder[i].localRotation = Quaternion.Euler(0, adjustedAngle, 0);

        // Apply turning force based on rudder's angle and boat's speed
        float turnForce = angle * boatForwardSpeed * rotationScale;
        boatRb.AddTorque(transform.up * turnForce);

        // // Generate keel lift force
        // float liftForceMagnitude = Mathf.Abs(boatForwardSpeed) * keelEfficiency;
        // Vector3 liftDirection = Vector3.Cross(transform.forward, boatRb.velocity).normalized;
        // Vector3 liftForce = liftDirection * liftForceMagnitude;
        // boatRb.AddForce(liftForce);

        if (boatForwardSpeed > 0.25 || boatForwardSpeed < -1)
        {
            Vector3 lateralVelocity = Vector3.Dot(boatRb.velocity, transform.right) * transform.right;
            boatRb.velocity -= lateralVelocity;
        }
        


        // If no input is received, set target angle back to center
        if (!inputReceived)
        {
            targetAngle = 0;
        }

        // Reset input flag for next frame
        inputReceived = false;
    }

    public void RudderLeft()
    {
        targetAngle -= 1F;
        targetAngle = Mathf.Clamp(targetAngle, -90F, 90F);
        inputReceived = true;
    }

    public void RudderRight()
    {
        targetAngle += 1F;
        targetAngle = Mathf.Clamp(targetAngle, -90F, 90F);
        inputReceived = true;
    }

    public float GetBoatForwardSpeed()
    {
        return boatForwardSpeed;
    }

    public void SetRudderTargetAngle(float targetAngle)
    {
        this.targetAngle = targetAngle;
    }
}
