using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using UnityEngine.AI;

public class BoatController : MonoBehaviour
{
  public PropellerBoats ship;
  public Rudder rudder;

  public JointSim jointSim;
  bool forward = true;


  public void OnMove(float value1) 
  {
    if (value1 >= 0)
      ship.ThrottleUp();
    else if (value1 < 0)
    {
      ship.ThrottleDown();
      ship.Brake();
    }
    else
      ship.ThrottleDown();
    // ship.Rudder(turn);
  }
  void Update()
  {
    if (Input.GetKey(KeyCode.Z)) rudder.RudderLeft();
    if (Input.GetKey(KeyCode.C)) rudder.RudderRight();
    if (Input.GetKey(KeyCode.B)) {
      jointSim.SteerLeft();
    } else if (Input.GetKey(KeyCode.M)) {
      jointSim.SteerRight();
    } else {
      jointSim.StopSteering();
    }
    // if (Input.GetKeyDown(KeyCode.Space)) jointSim.PowerOn();
    if (Input.GetKeyDown(KeyCode.R)) jointSim.ResetKite();

    // if (Input.GetKey(KeyCode.Q))
    //   ship.RudderLeft();
    // if (Input.GetKey(KeyCode.D))
    //   ship.RudderRight();

    // if (forward)
    // {
    //   if (Input.GetKey(KeyCode.Z))
    //     ship.ThrottleUp();
    //   else if (Input.GetKey(KeyCode.S))
    //   {
    //     ship.ThrottleDown();
    //     ship.Brake();
    //   }
    // }
    // else
    // {
    //   if (Input.GetKey(KeyCode.S))
    //     ship.ThrottleUp();
    //   else if (Input.GetKey(KeyCode.Z))
    //   {
    //     ship.ThrottleDown();
    //     ship.Brake();
    //   }
    // }

    // if (!Input.GetKey(KeyCode.Z) && !Input.GetKey(KeyCode.S))
    //   ship.ThrottleDown();

    // if (ship.engine_rpm == 0 && Input.GetKeyDown(KeyCode.S) && forward)
    // {
    //   forward = false;
    //   ship.Reverse();
    // }
    // else if (ship.engine_rpm == 0 && Input.GetKeyDown(KeyCode.Z) && !forward)
    // {
    //   forward = true;
    //   ship.Reverse();
    // }
  }

}
