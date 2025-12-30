using UnityEngine;
using System.Collections.Generic;

public class PuzzleOrder : Puzzle
{
    [SerializeField] private InteractionLever[] levers;

    private List<InteractionLever> activatedOrder = new();
    private int activatedCount = 0;

    private void OnEnable()
    {
        foreach (var lever in levers)
        {
            lever.onInteracted.AddListener(OnLeverActivated);
        }
    }

    private void OnDisable()
    {
        foreach (var lever in levers)
        {
            lever.onInteracted.RemoveListener(OnLeverActivated);
        }
    }

    private void OnLeverActivated(InteractionLever lever)
    {
        if (isSolved) return;

        // Evitar dobles activaciones
        if (activatedOrder.Contains(lever))
            return;

        activatedOrder.Add(lever);
        activatedCount++;

        //  SOLO validar cuando todas estén activadas
        if (activatedCount == levers.Length)
        {
            ValidateOrder();
        }
    }

    private void ValidateOrder()
    {
        for (int i = 0; i < levers.Length; i++)
        {
            if (activatedOrder[i] != levers[i])
            {
                Incorrect();
                return;
            }
        }

        Solved();
    }

    protected override void OnSolved()
    {
        Debug.Log("PuzzleOrder SOLVED");
    }

    protected override void OnIncorrect()
    {
        Debug.Log("PuzzleOrder INCORRECT → Reset");

        activatedOrder.Clear();
        activatedCount = 0;

        foreach (var lever in levers)
        {
            lever.Deactivate();
        }
    }
}
