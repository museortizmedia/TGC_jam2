using UnityEngine;
using UnityEngine.Events;

public class InteractionButton : InteractiveObject
{
    [Header("Button Settings")]
    [SerializeField] private bool singleUse = false;
    private bool alreadyUsed = false;

    [Header("Actions")]
    [SerializeField] private UnityEvent onPressed;
    [SerializeField] private UnityEvent onHighlighted;
    [SerializeField] private UnityEvent onDisHighlighted;

    protected override bool CanInteract()
    {
        return !singleUse || !alreadyUsed;
    }

    protected override void OnArrived()
    {
        onHighlighted?.Invoke();
    }

    protected override void OnLeave()
    {
        onDisHighlighted?.Invoke();
    }

    protected override void OnInteractStart()
    {
        PressButton();
    }

    private void PressButton()
    {
        if (singleUse)
            alreadyUsed = true;

        onPressed?.Invoke();

        MazeGenerator maze = GetComponentInParent<MazeGenerator>();
        if (maze != null)
        {
            Destroy(maze.gameObject);
        }
    }

    protected override void OnInteractEnd()
    {
        //
    }

    protected override void OnInteract()
    {
        //
    }
}