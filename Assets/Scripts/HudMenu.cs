using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HudMenu : MonoBehaviour
{
    public GameObject boat;
    public GameObject wind;
    public Text status;
    public RectTransform twaIndicator;
    public RectTransform awaIndicator;
    public RectTransform dirIndicator;
    public RectTransform speedIndicator;

    protected Vector3 awaVector;
    protected Vector3 headSailVector;
    protected Vector3 mainSailVector;
    
    // private float trueWindVector;
    // private float apparentWindVector;
    

    // Update is called once per frame
    void Update()
    {
        if (boat == null || wind == null)
            return;
        Rigidbody bootRb = boat.GetComponent<Rigidbody>();
        status.text = ""+System.Math.Round(bootRb.velocity.magnitude, 2);        
        
        Vector3 windDirection = new Vector3();
        windDirection.z = boat.transform.eulerAngles.y - wind.transform.eulerAngles.y;
        twaIndicator.localEulerAngles = windDirection;

        // Vector3 apparentWindVector = trueWindVector + bootRb.velocity;

        // float trueWindAngle = Vector3.SignedAngle(boat.transform.forward, wind.transform.forward, Vector3.up);
        // float apparentWindAngle = Vector3.SignedAngle(boat.transform.forward, apparentWindVector, Vector3.up);
        // twaIndicator.localEulerAngles = new Vector3(0, 0, trueWindAngle);
        // awaIndicator.localEulerAngles = new Vector3(0, 0, apparentWindAngle);

        int apparentWindAngleGrad = (int)Vector3.Angle(boat.transform.forward, -awaVector);
        int directionAngle = (int)Vector3.Angle(boat.transform.forward, bootRb.velocity);
        

        if(windDirection.z < 180){
            apparentWindAngleGrad = 180 - apparentWindAngleGrad;
            
        } else {
            apparentWindAngleGrad = 180 + apparentWindAngleGrad;
            directionAngle = 360 - directionAngle;
        }
        dirIndicator.eulerAngles = new Vector3(0, 0, directionAngle);
        awaIndicator.eulerAngles = new Vector3(0, 0, apparentWindAngleGrad);
        speedIndicator.eulerAngles = new Vector3(0, 0, 120 - bootRb.velocity.magnitude*24);
    }

    public void onAwaAngleChange(Vector3 vector) {
        awaVector = vector;
    }

    public void onHeadSailAngleChange(Vector3 vector) {
        headSailVector = vector;
    }

    public void onMainSailAngleChange(Vector3 vector) {
        mainSailVector = vector;
    }
}
