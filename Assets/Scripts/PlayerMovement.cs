using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 6f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private PlayerTools playerTools;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
        controller = GetComponent<CharacterController>();
        playerTools = GetComponent<PlayerTools>();
        
        // Add PlayerTools component if it doesn't exist
        if (playerTools == null)
        {
            playerTools = gameObject.AddComponent<PlayerTools>();
            Debug.Log("PlayerTools component added to player.");
        }
    }

    void Update() {
        //ground check
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0) {
            velocity.y = -2f;
        }

        //movement input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(speed * Time.deltaTime * move);

        //jumping
        if (Input.GetButtonDown("Jump") && isGrounded) {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        //apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if(transform.position.y < -10){
            transform.position = startPos;
        }
    }
}