using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public class PlayerFall : NetworkBehaviour
{
    [SerializeField] private float minY = -50f;
    public Transform spawnPoint;

    private Rigidbody rb;
    private bool respawning;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!IsServer || respawning)
            return;

        if (transform.position.y < minY)
        {
            respawning = true;
            HandleRespawn();
        }
    }

    private void HandleRespawn()
    {
        // Pedir transición SOLO para este jugador
        SessionManager.Instance.PlayLocalRespawnTransition(OwnerClientId);

        // Reset físico
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = spawnPoint.position;
        rb.rotation = spawnPoint.rotation;
        gameObject.transform.position = spawnPoint.position;
        gameObject.transform.rotation = spawnPoint.rotation;

        respawning = false;
    }
}