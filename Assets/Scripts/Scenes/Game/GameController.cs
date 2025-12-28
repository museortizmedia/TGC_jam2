using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Cinemachine;

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

    void Awake()
    {
        playerInstanced = new List<GameObject>();
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

        Vector3 pos = GetRandomSpawnPosition();

        var player = Instantiate(playerPrefab, pos, Quaternion.identity);
        var netObj = player.GetComponent<NetworkObject>();

        netObj.SpawnAsPlayerObject(clientId, true);
        spawnedPlayers.Add(clientId, netObj);
        playerInstanced.Add(player);
    }

    Vector3 GetRandomSpawnPosition()
    {
        Vector2 circle = Random.insideUnitCircle * spawnRadius;
        return spawnCenter + new Vector3(circle.x, 0, circle.y);
    }
}