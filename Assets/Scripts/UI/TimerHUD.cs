using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class TimerHUD : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Slider timerSlider;

    [Header("Settings")]
    [SerializeField] private float maxTime = 60f; // Duración en segundos

    // Variable de red sincronizada (Servidor escribe, todos leen)
    private NetworkVariable<float> remainingTimeNet = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            remainingTimeNet.Value = maxTime;
        }
    }

    void Update()
    {
        if (IsServer)
        {
            // El servidor reduce el tiempo
            if (remainingTimeNet.Value > 0)
            {
                remainingTimeNet.Value -= Time.deltaTime;
            }
        }

        // TODOS los clientes (y el servidor) actualizan la barra visual
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (timerSlider != null)
        {
            // Calculamos el porcentaje (de 1 a 0)
            timerSlider.value = remainingTimeNet.Value / maxTime;
        }
    }

    // Método público para reiniciar el timer (solo ejecutable por el servidor)
    public void ResetTimer()
    {
        if (IsServer)
        {
            remainingTimeNet.Value = maxTime;
        }
    }
}
