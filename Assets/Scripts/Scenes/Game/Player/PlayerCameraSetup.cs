using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerCameraSetup : NetworkBehaviour
{
    [SerializeField] private CinemachineCamera freeLook;
    [SerializeField] private GameObject playerCamera;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            // No es mi player → apago cámara
            freeLook.gameObject.SetActive(false);
            return;
        }

        // Es MI player
        freeLook.gameObject.SetActive(true);
    }
}
