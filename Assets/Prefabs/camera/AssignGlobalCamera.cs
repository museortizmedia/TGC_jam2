using Unity.Netcode;
using UnityEngine;

public class AssignGlobalCamera : NetworkBehaviour
{
    [SerializeField] private Transform cameraTarget;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        var camManager = CameraManager.Instance;

        if (camManager == null)
        {
            Debug.LogError("CameraManager no encontrado");
            return;
        }

        camManager.FreeLookCamera.Follow = cameraTarget;
        camManager.FreeLookCamera.LookAt = cameraTarget;
    }
}
