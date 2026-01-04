using Unity.Collections;
using Unity.Netcode;
using UnityEngine.Events;

public class PuzzleModule : NetworkBehaviour
{
    public string colorName;
    public NetworkVariable<FixedString32Bytes> ColorIdRute =
        new NetworkVariable<FixedString32Bytes>(
            "blanco",
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public UnityEvent<string> OnInitPuzzle;

    public override void OnNetworkSpawn()
    {
        ColorIdRute.OnValueChanged += OnColorChanged;
        OnColorChanged(default, ColorIdRute.Value);
    }

    private void OnDisable()
    {
        ColorIdRute.OnValueChanged -= OnColorChanged;
    }

    private void OnColorChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        OnInitPuzzle?.Invoke(newValue.ToString());
    }

    public void IniciarPuzzle()
    {
        OnInitPuzzle?.Invoke(ColorIdRute.Value.ToString());
    }
}
