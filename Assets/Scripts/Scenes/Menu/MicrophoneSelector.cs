using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Services.Vivox;
using System.Threading.Tasks;
using TMPro;

public class MicrophoneSelector : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;

    private List<VivoxInputDevice> inputDevices = new List<VivoxInputDevice>();
    private const string PlayerPrefKey = "SelectedMicrophoneIndex";

    private async void Start()
    {
        // Asegúrate de que Vivox esté inicializado y logueado antes.
        if (!VivoxManager.Instance.IsLoggedIn)
        {
            Debug.Log("Esperando a que Vivox inicie sesión...");
            await VivoxManager.Instance.InitializeVivoxAsync();
        }

        // Llenar micrófonos disponibles
        RefreshDevices();

        // Suscribirse a cambios de selección en el dropdown
        dropdown.onValueChanged.AddListener(OnMicSelected);

        // Cargar última selección guardada
        int savedIndex = PlayerPrefs.GetInt(PlayerPrefKey, 0);
        if (savedIndex >= 0 && savedIndex < inputDevices.Count)
        {
            dropdown.value = savedIndex;
            await inputDevices[savedIndex].SetActiveDeviceAsync();
            Debug.Log($"Micrófono restaurado: {inputDevices[savedIndex].DeviceName}");
        }
    }

    private void RefreshDevices()
    {
        inputDevices.Clear();

        foreach (var device in VivoxService.Instance.AvailableInputDevices)
        {
            inputDevices.Add(device);
        }

        // Llenar dropdown con los nombres de los dispositivos
        List<string> labels = new List<string>();
        foreach (var dev in inputDevices)
        {
            labels.Add(dev.DeviceName);
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(labels);

        // Seleccionar primer dispositivo por defecto si no hay guardado
        if (inputDevices.Count > 0 && !PlayerPrefs.HasKey(PlayerPrefKey))
        {
            _ = inputDevices[0].SetActiveDeviceAsync();
            dropdown.value = 0;
        }
    }

    private async void OnMicSelected(int index)
    {
        if (index < 0 || index >= inputDevices.Count) return;

        var device = inputDevices[index];

        Debug.Log($"Seleccionando micrófono: {device.DeviceName}");

        await device.SetActiveDeviceAsync();

        // Guardar la selección en PlayerPrefs
        PlayerPrefs.SetInt(PlayerPrefKey, index);
        PlayerPrefs.Save();

        Debug.Log("Micrófono activo cambiado a: " + device.DeviceName);
    }
}