using UnityEngine;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using System.Threading.Tasks;

public class SessionToNetcodeBridge : MonoBehaviour
{
    [SerializeField] private MenuController menuController;
    [SerializeField] private string sessionType = "default";

    private SessionObserver sessionObserver;
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
    }

    private async void OnSessionAdded(ISession session)
    {
        if (netcodeStarted) return;
        netcodeStarted = true;

        // Inicia Netcode
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

        string sessionId = session.Id;

        // VivoxManager se encarga de unir al canal de sesión
        if (VivoxManager.Instance != null)
        {
            await VivoxManager.Instance.JoinChannel(sessionId);
        }

        // Configurar los taps para el canal de sesión
        if (menuController != null)
        {
            menuController.SetupTapsForChannel(sessionId);
        }

        Debug.Log($"Sesión iniciada y taps configurados para canal: {sessionId}");
    }
}