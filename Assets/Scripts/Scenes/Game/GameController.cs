using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UIElements;

public enum GameEndResult
{
    None,
    ColorsWin,
    ImpostorWins
}

public enum PlayerRoleType
{
    Color,
    Impostor
}

public class GameController : NetworkBehaviour
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Vector3 spawnCenter;
    [SerializeField] List<GameObject> playerInstanced;
    public SystemCameraController systemCameraController;
    Dictionary<ulong, NetworkObject> spawnedPlayers = new();

    [SerializeField] ColorData[] playerColors;
    [SerializeField] List<ColorData> currentColors;
    [Tooltip("Norte, Sur, Este, Oeste")]
    public List<string> RutesColor;

    [SerializeField] WorldBuilder worldBuilder;

    private bool partidaFinalizada = false;


    void Awake()
    {
        playerInstanced = new List<GameObject>();

        currentColors = new List<ColorData>(playerColors);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        NetworkManager.SceneManager.OnSceneEvent += OnSceneEvent;

        if (worldBuilder != null)
        {
            worldBuilder.OnPlayerEnterInCenter += PlayerEnterCenter;
            worldBuilder.OnPlayerExitOfCenter += PlayerExitCenter;
        }

        SetupEndGameUI();
    }

    public override void OnNetworkDespawn()
    {
        if (!NetworkManager) return;

        NetworkManager.SceneManager.OnSceneEvent -= OnSceneEvent;

        if (worldBuilder != null)
        {
            worldBuilder.OnPlayerEnterInCenter -= PlayerEnterCenter;
            worldBuilder.OnPlayerExitOfCenter -= PlayerExitCenter;
        }
    }

    void OnSceneEvent(SceneEvent sceneEvent)
    {
        if (sceneEvent.SceneEventType != SceneEventType.LoadComplete)
            return;

        // Solo reaccionamos a GameScene
        if (sceneEvent.Scene.name != "GameScene")
            return;

        // Solo cuando TODOS terminaron
        if (!sceneEvent.ClientId.Equals(NetworkManager.ServerClientId))
            return;

        StartCoroutine(InitializeGameRoutine());

    }

    IEnumerator InitializeGameRoutine()
    {
        SpawnAllPlayers();
        yield return null;

        AssignRoles();
        UpdateActiveColorPlayers();
        yield return null;

        worldBuilder.BuildWorld();
    }

    #region INICIAR PARTIDA
    void SpawnAllPlayers()
    {
        foreach (var client in NetworkManager.ConnectedClientsList)
        {
            SpawnPlayerForClient(client.ClientId);
        }
    }

    void SpawnPlayerForClient(ulong clientId)
    {
        if (spawnedPlayers.ContainsKey(clientId))
            return;

        Transform SP = GetSpawnPosition();

        var player = Instantiate(playerPrefab, SP.position, Quaternion.identity);
        var netObj = player.GetComponent<NetworkObject>();

        netObj.SpawnAsPlayerObject(clientId, true);
        spawnedPlayers.Add(clientId, netObj);
        OnSpawnPlayer(player, SP);

    }

    Transform GetSpawnPosition()
    {
        return worldBuilder.GetCurrentSpawnPointPosition();
    }

    void OnSpawnPlayer(GameObject player, Transform spawpoint)
    {
        // solo lectura de la lista
        playerInstanced.Add(player);

        // Auto asignarse como referencia
        PlayerReferences referencias = player.AddComponent<PlayerReferences>();
        referencias.gameController = this;


        // Asignar color
        if (player.TryGetComponent(out PlayerColor playerColor))
        {
            ColorData colorData = currentColors[Random.Range(0, currentColors.Count - 1)];
            Debug.Log("Asignando color: " + colorData.name + " al jugador " + player.name);
            playerColor._color.Value = ColorDataMapper.ToNet(colorData);
            RutesColor.Add(colorData.name);
            currentColors.Remove(colorData);
        }
        // Asignar spawnpoint
        if (player.TryGetComponent(out PlayerFall playerFall))
        {
            playerFall.spawnPoint = spawpoint;
        }

        // Suscribirse al evento de muerte
        if (player.TryGetComponent(out PlayerDead playerDead))
        {
            playerDead.OnDead.AddListener(() => OnPlayerDead(player));
        }

        if (IsServer)
        {
            StartIntroCinematicClientRpc(spawnCenter);
        }

    }

    void AssignRoles()
    {
        if (!IsServer) return;

        var clients = NetworkManager.ConnectedClientsList;
        int impostorIndex = Random.Range(0, clients.Count);

        for (int i = 0; i < clients.Count; i++)
        {
            var netObj = clients[i].PlayerObject;
            if (netObj == null) continue;

            var role = netObj.GetComponent<PlayerRole>();
            if (role == null) continue;

            role.Role.Value = (i == impostorIndex)
                ? PlayerRoleType.Impostor
                : PlayerRoleType.Color;
        }

        Debug.Log($"Impostor asignado: Client {clients[impostorIndex].ClientId}");
    }

    private void OnPlayerDead(GameObject player)
    {
        if (!IsServer || partidaFinalizada)
            return;

        var netObj = player.GetComponent<NetworkObject>();
        if (netObj == null)
            return;

        // Remover de jugadores activos si era Color
        activeColorPlayers.Remove(netObj.OwnerClientId);

        Debug.Log($"Jugador {netObj.OwnerClientId} ha muerto. Activos restantes: {activeColorPlayers.Count}");

        // Comprobar si la partida termina
        CheckGameOver();
    }
    #endregion

    #region CIENMATICAS

    [ClientRpc]
    void StartIntroCinematicClientRpc(Vector3 center)
    {
        if (!IsClient)
            return;

        StartCoroutine(WaitForLocalPlayerAndPlay(center));
    }

    IEnumerator WaitForLocalPlayerAndPlay(Vector3 center)
    {
        NetworkObject localPlayer = null;

        // Esperar a que el LocalPlayer exista
        while (localPlayer == null)
        {
            localPlayer = NetworkManager.Singleton
                .SpawnManager
                .GetLocalPlayerObject();

            yield return null;
        }

        // Esperar a que esté activo y habilitado
        while (!localPlayer.isActiveAndEnabled)
            yield return null;

        // Frame extra de seguridad (muy recomendable)
        yield return null;

        if (systemCameraController == null)
        {
            Debug.LogError("SystemCameraController not found on client");
            yield break;
        }

        systemCameraController.PlayIntroCinematic(
            center,
            localPlayer.transform
        );
    }
    #endregion

    #region END PARTIDA

    // =========================
    // GAME STATE
    // =========================
    private HashSet<ulong> activeColorPlayers = new();
    private HashSet<ulong> colorPlayersInCenter = new();

    public GameEndResult gameResult = GameEndResult.None;

    // =========================
    // END GAME UI
    // =========================
    [Header("End Game UI")]
    [SerializeField] private UIDocument endGameUIDocument;
    [SerializeField] private UIDocument hudUIDocument;

    // Sprites según resultado
    [SerializeField] private Sprite backgroundColorsWin;
    [SerializeField] private Sprite backgroundImpostorWin;
    [SerializeField] private Sprite titleWin;
    [SerializeField] private Sprite titleLoose;
    [SerializeField] private Sprite subtitleColorsWin;
    [SerializeField] private Sprite subtitleImpostorWin;

    // Runtime UI refs
    private VisualElement endGamePanel;
    private Image titleImage;
    private Image subtitleImage;
    private Label hudRolLabel;

    // Colores tema
    private static readonly Color COLOR_COLORS_WIN = new(0f, 0.937f, 0.925f);
    private static readonly Color COLOR_IMPOSTOR_WIN = new(0.514f, 0.31f, 0.729f);

    // =========================
    // UI SETUP
    // =========================
    private void SetupEndGameUI()
    {
        if (endGameUIDocument == null)
            return;

        var root = endGameUIDocument.rootVisualElement;

        endGamePanel = root.Q<VisualElement>("EndGamePanel");
        titleImage = root.Q<Image>("TitleImage");
        subtitleImage = root.Q<Image>("SubtitleImage");

        var restartButton = root.Q<Button>("RestartButton");
        if (restartButton != null)
            restartButton.clicked += OnRestartButtonClicked;

        endGamePanel.style.display = DisplayStyle.None;
    }

    // =========================
    // HUD
    // =========================
    public void UpdateHUDForRole(PlayerRoleType role)
    {
        if (hudUIDocument == null)
            return;

        var root = hudUIDocument.rootVisualElement;

        hudRolLabel = root.Q<Label>("Role");
        var abilityElement = root.Q<VisualElement>("ability");

        if (hudRolLabel == null || abilityElement == null)
            return;

        if (role == PlayerRoleType.Color)
        {
            hudRolLabel.text = "You are a Color";
            abilityElement.style.display = DisplayStyle.None;
        }
        else
        {
            hudRolLabel.text = "You are the Depression";
            abilityElement.style.display = DisplayStyle.Flex;
        }
    }

    // =========================
    // PLAYER TRACKING
    // =========================
    private void UpdateActiveColorPlayers()
    {
        activeColorPlayers.Clear();

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObj = client.PlayerObject;
            if (playerObj == null || !playerObj.gameObject.activeInHierarchy)
                continue;

            var role = playerObj.GetComponent<PlayerRole>();
            if (role != null && role.IsColor)
            {
                activeColorPlayers.Add(client.ClientId);
            }
        }

        // Limpieza defensiva
        colorPlayersInCenter.RemoveWhere(id => !activeColorPlayers.Contains(id));
    }

    private void PlayerEnterCenter(GameObject player)
    {
        if (!IsServer || partidaFinalizada)
            return;

        var netObj = player.GetComponent<NetworkObject>();
        if (netObj == null)
            return;

        ulong clientId = netObj.OwnerClientId;

        UpdateActiveColorPlayers();

        if (!activeColorPlayers.Contains(clientId))
            return;

        colorPlayersInCenter.Add(clientId);
        CheckGameOver();
    }

    private void PlayerExitCenter(GameObject player)
    {
        if (!IsServer || partidaFinalizada)
            return;

        var netObj = player.GetComponent<NetworkObject>();
        if (netObj == null)
            return;

        colorPlayersInCenter.Remove(netObj.OwnerClientId);
        CheckGameOver();
    }

    // =========================
    // GAME OVER EVALUATION
    // =========================
    private void CheckGameOver()
    {
        if (partidaFinalizada)
            return;

        UpdateActiveColorPlayers();

        NetworkObject impostor = null;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObj = client.PlayerObject;
            if (playerObj == null || !playerObj.gameObject.activeInHierarchy)
                continue;

            var role = playerObj.GetComponent<PlayerRole>();
            if (role != null && role.IsImpostor)
            {
                impostor = playerObj;
                break;
            }
        }

        // 1️⃣ Todos los colores muertos → gana el Impostor
        if (activeColorPlayers.Count == 0)
        {
            EndGame(GameEndResult.ImpostorWins);
            return;
        }

        // 2️⃣ Todos los colores vivos en el centro → ganan los Colores
        if (colorPlayersInCenter.IsSupersetOf(activeColorPlayers))
        {
            EndGame(GameEndResult.ColorsWin);
            return;
        }

        // 3️⃣ Impostor muerto → ganan los Colores
        if (impostor == null)
        {
            EndGame(GameEndResult.ColorsWin);
            return;
        }
    }

    // =========================
    // END GAME
    // =========================
    private void EndGame(GameEndResult result)
    {
        if (partidaFinalizada)
            return;

        partidaFinalizada = true;
        gameResult = result;

        SessionManager.Instance.ChangeState(SessionManager.SessionState.End);

        ShowEndGameUIClientRpc(result);
    }

    // =========================
    // UI RPC
    // =========================
    [ClientRpc]
    private void ShowEndGameUIClientRpc(GameEndResult result)
    {
        if (endGamePanel == null)
            SetupEndGameUI();
        if (endGamePanel == null)
            return;

        endGamePanel.style.display = DisplayStyle.Flex;
        UnityEngine.Cursor.lockState = CursorLockMode.None;

        var localPlayer =
            NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        var role = localPlayer?.GetComponent<PlayerRole>();

        bool isColor = role != null && role.IsColor;
        bool localWon =
            (result == GameEndResult.ColorsWin && isColor) ||
            (result == GameEndResult.ImpostorWins && !isColor);

        if (result == GameEndResult.ColorsWin)
        {
            endGamePanel.style.backgroundImage =
                new StyleBackground(backgroundColorsWin);
            ApplyEndGameTheme(COLOR_COLORS_WIN);
            subtitleImage.sprite = subtitleColorsWin;
        }
        else
        {
            endGamePanel.style.backgroundImage =
                new StyleBackground(backgroundImpostorWin);
            ApplyEndGameTheme(COLOR_IMPOSTOR_WIN);
            subtitleImage.sprite = subtitleImpostorWin;
        }

        titleImage.sprite = localWon ? titleWin : titleLoose;
    }

    // =========================
    // UI THEME
    // =========================
    private void ApplyEndGameTheme(Color themeColor)
    {
        endGamePanel.style.unityBackgroundImageTintColor = themeColor;
        titleImage.tintColor = themeColor;
        subtitleImage.tintColor = themeColor;

        foreach (var btn in endGamePanel.Query<Button>().ToList())
        {
            btn.style.color = themeColor;
            btn.style.borderBottomColor =
            btn.style.borderTopColor =
            btn.style.borderLeftColor =
            btn.style.borderRightColor = themeColor;
        }
    }

    private void OnRestartButtonClicked()
    {
        SessionManager.Instance.ChangeState(SessionManager.SessionState.Lobby);
    }

    #endregion


}