using UnityEngine;
using UnityEngine.Events;

public class InteractionPressureplate : InteractiveObject
{
    [Header("Pressure plate")]
    [SerializeField] private Animator animator;

    [Header("Events")]
    public UnityEvent<InteractionPressureplate> onActivated;
    public UnityEvent<InteractionPressureplate> onDeactivated;

    private bool isActive;

    protected override void OnArrived()
    {
        Activate();
    }

    protected override void OnLeave()
    {
        Deactivate();
    }

    protected override void OnInteractStart() { }
    protected override void OnInteract() { }
    protected override void OnInteractEnd() { }

    private void Activate()
    {
        if (isActive) return;

        isActive = true;
        animator.SetBool("IsActive", true);

        Debug.Log($"[PressurePlate] Activated: {gameObject.name}", this);
        onActivated?.Invoke(this);
    }

    private void Deactivate()
    {
        if (!isActive) return;

        isActive = false;
        animator.SetBool("IsActive", false);

        Debug.Log($"[PressurePlate] Deactivated: {gameObject.name}", this);
        onDeactivated?.Invoke(this);
    }

    public bool IsActive => isActive;
}
