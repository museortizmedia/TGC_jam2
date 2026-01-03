using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleModule : NetworkBehaviour
{
    public string puzzleName;
    [Tooltip("Exactamente 4 módulos hijos")]
    public GameObject[] moduleSlots;
    public NetworkVariable<FixedString32Bytes> ColorIdRute =
        new NetworkVariable<FixedString32Bytes>(
            "blanco",
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
    public UnityEvent<string> OnInitPuzzle;

    public void IniciarPuzzle()
    {
        OnInitPuzzle?.Invoke(puzzleName);
    }

    public override void OnNetworkSpawn()
    {
        ColorIdRute.OnValueChanged += OnColorChanged;
        OnColorChanged(default, ColorIdRute.Value);
    }

    private void OnColorChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        //ApplyColor(newValue.ToString());
    }
}
