using System;
using UnityEngine;

public class ColorIdentity : MonoBehaviour {
    [SerializeField] private ColorData currentColor;
    
    // Evento al que se suscriben los desarrolladores de arte/sonido
    public event Action<ColorData> OnColorChanged;

    public ColorData CurrentColor => currentColor;

    public void SetColor(ColorData newData) {
        currentColor = newData;
        OnColorChanged?.Invoke(currentColor);
    }
}