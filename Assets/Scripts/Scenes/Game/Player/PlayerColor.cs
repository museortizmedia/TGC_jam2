using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerColor : NetworkBehaviour
{
    [Header("Shader Property Names")]
    [SerializeField] private string emitColorProperty = "_EmitColor";
    [SerializeField] private string emitIntensityProperty = "_EmitIntensity";

    [SerializeField] private GameObject maleMesh, femaleMesh;

    [SerializeField] Renderer[] renderers;
    private MaterialPropertyBlock propertyBlock;

    public NetworkVariable<ColorDataNet> _color =
        new NetworkVariable<ColorDataNet>(
            new() { r = 1f, g = 1f, b = 1f, a = 1f, intensity = 1f },
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );
    [SerializeField] string currenColorName;
    [SerializeField] ColorData currentColor;
    [SerializeField] ColorData[] allColors;

    private void Awake()
    {
        renderers ??= GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
    }

    public override void OnNetworkSpawn()
    {
        // Escuchar cambios sincronizados
        _color.OnValueChanged += OnColorChanged;
    }

    public override void OnNetworkDespawn()
    {
        _color.OnValueChanged -= OnColorChanged;
    }

    private void OnColorChanged(ColorDataNet oldColor, ColorDataNet newColor)
    {
        // Buscamos el color
        currentColor =
        allColors.FirstOrDefault(c => c.colorId == newColor.colorId);
        Debug.Log(currentColor == null ? $"ColorData con id '{currentColor.colorId}' no encontrado en catálogo." : $"ColorData con id '{currentColor.colorId}' resuelto correctamente.", transform);

        if (currentColor == null) { Debug.LogError($"ColorData con id '{newColor.colorId}' no encontrado en catálogo.", transform); return; }

        // Podemos hacer una espera o solo almacenar la info para lanzarla mas adelante
        ApplyColor(ColorDataMapper.ToUnityColor(newColor), ColorDataMapper.ToUnityIntensity(newColor));
        currenColorName = ColorDataMapper.ToUnityColorId(newColor);
    }

    /// <summary>
    /// Aplica color e intensidad de emisión.
    /// </summary>
    public void ApplyColor(Color color, float intensity = 1f)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer rend = renderers[i];

            rend.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(emitColorProperty, color);
            propertyBlock.SetFloat(emitIntensityProperty, intensity);
            rend.SetPropertyBlock(propertyBlock);

            if (currentColor.meshIndex == 0)
            {
                maleMesh.SetActive(true);
                femaleMesh.SetActive(false);
            }
            else
            {
                maleMesh.SetActive(false);
                femaleMesh.SetActive(true);
            }
        }
    }
}
