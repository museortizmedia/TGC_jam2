using System;
using UnityEngine;

[Obsolete]
public class CharacterColorRenderer : MonoBehaviour, IColorAffected
{
    private Renderer rend;
    private MaterialPropertyBlock block;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
    }

    public bool CanInteractive(ColorData colorData)
    {
        rend.GetPropertyBlock(block);
        //block.SetColor("_Color", colorData.dataColor.value);
        rend.SetPropertyBlock(block);
        return true;
    }
}