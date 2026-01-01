using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerInteractionTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] bool INTERACTION_DEBUG;

    public IInteractable CurrentInteractable { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IInteractable>(out var interactable))
        {
            CurrentInteractable = interactable;
            interactable.Arrived(transform.root);

            if (INTERACTION_DEBUG) { Debug.Log($"[InteractionTrigger] Arrived at interactable: {other.name}", other); }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<IInteractable>(out var interactable))
        {
            if (CurrentInteractable == interactable)
            {
                interactable.Leave(transform.root);

                if (INTERACTION_DEBUG) { Debug.Log($"[InteractionTrigger] Left interactable: {other.name}", other); }

                CurrentInteractable = null;
            }
        }
    }
}