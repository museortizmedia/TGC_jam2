using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class AssignGlobalCamera : NetworkBehaviour
{
    [SerializeField] private Transform cameraTarget;
    [SerializeField] PlayerMovementServerAuth playerMovementServerAuth;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        GameController controller = FindAnyObjectByType<GameController>();


        if (controller == null)
        {
            Debug.LogError("No se encontró el GameControler en la escena");
            return;
        }

        controller.Camera.Follow = cameraTarget;
        controller.Camera.LookAt = cameraTarget;

        playerMovementServerAuth.cameraReference = controller.Camera.transform;
    }
}
