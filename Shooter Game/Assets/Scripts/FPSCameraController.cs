using UnityEngine;

public class FPSCameraController : MonoBehaviour
{
    public Transform body;
    public Animator animator;

    public float mouseSensitivity;

    float xRot, yRot;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void FixedUpdate()
    {
        // Camera Input.
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.fixedDeltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.fixedDeltaTime;

        // Camera/Body Rotating.
        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);

        yRot = mouseX;
        body.Rotate(Vector3.up * yRot);
    }

    public void SetCameraShakeState(int state)
    {
        animator.SetInteger("State", state);
    }

    public void TriggerShake(string name)
    {
        animator.SetTrigger(name);
    }
}
