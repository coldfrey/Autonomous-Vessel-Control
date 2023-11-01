using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApproachingBuoyColliderListener : MonoBehaviour
{
    public BoatPPOAgent agent;
    public int segmentIndex = -1;

    private void Start()
    {
        // add a delay to allow the agent to be initialized
        StartCoroutine(DelayedStart());
        if (segmentIndex < 0)
        {
            Debug.LogError("Segment index must be >= 0");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == null) {
            print("some kak foiund");
            return;
        }
        if (other.gameObject.name == null) {
            print("some kak foiund no name");
            return;
        }
        if (agent == null) {
            print("some kak foiund no agent");
            return;}
        if (other.gameObject.name == "floor_proxy" || other.gameObject.name == "PathAgent") agent.ApproachSegmentEntered(segmentIndex);
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(1.0f);
        // agent = FindObjectOfType<BoatPPOAgent>();
    }
}
