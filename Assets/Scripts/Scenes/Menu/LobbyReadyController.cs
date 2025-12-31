using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections;
using System.Collections.Generic;

public class LobbyReadyController : NetworkBehaviour
{
    [Header("Lobby Settings")]
    [SerializeField] private int maxPlayers = 4;
    [SerializeField] private int countdownSeconds = 5;

    // =========================
    // EVENTS (UI)
    // =========================
    public event Action<bool> OnLobbyFullChanged;
    public event Action<string> OnStatusTextChanged;

    // =========================
    // NETWORK STATE
    // =========================
    private NetworkVariable<int> connectedPlayers =
        new(0, NetworkVariableReadPermission.Everyone,
               NetworkVariableWritePermission.Server);

    private NetworkVariable<int> readyCount =
        new(0, NetworkVariableReadPermission.Everyone,
               NetworkVariableWritePermission.Server);

    private NetworkVariable<int> countdownNet =
        new(-1, NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

    // =========================
    // INTERNAL STATE (SERVER)
    // =========================
    private HashSet<ulong> readyPlayers = new HashSet<ulong>();
    private bool gameStarted;
    private Coroutine countdownRoutine;

    // =========================
    // NETWORK LIFECYCLE
    // =========================
    public override void OnNetworkSpawn()
    {
        connectedPlayers.OnValueChanged += (_, __) => EmitAllState();
        readyCount.OnValueChanged += (_, __) => EmitAllState();
        countdownNet.OnValueChanged += (_, __) => EmitStatus();

        EmitAllState();

        if (!IsServer)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientChanged;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientChanged;

        RecalculateConnectedPlayers();
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer || NetworkManager.Singleton == null)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientChanged;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientChanged;
    }

    // =========================
    // SERVER LOGIC
    // =========================
    private void OnClientChanged(ulong clientId)
    {
        if (!IsServer)
            return;

        readyPlayers.Remove(clientId);
        RecalculateConnectedPlayers();
        readyCount.Value = readyPlayers.Count;

        TryStartGame();
    }

    private void RecalculateConnectedPlayers()
    {
        connectedPlayers.Value = NetworkManager.Singleton.ConnectedClients.Count;
    }

    private void RegisterReady(ulong clientId)
    {
        if (gameStarted || readyPlayers.Contains(clientId))
            return;

        readyPlayers.Add(clientId);
        readyCount.Value = readyPlayers.Count;

        TryStartGame();
    }

    private void TryStartGame()
    {
        bool lobbyFull = connectedPlayers.Value == maxPlayers;
        bool allReady = readyCount.Value == connectedPlayers.Value && lobbyFull;

        if (allReady && countdownRoutine == null)
        {
            countdownRoutine = StartCoroutine(StartCountdown());
        }
    }

    // =========================
    // PUBLIC API (UI)
    // =========================
    public void SetPlayerReady()
    {
        if (IsServer)
            RegisterReady(OwnerClientId);
        else
            SetPlayerReadyServerRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SetPlayerReadyServerRpc(RpcParams rpcParams = default)
    {
        RegisterReady(rpcParams.Receive.SenderClientId);
    }

    public bool IsLobbyFull =>
        connectedPlayers.Value == maxPlayers;

    public string StatusText
    {
        get
        {
            if (countdownNet.Value >= 0)
                return $"Starting in {countdownNet.Value}...";

            if (!IsLobbyFull)
                return $"Waiting players ({connectedPlayers.Value}/{maxPlayers})";

            return $"Ready {readyCount.Value}/{maxPlayers}";
        }
    }

    // =========================
    // GAME START
    // =========================
    private IEnumerator StartCountdown()
    {
        for (int i = countdownSeconds; i > 0; i--)
        {
            countdownNet.Value = i;
            yield return new WaitForSeconds(1f);
        }

        gameStarted = true;
        countdownNet.Value = -1;

        SessionManager.Instance.ChangeState(
            SessionManager.SessionState.Game);
    }

    // =========================
    // UI EMISSION
    // =========================
    private void EmitAllState()
    {
        OnLobbyFullChanged?.Invoke(IsLobbyFull);
        EmitStatus();
    }

    private void EmitStatus()
    {
        OnStatusTextChanged?.Invoke(StatusText);
    }
}