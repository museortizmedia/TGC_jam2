using System.Collections.Generic;
using UnityEngine;

public class PlayerColorAssigner : MonoBehaviour
{
    [SerializeField] private List<ColorData> possibleColors;

    private static HashSet<ColorData> usedColors = new HashSet<ColorData>();
    private ColorIdentity identity;

    void Awake()
    {
        identity = GetComponent<ColorIdentity>();
        AssignColor();
    }

    void AssignColor()
    {
        foreach (var color in possibleColors)
        {
            if (!usedColors.Contains(color))
            {
                usedColors.Add(color);
                identity.SetColor(color);
                return;
            }
        }

        Debug.LogWarning("No hay colores disponibles para asignar");
    }
}