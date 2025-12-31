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

    // ---------------- UPDATE ----------------

    void Update()
    {
        if (movement == null || visualsChild == null)
            return;

        // ---------- OWNER: calcula dirección y estado ----------
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

            float blend = Mathf.Clamp01(movement.moveInput.magnitude);
            float animSpeed = Mathf.Max(0.5f, blend);

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

            anim.SetFloat("VelocityZ", velocityZNet.Value);
            anim.SetFloat("AnimSpeed", animSpeedNet.Value);
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
