using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FollowAllBoats : MonoBehaviour
{
    // Quick and dirty script to follow all boats in the scene

    public GameObject[] boats;
    public GameObject boatToFollow;

    public Vector3 offset = new Vector3(0, 10, 30);
    void Start()
    {
        if (boats.Length == 0)
        {
            boats = GameObject.FindGameObjectsWithTag("Boat");
        }
        boatToFollow = boats[0];
    }

    // Update is called once per frame
    void Update()
    {
        // transform.position = boatToFollow.transform.position + offset;

        // lerp towards the boat
        transform.position = Vector3.Lerp(transform.position, boatToFollow.transform.position + offset, 0.1f);
        
    }
}
