using UnityEngine;

public class ColorReactivePlatform : MonoBehaviour
{
    [SerializeField] private float returnToNeutralSpeed = 2f;

    private Renderer rend;
    private MaterialPropertyBlock block;

    private ColorData currentOwner;
    private Color neutralColor = Color.white;

    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        block = new MaterialPropertyBlock();
        SetColor(neutralColor);
    }

    void OnCollisionEnter(Collision collision)
    {
        ColorIdentity identity = collision.gameObject.GetComponent<ColorIdentity>();
        if (!identity) return;

        // Plataforma neutra: se activa
        if (currentOwner == null)
        {
            currentOwner = identity.CurrentColor;
            SetColor(currentOwner.color);
        }
        // Plataforma ocupada por otro color: fallo
        else if (identity.CurrentColor != currentOwner)
        {
            collision.gameObject.SendMessage(
                "OnInvalidPlatform",
                SendMessageOptions.DontRequireReceiver
            );
        }
    }

    void OnCollisionExit(Collision collision)
    {
        ColorIdentity identity = collision.gameObject.GetComponent<ColorIdentity>();
        if (!identity) return;

        if (identity.CurrentColor == currentOwner)
        {
            currentOwner = null;
        }
    }

    void Update()
    {
        if (currentOwner == null)
        {
            rend.GetPropertyBlock(block);
            Color current = block.GetColor("_BaseColor");
            Color target = Color.Lerp(
                current,
                neutralColor,
                Time.deltaTime * returnToNeutralSpeed
            );
            block.SetColor("_BaseColor", target);
            rend.SetPropertyBlock(block);
        }
    }

    void SetColor(Color color)
    {
        rend.GetPropertyBlock(block);
        block.SetColor("_BaseColor", color);
        rend.SetPropertyBlock(block);
    }
}