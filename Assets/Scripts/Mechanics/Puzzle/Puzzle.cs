using UnityEngine;

public abstract class Puzzle : MonoBehaviour, IPuzzle
{
    protected bool isSolved;

    public virtual void Solved()
    {
        if (isSolved) return;

        isSolved = true;
        OnSolved();
    }

    public virtual void Incorrect()
    {
        if (isSolved) return;

        OnIncorrect();
    }

    // Hooks para clases hijas
    protected abstract void OnSolved();
    protected abstract void OnIncorrect();
}
