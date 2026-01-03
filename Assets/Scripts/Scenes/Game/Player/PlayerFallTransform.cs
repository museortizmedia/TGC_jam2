using UnityEngine;
using Unity.Netcode;

public class PlayerFallTransform : NetworkBehaviour
{
    [Header("Fall Settings")]
    [SerializeField] private float minY = -50f;

    [Header("Respawn")]
    [SerializeField] private Transform spawnPoint;

    private bool respawning;

    private void Update()
    {
        if (!IsServer || respawning || spawnPoint == null)
            return;

        if (transform.position.y < minY)
        {
            respawning = true;
            HandleRespawn();
        }
    }

    private void HandleRespawn()
    {
        // Transición SOLO para el cliente dueño
        SessionManager.Instance.PlayLocalRespawnTransition(
            OwnerClientId,
            ServerRespawn
        );
    }

    /// <summary>
    /// Respawn AUTORITATIVO (Servidor)
    /// </summary>
    private void ServerRespawn()
    {
        // Posicionar directamente el transform (Server Authority)
        transform.SetPositionAndRotation(
            spawnPoint.position,
            spawnPoint.rotation
        );

        respawning = false;
    }
}