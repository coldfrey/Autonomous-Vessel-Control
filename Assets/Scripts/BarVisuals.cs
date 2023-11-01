using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarVisuals : MonoBehaviour
{
  
  public GameObject leftFloatGO;
  public GameObject rightFloatGO;

  public float offsetAngle = 0.0f;


  // Update is called once per frame
  void Update()
  {
    float currentRotation = transform.localRotation.eulerAngles.z;
    leftFloatGO.transform.localRotation = Quaternion.Euler(0, 0, -currentRotation + offsetAngle);
    rightFloatGO.transform.localRotation = Quaternion.Euler(0, 0, -currentRotation - offsetAngle);
    
  }
}
