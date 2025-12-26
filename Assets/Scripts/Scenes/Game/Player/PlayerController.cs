using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerMovementServerAuth : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] Transform cameraTransform;


    Vector2 moveInput;

    // ---------- INPUT (SOLO OWNER) ----------
    public void OnMove(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        moveInput = context.ReadValue<Vector2>();


        // Dirección de la cámara (local)
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * moveInput.y + camRight * moveInput.x;

        Debug.Log($"[CLIENT {OwnerClientId}] Input: {moveInput}");
        SubmitMovementServerRpc(moveInput);
    }

    // ---------- RPC ----------
    [ServerRpc]
    void SubmitMovementServerRpc(Vector2 input)
    {
        Debug.Log($"[SERVER] Received input from {OwnerClientId}: {input}");
        moveInput = input;
    }

    // ---------- MOVIMIENTO REAL (SOLO SERVER) ----------
    void Update()
    {
        if (!IsServer) return;

        if (moveInput != Vector2.zero)
            Debug.Log($"[SERVER] Moving {OwnerClientId}");

        Vector3 dir = new Vector3(moveInput.x, 0f, moveInput.y);
        transform.position += dir * moveSpeed * Time.deltaTime;
    }



    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            GetComponent<PlayerInput>().enabled = false;
        }
    }

}
