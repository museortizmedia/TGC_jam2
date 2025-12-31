using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Unity.Services.Vivox;
using System.Threading.Tasks;

public class MicrophoneSelectorUIToolkit : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    DropdownField micDropdown;

    readonly List<VivoxInputDevice> inputDevices = new();
    const string PlayerPrefKey = "SelectedMicrophoneIndex";

    async void OnEnable()
    {
        var root = uiDocument.rootVisualElement;
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
            Debug.Log("Esperando inicialización de Vivox...");
            await VivoxManager.Instance.InitializeVivoxAsync();
        }

        RefreshDevices();

        micDropdown.RegisterValueChangedCallback(OnMicSelected);

        RestoreSelection();

        micDropdown.SetEnabled(true);
    }

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

        int savedIndex = PlayerPrefs.GetInt(PlayerPrefKey, 0);
        savedIndex = Mathf.Clamp(savedIndex, 0, inputDevices.Count - 1);

        micDropdown.index = savedIndex;

        await inputDevices[savedIndex].SetActiveDeviceAsync();

        Debug.Log($"Micrófono restaurado: {inputDevices[savedIndex].DeviceName}");
    }

    async void OnMicSelected(ChangeEvent<string> evt)
    {
        int index = micDropdown.index;
        if (index < 0 || index >= inputDevices.Count)
            return;

        var device = inputDevices[index];

        Debug.Log($"Seleccionando micrófono: {device.DeviceName}");

        await device.SetActiveDeviceAsync();

        PlayerPrefs.SetInt(PlayerPrefKey, index);
        PlayerPrefs.Save();
    }
}