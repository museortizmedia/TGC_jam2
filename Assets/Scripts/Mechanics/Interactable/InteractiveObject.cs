using UnityEngine;

[RequireComponent(typeof(ObjectColored))]
public abstract class InteractiveObject : MonoBehaviour, IInteractable
{
    public ObjectColored objectColored;
    protected Transform currentInteractor;
    protected bool isInteracting;


    void Awake()
    {
        objectColored = GetComponent<ObjectColored>();
    }

    public virtual void Arrived(Transform objectArriving)
    {
        currentInteractor = objectArriving;
        OnArrived();
    }

    public virtual void Leave(Transform objectLeaving)
    {
        if (currentInteractor == objectLeaving)
        {
            OnLeave();
            currentInteractor = null;
        }
    }

    public virtual void InteractStart(Transform objectInteracting)
    {
        // Matar al tratar de interactuar con un objeto de otro color
        if(objectInteracting.gameObject.TryGetComponent(out PlayerColor playerColor))
        {
            // Si no es del color del player
            if(!objectColored.IsThisColor(playerColor.currentColor))
            {
                if(objectInteracting.gameObject.TryGetComponent(out IDeadly deadly))
                {
                    deadly.Dead();
                }
            }
        }

        if (!CanInteract()) return;

        isInteracting = true;
        OnInteractStart();
    }

    public virtual void Interact(Transform objectInteracting)
    {
        if (!isInteracting) return;
        OnInteract();
    }

    public virtual void InteractEnd(Transform objectInteracting)
    {
        if (!isInteracting) return;

        isInteracting = false;
        OnInteractEnd();
    }

    protected virtual bool CanInteract() => true;

    protected abstract void OnArrived();
    protected abstract void OnLeave();
    protected abstract void OnInteractStart();
    protected abstract void OnInteract();
    protected abstract void OnInteractEnd();
}