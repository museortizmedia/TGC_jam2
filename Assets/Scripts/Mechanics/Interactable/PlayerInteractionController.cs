#define INTERACTION_DEBUG

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInteractionTrigger interactionTrigger;

    private IInteractable currentInteractable;

    public void OnInteract(InputAction.CallbackContext context)
    {
        currentInteractable = interactionTrigger.CurrentInteractable;

        if (currentInteractable == null)
        {
#if INTERACTION_DEBUG
            Debug.Log("[InteractionController] Interact input received, but no interactable in range.");
#endif
            return;
        }

        if (context.started)
        {
#if INTERACTION_DEBUG
            Debug.Log($"[InteractionController] Interact START with {currentInteractable}", this);
#endif
            currentInteractable.InteractStart(transform);
        }
        else if (context.performed)
        {
#if INTERACTION_DEBUG
            Debug.Log($"[InteractionController] Interact PERFORMED with {currentInteractable}", this);
#endif
            currentInteractable.Interact(transform);
        }
        else if (context.canceled)
        {
#if INTERACTION_DEBUG
            Debug.Log($"[InteractionController] Interact END with {currentInteractable}", this);
#endif
            currentInteractable.InteractEnd(transform);
        }
    }
}