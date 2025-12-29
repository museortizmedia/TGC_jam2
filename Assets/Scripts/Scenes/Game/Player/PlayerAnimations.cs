using UnityEngine;
using Unity.Netcode;

public class PlayerAnimations : NetworkBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] Rigidbody rb;
    [SerializeField] PlayerMovementServerAuth movement;

    [Header("Settings")]
    [SerializeField] float smoothTime = 10f;
    [SerializeField] float fallThreshold = 0.15f;
    [SerializeField] float turnSpeed = 15f;

    Transform visualsChild;

    float airTime;
    float targetYRotation;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        movement = GetComponent<PlayerMovementServerAuth>();

        if (animator != null)
            visualsChild = animator.transform;
        else
            Debug.LogError("Animator not found");
    }

    void Update()
    {
        if (!IsServer || animator == null || movement == null || visualsChild == null)
            return;

        // ---------- GROUND STATE ----------
        bool grounded = movement.IsGrounded;
        animator.SetBool("isGrounded", grounded);

        float verticalVel = movement.VerticalVelocity;

        if (!grounded)
        {
            airTime += Time.deltaTime;
            if (airTime >= fallThreshold)
                animator.SetBool("isFalling", true);

            animator.SetFloat("VerticalVelocity", verticalVel);
        }
        else
        {
            airTime = 0f;
            animator.SetBool("isFalling", false);
            animator.SetFloat("VerticalVelocity", 0f);
        }

        // ---------- LOCOMOTION ----------
        float baseSpeed = movement.MoveSpeed;
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        float horizontalSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;

        if (horizontalSpeed > 0.1f)
        {
            float angle = Mathf.Atan2(localVelocity.x, localVelocity.z) * Mathf.Rad2Deg;
            targetYRotation = angle;

            float blend = horizontalSpeed / baseSpeed;
            animator.SetFloat(
                "VelocityZ",
                Mathf.Lerp(animator.GetFloat("VelocityZ"), blend, Time.deltaTime * smoothTime)
            );

            animator.SetFloat("AnimSpeed", Mathf.Max(0.5f, blend));
        }
        else
        {
            animator.SetFloat(
                "VelocityZ",
                Mathf.Lerp(animator.GetFloat("VelocityZ"), 0f, Time.deltaTime * smoothTime)
            );
            animator.SetFloat("AnimSpeed", 1f);
        }

        // ---------- VISUAL ROTATION ----------
        if (horizontalSpeed > 0.1f)
        {
            visualsChild.localRotation = Quaternion.Slerp(
                visualsChild.localRotation,
                Quaternion.Euler(0, targetYRotation, 0),
                Time.deltaTime * turnSpeed
            );
        }
    }

    // ---------- EVENTOS ----------
    public void TriggerJump()
    {
        animator.SetTrigger("Jump");
    }
}