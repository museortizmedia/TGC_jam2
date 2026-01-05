using Unity.Netcode;
using UnityEngine;

public class PlayerRole : NetworkBehaviour
{
    public NetworkVariable<PlayerRoleType> Role =
        new NetworkVariable<PlayerRoleType>(
            PlayerRoleType.Color,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public bool IsImpostor => Role.Value == PlayerRoleType.Impostor;
    public bool IsColor => Role.Value == PlayerRoleType.Color;

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            Role.OnValueChanged += OnRoleChanged;
        }
    }

    private void OnRoleChanged(PlayerRoleType previous, PlayerRoleType current)
    {
        Debug.Log($"[ROLE] Rol cambiado de {previous} a {current} en client {OwnerClientId}");

        if (!IsOwner) return;

        GameController gc = FindFirstObjectByType<GameController>();
        if (gc != null)
        {
            gc.UpdateHUDForRole(current);
        }
    }
}