using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleModule : MonoBehaviour
{
    // Evento que entrega el color como string
    public UnityEvent<string> OnInitPuzzle;
    public ObjectColored[] ColorRouteObjects;
    public ObjectColored[] OtherColorRouteObjects;

    private bool initialized;

    // Llamado por WorldBuilder o ClientRpc
    public void Initialize(string color)
    {
        if (initialized) return;

        initialized = true;

        // Dispara el evento para que los suscriptores pinten o reaccionen
        OnInitPuzzle?.Invoke(color);
        ColorObjects(color.ToLower());

        if(color!="blanco") Debug.Log($"[PuzzleModule] Inicializado con color: {color}", transform);
    }

    // Permite reinicializar si se reconstruye el mundo
    public void ResetModule()
    {
        initialized = false;
    }
    [ContextMenu("Colorear Objects")]
    void ColorObjects(string color)
    {
        if (ColorRouteObjects == null) return;

        if (OtherColorRouteObjects == null) return;

        foreach (var objectColorRoute in ColorRouteObjects)
        {
            objectColorRoute.ApplyColorInObject(color);
        }
        foreach (var objectColorNoRoute in OtherColorRouteObjects)
        {
            objectColorNoRoute.ApplyColorInObject(color);
        }
    }
}