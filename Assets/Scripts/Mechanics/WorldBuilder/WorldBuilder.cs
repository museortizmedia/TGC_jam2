using System;
using UnityEngine;

public class WorldBuilder : MonoBehaviour
{
    public GameObject PiecePrefab;
    public Transform[] SpawnPoints;
    private int currentSpawnIndex = 0;

    public event Action<GameObject> OnPlayerEnterInCenter;

    public Transform GetCurrentSpawnPointPosition()
    {
        if (SpawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points defined in WorldBuilder.");
            return null;
        }

        Transform spawnPoint = SpawnPoints[currentSpawnIndex];
        currentSpawnIndex = (currentSpawnIndex + 1) % SpawnPoints.Length;
        return spawnPoint;
    }

    public void ReportPlayerInCenter(GameObject player)
    {
        OnPlayerEnterInCenter?.Invoke(player);
        Debug.Log("Player " + player.name + " is in the center area.");
    }
    
}
