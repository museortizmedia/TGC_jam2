using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class PlayerColor : NetworkBehaviour
{
    [Header("Color State")]
    [SerializeField] private GameObject objectToDisableWhenColored;

    public bool DEBUG_COLOR_COLLIDER;
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
    public string currenColorName;
    public ColorData currentColor;
    public ColorData CurrentColor => currentColor;
    [SerializeField] ColorData[] allColors;

    public UnityEvent<Color> OnStarPlayerColor;


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
        currentColor = allColors.FirstOrDefault(c => c.colorId == newColor.colorId);
        Debug.Log(currentColor == null ? $"ColorData con id '{currentColor.colorId}' no encontrado en catálogo." : $"ColorData con id '{currentColor.colorId}' resuelto correctamente.", transform);

        if (currentColor == null) { Debug.LogError($"ColorData con id '{newColor.colorId}' no encontrado en catálogo.", transform); return; }

        // Podemos hacer una espera o solo almacenar la info para lanzarla mas adelante
        currenColorName = ColorDataMapper.ToUnityColorId(newColor);

        Invoke(nameof(StartPlayerColor), 5f);
    }

    [ContextMenu("Start Player Color")]
    public void StartPlayerColor()
    {
        if (currentColor == null) return;

        ApplyColor(currentColor.color, currentColor.intensity);

        if (objectToDisableWhenColored != null)
            objectToDisableWhenColored.SetActive(false);

        OnStarPlayerColor?.Invoke(currentColor.color);
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

    void OnCollisionEnter(Collision collision)
    {
        GameObject target = collision.gameObject;

        ObjectColored objColored = null;

        // 1. Buscar en el objeto mismo
        objColored = target.GetComponent<ObjectColored>();

        // 2. Si no está, buscar en los hijos
        if (objColored == null)
        {
            objColored = target.GetComponentInChildren<ObjectColored>();
        }

        // 3. Si aún no se encuentra, buscar en el padre
        if (objColored == null && target.transform.parent != null)
        {
            objColored = target.transform.parent.GetComponent<ObjectColored>();
        }

        // 4. Si el padre se llama "Model", buscar en su padre
        if (objColored == null && target.transform.parent != null &&
            target.transform.parent.name == "Model" && target.transform.parent.parent != null)
        {
            objColored = target.transform.parent.parent.GetComponent<ObjectColored>();
        }

        // 5. Si no se encontró nada, no es un objeto de color
        if (objColored == null)
        {
            if (DEBUG_COLOR_COLLIDER)
                Debug.Log($"{target.name} no es un objeto de color", target.transform);
            return;
        }

        if (DEBUG_COLOR_COLLIDER)
            Debug.Log($"Se encontró ObjectColored: {objColored.name}", objColored.transform);

        // 6. Comprobar color (Si no es el color = muerte)
        if (!objColored.IsThisColor(currentColor))
        {
            if (DEBUG_COLOR_COLLIDER)
                Debug.Log($"{objColored.name} no puede interactuar con el color {currenColorName}", transform);

            // 7. Ejecutar Dead si implementa IDeadly
            if (objColored.TryGetComponent<IDeadly>(out var deadly))
            {
                deadly.Dead();
            }

            // Ejecutar el Dead propio
            if(gameObject.TryGetComponent(out PlayerDead playerDead))
            {
                playerDead.Dead();
                 Debug.LogWarning("MUERE PLAYER y lo mato: ", objColored.transform);
            }
        }
        else
        {
            if (DEBUG_COLOR_COLLIDER)
                Debug.Log($"{objColored.name} es del color correcto", objColored.transform);
        }
    }


}
