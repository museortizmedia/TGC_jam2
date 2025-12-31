using UnityEngine;
using Unity.Netcode;

public class PlayerAnimations : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] Animator[] animators;
    [SerializeField] Rigidbody rb;
    [SerializeField] PlayerMovementServerAuth movement;
    public Transform visualsChild;

    [Header("Settings")]
    [SerializeField] float smoothTime = 10f;
    [SerializeField] float fallThreshold = 0.15f;
    [SerializeField] float turnSpeed = 15f;

    // Estado interno
    float airTime;
    float targetYRotation;

    // NetworkVariable para replicar la rotación a todos los clientes
    public NetworkVariable<float> targetYRotationNet = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        movement = GetComponent<PlayerMovementServerAuth>();
    }

    void Update()
    {
        if (movement == null || visualsChild == null)
            return;

        // ---------- ANIMACIONES DE ESTADO (solo servidor) ----------
        if (IsServer)
        {
            bool grounded = movement.isGrounded;
            SetAnimatorsBool("isGrounded", grounded);

            float verticalVel = movement.verticalFreeMoveSpeed;

            if (!grounded)
            {
                airTime += Time.deltaTime;
                if (airTime >= fallThreshold)
                    SetAnimatorsBool("isFalling", true);

                SetAnimatorsFloat("VerticalVelocity", verticalVel);
            }
            else
            {
                airTime = 0f;
                SetAnimatorsBool("isFalling", false);
                SetAnimatorsFloat("VerticalVelocity", 0f);
            }
        }

        // ---------- CALCULO DE ROTACIÓN VISUAL ----------
        Vector3 desiredDirection;

        if (IsOwner)
        {
            // Cliente local: usar input del jugador y dirección de la cámara
            Vector3 camForward = Vector3.ProjectOnPlane(movement.cameraReference.forward, Vector3.up).normalized;
            Vector3 camRight = movement.cameraReference.right;
            desiredDirection = camForward * movement.moveInput.y + camRight * movement.moveInput.x;
        }
        else
        {
            // Otros clientes: usar Rigidbody del jugador para rotación
            Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            desiredDirection = horizontalVel;
        }

        if (desiredDirection.sqrMagnitude > 0.01f)
        {
            targetYRotation = Mathf.Atan2(desiredDirection.x, desiredDirection.z) * Mathf.Rad2Deg;

            // El servidor actualiza la NetworkVariable
            if (IsServer)
                targetYRotationNet.Value = targetYRotation;
        }

        // ---------- APLICAR ROTACIÓN DEL VISUAL ----------
        float rotationToUse = IsOwner ? targetYRotation : targetYRotationNet.Value;

        visualsChild.localRotation = Quaternion.Slerp(
            visualsChild.localRotation,
            Quaternion.Euler(0, rotationToUse, 0),
            Time.deltaTime * turnSpeed
        );

        // ---------- LOCOMOCIÓN (VelocityZ y AnimSpeed) ----------
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float horizontalSpeed = horizontalVelocity.magnitude;

        float blend = Mathf.Clamp01(horizontalSpeed / movement.moveSpeed);

        foreach (Animator anim in animators)
        {
            if (anim == null || !anim.gameObject.activeSelf || !anim.enabled || anim.runtimeAnimatorController == null)
                continue;

            float current = anim.GetFloat("VelocityZ");
            float target = Mathf.Lerp(current, blend, Time.deltaTime * smoothTime);
            anim.SetFloat("VelocityZ", target);
            anim.SetFloat("AnimSpeed", Mathf.Max(0.5f, blend));
        }
    }

    // ---------- EVENTOS ----------
    public void TriggerJump()
    {
        SetAnimatorsTrigger("Jump");
    }

    public void SetAnimatorsTrigger(string paramName)
    {
        foreach (Animator anim in animators)
        {
            if (anim == null || !anim.gameObject.activeSelf || !anim.enabled || anim.runtimeAnimatorController == null)
                continue;

            anim.SetTrigger(paramName);
        }
    }

    public void SetAnimatorsBool(string paramName, bool value)
    {
        foreach (Animator anim in animators)
        {
            if (anim == null || !anim.gameObject.activeSelf || !anim.enabled || anim.runtimeAnimatorController == null)
                continue;

            anim.SetBool(paramName, value);
        }
    }

    public void SetAnimatorsFloat(string paramName, float value)
    {
        foreach (Animator anim in animators)
        {
            if (anim == null || !anim.gameObject.activeSelf || !anim.enabled || anim.runtimeAnimatorController == null)
                continue;

            anim.SetFloat(paramName, value);
        }
    }
}