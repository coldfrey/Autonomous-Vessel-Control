using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WindSim : MonoBehaviour
{
    public Vector3 wind = new Vector3(0, 0, -40);

    public bool varyWind = false;

    void Start()
    {
        varyWind = PlayerPrefs.GetString("varyWind") == "true";
    }

    void Update()
    {
        if (varyWind)
        {
            wind = new Vector3(0, 0, -40 + Mathf.Sin(Time.time) * 10);
        }
    }
}