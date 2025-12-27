using UnityEngine;
using Unity.Netcode;

public class PlayerAnimations : NetworkBehaviour
{
    private Animator animator;
    private Rigidbody rb;
    private PlayerMovementServerAuth movementScript;

    [Header("Settings")]
    [SerializeField] float smoothTime = 10f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float groundCheckDistance = 0.1f;
    [SerializeField] float fallThreshold = 0.15f; // Ajustado para evitar parpadeos en saltos pequeños
    [SerializeField] float turnSpeed = 15f;

    private Transform visualsChild;
    private float airTime = 0f;
    private float targetYRotation = 0f;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        movementScript = GetComponent<PlayerMovementServerAuth>();

        if (animator != null) visualsChild = animator.transform;
        else Debug.LogError("No Animator found in children of " + gameObject.name);
    }

    void Update()
    {
        if (rb == null || animator == null || movementScript == null || visualsChild == null) return;

        // 1. ESTADO DE SUELO Y FÍSICA VERTICAL
        bool grounded = CheckIfGrounded();
        animator.SetBool("isGrounded", grounded);
        float verticalVel = rb.linearVelocity.y;

        if (!grounded)
        {
            // Gestión de tiempo de caída
            airTime += Time.deltaTime;
            if (airTime >= fallThreshold)
            {
                animator.SetBool("isFalling", true);
            }

            // Sincronización de animación de aire por velocidad real del Rigidbody
            animator.SetFloat("VerticalVelocity", verticalVel);
        }
        else
        {
            airTime = 0f;
            animator.SetBool("isFalling", false);
            // Al tocar suelo, forzamos el parámetro a 0 para estabilidad
            animator.SetFloat("VerticalVelocity", 0f);
        }

        // 2. CÁLCULO DE VELOCIDADES HORIZONTALES
        float currentBaseSpeed = movementScript.moveSpeed;
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        float horizontalSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;

        // 3. LÓGICA DE ROTACIÓN Y LOCOMOCIÓN (Blend Tree 1D)
        if (horizontalSpeed > 0.5f)
        {
            float angle = Mathf.Atan2(localVelocity.x, localVelocity.z) * Mathf.Rad2Deg;
            targetYRotation = angle;

            float blendZ = horizontalSpeed / currentBaseSpeed;
            animator.SetFloat("VelocityZ", Mathf.Lerp(animator.GetFloat("VelocityZ"), blendZ, Time.deltaTime * smoothTime));

            float speedMultiplier = horizontalSpeed / currentBaseSpeed;
            animator.SetFloat("AnimSpeed", Mathf.Max(0.5f, speedMultiplier));
        }
        else
        {
            animator.SetFloat("VelocityZ", Mathf.Lerp(animator.GetFloat("VelocityZ"), 0f, Time.deltaTime * smoothTime));
            animator.SetFloat("AnimSpeed", 1f);
        }

        // 4. APLICAR ROTACIÓN AL MODELO
        if (horizontalSpeed > 0.1f)
        {
            visualsChild.localRotation = Quaternion.Slerp(
                visualsChild.localRotation,
                Quaternion.Euler(0, targetYRotation, 0),
                Time.deltaTime * turnSpeed
            );
        }
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
        if (animator != null) animator.SetTrigger("Jump");
    }
}