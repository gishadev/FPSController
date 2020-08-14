using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Controller")]
    private CharacterController controller;
    public const float gravity = -9.81f;

    public float walkSpeed = 7.5f;
    public float sprintSpeed = 11f;
    public float jumpForce = 10f;
    public float gravityMultiplier = 4.5f;
    [Space]
    [Range(0f, 1f)] public float airMovementMultiplier = 0.5f;
    [Range(0f, 1f)] public float crouchMovementMultiplier = 0.5f;
    float nowJumpMultiplier = 1f;
    //private float nowMovementMultiplier = 1f;
    [Space]
    public float crouchSmothness;
    public float crouchHeight = 0.5f;
    // float defaultHeight;

    [Header("Checkers")]
    public Transform groundChecker;
    public float groundCheckerRadius = 0.45f;
    public LayerMask groundMask;

    float nowMoveSpeed;
    Vector3 velocity;
    Vector3 moveInput;

    bool isGrounded;
    bool isSprinting;
    bool isCrouching;

    float hInput;
    float vInput;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Input //
        hInput = Input.GetAxis("Horizontal");
        vInput = Input.GetAxis("Vertical");

        isGrounded = CheckGroundCollider();
        isSprinting = Input.GetKey(KeyCode.LeftShift) && !isCrouching;

        // Jumping //
        if (Input.GetButtonDown("Jump") && isGrounded)
            Jump();

        // Crouching //
        if (Input.GetKeyDown(KeyCode.LeftControl))
            SetCrouch(true);
        if (Input.GetKeyUp(KeyCode.LeftControl))
            SetCrouch(false);
    }

    void FixedUpdate()
    {
        Movement();
    }

    void Movement()
    {
        // When player is on ground.
        if (isGrounded && velocity.y < 0f)
        {
            velocity = Vector3.up * -2f;
            controller.slopeLimit = 50f;
        }

        // Variable to control moveSpeed;
        nowMoveSpeed = !isSprinting ? walkSpeed : sprintSpeed;

        moveInput = transform.forward * vInput + transform.right * hInput;

        controller.Move(Vector3.ClampMagnitude(moveInput, 1f) * nowMoveSpeed * GetMovementMultiplier() * Time.deltaTime);

        // Applying gravity to player controller.
        velocity.y += gravity * gravityMultiplier * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    #region Actions
    void SetCrouch(bool state)
    {
        isCrouching = state;

        if (isCrouching)
        {
            StartCoroutine(CrouchCoroutine());
            nowJumpMultiplier = 0.5f;
        }

        else
        {
            StartCoroutine(UnCrouchCoroutine());
            nowJumpMultiplier = 1f;
        }

    }

    #region Crouch Coroutines
    IEnumerator CrouchCoroutine()
    {
        float vel = 0;

        while (isCrouching && !Input.GetKeyUp(KeyCode.LeftControl))
        {
            float scaleY = Mathf.SmoothDamp(transform.localScale.y, crouchHeight, ref vel, crouchSmothness);
            float value = transform.localScale.y - scaleY;
            Vector3 pos = -Vector3.up * value;

            transform.localScale = new Vector3(1f, scaleY, 1f);
            transform.position += pos;

            yield return null;
        }
    }

    IEnumerator UnCrouchCoroutine()
    {
        float vel = 0;

        while (!isCrouching && !Input.GetKeyDown(KeyCode.LeftControl))
        {
            float scaleY = Mathf.SmoothDamp(transform.localScale.y, 1f, ref vel, crouchSmothness);
            float value = transform.localScale.y - scaleY;
            Vector3 pos = -Vector3.up * value;

            transform.localScale = new Vector3(1f, scaleY, 1f);
            transform.position += pos;

            yield return null;
        }
    }
    #endregion

    void Jump()
    {
        velocity = CalculateJumpVelocity(moveInput * nowMoveSpeed);
        controller.slopeLimit = 90f;
    }
    #endregion

    #region Collider Checkers
    bool CheckGroundCollider()
    {
        return Physics.CheckSphere(groundChecker.position, groundCheckerRadius, groundMask);
    }
    #endregion

    Vector3 CalculateJumpVelocity(Vector3 moveVel)
    {
        Vector3 vel;

        vel.x = moveVel.x * GetMovementMultiplier();
        vel.y = Mathf.Sqrt((jumpForce * nowJumpMultiplier) * -2f * gravity);
        vel.z = moveVel.z * GetMovementMultiplier();

        return vel;
    }

    float GetMovementMultiplier()
    {
        float n1 = 1f, n2 = 1f;

        if (!isGrounded)
            n1 = airMovementMultiplier;
        if (isCrouching)
            n2 = crouchMovementMultiplier;

        return n1 * n2;
    }
}
