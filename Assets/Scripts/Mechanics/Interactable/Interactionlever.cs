using UnityEngine;
using UnityEngine.Events;

public class InteractionLever : InteractiveObject
{
    [Header("Lever")]
    [SerializeField] private Animator animator;

    [Header("Events")]
    public UnityEvent<InteractionLever> onActivated;
    public UnityEvent<InteractionLever> onDeactivated;

    private bool isActive;

    protected override void OnArrived() { }
    protected override void OnLeave() { }

    protected override void OnInteractStart()
    {
        if (isActive) return;

        SetActive(true);
        onActivated?.Invoke(this);
    }

    protected override void OnInteract() { }
    protected override void OnInteractEnd() { }

    // =========================
    // CONTROL
    // =========================

    public void SetActive(bool value)
    {
        isActive = value;
        animator.SetBool("IsActive", isActive);

        if (!value)
            onDeactivated?.Invoke(this);
    }

    public bool IsActive => isActive;
}
