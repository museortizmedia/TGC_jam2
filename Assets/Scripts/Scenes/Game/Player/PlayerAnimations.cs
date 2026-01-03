using UnityEngine;
using Unity.Netcode;

public class PlayerAnimations : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] Animator[] animators;
    [SerializeField] PlayerMovementServerAuth movement;
    public Transform visualsChild;

    [Header("Settings")]
    //[SerializeField] float smoothTime = 10f;
    //[SerializeField] float fallThreshold = 0.15f;

    [SerializeField] float turnSpeed = 15f;

    float airTime;
    float targetYRotation;

    // ---------------- NETWORK VARIABLES ----------------

    public NetworkVariable<float> targetYRotationNet = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<float> velocityZNet = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<float> animSpeedNet = new NetworkVariable<float>(
        1f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<float> verticalVelocityNet = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // ---------------- UPDATE ----------------

    void Update()
    {
        if (movement == null || visualsChild == null)
            return;

        // ---------- SERVER: Leer velocidad física para el Blend Tree Airborne ----------
        if (IsServer)
        {
            Rigidbody rb = movement.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Sincroniza la velocidad Y para jump_start y jump_Middle
                verticalVelocityNet.Value = rb.linearVelocity.y;
            }
        }

        // ---------- OWNER: calcula dirección y estado de movimiento ----------
        if (IsOwner)
        {
            Vector3 camForward = Vector3.ProjectOnPlane(
                movement.cameraReference.forward,
                Vector3.up
            ).normalized;

            Vector3 camRight = movement.cameraReference.right;
            Vector3 desiredDirection =
                camForward * movement.moveInput.y +
                camRight * movement.moveInput.x;

            // Rotación visual
            if (desiredDirection.sqrMagnitude > 0.01f)
            {
                targetYRotation =
                    Mathf.Atan2(desiredDirection.x, desiredDirection.z) *
                    Mathf.Rad2Deg;

                if (IsServer)
                    targetYRotationNet.Value = targetYRotation;
                else
                    SubmitRotationServerRpc(targetYRotation);
            }

            // Lógica de Sprint para VelocityZ
            float inputMagnitude = movement.moveInput.magnitude;
            float targetVelocityZ = inputMagnitude;

            // Si sprintInput es true, subimos el valor a 1.5 para activar "running"
            if (movement.sprintInput && movement.isGrounded && inputMagnitude > 0.1f)
            {
                targetVelocityZ = inputMagnitude * 1.5f;
            }

            float blend = targetVelocityZ;
            float animSpeed = Mathf.Max(0.5f, inputMagnitude);

            if (IsServer)
            {
                velocityZNet.Value = blend;
                animSpeedNet.Value = animSpeed;
            }
            else
            {
                SubmitAnimationServerRpc(blend, animSpeed);
            }
        }

        // ---------- TODOS LOS CLIENTES: aplicar animaciones ----------
        foreach (Animator anim in animators)
        {
            if (anim == null || !anim.gameObject.activeSelf || !anim.enabled || anim.runtimeAnimatorController == null)
                continue;

            // Controla Idle (0), Walking (1) y Running (1.5)
            anim.SetFloat("VelocityZ", velocityZNet.Value);
            anim.SetFloat("AnimSpeed", animSpeedNet.Value);

            // Controla la pose de salto/caída dentro del Blend Tree Airborne
            anim.SetFloat("VerticalVelocity", verticalVelocityNet.Value);

            // CAMBIO: Verificar que movement.isGroundedNet existe antes de usarla
            if (movement.isGroundedNet != null)
            {
                anim.SetBool("isGrounded", movement.isGroundedNet.Value);
            }
            else
            {
                // Fallback: usa el valor local
                anim.SetBool("isGrounded", movement.isGrounded);
            }
        }

        // ---------- TODOS LOS CLIENTES: aplicar rotación visual ----------
        float rotationToUse = IsOwner ? targetYRotation : targetYRotationNet.Value;

        visualsChild.localRotation = Quaternion.Slerp(
            visualsChild.localRotation,
            Quaternion.Euler(0f, rotationToUse, 0f),
            Time.deltaTime * turnSpeed
        );
    }

    // ---------------- RPCs ----------------

    [ServerRpc]
    void SubmitAnimationServerRpc(float velocityZ, float animSpeed)
    {
        velocityZNet.Value = velocityZ;
        animSpeedNet.Value = animSpeed;
    }

    [ServerRpc]
    void SubmitRotationServerRpc(float yRotation)
    {
        targetYRotationNet.Value = yRotation;
    }

    // ---------------- EVENTS ----------------

    public void TriggerJump()
    {
        if (IsOwner)
            TriggerJumpServerRpc();
    }

    [ServerRpc]
    void TriggerJumpServerRpc()
    {
        TriggerJumpClientRpc();
    }

    [ClientRpc]
    void TriggerJumpClientRpc()
    {
        SetAnimatorsTrigger("Jump");
    }

    // ---------------- ANIMATOR HELPERS ----------------

    void SetAnimatorsTrigger(string paramName)
    {
        foreach (var anim in animators)
        {
            if (anim && anim.isActiveAndEnabled)
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