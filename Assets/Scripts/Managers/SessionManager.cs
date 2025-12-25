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

    [SerializeField] ScreenTransitionManager screenTransitionManager;
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
                //
                break;
        }

        OnSessionStateChanged?.Invoke(newState);
    }

    private void LoadNetworkScene(string sceneName)
    {
        if (screenTransitionManager != null)
        {
            screenTransitionManager.Transition(() =>
            {
                NetworkManager.Singleton.SceneManager.LoadScene(
                    sceneName,
                    UnityEngine.SceneManagement.LoadSceneMode.Single
                );
            });
        }
        else
        {
            NetworkManager.Singleton.SceneManager.LoadScene(
                sceneName,
                UnityEngine.SceneManagement.LoadSceneMode.Single
            );
        }
    }

}