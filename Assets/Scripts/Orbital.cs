using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 5, -10); // Offset from the target
    public float followSpeed = 10f;
    public float flySpeed = 10f;

    private Vector3 initialOffset;
    private bool isFollowing = true;

    void Start()
    {
        // Store the initial offset
        initialOffset = offset;
    }

    void Update()
  {
      if (Input.GetKeyDown(KeyCode.P))
      {
          // Reset the offset and enable following
          offset = initialOffset;
          isFollowing = true;
      }
      else if (Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.L) || Input.GetKey(KeyCode.I) || 
              Input.GetKey(KeyCode.K) || Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
      {
          // If any control key is pressed, disable following
          isFollowing = false;
      }

      if (isFollowing)
      {
          FollowTarget();
      }
      else
      {
          FlyAround();
      }
  }


    void FollowTarget()
    {
        // Calculate the position to move towards
        Vector3 targetPosition = target.position + target.TransformDirection(offset);

        // Smoothly move towards the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        transform.LookAt(target);
    }

    void FlyAround()
    {
        // Flying movement
        float panHorizontal = Input.GetAxis("Horizontal");
        float panVertical = Input.GetAxis("Vertical");
        float moveHorizontal = 0f;
        float moveVertical = 0f;

        if (Input.GetKey(KeyCode.J)) moveHorizontal = -1f;
        if (Input.GetKey(KeyCode.L)) moveHorizontal = 1f;
        if (Input.GetKey(KeyCode.I)) moveVertical = 1f;
        if (Input.GetKey(KeyCode.K)) moveVertical = -1f;

        Vector3 panMovement = new Vector3(panHorizontal, panVertical, 0) * flySpeed * Time.deltaTime;
        Vector3 moveMovement = new Vector3(moveHorizontal, 0, moveVertical) * flySpeed * Time.deltaTime;

        // Apply movement
        transform.Translate(panMovement, Space.World);
        transform.Translate(moveMovement, Space.Self);

        // Switch to fly mode if there is input from the user
        if (panHorizontal != 0 || panVertical != 0 || moveHorizontal != 0 || moveVertical != 0)
        {
            isFollowing = false;
        }
    }

}
