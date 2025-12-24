using UnityEngine;

[CreateAssetMenu(fileName = "NewColor", menuName = "Oniric/ColorData")]
public class ColorData : ScriptableObject {
    public string colorName;
    public Color value;
    public Material ghostMaterial;
}