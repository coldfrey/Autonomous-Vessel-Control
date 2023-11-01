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
        StartCoroutine(DelayedStart());
        if (segmentIndex < 0)
        {
            Debug.LogError("Segment index must be >= 0");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (agent == null) return;
        if (other.gameObject.name == "floor_proxy" || other.gameObject.name == "PathAgent") agent.ExitSegmentEntered(segmentIndex);
    }
    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(1.0f);
        // agent = FindObjectOfType<BoatPPOAgent>();
    }
}
