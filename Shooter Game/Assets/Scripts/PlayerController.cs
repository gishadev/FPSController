using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Controller")]
    private CharacterController controller;
    public const float gravity = -9.81f;

    public float walkSpeed = 7f;
    public float sprintSpeed = 7f;
    [Space]
    public float jumpForce = 7f;
    public float gravityMultiplier = 2.5f;

    [Header("Ground Checker")]
    public Transform groundChecker;
    public float checkerRadius;
    public LayerMask groundMask;

    Vector3 velocity;

    bool isGrounded;
    bool isSprinting;

    float horizontalAxis;
    float verticalAxis;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        MovementInput();
    }
    void FixedUpdate()
    {
        Movement();
    }

    void MovementInput()
    {
        horizontalAxis = Input.GetAxis("Horizontal");
        verticalAxis = Input.GetAxis("Vertical");

        if (Input.GetButtonDown("Jump") && isGrounded)
            Jump();

        isSprinting = Input.GetKey(KeyCode.LeftShift);
    }

    void Movement()
    {
        isGrounded = Physics.CheckSphere(groundChecker.position, checkerRadius, groundMask);

        // Grounded Physics.
        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
            controller.slopeLimit = 45.0f;
        }

        float moveSpeed = !isSprinting ? walkSpeed : sprintSpeed;
        Vector3 movement = transform.right * horizontalAxis + transform.forward * verticalAxis;
        controller.Move(movement * moveSpeed * Time.deltaTime);

        velocity.y += gravity * gravityMultiplier * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        controller.slopeLimit = 90f;
    }
}
