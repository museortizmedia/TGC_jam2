using UnityEngine;
using Unity.Netcode;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] Camera playerCamera;

    public override void OnNetworkSpawn()
    {
        playerCamera.enabled = IsOwner;
    }
}