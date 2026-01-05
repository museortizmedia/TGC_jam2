using UnityEngine;

public class ColorReactivePlatform : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float returnToNeutralSpeed = 2f;
    [SerializeField] private float holdTime = 1f;

    [Header("Colores")]
    [SerializeField] private Color neutralColor = Color.white;
    [SerializeField] private float activeEmissionIntensity = 1.2f;

    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    private Renderer rend;
    private MaterialPropertyBlock block;

    private PlayerColor currentPlayer;
    private float timer;
    private bool pintada;
    private string colorName;

    private Color defaultEmissionColor;
    private bool hasDefaultEmission;

    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        block = new MaterialPropertyBlock();

        Material mat = rend.sharedMaterial;

        if (mat != null && mat.HasProperty(EmissionColorID))
        {
            defaultEmissionColor = mat.GetColor(EmissionColorID);
            hasDefaultEmission = true;
            mat.EnableKeyword("_EMISSION");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        PlayerColor player = collision.gameObject.GetComponent<PlayerColor>();
        if (player == null || player.CurrentColor == null) return;

        if (pintada && player.CurrentColor.colorId != colorName)
        {
            if (collision.gameObject.TryGetComponent(out IDeadly deadly))
                deadly.Dead();
            return;
        }

        currentPlayer = player;
        colorName = player.CurrentColor.colorId;
        pintada = true;

        SetActiveColor(player.CurrentColor.color);
        timer = holdTime;
    }

    void OnCollisionExit(Collision collision)
    {
        PlayerColor player = collision.gameObject.GetComponent<PlayerColor>();
        if (player == currentPlayer)
            currentPlayer = null;
    }

    void Update()
    {
        // Jugador encima → nunca se apaga
        if (currentPlayer != null)
        {
            SetActiveColor(currentPlayer.CurrentColor.color);
            timer = holdTime;
            return;
        }

        // Tiempo de gracia
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            return;
        }

        // Volver suavemente al estado original
        rend.GetPropertyBlock(block);

        Color baseCurrent = block.GetColor(BaseColorID);
        Color emissionCurrent = block.GetColor(EmissionColorID);

        Color baseTarget = Color.Lerp(
            baseCurrent,
            neutralColor,
            Time.deltaTime * returnToNeutralSpeed
        );

        Color emissionTarget = hasDefaultEmission
            ? Color.Lerp(emissionCurrent, defaultEmissionColor, Time.deltaTime * returnToNeutralSpeed)
            : emissionCurrent;

        block.SetColor(BaseColorID, baseTarget);
        block.SetColor(EmissionColorID, emissionTarget);
        rend.SetPropertyBlock(block);

        pintada = false;
    }

    void SetActiveColor(Color baseColor)
    {
        Color emissionColor = baseColor * activeEmissionIntensity;

        block.Clear();
        block.SetColor(BaseColorID, baseColor);
        block.SetColor(EmissionColorID, emissionColor);
        rend.SetPropertyBlock(block);
    }
}