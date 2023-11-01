using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimplePlayer : MonoBehaviour
{
    
    private Vector2 moveInput;
    private Vector2 lookInput;

    private float upDownInput;

    private float HorizontalSpeed = 0.2f;
    private float VerticalSpeed = 8f;
    
    public Rigidbody rb;


    void Start()
    {
        // Get Rigidbody component



    }

    private void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        if (moveInput.magnitude < 0.1)
        {
            moveInput = Vector2.zero;
        }
        moveInput = moveInput * HorizontalSpeed;
    }

    private void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
        if (lookInput.magnitude < 0.1)
        {
            lookInput = Vector2.zero;
        }
    }

    private void OnUpDown(InputValue value)
    {
        // get 1d axis
        upDownInput = value.Get<float>();
        // Debug.Log("UpDown: " + upDown);
        
    }

    void Update()
    {
        // transform.position += new Vector3(moveInput.x, 0, moveInput.y) * Time.deltaTime;

        float angle = Mathf.Atan2(lookInput.x, lookInput.y);
        float heading = transform.eulerAngles.y + angle;
        // transform.position += Quaternion.Euler(0, heading, 0) * new Vector3(moveInput.x, 0, moveInput.y);
        float movementForce = 40f;
        // if velocity is greater than 10, up the force to 100
        if (rb.velocity.magnitude > 10)
        {
            movementForce = 100f;
        }
        // if the velocity is slower than 2, slowly decrease the velocity to 0
        if (rb.velocity.magnitude < 5)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, 0.01f);
        }

        rb.AddForce(Quaternion.Euler(0, heading, 0) * new Vector3(moveInput.x, 0, moveInput.y) * movementForce);

        // add vertical looking
        transform.rotation *= Quaternion.Euler(-lookInput.y, 0, 0);
        // store vertical looking
        float vertical = transform.rotation.eulerAngles.x;
        transform.rotation = Quaternion.Euler(vertical, heading, 0);

        bool upDown = false;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftShiftKey.isPressed)
            {
                // transform.position += Vector3.up * VerticalSpeed * Time.deltaTime;
                rb.AddForce(Vector3.up  * 10f);
                upDown = true;
            }

            if (Keyboard.current.leftCtrlKey.isPressed && !upDown)
            {
                // transform.position += Vector3.down * VerticalSpeed * Time.deltaTime;
                rb.AddForce(Vector3.down * 10f);
                upDown = true;
            }
        }
        // if (Gamepad.current != null)
        // {
        //     if (Gamepad.current.rightTrigger.isPressed && !upDown)
        //     {
        //         // transform.position += Vector3.up * VerticalSpeed * Time.deltaTime;
        //         rb.AddForce(Vector3.up * 10f);
        //         upDown = true;
        //     }
        //     if (Gamepad.current.leftTrigger.isPressed && !upDown)
        //     {
        //         // transform.position += Vector3.down * VerticalSpeed * Time.deltaTime;
        //         rb.AddForce(Vector3.down * 10f);
        //         upDown = true;
        //     }
        // }
        if (upDownInput > 0.1f && !upDown)
        {
            // transform.position += Vector3.up * VerticalSpeed * Time.deltaTime;
            rb.AddForce(Vector3.up * 10f);
            upDown = true;
        }
        if (upDownInput < -0.1f && !upDown)
        {
            // transform.position += Vector3.down * VerticalSpeed * Time.deltaTime;
            rb.AddForce(Vector3.down * 10f);
            upDown = true;
        }
        

        // transform.LookAt(transform.position + new Vector3(lookInput.x, 0, lookInput.y));
    }
}
