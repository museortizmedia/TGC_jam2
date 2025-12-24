using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class TextHoverPressColor : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Text Reference")]
    [SerializeField] private TMP_Text text;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color pressedColor = Color.red;

    [Header("Scale Settings")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float scaleSpeed = 10f;

    private Vector3 normalScale;
    private Vector3 targetScale;

    private void Reset()
    {
        text = GetComponent<TMP_Text>();
    }

    private void Awake()
    {
        if (text == null)
            text = GetComponent<TMP_Text>();

        normalScale = transform.localScale;
        targetScale = normalScale;
        text.color = normalColor;
    }

    private void Update()
    {
        // Escalado suave
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.unscaledDeltaTime * scaleSpeed
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.color = hoverColor;
        targetScale = normalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.color = normalColor;
        targetScale = normalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        text.color = pressedColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        text.color = hoverColor;
    }
}
