using UnityEngine;
using Unity.Services.Vivox;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Events;

public class VivoxManager : MonoBehaviour
{
    public static VivoxManager Instance { get; private set; }

    public bool IsLoggedIn => VivoxService.Instance.IsLoggedIn;

    private HashSet<string> joinedChannels = new HashSet<string>();
    public UnityEvent OnInitVivox;


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

    public void StartVivox() => _ = InitializeVivoxAsync();

    public async Task InitializeVivoxAsync()
    {
        if (!UnityServices.State.Equals(ServicesInitializationState.Initialized))
            await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        if (!IsLoggedIn)
        {
            await VivoxService.Instance.LoginAsync();
            Debug.Log("Vivox logueado");
            OnInitVivox?.Invoke();
        }
    }

    public async Task JoinChannel(string channelName)
    {
        if (!IsLoggedIn)
        {
            Debug.LogWarning("Vivox no logueado, no se puede unir al canal.");
            return;
        }

        if (joinedChannels.Contains(channelName))
        {
            Debug.Log($"Ya conectado al canal: {channelName}, no se hace nada.");
            return;
        }

        await VivoxService.Instance.JoinGroupChannelAsync(
            channelName,
            ChatCapability.AudioOnly,
            new ChannelOptions { MakeActiveChannelUponJoining = true }
        );

        joinedChannels.Add(channelName);
        Debug.Log($"Unido al canal: {channelName}");
    }

    public async Task LeaveChannel(string channelName)
    {
        if (!IsLoggedIn || !joinedChannels.Contains(channelName)) return;

        await VivoxService.Instance.LeaveChannelAsync(channelName);
        joinedChannels.Remove(channelName);
        Debug.Log($"Salido del canal: {channelName}");
    }
}
