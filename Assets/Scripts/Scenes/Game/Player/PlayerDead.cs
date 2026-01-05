using UnityEngine;
using UnityEngine.Events;

class PlayerDead : MonoBehaviour, IDeadly
{
    public UnityEvent OnDead;

    public void Dead()
    {
        OnDead?.Invoke();
    }
}