using UnityEngine;
using Unity.Services.Vivox.AudioTaps;

public class MenuController : MonoBehaviour
{
    [Header("Vivox Taps")]
    [SerializeField] private VivoxChannelAudioTap vivoxChannelAudioTap;

    /// <summary>
    /// Configura los taps para un canal ya conectado en VivoxManager.
    /// No realiza la unión al canal, solo asigna el ChannelName y activa los taps.
    /// </summary>
    /// <param name="channelName">ID de la sesión o canal.</param>
    public void SetupTapsForChannel(string channelName)
    {
        vivoxChannelAudioTap.ChannelName = channelName;

        Debug.Log($"Taps configurados para canal: {channelName}");
    }
}
