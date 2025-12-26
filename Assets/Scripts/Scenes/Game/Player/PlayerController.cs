using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovementServerAuth : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 5f;
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
    [SerializeField] float sprintMultiplier = 1.5f;
    [SerializeField] float jumpForce = 5f;

    [Header("References")]
    [SerializeField] Transform cameraTransform;

    [Header("Debug / State (Inspector)")]
    [SerializeField] bool canMove;

    Rigidbody rb;

    Vector2 moveInput;
    float verticalInput;
    bool isSprinting;
    bool jumpRequested;
    float mouseX;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // ---------- NETWORK SPAWN ----------
    public override void OnNetworkSpawn()
    {
        // Solo el propietario local puede moverse
        canMove = IsOwner;

        // Desactivamos PlayerInput en no propietarios
        if (!IsOwner)
        {
            GetComponent<PlayerInput>().enabled = false;
        }
    }

    // ---------- INPUT (CLIENTE PROPIETARIO) ----------
    public void OnMove(InputAction.CallbackContext context)
    {
        if (!IsOwner || !canMove) return;

        moveInput = context.ReadValue<Vector2>();
        SubmitMovementServerRpc(moveInput, verticalInput, isSprinting, jumpRequested, mouseX);
    }

    public void OnVertical(InputAction.CallbackContext context)
    {
        if (!IsOwner || !canMove) return;

        verticalInput = context.ReadValue<float>();
        SubmitMovementServerRpc(moveInput, verticalInput, isSprinting, jumpRequested, mouseX);
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (!IsOwner || !canMove) return;

        isSprinting = context.performed;
        SubmitMovementServerRpc(moveInput, verticalInput, isSprinting, jumpRequested, mouseX);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!IsOwner || !canMove || !context.performed) return;

        jumpRequested = true;
        SubmitMovementServerRpc(moveInput, verticalInput, isSprinting, true, mouseX);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (!IsOwner || !canMove) return;

        mouseX = context.ReadValue<Vector2>().x;
        SubmitMovementServerRpc(moveInput, verticalInput, isSprinting, jumpRequested, mouseX);
    }

    // ---------- SERVER RPC ----------
    [ServerRpc]
    void SubmitMovementServerRpc(
        Vector2 move,
        float vertical,
        bool sprint,
        bool jump,
        float rotX)
    {
        // Seguridad adicional
        if (!canMove) return;

        moveInput = move;
        verticalInput = vertical;
        isSprinting = sprint;

        if (jump)
            jumpRequested = true;

        // Rotación server-authoritative
        transform.Rotate(Vector3.up * rotX * 0.1f);
    }

    // ---------- MOVIMIENTO REAL (SOLO SERVER) ----------
    void FixedUpdate()
    {
        if (!IsServer || !canMove) return;

        bool isSpectator = CheckIfSpectator();
        rb.useGravity = !isSpectator;

        float currentSpeed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);
        Vector3 relativeDir = transform.forward * moveInput.y + transform.right * moveInput.x;

        if (isSpectator)
        {
            rb.linearVelocity = new Vector3(
                relativeDir.x * currentSpeed,
                verticalInput * currentSpeed,
                relativeDir.z * currentSpeed
            );
        }
        else
        {
            rb.linearVelocity = new Vector3(
                relativeDir.x * currentSpeed,
                rb.linearVelocity.y,
                relativeDir.z * currentSpeed
            );

            if (jumpRequested)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                jumpRequested = false;

                if (TryGetComponent<PlayerAnimations>(out PlayerAnimations anims))
                {
                    anims.TriggerJump();
                }
            }
        }
    }

    // ---------- UTIL ----------
    bool CheckIfSpectator()
    {
        return false;
    }
}
