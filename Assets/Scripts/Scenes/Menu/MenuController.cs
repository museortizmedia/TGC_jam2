using UnityEngine;
using Unity.Services.Vivox.AudioTaps;
using System;
using MuseOrtizLibrary;

public class MenuController : MonoBehaviour
{
    [Header("Vivox Taps")]
    [SerializeField] private VivoxChannelAudioTap vivoxChannelAudioTap;

    public ScriptableAudioClip MenuSound;

    void Start()
    {
        MenuSound.PlayAudio();
    }

    void OnDisable()
    {
        MenuSound.StopAudio();
    }

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

    internal void ResetMenuState()
    {
        //throw new NotImplementedException();
    }
}
