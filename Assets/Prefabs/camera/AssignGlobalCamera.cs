using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class AssignGlobalCamera : NetworkBehaviour
{
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private PlayerMovementServerAuth playerMovementServerAuth;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        GameController controller = FindAnyObjectByType<GameController>();

        if (controller == null)
        {
            Debug.LogError("No se encontró el GameController en la escena");
            return;
        }

        var gameplayCamera = controller.systemCameraController.GameplayCamera;

        if (gameplayCamera == null)
        {
            Debug.LogError("GameController no tiene referencia a la cámara de gameplay");
            return;
        }

        gameplayCamera.Follow = cameraTarget;
        gameplayCamera.LookAt = cameraTarget;

        if (playerMovementServerAuth != null)
        {
            playerMovementServerAuth.cameraReference = gameplayCamera.transform;
        }
    }
}
