using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovementServerAuth : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 5f;
    public float MoveSpeed => moveSpeed;

    [SerializeField] float sprintMultiplier = 1.5f;
    [SerializeField] float jumpForce = 6f;

    [Header("Ground Check")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float groundCheckDistance = 0.35f;
    [SerializeField] float groundCheckOffset = 0.2f;
    [SerializeField] float coyoteTime = 0.15f;

    [Header("Camera")]
    public Transform cameraTransform;

    Rigidbody rb;
    PlayerInput playerInput;

    Vector2 moveInput;
    bool isSprinting;

    bool jumpBuffered;   // input recibido
    bool jumpConsumed;   // salto ejecutado

    float lastGroundedTime;

    public bool IsGrounded { get; private set; }
    public float VerticalVelocity => rb.linearVelocity.y;

    bool canSendInput;
    bool canMoveServer;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();

        if (IsOwner)
            canSendInput = true;
        else
        {
            canSendInput = false;
            playerInput.enabled = false;
        }

        if (IsServer)
            canMoveServer = true;
    }

    // ---------- INPUT ----------
    public void OnMove(InputAction.CallbackContext context)
    {
        if (!IsOwner || !canSendInput) return;
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (!IsOwner || !canSendInput) return;
        isSprinting = context.performed;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!IsOwner || !canSendInput) return;
        if (context.performed) {
            jumpBuffered = true;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void Update()
    {
        if (!IsOwner || !canSendInput) return;
        SubmitMovementServerRpc(moveInput, isSprinting, jumpBuffered);
        jumpBuffered = false;
    }

    [ServerRpc]
    void SubmitMovementServerRpc(Vector2 move, bool sprint, bool jump)
    {
        if (!canMoveServer) return;

        moveInput = move;
        isSprinting = sprint;

        if (jump)
            jumpBuffered = true;
    }

    // ---------- SERVER PHYSICS ----------
    void FixedUpdate()
    {
        if (!IsServer || !canMoveServer) return;

        UpdateGroundedState();

        float speed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        if (cameraTransform != null)
        {
            forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
        }

        Vector3 direction = forward * moveInput.y + right * moveInput.x;

        rb.linearVelocity = new Vector3(
            direction.x * speed,
            rb.linearVelocity.y,
            direction.z * speed
        );

        bool canJump =
            jumpBuffered &&
            (IsGrounded || Time.time - lastGroundedTime <= coyoteTime) &&
            !jumpConsumed;

        if (canJump)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            jumpConsumed = true;
            jumpBuffered = false;
        }

        if (IsGrounded)
            jumpConsumed = false;
    }

    void UpdateGroundedState()
    {
        Vector3 origin = transform.position + Vector3.up * groundCheckOffset;

        IsGrounded = Physics.Raycast(
            origin,
            Vector3.down,
            groundCheckDistance,
            groundLayer
        );

        if (IsGrounded)
            lastGroundedTime = Time.time;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position + Vector3.up * groundCheckOffset;
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawSphere(origin + Vector3.down * groundCheckDistance, 0.08f);
        Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
    }
#endif
}
