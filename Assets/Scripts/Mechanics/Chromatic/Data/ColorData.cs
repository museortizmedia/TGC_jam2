using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "NewColor", menuName = "BETWEEN/ColorData")]
public class ColorData : ScriptableObject
{
    public string colorId;
    public Color color;
    public float intensity;
    public int meshIndex;

    // No enviados por red
    public Material ghostMaterial;
}