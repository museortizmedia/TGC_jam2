using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

#region Data

[System.Serializable]
public class RouteTemplate
{
    public string routeName;
    public Transform[] moduleSlots; // 4 slots por ruta
}

public struct PuzzlePlacement : INetworkSerializable
{
    public int PuzzleIndex;   // índice en puzzles[]
    public int RouteIndex;    // 0..3
    public int LevelIndex;    // 0..3
    public FixedString32Bytes Color;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref PuzzleIndex);
        serializer.SerializeValue(ref RouteIndex);
        serializer.SerializeValue(ref LevelIndex);
        serializer.SerializeValue(ref Color);
    }
}

#endregion

public class WorldBuilder : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameController gameController;

    [Header("Templates (Norte, Este, Sur, Oeste)")]
    [SerializeField] private GameObject[] templates;

    [Header("All puzzles in scene (ORDER IS IMPORTANT)")]
    [SerializeField] private GameObject[] puzzles;

    [Header("Routes")]
    [SerializeField] private RouteTemplate[] routes;

    [Header("Spawns")]
    [SerializeField] private Transform[] spawnPoints;

    public event Action<GameObject> OnPlayerEnterInCenter;

    private int currentSpawnIndex = 0;

    private PuzzlePlacement[] currentLayout;
    private bool worldBuilt;

    #region Spawns

    public Transform GetCurrentSpawnPointPosition()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return null;

        Transform spawn = spawnPoints[currentSpawnIndex];
        currentSpawnIndex = (currentSpawnIndex + 1) % spawnPoints.Length;
        return spawn;
    }

    public void ReportPlayerInCenter(GameObject player)
    {
        OnPlayerEnterInCenter?.Invoke(player);
    }

    #endregion

    #region Network lifecycle

    public override void OnNetworkSpawn()
    {
        if (!IsServer && worldBuilt && currentLayout != null)
        {
            ApplyWorldLayout(currentLayout);
        }
    }

    #endregion

    #region Build World

    [ContextMenu("Construir Mundo")]
    public void BuildWorld()
    {
        if (!IsServer || worldBuilt)
            return;

        StartCoroutine(BuildWorldRoutine());
    }

    private IEnumerator BuildWorldRoutine()
    {
        // 1. Colores por ruta
        List<string> colorRoutes = gameController.RutesColor;

        if (colorRoutes == null || colorRoutes.Count == 0)
            colorRoutes = new List<string> { "amarillo", "azul", "rojo", "verde" };

        while (colorRoutes.Count < 4)
            colorRoutes.Add("blanco");

        // 2. Elegir exactamente 4 puzzles
        GameObject[] puzzlesElegidos =
            Shuffle(puzzles).Take(4).ToArray();

        // 3. Agrupar niveles por índice
        List<PuzzleModule>[] nivelesPorIndice = new List<PuzzleModule>[4];
        for (int i = 0; i < 4; i++)
            nivelesPorIndice[i] = new List<PuzzleModule>();

        Dictionary<PuzzleModule, int> moduloToGlobalPuzzleIndex =
            new Dictionary<PuzzleModule, int>();

        for (int globalIndex = 0; globalIndex < puzzles.Length; globalIndex++)
        {
            GameObject puzzle = puzzles[globalIndex];

            if (!puzzlesElegidos.Contains(puzzle))
                continue;

            puzzle.SetActive(true);

            PuzzleModule[] niveles =
                puzzle.GetComponentsInChildren<PuzzleModule>(true);

            if (niveles.Length != 4)
            {
                Debug.LogError($"Puzzle {puzzle.name} no tiene 4 niveles.");
                continue;
            }

            for (int nivel = 0; nivel < 4; nivel++)
            {
                PuzzleModule modulo = niveles[nivel];
                modulo.gameObject.SetActive(false);

                nivelesPorIndice[nivel].Add(modulo);
                moduloToGlobalPuzzleIndex[modulo] = globalIndex;
            }
        }

        // 4. Construir layout lógico
        List<PuzzlePlacement> layout = new();

        for (int nivel = 0; nivel < 4; nivel++)
        {
            PuzzleModule[] candidatos =
                Shuffle(nivelesPorIndice[nivel].ToArray());

            if (candidatos.Length < 4)
            {
                Debug.LogError($"Nivel {nivel} no tiene suficientes módulos.");
                yield break;
            }

            for (int ruta = 0; ruta < 4; ruta++)
            {
                PuzzleModule modulo = candidatos[ruta];

                layout.Add(new PuzzlePlacement
                {
                    PuzzleIndex = moduloToGlobalPuzzleIndex[modulo],
                    RouteIndex = ruta,
                    LevelIndex = nivel,
                    Color = colorRoutes[ruta]
                });
            }
        }

        currentLayout = layout.ToArray();
        worldBuilt = true;

        // 5. Aplicar en servidor
        ApplyWorldLayout(currentLayout);

        // 6. Replicar a clientes
        ApplyWorldLayoutClientRpc(currentLayout);

        yield return null;
    }

    #endregion

    #region Apply Layout

    [ClientRpc]
    private void ApplyWorldLayoutClientRpc(PuzzlePlacement[] layout)
    {
        ApplyWorldLayout(layout);
    }

    private void ApplyWorldLayout(PuzzlePlacement[] layout)
    {
        foreach (var t in templates)
            t.SetActive(false);

        foreach (PuzzlePlacement p in layout)
        {
            GameObject puzzle = puzzles[p.PuzzleIndex];

            if (!puzzle.activeSelf)
                puzzle.SetActive(true);

            PuzzleModule[] niveles =
                puzzle.GetComponentsInChildren<PuzzleModule>(true);

            PuzzleModule modulo = niveles[p.LevelIndex];
            Transform slot = routes[p.RouteIndex].moduleSlots[p.LevelIndex];

            Transform tr = modulo.transform;
            tr.position = slot.position;
            tr.rotation = slot.rotation;
            tr.localScale = slot.localScale;

            modulo.gameObject.SetActive(true);
            //if(p.Color != "blanco") { Debug.Log($"[WorldBuilder] Inicicando {modulo.gameObject.name} con {p.Color.ToString()}"); }
            modulo.Initialize(p.Color.ToString());
        }
    }

    #endregion

    #region Utils

    private T[] Shuffle<T>(T[] array)
    {
        T[] result = array.ToArray();

        for (int i = result.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (result[i], result[j]) = (result[j], result[i]);
        }

        return result;
    }

    #endregion
}