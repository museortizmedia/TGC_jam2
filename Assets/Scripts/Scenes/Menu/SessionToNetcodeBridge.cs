using UnityEngine;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using System.Threading.Tasks;

public class SessionToNetcodeBridge : MonoBehaviour
{
    [SerializeField] private MenuController menuController;
    [SerializeField] private string sessionType = "default-session";

    private SessionObserver sessionObserver;
    private ISession currentSession;
    private bool netcodeStarted;

    private void Awake()
    {
        Debug.Log("SessionToNetcodeBridge Awake");

        sessionObserver = new SessionObserver(sessionType);
        sessionObserver.SessionAdded += OnSessionAdded;
    }

    private void OnDestroy()
    {
        if (sessionObserver != null)
        {
            sessionObserver.SessionAdded -= OnSessionAdded;
            sessionObserver.Dispose();
        }

        UnsubscribeFromSession();
    }

    private async void OnSessionAdded(ISession session)
    {
        if (netcodeStarted)
            return;

        netcodeStarted = true;
        currentSession = session;

        // Escuchar salida / eliminación de sesión
        currentSession.RemovedFromSession += OnSessionEnded;
        currentSession.Deleted += OnSessionEnded;

        // Iniciar Netcode
        if (session.IsHost)
        {
            Debug.Log("Starting Netcode as HOST");
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            Debug.Log("Starting Netcode as CLIENT");
            NetworkManager.Singleton.StartClient();
        }

        // Voz
        if (VivoxManager.Instance != null)
        {
            await VivoxManager.Instance.JoinChannel(session.Id);
        }

        // UI
        if (menuController != null)
        {
            menuController.SetupTapsForChannel(session.Id);
        }

        Debug.Log($"Sesión iniciada: {session.Id}");
    }

    private async void OnSessionEnded()
    {
        // Salir del canal de voz
        if (VivoxManager.Instance != null && currentSession != null)
        {
            await VivoxManager.Instance.LeaveChannel(currentSession.Id);
        }

        // Detener Netcode
        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }

        // Limpieza UI
        if (menuController != null)
        {
            menuController.ResetMenuState();
        }

        UnsubscribeFromSession();

        netcodeStarted = false;
    }

    private void UnsubscribeFromSession()
    {
        if (currentSession == null)
            return;

        currentSession.RemovedFromSession -= OnSessionEnded;
        currentSession.Deleted -= OnSessionEnded;
        currentSession = null;
    }
}