using UnityEngine;
using Unity.Netcode;

public class PlayerAnimations : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] Animator[] animators;
    [SerializeField] PlayerMovementServerAuth movement;
    public Transform visualsChild;

    [Header("Settings")]
    [SerializeField] float smoothTime = 10f;
    [SerializeField] float fallThreshold = 0.15f;
    [SerializeField] float turnSpeed = 15f;

    float airTime;
    float targetYRotation;

    // NetworkVariable para replicar la rotación
    public NetworkVariable<float> targetYRotationNet = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    void Update()
    {
        if (movement == null || visualsChild == null)
            return;

        // ---------- ANIMACIONES DE ESTADO (solo servidor) ----------
        if (IsServer)
        {
            bool grounded = movement.isGrounded;
            SetAnimatorsBool("isGrounded", grounded);

            if (!grounded)
            {
                airTime += Time.deltaTime;
                if (airTime >= fallThreshold)
                    SetAnimatorsBool("isFalling", true);
            }
            else
            {
                airTime = 0f;
                SetAnimatorsBool("isFalling", false);
            }
        }

        // ---------- CALCULO DE DIRECCION Y ROTACION ----------
        Vector3 desiredDirection;

        if (IsOwner)
        {
            Vector3 camForward = Vector3.ProjectOnPlane(movement.cameraReference.forward, Vector3.up).normalized;
            Vector3 camRight = movement.cameraReference.right;
            desiredDirection = camForward * movement.moveInput.y + camRight * movement.moveInput.x;
        }
        else
        {
            // Otros clientes pueden usar la última dirección replicada
            desiredDirection = Vector3.forward;
        }

        if (desiredDirection.sqrMagnitude > 0.01f)
        {
            targetYRotation = Mathf.Atan2(desiredDirection.x, desiredDirection.z) * Mathf.Rad2Deg;

            if (IsServer)
                targetYRotationNet.Value = targetYRotation;
        }

        float rotationToUse = IsOwner ? targetYRotation : targetYRotationNet.Value;
        visualsChild.localRotation = Quaternion.Slerp(
            visualsChild.localRotation,
            Quaternion.Euler(0, rotationToUse, 0),
            Time.deltaTime * turnSpeed
        );

        // ---------- CALCULO DE VELOCIDAD PARA ANIMACIONES (desde input) ----------
        float inputMagnitude = movement.moveInput.magnitude;
        float blend = Mathf.Clamp01(inputMagnitude);

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
