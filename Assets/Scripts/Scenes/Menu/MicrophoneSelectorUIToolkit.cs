using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Unity.Services.Vivox;
using System.Threading.Tasks;

public class MicrophoneSelectorUIToolkit : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private UIDocument uiDocument;

    // --- Referencias UI ---
    private DropdownField micDropdown;
    private Slider musicSlider;

    // --- Constantes Audio ---
    private const string MusicSliderName = "SliderMusic"; 
    private const string MusicPrefKey = "MusicVolume";    
    private const float DefaultVolume = 0.5f;             

    // --- Variables Vivox ---
    private readonly List<VivoxInputDevice> inputDevices = new();
    private const string MicPlayerPrefKey = "SelectedMicrophoneIndex";

    async void OnEnable()
    {
        var root = uiDocument.rootVisualElement;

        // =========================================================
        // 1. LÓGICA DE VOLUMEN (Sincronizada al 50% o Guardado)
        // =========================================================
        musicSlider = root.Q<Slider>(MusicSliderName);

        if (musicSlider != null)
        {
            // A. Cargar el valor guardado. Si no existe, usa 0.5f (50%)
            float currentVolume = PlayerPrefs.GetFloat(MusicPrefKey, DefaultVolume);

            // B. Sincronizar Audio del Juego
            AudioListener.volume = currentVolume;

            // C. Sincronizar Slider Visual
            musicSlider.value = currentVolume;

            // D. Registrar cambios (cuando el jugador mueva la rueda)
            musicSlider.RegisterValueChangedCallback(evt =>
            {
                float newVolume = evt.newValue;

                // Actualizar volumen global
                AudioListener.volume = newVolume;

                // Guardar preferencia
                PlayerPrefs.SetFloat(MusicPrefKey, newVolume);
                PlayerPrefs.Save();
            });

            Debug.Log($"[Audio] Volumen inicializado al: {currentVolume * 100}%");
        }
        else
        {
            Debug.LogWarning($"[MicrophoneSelector] No se encontró el Slider '{MusicSliderName}'. Revisa el UI Builder.");
        }

        // =========================================================
        // 2. LÓGICA DE MICRÓFONO (Vivox)
        // =========================================================
        micDropdown = root.Q<DropdownField>("mic-dropdown");

        if (micDropdown == null)
        {
            Debug.LogError("DropdownField 'mic-dropdown' no encontrado en UXML");
            return;
        }

        micDropdown.SetEnabled(false);

        // Asegurar Vivox listo
        if (!VivoxManager.Instance.IsLoggedIn)
        {
            // Debug.Log("Esperando inicialización de Vivox...");
            await VivoxManager.Instance.InitializeVivoxAsync();
        }

        RefreshDevices();

        micDropdown.RegisterValueChangedCallback(OnMicSelected);

        RestoreSelection();

        micDropdown.SetEnabled(true);
    }

    // ---------------------------------------------------------
    // MÉTODOS DE VIVOX (Sin cambios)
    // ---------------------------------------------------------

    void RefreshDevices()
    {
        inputDevices.Clear();

        foreach (var device in VivoxService.Instance.AvailableInputDevices)
        {
            inputDevices.Add(device);
        }

        var labels = new List<string>();
        foreach (var dev in inputDevices)
        {
            labels.Add(dev.DeviceName);
        }

        micDropdown.choices = labels;
    }

    async void RestoreSelection()
    {
        if (inputDevices.Count == 0)
            return;

        int savedIndex = PlayerPrefs.GetInt(MicPlayerPrefKey, 0);
        savedIndex = Mathf.Clamp(savedIndex, 0, inputDevices.Count - 1);

        micDropdown.index = savedIndex;

        await inputDevices[savedIndex].SetActiveDeviceAsync();

        // Debug.Log($"Micrófono restaurado: {inputDevices[savedIndex].DeviceName}");
    }

    async void OnMicSelected(ChangeEvent<string> evt)
    {
        int index = micDropdown.index;
        if (index < 0 || index >= inputDevices.Count)
            return;

        var device = inputDevices[index];

        Debug.Log($"Seleccionando micrófono: {device.DeviceName}");

        await device.SetActiveDeviceAsync();

        PlayerPrefs.SetInt(MicPlayerPrefKey, index);
        PlayerPrefs.Save();
    }
}