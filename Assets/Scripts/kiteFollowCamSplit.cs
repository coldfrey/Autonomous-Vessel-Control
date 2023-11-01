using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class kiteFollowCamSplit : MonoBehaviour
{
  public Transform Car;
  public Transform Kite;
  public float followSpeed = 0.01f;
  public float lookSpeed = 0.05f;
  public float followHorizontalDistanceOffset = 50f;
  public float followVerticleDistanceOffset = 15f;
  public float lookHeightOffset = 5f;
  public float tackingOffset = 15f;
  public int tackingStepLimit = 150;
  public int downwindStepLimit = 50;
  
  private Transform[] objects;

  // state
  private Vector3 averagePosition;
  private Vector3 prevPosition;
  private Vector3 prevCarPosition;
  private Vector3 lookAt;
  private Vector3 moveDirection;
  private Vector3 targetPosition;

  private bool starboard = false;
  private bool downwind = false;
  private int tackCount = 0;
  private int downwindCount = 0;

  void Start()
  {
    objects = new Transform[] {Car, Kite};
    
    prevPosition = transform.position;
    prevCarPosition = Car.position;
  }

  // Update is called once per frame
  void Update()
  {
    //calculate average position of all objects
    averagePosition = Vector3.zero;
    foreach (Transform t in objects) {
      averagePosition += t.position;
    }
    averagePosition /= objects.Length;
    // set camera to look at average position
    lookAt = averagePosition - transform.position + Vector3.up * lookHeightOffset;
    Debug.DrawLine(averagePosition, averagePosition + lookAt.normalized * 3f, Color.blue);
    moveDirection = (transform.position - prevPosition).normalized;

    //derive a target position from the average position and the max distance
    targetPosition = averagePosition + Vector3.up * followVerticleDistanceOffset - Vector3.forward * followHorizontalDistanceOffset;
    
    if (starboard && !downwind) {
      targetPosition += Vector3.left * tackingOffset;
    } else if (!downwind) {
      targetPosition += Vector3.right * tackingOffset;
    }
    // move towards the target position
    transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed);
    // lerp transform rotation to look at kite
    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lookAt), lookSpeed);

    prevPosition = transform.position;
  }

  void FixedUpdate() 
  {
    // work out tack
    // checks if on starboard tack
    if (Car.position.x < Kite.position.x) {
      if ((!starboard))
      {
        if (tackCount < tackingStepLimit)
        {;
          tackCount++;
        } else
        {
          Debug.Log("Tacking to starboard");
          starboard = true;
          tackCount = 0;
        }
      } else
      {
        tackCount = 0;
      }
    }     
    else
    {
      if (starboard)
      {
        if (tackCount < tackingStepLimit)
        {
          tackCount++;
        } else
        {
          Debug.Log("Tacking to port");
          starboard = false;
          tackCount = 0;
        }
      } else
      {
        tackCount = 0;
      }
    }

    // checks if traveling downwind
    if (prevCarPosition.z < Car.position.z) {
      if (!downwind) {
        if (downwindCount < downwindStepLimit) {
          downwindCount++;
        } else {
          Debug.Log("Downwind");
          downwind = true;
          downwindCount = 0;
        }
      } else {
        downwindCount = 0;
      }
    } else {
      if (downwind) {
        if (downwindCount < downwindStepLimit) {
          downwindCount++;
        } else {
          Debug.Log("Upwind");
          downwind = false;
          downwindCount = 0;
        }
      } else {
        downwindCount = 0;
      }
    }
    prevCarPosition = Car.position;
  }
}
