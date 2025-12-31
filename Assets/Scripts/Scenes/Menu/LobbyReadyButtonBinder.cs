using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class LobbyReadyButtonBinder : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private LobbyReadyController lobbyReadyController;

    private Button settingButton;
    private Button readyButton;
    private Label statusLabel;
    private VisualElement settingPanelOverlay;
    private VisualElement settingPanel;

    private bool uiInitialized;

    // =========================
    // UNITY LIFECYCLE
    // =========================
    void Start()
    {
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument no asignado");
            return;
        }

        if (lobbyReadyController == null)
        {
            Debug.LogError("LobbyReadyController no asignado");
            return;
        }

        StartCoroutine(WaitForRootVisualElement());
    }

    private IEnumerator WaitForRootVisualElement()
    {
        while (uiDocument.rootVisualElement == null)
            yield return null;

        InitializeUI(uiDocument.rootVisualElement);
    }

    void OnDisable()
    {
        if (!uiInitialized)
            return;

        if (settingButton != null)
            settingButton.clicked -= OnSettingButtonClicked;

        if (readyButton != null)
            readyButton.clicked -= OnReadyClicked;

        if (settingPanelOverlay != null)
            settingPanelOverlay.UnregisterCallback<PointerDownEvent>(OnOverlayClicked);

        if (lobbyReadyController != null)
        {
            lobbyReadyController.OnLobbyFullChanged -= OnLobbyFullChanged;
            lobbyReadyController.OnStatusTextChanged -= OnStatusTextChanged;
        }

        uiInitialized = false;
    }

    [ContextMenu("Initialize UI")]
    void ContextMenuInitializeUI()
    {
        if (uiDocument != null && uiDocument.rootVisualElement != null)
        {
            InitializeUI(uiDocument.rootVisualElement);
        }
    }

    // =========================
    // UI INITIALIZATION
    // =========================
    private void InitializeUI(VisualElement root)
    {
        if (uiInitialized)
            return;

        settingButton = root.Q<Button>("settings-button");
        settingPanelOverlay = root.Q<VisualElement>("settings-panel-overlay");
        settingPanel = root.Q<VisualElement>("settings-panel");
        readyButton = root.Q<Button>("start-game-button");
        statusLabel = root.Q<Label>("lobby-status-label");

        if (settingButton == null ||
            settingPanelOverlay == null ||
            settingPanel == null ||
            readyButton == null ||
            statusLabel == null)
        {
            Debug.LogError("Uno o más elementos de UI no fueron encontrados");
            return;
        }

        // UI callbacks
        settingButton.clicked += OnSettingButtonClicked;
        readyButton.clicked += OnReadyClicked;

        settingPanelOverlay.RegisterCallback<PointerDownEvent>(OnOverlayClicked);
        settingPanel.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());

        // Network state callbacks
        lobbyReadyController.OnLobbyFullChanged += OnLobbyFullChanged;
        lobbyReadyController.OnStatusTextChanged += OnStatusTextChanged;

        // Estado inicial (clientes tardíos)
        OnLobbyFullChanged(lobbyReadyController.IsLobbyFull);
        OnStatusTextChanged(lobbyReadyController.StatusText);

        uiInitialized = true;
    }

    // =========================
    // UI EVENTS
    // =========================
    private void OnReadyClicked()
    {
        Debug.Log("READY clicked");

        // Evita spam local
        readyButton.SetEnabled(false);

        // Marca al jugador como READY (servidor decide)
        lobbyReadyController.SetPlayerReady();
    }

    private void OnSettingButtonClicked()
    {
        settingPanelOverlay.style.display = DisplayStyle.Flex;
    }

    private void OnOverlayClicked(PointerDownEvent evt)
    {
        settingPanelOverlay.style.display = DisplayStyle.None;
    }

    // =========================
    // NETWORK STATE EVENTS
    // =========================
    private void OnLobbyFullChanged(bool lobbyFull)
    {
        if (readyButton != null)
            readyButton.SetEnabled(lobbyFull);
    }

    private void OnStatusTextChanged(string text)
    {
        if (statusLabel != null)
            statusLabel.text = text;
    }
}