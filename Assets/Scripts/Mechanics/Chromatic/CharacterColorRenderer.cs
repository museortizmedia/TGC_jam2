using UnityEngine;

public class CharacterColorRenderer : MonoBehaviour, IColorAffected
{
    private Renderer rend;
    private MaterialPropertyBlock block;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
    }

    public void EvaluateColor(ColorData colorData)
    {
        rend.GetPropertyBlock(block);
        //block.SetColor("_Color", colorData.dataColor.value);
        rend.SetPropertyBlock(block);
    }
}