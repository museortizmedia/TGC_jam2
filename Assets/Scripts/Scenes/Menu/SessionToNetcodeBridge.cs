using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Multiplayer;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System.Threading.Tasks;

public class SessionToNetcodeBridge : MonoBehaviour
{
    [Header("Relay")]
    public bool UseRelay = true;

    [Header("UI")]
    [SerializeField] private MenuController menuController;
    [SerializeField] private string sessionType = "default-session";

    private SessionObserver sessionObserver;
    private ISession currentSession;
    private bool netcodeStarted;

    // Join Code compartido por el flujo de menú
    public static string RelayJoinCode;

    private void Awake()
    {
        Debug.Log("[SessionToNetcodeBridge] Awake");

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

        currentSession.RemovedFromSession += OnSessionEnded;
        currentSession.Deleted += OnSessionEnded;

        if (session.IsHost)
        {
            Debug.Log("[Netcode] Starting as HOST");

            if (UseRelay)
                await StartHostWithRelay();
            else
                NetworkManager.Singleton.StartHost();
        }
        else
        {
            Debug.Log("[Netcode] Starting as CLIENT");

            if (UseRelay)
                await StartClientWithRelay();
            else
                NetworkManager.Singleton.StartClient();
        }

        if (VivoxManager.Instance != null)
            await VivoxManager.Instance.JoinChannel(session.Id);

        if (menuController != null)
            menuController.SetupTapsForChannel(session.Id);
    }

    private async void OnSessionEnded()
    {
        if (VivoxManager.Instance != null && currentSession != null)
            await VivoxManager.Instance.LeaveChannel(currentSession.Id);

        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }

        if (menuController != null)
            menuController.ResetMenuState();

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

    #region RELAY (LEGACY API)

    private async Task StartHostWithRelay()
    {
        Allocation allocation =
            await RelayService.Instance.CreateAllocationAsync(3);

        RelayJoinCode =
            await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        Debug.Log("[Relay][HOST] Join Code: " + RelayJoinCode);

        UnityTransport transport =
            NetworkManager.Singleton.GetComponent<UnityTransport>();

        transport.SetHostRelayData(
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData
        );

        NetworkManager.Singleton.StartHost();
    }

    private async Task StartClientWithRelay()
    {
        if (string.IsNullOrEmpty(RelayJoinCode))
        {
            Debug.LogError("[Relay][CLIENT] Join Code no asignado");
            return;
        }

        JoinAllocation joinAllocation =
            await RelayService.Instance.JoinAllocationAsync(RelayJoinCode);

        UnityTransport transport =
            NetworkManager.Singleton.GetComponent<UnityTransport>();

        transport.SetClientRelayData(
            joinAllocation.RelayServer.IpV4,
            (ushort)joinAllocation.RelayServer.Port,
            joinAllocation.AllocationIdBytes,
            joinAllocation.Key,
            joinAllocation.ConnectionData,
            joinAllocation.HostConnectionData
        );

        NetworkManager.Singleton.StartClient();
    }

    #endregion
}