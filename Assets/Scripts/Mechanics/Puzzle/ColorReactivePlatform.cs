using UnityEngine;

public class ColorReactivePlatform : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float returnToNeutralSpeed = 2f;
    [SerializeField] private float holdTime = 1f; // tiempo que mantiene el color después de que el jugador se va
    [SerializeField] private Color neutralColor = Color.white;

    private Renderer rend;
    private MaterialPropertyBlock block;

    private PlayerColor currentPlayer; // jugador que activó la plataforma
    private float timer = 0f;          // temporizador para mantener color

    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        block = new MaterialPropertyBlock();
        SetColor(neutralColor);
    }

    void OnCollisionEnter(Collision collision)
    {
        PlayerColor player = collision.gameObject.GetComponent<PlayerColor>();
        if (player == null || player.CurrentColor == null) return;

        // Solo tomar color si la plataforma está blanca
        if (currentPlayer == null)
        {
            currentPlayer = player;
            SetColor(player.CurrentColor.color);
            timer = holdTime;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        PlayerColor player = collision.gameObject.GetComponent<PlayerColor>();
        if (player == currentPlayer)
        {
            currentPlayer = null; // libera la plataforma
        }
    }

    void Update()
    {
        rend.GetPropertyBlock(block);
        Color current = block.GetColor("_BaseColor");

        if (currentPlayer != null)
        {
            // Mantener color mientras el jugador esté encima
            SetColor(currentPlayer.CurrentColor.color);
            timer = holdTime; // reinicia temporizador
        }
        else if (timer > 0f)
        {
            // Mantener color un tiempo
            timer -= Time.deltaTime;
            SetColor(current);
        }
        else
        {
            // Volver suavemente a blanco
            Color target = Color.Lerp(current, neutralColor, Time.deltaTime * returnToNeutralSpeed);
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