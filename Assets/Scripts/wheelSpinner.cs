using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wheelSpinner : MonoBehaviour
{
  public Transform wheelTransform;
  public bool reversed = false;
  
  private WheelCollider wheelCollider;

  private float travel = 0.1f;
  private float offsetDistance = 1.9f;
  private float maxDistance;
  private float distance;
  private float targetSuspenionDistance;

  void Start()
  {
    wheelCollider = GetComponent<WheelCollider>();
    maxDistance = wheelCollider.suspensionDistance;
    if (reversed) {
      offsetDistance -= 0.53f;;
    }
    targetSuspenionDistance = 0.0f;
    // add tiny torque to avoid stuck
    wheelCollider.motorTorque = 0.1f;
  }

  void Update()
  {
    if (reversed) {
      wheelTransform.localPosition = Vector3.Lerp(wheelTransform.localPosition, targetSuspenionDistance * Vector3.up, 0.1f);
    } else {
      wheelTransform.localPosition = Vector3.Lerp(wheelTransform.localPosition, targetSuspenionDistance * Vector3.right, 0.1f);
    }
  }

  void FixedUpdate()
  {

    // rotate the wheel
    if (reversed) {
      wheelTransform.Rotate(-wheelCollider.rpm / 60 * 360 * Time.deltaTime, 0, 0);
    } else {
      wheelTransform.Rotate(wheelCollider.rpm / 60 * 360 * Time.fixedDeltaTime, 0, 0);
    }

    // move wheel based on suspension
    WheelHit hit;
    if(wheelCollider.GetGroundHit(out hit)){
      var point = hit.point + (transform.up * wheelCollider.radius);
      distance = Vector3.Distance(transform.position, point);
    }
    targetSuspenionDistance = ((travel * (1 - (distance / maxDistance))) + offsetDistance * travel);
  }

  void OnGUI() {
    GUI.Label(new Rect(10, 80, 100, 20), "RPM: " + wheelCollider.rpm.ToString("0"));
    GUI.Label(new Rect(10, 100, 100, 20), "suspension: " + wheelCollider.suspensionDistance.ToString("0.00"));
  }
}
