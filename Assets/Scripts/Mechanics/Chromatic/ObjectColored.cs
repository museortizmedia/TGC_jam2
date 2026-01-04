using System.Linq;
using UnityEngine;
using Unity.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ObjectColored : MonoBehaviour, IColorAffected
{
    [Header("Shader Property Names")]
    [SerializeField] private string emitColorProperty = "_EmitColor";
    [SerializeField] private string emitIntensityProperty = "_EmitIntensity";
    [SerializeField] Renderer render;

    [Header("Read Only")]
    public bool IsColored;
    [SerializeField] ColorData currentColor;
    [SerializeField] ColorData[] allColors;
    [SerializeField] Material writtingMaterial;

    private MaterialPropertyBlock propertyBlock;


#if UNITY_EDITOR
    private void OnValidate()
    {
        string[] requiredNames =
        {
        "Amarillo",
        "Azul",
        "Verde",
        "Rojo"
        };

        var guids = AssetDatabase.FindAssets("t:ColorData");
        var allColorAssets = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<ColorData>(
                AssetDatabase.GUIDToAssetPath(guid)))
            .ToList();

        allColors = requiredNames
            .Select(name => allColorAssets.FirstOrDefault(c => c.name == name))
            .Where(c => c != null)
            .ToArray();

        writtingMaterial = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath("91b693067dde48240b7dd7d61301e743"));
        if(render != null && render.sharedMaterial != writtingMaterial) { render.sharedMaterial = writtingMaterial; }
    }
#endif

    public string GetOtherColorThan(string color)
    {
        return allColors.FirstOrDefault(c => c.colorId != color).colorId;
    }

    public void ApplyColorInObject(FixedString32Bytes colorName)
    {
        currentColor = null;
        currentColor = allColors.FirstOrDefault(c => c.colorId == colorName);
        if (currentColor != null)
        {
            ApplyColorInObject();
        }
    }

    public void ApplyColorInObject(ColorDataNet colorDataNet)
    {
        currentColor = null;
        currentColor = allColors.FirstOrDefault(c => c.colorId == colorDataNet.colorId);
        if (currentColor != null)
        {
            ApplyColorInObject();
        }
    }

    public void ApplyColorInObject(ColorData colorData)
    {
        currentColor = null;
        currentColor = colorData;
        if (currentColor != null)
        {
            ApplyColorInObject();
        }
    }
    [ContextMenu("Apply Current Color")]
    public void ApplyColorInObject()
    {
        if (currentColor != null)
        {
            propertyBlock ??= new();
            render.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(emitColorProperty, currentColor.color);
            propertyBlock.SetFloat(emitIntensityProperty, currentColor.intensity);
            render.SetPropertyBlock(propertyBlock);
            IsColored = true;
        }
    }

    public bool CanInteractive(ColorData colorData)
    {
        return currentColor == null ? true : colorData.colorId != currentColor.colorId;
    }
}