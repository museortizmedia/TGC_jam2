using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Cinemachine;
using Mono.Cecil.Cil;

public class GameController : NetworkBehaviour
{
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Vector3 spawnCenter;
    [SerializeField] float spawnRadius = 5f;
    [SerializeField] List<GameObject> playerInstanced;
    [SerializeField] CinemachineVirtualCameraBase freelookCamera;
    public CinemachineVirtualCameraBase Camera => freelookCamera;
    Dictionary<ulong, NetworkObject> spawnedPlayers = new();
    [SerializeField] int localPlayerIndex = -1;

    [SerializeField] ColorData[] playerColors;
    List<ColorData> currentColors;

    [SerializeField] WorldBuilder worldBuilder;

    void Awake()
    {
        playerInstanced = new List<GameObject>();

        currentColors = new List<ColorData>(playerColors);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        NetworkManager.SceneManager.OnSceneEvent += OnSceneEvent;
    }

    public override void OnNetworkDespawn()
    {
        if (!NetworkManager) return;

        NetworkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
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

        Vector3 pos = GetSpawnPosition();

        var player = Instantiate(playerPrefab, pos, Quaternion.identity);
        var netObj = player.GetComponent<NetworkObject>();

        netObj.SpawnAsPlayerObject(clientId, true);
        spawnedPlayers.Add(clientId, netObj);
        OnSpawnPlayer(player);
        
    }

    Vector3 GetSpawnPosition()
    {
        return worldBuilder.GetCurrentSpawnPointPosition();
        //Vector2 circle = Random.insideUnitCircle * spawnRadius;
        //return spawnCenter + new Vector3(circle.x, 0, circle.y);
    }

    void OnSpawnPlayer(GameObject player)
    {
        // solo lectura de la lista
        playerInstanced.Add(player);

        // Auto asignarse como referencia
        PlayerReferences referencias = player.AddComponent<PlayerReferences>();
        referencias.gameController = this;


        // Asignar color
        player.TryGetComponent<PlayerColor>(out var playerColor);
        if (playerColor != null)
        {
            ColorData colorData = currentColors[ Random.Range(0, playerInstanced.Count - 1) ];
            Debug.Log("Asignando color: " + colorData.name + " al jugador " + player.name);
            playerColor._color.Value = ColorDataMapper.ToNet(colorData);
            currentColors.Remove(colorData);
        }

    }
}