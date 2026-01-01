using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractionController : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] bool INTERACTION_DEBUG;

    [Header("References")]
    [SerializeField] private PlayerInteractionTrigger interactionTrigger;

    private IInteractable currentInteractable;
    private PlayerInputAction input;

    void Awake()
    {
        input = new PlayerInputAction();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        EnableInput();
    }

    public override void OnNetworkDespawn()
    {
        DisableInput();
    }

    // ---------------- INPUT ----------------

    private void EnableInput()
    {
        input.Player.Interact.started += OnInteract;
        input.Player.Interact.performed += OnInteract;
        input.Player.Interact.canceled += OnInteract;

        input.Enable();
    }

    private void DisableInput()
    {
        if (input == null)
            return;

        input.Player.Interact.started -= OnInteract;
        input.Player.Interact.performed -= OnInteract;
        input.Player.Interact.canceled -= OnInteract;

        input.Disable();
    }

    // ---------------- INTERACTION ----------------

    public void OnInteract(InputAction.CallbackContext context)
    {
        currentInteractable = interactionTrigger.CurrentInteractable;

        if (currentInteractable == null)
        {
            if (INTERACTION_DEBUG)
            {
                Debug.Log("[InteractionController] Interact input received, but no interactable in range.");
            }
            return;
        }

        if (context.started)
        {
            if (INTERACTION_DEBUG)
            {
                Debug.Log($"[InteractionController] Interact START with {currentInteractable}", this);
            }
            currentInteractable.InteractStart(transform);
        }
        else if (context.performed)
        {
            if (INTERACTION_DEBUG)
            {
                Debug.Log($"[InteractionController] Interact PERFORMED with {currentInteractable}", this);
            }
            currentInteractable.Interact(transform);
        }
        else if (context.canceled)
        {
            if (INTERACTION_DEBUG)
            {
                Debug.Log($"[InteractionController] Interact END with {currentInteractable}", this);
            }
            currentInteractable.InteractEnd(transform);
        }
    }
}