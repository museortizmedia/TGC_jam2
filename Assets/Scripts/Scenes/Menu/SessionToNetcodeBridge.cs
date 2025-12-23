using UnityEngine;
using Unity.Netcode;
using Unity.Services.Multiplayer;

public class SessionToNetcodeBridge : MonoBehaviour
{
    [SerializeField] string sessionType = "default";

    SessionObserver sessionObserver;
    bool netcodeStarted;

    void Awake()
    {
        Debug.Log("SessionToNetcodeBridge Awake");
        sessionObserver = new SessionObserver(sessionType);
        sessionObserver.SessionAdded += OnSessionAdded;
    }

    void OnDestroy()
    {
        if (sessionObserver != null)
        {
            sessionObserver.SessionAdded -= OnSessionAdded;
            sessionObserver.Dispose();
        }
    }

    void OnSessionAdded(ISession session)
    {
        if (netcodeStarted) return;
        netcodeStarted = true;

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
    }
}