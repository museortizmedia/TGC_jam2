using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementServerAuth : NetworkBehaviour
{
    public enum MovementMode
    {
        LocalPlayer,
        ServerPlayer
    }

    [Header("Movement Mode")]
    public MovementMode movementMode = MovementMode.ServerPlayer;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public float verticalFreeMoveSpeed = 5f;

    [Header("Camera Reference")]
    public Transform cameraReference;

    [Header("Ground Detection")]
    public LayerMask groundLayers;
    public float groundCheckRadius = 0.25f;
    public Vector3 groundCheckOffset = new(0, -0.5f, 0);

    [Header("Crouch Settings")]
    public float crouchHeightOffset = 0.5f;

    [Header("Free Navigation Mode")]
    public bool disableJumpAndCrouch = false;

    [Header("Input Debug (Inspector)")]
    public Vector2 moveInput;
    public bool jumpInput;
    public bool crouchInput;
    public float verticalInput;

    private Rigidbody rb;
    private CapsuleCollider playerCollider;

    private bool isGrounded;
    private bool isCrouched;
    private float originalColliderCenterY;

    // INPUT SYSTEM (C#)
    private PlayerInputAction input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();

        rb.freezeRotation = true;
        rb.useGravity = true;

        originalColliderCenterY = playerCollider.center.y;
    }

    public override void OnNetworkSpawn()
    {
        if (movementMode == MovementMode.ServerPlayer && !IsOwner)
            return;

        if (cameraReference == null)
            cameraReference = transform;

        EnableInput();
    }

    public override void OnNetworkDespawn()
    {
        DisableInput();
    }

    #region INPUT SYSTEM (C# ONLY)

    private void EnableInput()
    {
        input = new PlayerInputAction();

        input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled += _ => moveInput = Vector2.zero;

        input.Player.Jump.performed += _ => jumpInput = true;
        input.Player.Jump.canceled += _ => jumpInput = false;

        input.Player.Crouch.performed += _ => crouchInput = true;
        input.Player.Crouch.canceled += _ => crouchInput = false;

        input.Player.Vertical.performed += ctx => verticalInput = ctx.ReadValue<float>();
        input.Player.Vertical.canceled += _ => verticalInput = 0f;

        input.Enable();
    }

    private void DisableInput()
    {
        if (input == null) return;

        input.Disable();
        input.Dispose();
        input = null;
    }

    #endregion

    private void FixedUpdate()
    {
        if (movementMode == MovementMode.ServerPlayer)
        {
            if (!IsOwner) return;

            Vector3 camForward = Vector3.ProjectOnPlane(cameraReference.forward, Vector3.up).normalized;
            Vector3 camRight = cameraReference.right;

            SendInputServerRpc(moveInput, jumpInput, crouchInput, verticalInput,
                   camForward.x, camForward.y, camForward.z,
                   camRight.x, camRight.y, camRight.z);
        }
        else
        {
            ProcessMovement(moveInput, jumpInput, crouchInput, verticalInput,
                            Vector3.ProjectOnPlane(cameraReference.forward, Vector3.up).normalized,
                            cameraReference.right);
        }
    }


    #region SERVER RPC

    [ServerRpc]
    private void SendInputServerRpc(
    Vector2 move, bool jump, bool crouch, float vertical,
    float camForwardX, float camForwardY, float camForwardZ,
    float camRightX, float camRightY, float camRightZ)
    {
        Vector3 camForward = new(camForwardX, camForwardY, camForwardZ);
        Vector3 camRight = new(camRightX, camRightY, camRightZ);

        ProcessMovement(move, jump, crouch, vertical, camForward, camRight);
    }


    #endregion

    #region MOVEMENT LOGIC
    private void ProcessMovement(Vector2 move, bool jump, bool crouch, float vertical, Vector3 forward, Vector3 right)
    {
        CheckGround();

        // Calculamos velocidad horizontal
        Vector3 horizontalVelocity = (forward * move.y + right * move.x) * moveSpeed;

        if (disableJumpAndCrouch)
        {
            // Modo libre: movemos en eje Y con input vertical y desactivamos gravedad
            rb.useGravity = false;
            rb.linearVelocity = new Vector3(horizontalVelocity.x, vertical * verticalFreeMoveSpeed, horizontalVelocity.z);
        }
        else
        {
            // Modo normal: activamos gravedad
            rb.useGravity = true;

            // Conservamos la velocidad vertical actual
            float newY = rb.linearVelocity.y;

            // Salto
            if (jump && isGrounded)
            {
                newY = jumpForce;
            }

            // Aplicamos velocidad horizontal + vertical (incluye salto si corresponde)
            rb.linearVelocity = new Vector3(horizontalVelocity.x, newY, horizontalVelocity.z);

            // Crouch
            HandleCrouch(crouch);
        }
    }

    private void HandleCrouch(bool crouch)
    {
        if (crouch && !isCrouched)
        {
            playerCollider.center = new Vector3(
                playerCollider.center.x,
                originalColliderCenterY - crouchHeightOffset,
                playerCollider.center.z
            );
            isCrouched = true;
        }
        else if (!crouch && isCrouched)
        {
            playerCollider.center = new Vector3(
                playerCollider.center.x,
                originalColliderCenterY,
                playerCollider.center.z
            );
            isCrouched = false;
        }
    }

    private void CheckGround()
    {
        Vector3 checkPosition = transform.position + groundCheckOffset;
        isGrounded = Physics.CheckSphere(
            checkPosition,
            groundCheckRadius,
            groundLayers,
            QueryTriggerInteraction.Ignore
        );
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position + groundCheckOffset, groundCheckRadius);
    }
}