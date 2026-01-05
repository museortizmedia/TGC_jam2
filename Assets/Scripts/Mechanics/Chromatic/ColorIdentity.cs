using System;
using UnityEngine;
using System.Linq;

[Obsolete]
public class ColorIdentity : MonoBehaviour {
    [SerializeField] private ColorData currentColor;
    
    // Evento al que se suscriben los desarrolladores de arte/sonido
    public event Action<ColorData> OnColorChanged;

    public ColorData CurrentColor => currentColor;

    public void SetColor(ColorData newData) {
        currentColor = newData;
        OnColorChanged?.Invoke(currentColor);

        // Notificar a todos los componentes que reaccionan al color
        var affectedComponents = GetComponentsInChildren<IColorAffected>();
        foreach (var comp in affectedComponents)
        {
            //comp.CanInteractive(currentColor);
            
        }
    }
}