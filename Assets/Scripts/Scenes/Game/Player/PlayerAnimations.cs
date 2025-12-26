using UnityEngine;
using Unity.Netcode;

public class PlayerAnimations : NetworkBehaviour
{
    private Animator animator;
    private Rigidbody rb;
    private PlayerMovementServerAuth movementScript;

    [SerializeField] float smoothTime = 10f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float groundCheckDistance = 0.01f;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        movementScript = GetComponent<PlayerMovementServerAuth>();
    }

    void Update()
    {
        if (rb == null || animator == null || movementScript == null) return;

        bool grounded = CheckIfGrounded();
        animator.SetBool("isGrounded", grounded);

        Debug.Log($"�Tocando suelo?: {grounded}");

        // 2. Locomoci�n (Blend Tree)
        float currentBaseSpeed = movementScript.MoveSpeed;
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);

        float targetX = localVelocity.x / currentBaseSpeed;
        float targetZ = localVelocity.z / currentBaseSpeed;

        float finalX = Mathf.Lerp(animator.GetFloat("VelocityX"), targetX, Time.deltaTime * smoothTime);
        float finalZ = Mathf.Lerp(animator.GetFloat("VelocityZ"), targetZ, Time.deltaTime * smoothTime);

        animator.SetFloat("VelocityX", finalX);
        animator.SetFloat("VelocityZ", finalZ);
    }

    private bool CheckIfGrounded()
    {
        float extraHeight = 0.3f;
        Vector3 rayOrigin = transform.position + Vector3.up * extraHeight;
        float rayDistance = extraHeight + groundCheckDistance;

        return Physics.Raycast(rayOrigin, Vector3.down, rayDistance, groundLayer);
    }

    public void TriggerJump()
    {
        if (animator != null)
        {
            animator.SetTrigger("Jump");
            Debug.Log("�Animaci�n de Salto activada!");
        }
    }

}