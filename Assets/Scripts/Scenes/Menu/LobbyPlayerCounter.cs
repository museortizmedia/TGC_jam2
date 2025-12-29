using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LobbyPlayerCounter : NetworkBehaviour
{
    [SerializeField] int maxPlayers = 4;
    bool gameStarted;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        CheckPlayerCount();
    }

    public override void OnNetworkDespawn()
    {
        if (!NetworkManager.Singleton) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    void OnClientConnected(ulong clientId)
    {
        CheckPlayerCount();
    }

    void OnClientDisconnected(ulong clientId)
    {
        CheckPlayerCount();
    }

    [ContextMenu("Check Player Count")]
    void CheckPlayerCount()
    {
        if (gameStarted) return;

        int connectedPlayers = NetworkManager.Singleton.ConnectedClients.Count;

        Debug.Log($"Players connected: {connectedPlayers}/{maxPlayers}");

        if (connectedPlayers >= maxPlayers)
        {
            gameStarted = true;

            SessionManager.Instance.ChangeState(SessionManager.SessionState.Game);
        }
    }
}