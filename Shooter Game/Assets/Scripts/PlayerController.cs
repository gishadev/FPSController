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
    [Space]
    [Range(0f, 1f)] public float airMovementMultiplier = 0.5f;
    float movementMultiplier = 1f;
    [Header("Ground Checker")]
    public Transform groundChecker;
    public float checkerRadius;
    public LayerMask groundMask;

    Vector3 velocity;
    Vector3 moveInput;
    float nowMoveSpeed;

    bool isSprinting = false;

    float hInput;
    float vInput;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        hInput = Input.GetAxis("Horizontal");
        vInput = Input.GetAxis("Vertical");

        isSprinting = Input.GetKey(KeyCode.LeftShift);

        // Jumping.
        if (Input.GetButtonDown("Jump") && IsGrounded())
            Jump();
    }

    void FixedUpdate()
    {
        Movement();
    }

    void Movement()
    {
        // When player on ground.
        if (IsGrounded() && velocity.y < 0f)
        {
            velocity = new Vector3(0f, -2f, 0f);
            controller.slopeLimit = 50f;
        }

        // Variable to control moveSpeed;
        movementMultiplier = IsGrounded() ? 1f : airMovementMultiplier;
        nowMoveSpeed = !isSprinting ? walkSpeed : sprintSpeed;

        moveInput = transform.forward * vInput + transform.right * hInput;

        controller.Move(Vector3.ClampMagnitude(moveInput, 1f) * nowMoveSpeed * movementMultiplier * Time.deltaTime);

        // Applying gravity to player controller.
        velocity.y += gravity * gravityMultiplier * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void Jump()
    {
        velocity = CalculateAirVelocity(moveInput * nowMoveSpeed);
        controller.slopeLimit = 90f;
    }

    bool IsGrounded()
    {
        return Physics.CheckSphere(groundChecker.position, checkerRadius, groundMask);
    }

    Vector3 CalculateAirVelocity(Vector3 moveVel)
    {
        Vector3 vel;

        vel.x = moveVel.x;
        vel.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        vel.z = moveVel.z;

        return vel;
    }
}
