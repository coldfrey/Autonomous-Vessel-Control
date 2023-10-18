using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitingBuoyColliderListener : MonoBehaviour
{
    public BoatPPOAgent agent;
    public int segmentIndex = -1;
    // Start is called before the first frame update
    private void Start()
    {
        if (segmentIndex < 0)
        {
            Debug.LogError("Segment index must be >= 0");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "floor_proxy") agent.ExitSegmentEntered(segmentIndex);
        print("ExitingBuoyColliderListener: OnTriggerEnter");
    }
}
