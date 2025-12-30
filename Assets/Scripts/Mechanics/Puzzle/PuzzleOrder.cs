using UnityEngine;

public class PuzzleOrder : Puzzle
{
    [SerializeField] private InteractionLever[] levers;

    private int currentIndex = 0;

    private void OnEnable()
    {
        foreach (var lever in levers)
        {
            lever.onActivated.AddListener(OnLeverActivated);
        }
    }

    private void OnDisable()
    {
        foreach (var lever in levers)
        {
            lever.onActivated.RemoveListener(OnLeverActivated);
        }
    }

    public void OnLeverActivated(InteractionLever lever)
    {
        if (isSolved) return;

        if (levers[currentIndex] == lever)
        {
            currentIndex++;

            if (currentIndex >= levers.Length)
            {
                Solved();
            }
        }
        else
        {
            Incorrect();
        }
    }

    protected override void OnSolved()
    {
        Debug.Log("PuzzleOrder SOLVED");
    }

    protected override void OnIncorrect()
    {
        Debug.Log("PuzzleOrder INCORRECT → Reset");

        currentIndex = 0;

        foreach (var lever in levers)
        {
            lever.SetActive(false);
        }
    }
}
