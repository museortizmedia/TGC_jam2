using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public class PlayerFall : NetworkBehaviour
{
    [Header("Fall Settings")]
    [SerializeField] private float minY = -50f;

    [Header("Respawn")]
    public Transform spawnPoint;

    private Rigidbody rb;
    private bool respawning;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!IsServer || respawning || spawnPoint == null)
            return;

        if (rb.position.y < minY)
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
    /// Lógica de respawn AUTORITATIVA (servidor)
    /// </summary>
    private void ServerRespawn()
    {
        rb.Sleep();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = spawnPoint.position;
        rb.rotation = spawnPoint.rotation;

        transform.SetPositionAndRotation(
            spawnPoint.position,
            spawnPoint.rotation
        );

        respawning = false;
    }

}