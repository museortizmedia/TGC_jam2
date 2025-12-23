using UnityEngine;
using Unity.Netcode;

public class PlayerColor : NetworkBehaviour
{
    [SerializeField] Renderer rend;

    private NetworkVariable<Color> playerColor =
        new(writePerm: NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            playerColor.Value = Random.ColorHSV();
        }

        ApplyColor(playerColor.Value);
        playerColor.OnValueChanged += OnColorChanged;
    }

    public override void OnNetworkDespawn()
    {
        playerColor.OnValueChanged -= OnColorChanged;
    }

    void OnColorChanged(Color oldColor, Color newColor)
    {
        ApplyColor(newColor);
    }

    void ApplyColor(Color color)
    {
        rend.material.color = color;
    }
}