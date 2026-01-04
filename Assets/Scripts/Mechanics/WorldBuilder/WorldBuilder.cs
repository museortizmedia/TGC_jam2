using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;

[System.Serializable]
public class RouteTemplate
{
    public string routeName;
    public Transform[] moduleSlots; // 4 slots por ruta
}

public class WorldBuilder : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameController gameController;
    [SerializeField] private Transform rutasParent;

    [Header("Templates (Norte, Este, Sur, Oeste)")]
    [SerializeField] private GameObject[] templates;

    [Header("All Puzzles in Scene")]
    [SerializeField] private GameObject[] puzzles;

    [Header("Spawns")]
    [SerializeField] private Transform[] spawnPoints;
    private int currentSpawnIndex = 0;

    [Header("Routes")]
    [SerializeField] private RouteTemplate[] routes;

    public event Action<GameObject> OnPlayerEnterInCenter;

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

    #region World Build
    [ContextMenu("Construir")]
    public void BuildWorld()
    {
        if (!IsServer)
            return;

        StartCoroutine(BuildWorldRoutine());
    }

    private IEnumerator BuildWorldRoutine()
    {
        // 1. Colores
        List<string> colorRoutes = gameController.RutesColor;

        if (colorRoutes.Count == 0)
            colorRoutes.AddRange(new[] { "amarillo", "azul", "rojo", "verde" });

        while (colorRoutes.Count < 4)
            colorRoutes.Add("blanco");

        // 2. Elegir SOLO 4 puzzles
        GameObject[] puzzlesElegidos = Shuffle(puzzles)
            .Take(4)
            .ToArray();

        // 3. Agrupar niveles por índice (0..3)
        List<PuzzleModule>[] nivelesGlobales = new List<PuzzleModule>[4];
        for (int i = 0; i < 4; i++)
            nivelesGlobales[i] = new List<PuzzleModule>();

        foreach (GameObject puzzle in puzzlesElegidos)
        {
            puzzle.SetActive(true); // 🔴 IMPORTANTE: activar el puzzle padre

            PuzzleModule[] niveles = puzzle.GetComponentsInChildren<PuzzleModule>(true);

            if (niveles.Length != 4)
            {
                Debug.LogError($"Puzzle {puzzle.name} no tiene 4 niveles.");
                continue;
            }

            for (int nivel = 0; nivel < 4; nivel++)
            {
                nivelesGlobales[nivel].Add(niveles[nivel]);
                niveles[nivel].gameObject.SetActive(false);
            }
        }

        // 4. Repartir niveles: 1 por ruta, sin repetir
        for (int nivel = 0; nivel < 4; nivel++)
        {
            PuzzleModule[] candidatos = Shuffle(nivelesGlobales[nivel].ToArray());

            if (candidatos.Length < 4)
            {
                Debug.LogError($"Nivel {nivel} no tiene suficientes módulos.");
                continue;
            }

            for (int ruta = 0; ruta < 4; ruta++)
            {
                PuzzleModule modulo = candidatos[ruta];
                Transform slot = routes[ruta].moduleSlots[nivel];

                Transform t = modulo.transform;
                t.position = slot.position;
                t.rotation = slot.rotation;
                t.localScale = slot.localScale;

                // Server-only
                modulo.ColorIdRute.Value = colorRoutes[ruta];
                modulo.colorName = colorRoutes[ruta];

                modulo.gameObject.SetActive(true);
            }
        }

        yield return null;

        // 5. Ocultar templates (YA NO SE NECESITAN)
        foreach (GameObject template in templates)
            template.SetActive(false);

        // 6. Inicializar SOLO módulos activos
        foreach (var modulo in puzzlesElegidos.SelectMany(p =>
                 p.GetComponentsInChildren<PuzzleModule>(true)))
        {
            if (modulo.gameObject.activeSelf)
                modulo.IniciarPuzzle();
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