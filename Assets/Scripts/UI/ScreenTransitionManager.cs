using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenTransitionManager : MonoBehaviour
{
    private static readonly WaitForSeconds DefaultWait = new(1f);

    public static ScreenTransitionManager Instance { get; private set; }

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private Color fadeColor = Color.black;

    private Canvas transitionCanvas;
    private Image fadeImage;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureCanvasExists();
    }

    private void EnsureCanvasExists()
    {
        transitionCanvas = GetComponentInChildren<Canvas>(true);
        if (transitionCanvas != null)
        {
            fadeImage = transitionCanvas.GetComponentInChildren<Image>(true);
            return;
        }

        GameObject canvasGO = new GameObject("ScreenTransitionCanvas");
        canvasGO.transform.SetParent(transform);

        transitionCanvas = canvasGO.AddComponent<Canvas>();
        transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        transitionCanvas.sortingOrder = short.MaxValue;

        canvasGO.AddComponent<CanvasScaler>().uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        canvasGO.AddComponent<GraphicRaycaster>().enabled = false;

        GameObject imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(canvasGO.transform);

        fadeImage = imageGO.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        fadeImage.raycastTarget = false;

        RectTransform rect = fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    #region PUBLIC API
    public void PlayTransition(System.Action middleAction, WaitForSeconds wait = null)
    {
        StartCoroutine(TransitionRoutine(middleAction, wait ?? DefaultWait));
    }
    #endregion

    #region TRANSITION LOGIC
    private IEnumerator TransitionRoutine(System.Action middleAction, WaitForSeconds waitSeconds)
    {
        yield return FadeIn();

        //yield return waitSeconds;

        middleAction?.Invoke();

        yield return waitSeconds;

        yield return FadeOut();
    }

    public IEnumerator FadeIn()
    {
        yield return Fade(0f, 1f);
    }

    public IEnumerator FadeOut()
    {
        yield return Fade(1f, 0f);
    }

    private IEnumerator Fade(float from, float to)
    {
        float time = 0f;
        Color color = fadeImage.color;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(from, to, time / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = to;
        fadeImage.color = color;
    }
    #endregion
}