using UnityEngine;

public class RandomPlatformColor : MonoBehaviour
{
    [SerializeField] private Color[] possibleColors;

    private Renderer rend;
    private MaterialPropertyBlock block;

    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        block = new MaterialPropertyBlock();

        AssignRandomColor();
    }

    void AssignRandomColor()
    {
        if (possibleColors == null || possibleColors.Length == 0)
            return;

        Color chosen = possibleColors[Random.Range(0, possibleColors.Length)];

        rend.GetPropertyBlock(block);
        block.SetColor("_BaseColor", chosen);
        rend.SetPropertyBlock(block);
    }
}
