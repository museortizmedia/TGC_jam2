using UnityEngine;
using UnityEngine.Events;

public class PuzzleModule : MonoBehaviour, IPuzzles
{
    public string puzzleName;
    [Tooltip("Exactamente 4 módulos hijos")]
    public GameObject[] moduleSlots;
    public string ColorIdRute;
    public UnityEvent<string> OnInitPuzzle;

    public void IniciarPuzzle()
    {
        OnInitPuzzle?.Invoke(puzzleName);
    }
}
