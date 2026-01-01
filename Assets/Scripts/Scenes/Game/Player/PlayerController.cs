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

    public float sprintSpeed = 10f;

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
    // AGREGADO: Variable para rastrear el estado del sprint
    public bool sprintInput;

    private Rigidbody rb;
    private CapsuleCollider playerCollider;

    [HideInInspector] public bool isGrounded;
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

    public NetworkVariable<bool> isGroundedNet = new NetworkVariable<bool>(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

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

        // Registro de la acción de Sprint
        input.Player.Sprint.performed += _ => sprintInput = true;
        input.Player.Sprint.canceled += _ => sprintInput = false;

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

            // Enviamos el sprintInput al servidor
            SendInputServerRpc(moveInput, jumpInput, crouchInput, verticalInput, sprintInput,
                   camForward.x, camForward.y, camForward.z,
                   camRight.x, camRight.y, camRight.z);
        }
        else
        {
            // Procesamiento local incluyendo sprintInput
            ProcessMovement(moveInput, jumpInput, crouchInput, verticalInput, sprintInput,
                            Vector3.ProjectOnPlane(cameraReference.forward, Vector3.up).normalized,
                            cameraReference.right);
        }
    }


    #region SERVER RPC

    [ServerRpc]
    private void SendInputServerRpc(
    Vector2 move, bool jump, bool crouch, float vertical, bool sprint,
    float camForwardX, float camForwardY, float camForwardZ,
    float camRightX, float camRightY, float camRightZ)
    {
        Vector3 camForward = new(camForwardX, camForwardY, camForwardZ);
        Vector3 camRight = new(camRightX, camRightY, camRightZ);

        ProcessMovement(move, jump, crouch, vertical, sprint, camForward, camRight);
    }


    #endregion

    #region MOVEMENT LOGIC
    private void ProcessMovement(Vector2 move, bool jump, bool crouch, float vertical, bool sprint, Vector3 forward, Vector3 right)
    {
        CheckGround();

        // Aplicación de velocidad: Sprint solo funciona si estamos en el suelo
        float currentSpeed = (sprint && isGrounded) ? sprintSpeed : moveSpeed;

        Vector3 horizontalVelocity = (forward * move.y + right * move.x) * currentSpeed;


        if (disableJumpAndCrouch)
        {
            rb.useGravity = false;
            rb.linearVelocity = new Vector3(horizontalVelocity.x, vertical * verticalFreeMoveSpeed, horizontalVelocity.z);
        }
        else
        {
            rb.useGravity = true;

            float newY = rb.linearVelocity.y;

            if (jump && isGrounded)
            {
                newY = jumpForce;
            }

            rb.linearVelocity = new Vector3(horizontalVelocity.x, newY, horizontalVelocity.z);

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

        Collider[] hits = Physics.OverlapSphere(
            checkPosition,
            groundCheckRadius,
            groundLayers,
            QueryTriggerInteraction.Ignore
        );

        bool groundedResult = false;
        foreach (var hit in hits)
        {
            if (hit != playerCollider)
            {
                groundedResult = true;
                break;
            }
        }

        isGrounded = groundedResult;

        if (IsServer)
        {
            isGroundedNet.Value = isGrounded;
        }
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position + groundCheckOffset, groundCheckRadius);
    }
}