using UnityEngine;

public interface IInteractable
{
    void InteractStart(Transform objectInteracting);
    void Interact(Transform objectInteracting);
    void InteractEnd(Transform objectInteracting);
    void Arrived(Transform objectArriving);
    void Leave(Transform objectLeaving);
}
