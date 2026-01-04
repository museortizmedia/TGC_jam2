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
}