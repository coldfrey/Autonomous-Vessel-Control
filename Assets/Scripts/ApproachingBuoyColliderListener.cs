using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApproachingBuoyColliderListener : MonoBehaviour
{
    public BoatPPOAgent agent;
    public int segmentIndex = -1;

    private void Start()
    {
        if (segmentIndex < 0)
        {
            Debug.LogError("Segment index must be >= 0");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "floor_proxy") agent.ApproachSegmentEntered(segmentIndex);
    }
}
