using UnityEngine;
using Unity.Netcode;

public class PlayerAnimations : NetworkBehaviour
{
    [SerializeField] Animator[] animators;
    [SerializeField] Rigidbody rb;
    [SerializeField] PlayerMovementServerAuth movement;

    [Header("Settings")]
    [SerializeField] float smoothTime = 10f;
    [SerializeField] float fallThreshold = 0.15f;
    [SerializeField] float turnSpeed = 15f;

    public Transform visualsChild;

    float airTime;
    float targetYRotation;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        movement = GetComponent<PlayerMovementServerAuth>();
    }

    void Update()
    {
        if (!IsServer || movement == null || visualsChild == null)
            return;

        // ---------- GROUND STATE ----------
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

        // ---------- LOCOMOTION ----------
        float baseSpeed = movement.moveSpeed;
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        float horizontalSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;

        if (horizontalSpeed > 0.1f)
        {
            float angle = Mathf.Atan2(localVelocity.x, localVelocity.z) * Mathf.Rad2Deg;
            targetYRotation = angle;

            float blend = horizontalSpeed / baseSpeed;
            foreach (Animator anim in animators)
            {
                float current = anim.GetFloat("VelocityZ");
                float target = Mathf.Lerp(current, blend, Time.deltaTime * smoothTime);
                anim.SetFloat("VelocityZ", target);
            }

            SetAnimatorsFloat("AnimSpeed", Mathf.Max(0.5f, blend));
        }
        else
        {
            foreach (Animator anim in animators)
            {
                float current = anim.GetFloat("VelocityZ");
                float target = Mathf.Lerp(current, 0f, Time.deltaTime * smoothTime);
                anim.SetFloat("VelocityZ", target);
            }
            SetAnimatorsFloat("AnimSpeed", 1f);
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
        SetAnimatorsTrigger("Jump");
    }

    public void SetAnimatorsTrigger(string ParamName)
    {
        foreach(Animator anim in animators)
        {
            anim.SetTrigger(ParamName);
        }
    }
    public void SetAnimatorsBool(string ParamName, bool value)
    {
        foreach(Animator anim in animators)
        {
            anim.SetBool(ParamName, value);
        }
    }
    public void SetAnimatorsFloat(string ParamName, float value)
    {
        foreach(Animator anim in animators)
        {
            anim.SetFloat(ParamName, value);
        }
    }
}