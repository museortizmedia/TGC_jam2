using System;
using System.Collections;
using MuseOrtizLibrary;
using Unity.Netcode;
using UnityEngine;

public class SessionManager : NetworkBehaviour
{
    private static WaitForSeconds _waitForSeconds1 = new WaitForSeconds(1f);

    public static SessionManager Instance { get; private set; }

    [Header("References")]
    public ScreenTransitionManager screenTransitionManager;

    [Header("Audios")]
    public ScriptableAudioClip MenuMusic;
    public ScriptableAudioClip GameMusic, EndMusic;
    public ScriptableAudioClip[] SfxAudios;

    public Action<SessionState> OnSessionStateChanged;

    public enum SessionState
    {
        Lobby,
        Game,
        End
    }

    public SessionState CurrentState = SessionState.Lobby;

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
                Time.timeScale = 1;
                LoadNetworkScene("MenuScene");
                break;

            case SessionState.Game:
                LoadNetworkScene("GameScene");
                MenuMusic.StopAudio();
                GameMusic.PlayAudio();
                Cursor.lockState = CursorLockMode.Locked;
                break;

            case SessionState.End:
                Time.timeScale = 0;
                GameMusic.StopAudio();
                EndMusic.PlayAudio();
                Cursor.lockState = CursorLockMode.None;
                break;
        }

        OnSessionStateChanged?.Invoke(newState);
    }

    private void LoadNetworkScene(string sceneName)
    {
        // Transición GLOBAL para todos los clientes
        ScreenTransitionAllClientRpc(sceneName);
    }

    #endregion

    #region SCREEN TRANSITIONS (GLOBAL)

    [ClientRpc]
    private void ScreenTransitionAllClientRpc(string sceneName)
    {
        if (screenTransitionManager == null)
        {
            LoadScene(sceneName);
            return;
        }

        screenTransitionManager.PlayTransition(() =>
        {
            if (IsServer)
            {
                LoadScene(sceneName);
            }
        });
    }


    private void LoadScene(string sceneName)
    {
        // SOLO el servidor carga la escena
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(
                sceneName,
                UnityEngine.SceneManagement.LoadSceneMode.Single
            );
        }
    }

    #endregion

    #region SCREEN TRANSITIONS (LOCAL / RESPAWN)

    public void PlayLocalRespawnTransition(
        ulong targetClientId,
        Action serverRespawnAction
    )
    {
        if (!IsServer)
            return;

        // 1. Fade IN solo para el cliente
        FadeInClientRpc(targetClientId);

        // 2. Ejecutar lógica de servidor bajo negro
        serverRespawnAction?.Invoke();

        // 3. Fade OUT tras un pequeño delay
        StartCoroutine(FadeOutDelayed(targetClientId));
    }

    private IEnumerator FadeOutDelayed(ulong clientId)
    {
        yield return _waitForSeconds1;
        FadeOutClientRpc(clientId);
    }

    [ClientRpc]
    private void FadeInClientRpc(ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId)
            return;

        screenTransitionManager?.FadeIn();
    }

    [ClientRpc]
    private void FadeOutClientRpc(ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId)
            return;

        screenTransitionManager?.FadeOut();
    }

    #endregion

}