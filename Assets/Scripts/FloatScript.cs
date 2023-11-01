using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class FloatScript : MonoBehaviour
{
    // Start is called before the first frame update
    public WaterSurface water;
    WaterSearchParameters searchParameters = new WaterSearchParameters();
    WaterSearchResult searchResult = new WaterSearchResult();
    public Rigidbody rigidbody;
    public float depthBeforeSubmerged = 1f;
    public float displacementAmount = 0.2f;
    // public float waweHeight = 0f;
    public float waterHeight = 0f;
    public float smoothingFactor = 0.1f;  // Adjust this value to get the desired smoothing (0 for no smoothing, 1 for instant change)
    public float smoothedWaterHeight = 0f;


    private void Start()
    {
        // get water surface
        water = GameObject.Find("Ocean").GetComponent<WaterSurface>();
        rigidbody.centerOfMass = new Vector3(0, 0, -1);
        // only set the center of mass if the rigidbody is the buoy
        // if (gameObject.name == "buoy(clone) (rigidbody)")
        // {
        //     rigidbody.centerOfMass = new Vector3(0, 0, -3);
        // }
    }
    void FixedUpdate()
    {
        if (water == null) return;
        if (water.ProjectPointOnWaterSurface(searchParameters, out searchResult))
        {
            float targetWaterHeight = searchResult.projectedPositionWS.y;
            smoothedWaterHeight = Mathf.Lerp(smoothedWaterHeight, targetWaterHeight, smoothingFactor);
        }

        if(transform.position.y < smoothedWaterHeight)
        {
            float displacementMultiplier = Mathf.Clamp01((smoothedWaterHeight - transform.position.y) / depthBeforeSubmerged ) * displacementAmount;
            rigidbody.AddForceAtPosition(new Vector3(0f, Mathf.Abs(Physics.gravity.y) * displacementMultiplier, 0f), transform.position, ForceMode.Acceleration);
        }
    }
}
