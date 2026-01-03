using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public class RouteTemplate
{
    public string routeName;
    public Transform[] moduleSlots; // 4 slots
}

public class WorldBuilder : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] GameController gameController;
    [SerializeField] Transform RutasParent;

    [Header("Spawns")]
    [Tooltip("Norte, Sur, Este, Oeste")]
    public Transform[] SpawnPoints;
    private int currentSpawnIndex = 0;

    public event Action<GameObject> OnPlayerEnterInCenter;

    /*void Start()
    {
        StartCoroutine(BuildWorldRoutine());
    }*/

    private void Reset()
    {
        routes = new RouteTemplate[4];

        string[] names = { "Norte", "Sur", "Este", "Oeste" };

        for (int i = 0; i < routes.Length; i++)
        {
            routes[i] = new RouteTemplate
            {
                routeName = names[i],
                moduleSlots = new Transform[4]
            };
        }
    }

    #region Spawns
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
    #endregion

    #region Puzzles
    [Header("Puzzles")]
    [SerializeField] GameObject[] puzzlePrefabs;
    public RouteTemplate[] routes;
    public void BuildWorld()
    {
        if (!IsServer)
            return;

        StartCoroutine(BuildWorldRoutine());
    }

    private IEnumerator BuildWorldRoutine()
    {
        List<string> ColorRoutes = gameController.RutesColor;
        if(ColorRoutes.Count == 0)
        {
            ColorRoutes.Add("amarillo");
            ColorRoutes.Add("azul");
            ColorRoutes.Add("rojo");
            ColorRoutes.Add("verde");
        }
        while (ColorRoutes.Count < 4)
        {
            ColorRoutes.Add("blanco");
        }

        // sacar las posiciones y rotaciones de cada modulo de todas las rutas
        Transform[][] levels = new Transform[][]
        {
            new Transform[] { routes[0].moduleSlots[0], routes[1].moduleSlots[0], routes[2].moduleSlots[0], routes[3].moduleSlots[0] }, // Level 4
            new Transform[] { routes[0].moduleSlots[1], routes[1].moduleSlots[1], routes[2].moduleSlots[1], routes[3].moduleSlots[1] }, // Level 3
            new Transform[] { routes[0].moduleSlots[2], routes[1].moduleSlots[2], routes[2].moduleSlots[2], routes[3].moduleSlots[2] }, // Level 2
            new Transform[] { routes[0].moduleSlots[3], routes[1].moduleSlots[3], routes[2].moduleSlots[3], routes[3].moduleSlots[3] }  // Level 1
        };

        // Desordenar niveles
        for (int nivel = 0; nivel < levels.Length; nivel++)
        {
            levels[nivel] = Shuffle(levels[nivel]);
        }

        for (int i = 0; i < puzzlePrefabs.Length; i++) //puzzles
        {
            GameObject instanciaPuzzle = Instantiate(puzzlePrefabs[i], RutasParent); // Se crea el puzzle con los 4 modulos           
            PuzzleModule puzzleModule = instanciaPuzzle.GetComponent<PuzzleModule>();

            puzzleModule.ColorIdRute.Value = new FixedString32Bytes(ColorRoutes[i]);
            
            GameObject[] modulosDelPuzle = puzzleModule.moduleSlots; // referencia a los 4 modulos del puzzle

            for (int j = 0; j < 4; j++) // niveles
            {
                Transform referencia = levels[j][i];
                GameObject nivelDelModulo = modulosDelPuzle[j];

                nivelDelModulo.transform.position = referencia.position;
                nivelDelModulo.transform.rotation = referencia.rotation;
                nivelDelModulo.transform.localScale = referencia.localScale;


                referencia.gameObject.SetActive(false);
            }

            instanciaPuzzle.GetComponent<NetworkObject>().Spawn(true);
        }
        yield return null;


        // Limpiar Plantillas
        RutasParent.GetChild(0).gameObject.SetActive(false);
        RutasParent.GetChild(1).gameObject.SetActive(false);
        RutasParent.GetChild(2).gameObject.SetActive(false);
        RutasParent.GetChild(3).gameObject.SetActive(false);
        RutasParent.GetChild(0).GetComponent<NetworkObject>().Despawn(true);
        RutasParent.GetChild(1).GetComponent<NetworkObject>().Despawn(true);
        RutasParent.GetChild(2).GetComponent<NetworkObject>().Despawn(true);
        RutasParent.GetChild(3).GetComponent<NetworkObject>().Despawn(true);

        //Activar Puzzles
        PuzzleModule[] puzzles = RutasParent.GetComponentsInChildren<PuzzleModule>();
        foreach (var puzz in puzzles)
        {
            puzz.IniciarPuzzle();
        }
    }

    private Transform[] Shuffle(Transform[] array)
    {
        Transform[] result = array.ToArray();

        for (int i = 0; i < result.Length; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, result.Length);
            (result[i], result[randomIndex]) = (result[randomIndex], result[i]);
        }

        return result;
    }

    #endregion


}
