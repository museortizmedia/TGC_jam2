using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Cinemachine;
using Mono.Cecil.Cil;
using System.Collections;
using Unity.VisualScripting;

public class GameController : NetworkBehaviour
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Vector3 spawnCenter;
    [SerializeField] List<GameObject> playerInstanced;
    [SerializeField] CinemachineVirtualCameraBase freelookCamera;
    public CinemachineVirtualCameraBase Camera => freelookCamera;
    Dictionary<ulong, NetworkObject> spawnedPlayers = new();

    [SerializeField] ColorData[] playerColors;
    [SerializeField] List<ColorData> currentColors;

    [SerializeField] WorldBuilder worldBuilder;

    private HashSet<ulong> playersThatEnteredCenter = new();
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
            worldBuilder.OnPlayerEnterInCenter += PlayerInCenterCounter;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!NetworkManager) return;

        NetworkManager.SceneManager.OnSceneEvent -= OnSceneEvent;

        if (worldBuilder != null)
        {
            worldBuilder.OnPlayerEnterInCenter -= PlayerInCenterCounter;
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

        SpawnAllPlayers();
    }

    void SpawnAllPlayers()
    {
        int i = 0;
        foreach (var client in NetworkManager.ConnectedClientsList)
        {
            //Debug.Log(i);
            SpawnPlayerForClient(client.ClientId);
            i++;
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
            currentColors.Remove(colorData);
        }
        // Asignar spawnpoint
        if(player.TryGetComponent(out PlayerFall playerFall))
        {
            playerFall.spawnPoint = spawpoint;
        }

    }

    void PlayerInCenterCounter(GameObject player)
    {
        if (!IsServer || partidaFinalizada)
            return;

        var netObj = player.GetComponent<NetworkObject>();
        if (netObj == null)
            return;

        ulong clientId = netObj.OwnerClientId;

        // Si ya entró antes, ignoramos
        if (!playersThatEnteredCenter.Add(clientId))
            return;

        Debug.Log(
            $"Jugador {clientId} entró al centro " +
            $"({playersThatEnteredCenter.Count}/{NetworkManager.ConnectedClientsList.Count})"
        );

        // ¿Todos han entrado?
        if (playersThatEnteredCenter.Count == NetworkManager.ConnectedClientsList.Count)
        {
            FinalizarPartida();
        }
    }

    void FinalizarPartida()
    {
        if (partidaFinalizada)
            return;

        partidaFinalizada = true;

        Debug.Log("Todos los jugadores han entrado al centro. Finalizando partida.");
        // Reportar al SessionManager
        SessionManager.Instance.ChangeState(SessionManager.SessionState.End);
        // - cada instancia de player debe quita el input
        // Activar cámara de finalización y lógica de definición de ganador
        StartCoroutine(FinalizarPartidaCoroutine());
    }

    IEnumerator FinalizarPartidaCoroutine()
    {
        // Esperar un momento para que todos vean que han entrado
        Debug.Log("5 segundos para reiniciar...");
        yield return new WaitForSeconds(5f);
        SessionManager.Instance.ChangeState(SessionManager.SessionState.Lobby);
    }
}