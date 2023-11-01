using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.

public class KiteFollowCam : MonoBehaviour
{
  public Transform object1;
  public Transform object2;
  public Transform object3;
  public Transform object4;
  public float followSpeed = 0.03f;
  public float followHeightOffset = 15f;
  public float followHorizontalDistanceOffset = 10f;
  public float followUpwindDistanceOffset = 10f;
  public float lookHeightOffset = 5f;
  
  private Transform[] objects;

  void Start()
  {
    Transform[] objectsBad = new Transform[] { object1, object2, object3, object4 };
    int objectCount = 0;
    int[] objectIndexes = new int[4];
    // all zeroed out
    for (int i = 0; i < objectIndexes.Length; i++)
    {
      objectIndexes[i] = 0;
    }
    if (object1 != null) {
      objectCount++;
      objectIndexes[0] = 1;
    }
    if (object2 != null) {
      objectCount++;
      objectIndexes[1] = 1;
    }
    if (object3 != null) {
      objectCount++;
      objectIndexes[2] = 1;
    }
    if (object4 != null) {
      objectCount++;
      objectIndexes[3] = 1;
    }
    objects = new Transform[objectCount];
    int objectIndex = 0;
    for (int i = 0; i < objectIndexes.Length; i++)
    {
      if (objectIndexes[i] == 1)
      {
        objects[objectIndex] = objectsBad[i];
        objectIndex++;
      }
    }
  }

  // Update is called once per frame
  void Update()
  {
    //calculate average position of all objects
    Vector3 averagePosition = Vector3.zero;
    foreach (Transform t in objects) {
      averagePosition += t.position;
    }
    averagePosition /= objects.Length;
    // set camera to look at average position
    Vector3 lookAt = averagePosition - transform.position + Vector3.up * lookHeightOffset;
    // work out the furthest distance distance between the objects
    float maxDistance = 0;
    float maxHight = 0;
    foreach (Transform t in objects) {
      float distance = Vector3.Distance(t.position, averagePosition);
      float hight = t.position.y;
      if (distance > maxDistance) {
        maxDistance = distance;
      }
      if (hight > maxHight) {
        maxHight = hight;
      }
    }
    maxDistance *= 1.1f;
    //derive a target position from the average position and the max distance
    Vector3 targetPosition = averagePosition + new Vector3(0, followHeightOffset,0) - (maxDistance + followHorizontalDistanceOffset) * Vector3.right - (followUpwindDistanceOffset) * Vector3.forward;
    Vector3 targetPositionConstrained = new Vector3(targetPosition.x, Mathf.Max(3, targetPosition.y), targetPosition.z);
    transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed);
    // lerp transform rotation to look at kite
    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(lookAt), followSpeed);
  }
}
