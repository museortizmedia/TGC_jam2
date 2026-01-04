using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [Header("Player Safety")]
    [SerializeField] private Transform player;
    [SerializeField] private float playerSafeRadius = 1.5f;

    [Header("Maze Cell Settings")]
    [SerializeField] private float cellSize = 2f;

    [Header("Zone Shape (LOCAL SPACE)")]
    [SerializeField] private Vector3[] localAreaPoints;

    [Header("Wall Setup")]
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private Transform wallsParent;
    [SerializeField] private float perimeterWallThickness = 0.3f;

    [Header("Target / Button")]
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private int hubRadius = 2;
    [SerializeField] private int extraConnections = 6;

    private bool[,] maze;
    private Vector2Int targetCell;
    private List<GameObject> spawnedWalls = new();

    void Start()
    {
        if (localAreaPoints == null || localAreaPoints.Length < 3)
        {
            Debug.LogWarning("MazeGenerator: zona inválida");
            return;
        }

        GenerateAndBuild();
    }

    // ---------- MAIN FLOW ----------

    void GenerateAndBuild()
    {
        Bounds bounds = GetLocalBounds();

        int width = Mathf.FloorToInt(bounds.size.x / cellSize);
        int height = Mathf.FloorToInt(bounds.size.z / cellSize);

        width = Mathf.Max(width | 1, 3);
        height = Mathf.Max(height | 1, 3);

        GenerateMaze(width, height);

        targetCell = GetRandomTargetCell();
        CarveHub(targetCell, hubRadius);
        ConnectToTarget(targetCell, extraConnections);

        BuildPolygonWalls();

        BuildMaze(bounds);
        SpawnButton(bounds, targetCell);
    }

    // ---------- MAZE GENERATION ----------

    void GenerateMaze(int width, int height)
    {
        maze = new bool[width, height];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                maze[x, y] = true;

        Stack<Vector2Int> stack = new();
        Vector2Int current = new(1, 1);

        maze[1, 1] = false;
        stack.Push(current);

        Vector2Int[] dirs =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        while (stack.Count > 0)
        {
            current = stack.Pop();
            List<Vector2Int> neighbors = new();

            foreach (var d in dirs)
            {
                Vector2Int next = current + d * 2;
                if (IsInsideGrid(next, width, height) && maze[next.x, next.y])
                    neighbors.Add(d);
            }

            if (neighbors.Count == 0) continue;

            stack.Push(current);
            Vector2Int dir = neighbors[Random.Range(0, neighbors.Count)];

            Vector2Int wall = current + dir;
            Vector2Int nextCell = current + dir * 2;

            maze[wall.x, wall.y] = false;
            maze[nextCell.x, nextCell.y] = false;

            stack.Push(nextCell);
        }
    }

    bool IsInsideGrid(Vector2Int p, int w, int h)
    {
        return p.x > 0 && p.y > 0 && p.x < w - 1 && p.y < h - 1;
    }

    bool IsNearPlayer(Vector3 localPos)
    {
        if (!player) return false;

        Vector3 playerLocal = transform.InverseTransformPoint(player.position);
        return Vector3.Distance(playerLocal, localPos) < playerSafeRadius;
    }

    // ---------- TARGET LOGIC ----------

    Vector2Int GetRandomTargetCell()
    {
        int w = maze.GetLength(0);
        int h = maze.GetLength(1);

        for (int i = 0; i < 100; i++)
        {
            int x = Random.Range(2, w - 2);
            int y = Random.Range(2, h - 2);

            if (x % 2 == 0) x++;
            if (y % 2 == 0) y++;

            Vector3 localPos = GridToLocalPosition(GetLocalBounds(), x, y);
            if (IsPointInsidePolygon(localPos))
                return new Vector2Int(x, y);
        }

        return new Vector2Int(w / 2, h / 2);
    }

    void CarveHub(Vector2Int center, int radius)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                int cx = center.x + x;
                int cy = center.y + y;

                if (cx >= 0 && cy >= 0 &&
                    cx < maze.GetLength(0) &&
                    cy < maze.GetLength(1))
                {
                    maze[cx, cy] = false;
                }
            }
        }
    }

    void ConnectToTarget(Vector2Int target, int connections)
    {
        int w = maze.GetLength(0);
        int h = maze.GetLength(1);

        for (int i = 0; i < connections; i++)
        {
            Vector2Int start = new(
                Random.Range(1, w - 1),
                Random.Range(1, h - 1)
            );

            CarvePath(start, target);
        }
    }

    void CarvePath(Vector2Int from, Vector2Int to)
    {
        Vector2Int current = from;

        while (current != to)
        {
            maze[current.x, current.y] = false;

            if (Random.value > 0.5f)
                current.x += (int)Mathf.Sign(to.x - current.x);
            else
                current.y += (int)Mathf.Sign(to.y - current.y);
        }

        maze[to.x, to.y] = false;
    }

    // ---------- BUILD ----------

    void BuildMaze(Bounds bounds)
    {
        for (int x = 0; x < maze.GetLength(0); x++)
        {
            for (int y = 0; y < maze.GetLength(1); y++)
            {
                if (!maze[x, y]) continue;

                Vector3 localPos = GridToLocalPosition(bounds, x, y);
                if (!IsPointInsidePolygon(localPos)) continue;

                if (IsNearPlayer(localPos))
                {
                    maze[x, y] = false; // libera la celda
                    continue;
                }

                GameObject wall = Instantiate(wallPrefab, wallsParent);
                wall.transform.localPosition = localPos;
                wall.transform.localScale = new Vector3(
                    cellSize,
                    wall.transform.localScale.y,
                    cellSize
                );
            }
        }
    }

    void BuildWallSegment(Vector3 start, Vector3 end)
    {
        Vector3 mid = (start + end) * 0.5f;
    Vector3 dir = end - start;
    float length = dir.magnitude;

    GameObject wall = Instantiate(wallPrefab, wallsParent);

    wall.transform.localPosition = mid;
    wall.transform.localRotation = Quaternion.LookRotation(dir.normalized);

    wall.transform.localScale = new Vector3(
        perimeterWallThickness,          // grosor
        wall.transform.localScale.y,     // altura
        length                            // longitud
    );

    spawnedWalls.Add(wall);
    }

    void BuildPolygonWalls()
    {
        for (int i = 0; i < localAreaPoints.Length; i++)
        {
            Vector3 a = localAreaPoints[i];
            Vector3 b = localAreaPoints[(i + 1) % localAreaPoints.Length];

            BuildWallSegment(a, b);
        }
    }

    void SpawnButton(Bounds bounds, Vector2Int cell)
    {
        if (!buttonPrefab) return;

        Vector3 pos = GridToLocalPosition(bounds, cell.x, cell.y);
        Instantiate(buttonPrefab, transform.TransformPoint(pos), Quaternion.identity, transform);
    }

    Vector3 GridToLocalPosition(Bounds bounds, int x, int y)
    {
        return new Vector3(
            bounds.min.x + x * cellSize,
            0f,
            bounds.min.z + y * cellSize
        );
    }

    // ---------- POLYGON UTILS ----------

    Bounds GetLocalBounds()
    {
        Bounds b = new(localAreaPoints[0], Vector3.zero);
        for (int i = 1; i < localAreaPoints.Length; i++)
            b.Encapsulate(localAreaPoints[i]);
        return b;
    }

    bool IsPointInsidePolygon(Vector3 point)
    {
        bool inside = false;

        for (int i = 0, j = localAreaPoints.Length - 1; i < localAreaPoints.Length; j = i++)
        {
            Vector3 pi = localAreaPoints[i];
            Vector3 pj = localAreaPoints[j];

            if (((pi.z > point.z) != (pj.z > point.z)) &&
                (point.x < (pj.x - pi.x) * (point.z - pi.z) / (pj.z - pi.z) + pi.x))
            {
                inside = !inside;
            }
        }
        return inside;
    }

    // ---------- GIZMOS ----------

    void OnDrawGizmos()
    {
        if (localAreaPoints == null || localAreaPoints.Length < 3) return;

        Gizmos.color = Color.yellow;

        for (int i = 0; i < localAreaPoints.Length; i++)
        {
            Vector3 a = transform.TransformPoint(localAreaPoints[i]);
            Vector3 b = transform.TransformPoint(localAreaPoints[(i + 1) % localAreaPoints.Length]);
            Gizmos.DrawLine(a, b);
        }
    }

}