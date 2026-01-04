using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class EndGameUIDocumentController : MonoBehaviour
{
    
    [Header("Local player")]
    [SerializeField] GameController gameController;

    private VisualElement root;

    private VisualElement colorsBackground;
    private VisualElement impostorBackground;

    private VisualElement colorsTitle;
    private VisualElement impostorTitle;

    private VisualElement colorsSubtitle;
    private VisualElement impostorSubtitle;

    private Button lobbyButton;

    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        CacheVisualElements();
        HideAllVisuals();

        lobbyButton.clicked += ReturnToLobby;

        // Suscripción al evento de fin de partida
        SessionManager.Instance.OnSessionStateChanged += OnGameStateChange;
    }

    void OnDisable()
    {
        lobbyButton.clicked -= ReturnToLobby;
        SessionManager.Instance.OnSessionStateChanged -= OnGameStateChange;
    }

    private void CacheVisualElements()
    {
        colorsBackground   = root.Q<VisualElement>("colorsBackground");
        impostorBackground = root.Q<VisualElement>("impostorBackground");

        colorsTitle   = root.Q<VisualElement>("colorsTitle");
        impostorTitle = root.Q<VisualElement>("impostorTitle");

        colorsSubtitle   = root.Q<VisualElement>("colorsSubtitle");
        impostorSubtitle = root.Q<VisualElement>("impostorSubtitle");

        lobbyButton = root.Q<Button>("lobbyButton");
    }

    private void HideAllVisuals()
    {
        colorsBackground.style.display   = DisplayStyle.None;
        impostorBackground.style.display = DisplayStyle.None;

        colorsTitle.style.display   = DisplayStyle.None;
        impostorTitle.style.display = DisplayStyle.None;

        colorsSubtitle.style.display   = DisplayStyle.None;
        impostorSubtitle.style.display = DisplayStyle.None;
    }

    void OnGameStateChange(SessionManager.SessionState newState)
    {
        if(newState == SessionManager.SessionState.End)
        {
            OnGameEnded();
        }
    }

    private void OnGameEnded()
    {
        /*HideAllVisuals();
        root.style.display = DisplayStyle.Flex;

        bool colorsWon = result == gameController.GameEndResult.ColorsWin;

        // Fondo según resultado
        colorsBackground.style.display   = colorsWon ? DisplayStyle.Flex : DisplayStyle.None;
        impostorBackground.style.display = colorsWon ? DisplayStyle.None : DisplayStyle.Flex;

        // UI según tipo de jugador local
        if (gameController.localPlayerType == gameController.PlayerType.Color)
        {
            colorsTitle.style.display    = DisplayStyle.Flex;
            colorsSubtitle.style.display = DisplayStyle.Flex;
        }
        else
        {
            impostorTitle.style.display    = DisplayStyle.Flex;
            impostorSubtitle.style.display = DisplayStyle.Flex;
        }*/
    }

    private void ReturnToLobby()
    {
        SceneManager.LoadScene("Lobby");
    }
}