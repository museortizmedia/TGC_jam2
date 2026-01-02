using System;
using Unity.Netcode;
using UnityEngine;

public class SessionManager : NetworkBehaviour
{
    public static SessionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public ScreenTransitionManager screenTransitionManager;
    public Action<SessionState> OnSessionStateChanged;

    public enum SessionState
    {
        Lobby,
        Game,
        End
    }

    public SessionState CurrentState = SessionState.Lobby;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            ChangeState(SessionState.Lobby);
        }
    }

    #region SESSION STATE

    public void ChangeState(SessionState newState)
    {
        if (!IsServer)
        {
            Debug.LogWarning("Solo el servidor puede cambiar el estado de la sesión.");
            return;
        }

        if (CurrentState == newState)
            return;

        CurrentState = newState;
        OnStateChanged(newState);
    }

    private void OnStateChanged(SessionState newState)
    {
        switch (newState)
        {
            case SessionState.Lobby:
                LoadNetworkScene("MenuScene");
                break;

            case SessionState.Game:
                LoadNetworkScene("GameScene");
                break;

            case SessionState.End:
                break;
        }

        OnSessionStateChanged?.Invoke(newState);
    }

    private void LoadNetworkScene(string sceneName)
    {
        // Transición PARA TODOS
        ScreenTransitionAllClientRpc(sceneName);
    }

    #endregion

    #region SCREEN TRANSITIONS

    /// <summary>
    /// Transición visible para TODOS los jugadores (cambio de escena)
    /// </summary>
    [ClientRpc]
    private void ScreenTransitionAllClientRpc(string sceneName)
    {
        if (screenTransitionManager == null)
        {
            LoadScene(sceneName);
            return;
        }

        screenTransitionManager.Transition(() =>
        {
            LoadScene(sceneName);
        });
    }

    private void LoadScene(string sceneName)
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(
                sceneName,
                UnityEngine.SceneManagement.LoadSceneMode.Single
            );
        }
    }

    /// <summary>
    /// Transición SOLO para un jugador (respawn, muerte, etc.)
    /// </summary>
    public void PlayLocalRespawnTransition(ulong targetClientId)
    {
        if (!IsServer)
            return;

        ScreenTransitionLocalClientRpc(targetClientId);
    }

    [ClientRpc]
    private void ScreenTransitionLocalClientRpc(ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId)
            return;

        if (screenTransitionManager != null)
        {
            screenTransitionManager.Transition(null);
        }
    }

    #endregion
}