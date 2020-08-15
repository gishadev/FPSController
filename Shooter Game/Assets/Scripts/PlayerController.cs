using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Controller")]
    public FPSCameraController fpsCamera;
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
    public float slopeForce;
    public float slopeForceRayLength;

    // Private Vars //
    float nowMoveSpeed;
    Vector3 velocity;
    Vector3 moveInput;

    bool isGrounded;
    bool isJumping;
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

        PlayerStateAnimation();

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
        float slopeMultiplier;
        // When player is on ground //
        if (isGrounded && velocity.y < 0f)
        {
            if (isJumping)
                isJumping = false;

            velocity = Vector3.up * -2f;
            controller.slopeLimit = 50f;
        }

        // Variable to control moveSpeed //
        nowMoveSpeed = !isSprinting ? walkSpeed : sprintSpeed;

        moveInput = transform.forward * vInput + transform.right * hInput;

        controller.Move(Vector3.ClampMagnitude(moveInput, 1f) * nowMoveSpeed * GetMovementMultiplier() * Time.deltaTime);

        // Applying gravity to player controller //

        // If On Slope => Additional scale for gravity to reduce "bouncing" bug.
        if (IsOnSlope())
            slopeMultiplier = slopeForce;
        else
            slopeMultiplier = 1f;

        velocity.y += gravity * gravityMultiplier * slopeMultiplier * Time.deltaTime;
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

            fpsCamera.TriggerShake("Crouch");
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
        isJumping = true;
        velocity = CalculateJumpVelocity(moveInput * nowMoveSpeed);
        controller.slopeLimit = 90f;

        fpsCamera.TriggerShake("Jump");
    }
    #endregion

    #region Checkers
    bool CheckGroundCollider()
    {
        return Physics.CheckSphere(groundChecker.position, groundCheckerRadius, groundMask);
    }

    bool IsOnSlope()
    {
        if (!isGrounded || isJumping)
            return false;

        RaycastHit hitInfo;

        if (Physics.Raycast(groundChecker.position, Vector3.down, out hitInfo, slopeForceRayLength))
            if (hitInfo.normal != Vector3.up)
                return true;
        return false;
    }
    #endregion

    #region Calculations
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
    #endregion

    #region Animator
    void PlayerStateAnimation()
    {
        int state;

        if (isJumping || !isGrounded)
        {
            fpsCamera.SetCameraShakeState((int)State.Idle);
            return;
        }

        if (hInput == 0f && vInput == 0)
            state = (int)State.Idle;
        else
        {
            if (isSprinting)
                state = (int)State.Run;
            else
                state = (int)State.Walk;
        }

        fpsCamera.SetCameraShakeState(state);
    }
    #endregion
}

public enum State
{
    Idle,
    Walk,
    Run
}
