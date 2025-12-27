using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementServerAuth : NetworkBehaviour
{
    [SerializeField] public float moveSpeed = 20f;
    [SerializeField] float sprintMultiplier = 1.5f;
    [SerializeField] float jumpForce = 5f;

    Rigidbody rb;
    Vector2 moveInput;
    float verticalInput;
    bool isSprinting;
    bool jumpRequested;
    float mouseX;

    void Awake() => rb = GetComponent<Rigidbody>();

    // ---------- INPUT (SOLO OWNER) ----------
    public void OnMove(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        moveInput = context.ReadValue<Vector2>();
        // Enviamos todo, incluyendo el mouseX actual
        SubmitMovementServerRpc(moveInput, verticalInput, isSprinting, jumpRequested, mouseX);
    }

    public void OnVertical(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        verticalInput = context.ReadValue<float>();
        SubmitMovementServerRpc(moveInput, verticalInput, isSprinting, jumpRequested, mouseX);
    }

    public void OnSprint(InputAction.CallbackContext context)
    {

        if (!IsOwner) return;
        isSprinting = context.performed;
        SubmitMovementServerRpc(moveInput, verticalInput, isSprinting, jumpRequested, mouseX);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!IsOwner || !context.performed) return;
        jumpRequested = true;
        SubmitMovementServerRpc(moveInput, verticalInput, isSprinting, true, mouseX);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        //if (!IsOwner) return;
        //mouseX = context.ReadValue<Vector2>().x;
        //SubmitMovementServerRpc(moveInput, verticalInput, isSprinting, jumpRequested, mouseX);
    }

    // ---------- RPC ----------
    [ServerRpc]
    void SubmitMovementServerRpc(Vector2 move, float vertical, bool sprint, bool jump, float rotX)
    {
        moveInput = move;
        verticalInput = vertical;
        isSprinting = sprint;
        if (jump) jumpRequested = true;

        // El servidor aplica la rotación directamente
        transform.Rotate(Vector3.up * rotX * 0.1f);
    }

    // ---------- MOVIMIENTO REAL (SOLO SERVER) ----------
    void FixedUpdate() //mejor con fisicas. 
    {
        if (!IsServer) return;

        bool isSpectator = CheckIfSpectator();
        rb.useGravity = !isSpectator;

        float currentSpeed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);
        Vector3 relativeDir = transform.forward * moveInput.y + transform.right * moveInput.x;

        if (isSpectator)
        {
            // Movimiento libre (vuelo)
            rb.linearVelocity = new Vector3(relativeDir.x * currentSpeed, verticalInput * currentSpeed, relativeDir.z * currentSpeed);
        }
        else
        {
            // 2. Movimiento terrestre
            rb.linearVelocity = new Vector3(relativeDir.x * currentSpeed, rb.linearVelocity.y, relativeDir.z * currentSpeed);

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

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) GetComponent<PlayerInput>().enabled = false;
    }

    private bool CheckIfSpectator() { return false; }
}