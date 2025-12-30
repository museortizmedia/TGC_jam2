using UnityEngine;
using UnityEngine.Events;

public class InteractionLever : InteractiveObject
{
    [Header("Lever")]
    [SerializeField] private Animator animator;

    [Header("Events")]
    public UnityEvent<InteractionLever> onInteracted;

    private bool isActive;

    protected override void OnArrived() { }
    protected override void OnLeave() { }

    protected override void OnInteractStart()
    {
        if (isActive) return;

        isActive = true;
        animator.SetBool("IsActive", true);

        Debug.Log($"[InteractionLever] Interacted: {gameObject.name}", this);
        onInteracted?.Invoke(this);
    }



    // Implementación requerida por la clase base
    protected override void OnInteract()
    {
        // Lógica de interacción (puede dejarse vacío si no se necesita)
    }

    protected override void OnInteractEnd()
    {
        // Lógica al finalizar la interacción (puede dejarse vacío si no se necesita)
    }

    // =========================
    // STATE (control externo)
    // =========================

    public void Activate()
    {
        Debug.Log($"[InteractionLever] Activated: {gameObject.name}", this);
        animator.SetBool("IsActive", true);
    }

    public void Deactivate()
    {
        isActive = false;
        animator.SetBool("IsActive", false);

    }

    public bool IsActive => isActive;
}
