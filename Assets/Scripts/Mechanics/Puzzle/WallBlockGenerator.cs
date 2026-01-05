using UnityEngine;

public class WallBlockGenerator : MonoBehaviour
{
    [Header("Wall Shape (LOCAL SPACE)")]
    [Tooltip("Línea abierta que define la base del muro")]
    [SerializeField] private Vector3[] localLinePoints;

    [Header("Wall Size")]
    [SerializeField] private int height = 4;
    [SerializeField] private int depth = 1;

    [Header("Block Settings")]
    [SerializeField] private GameObject blockPrefab;
    [SerializeField] private Vector3 blockSize = Vector3.one;
    [SerializeField] private float spacing = 0.02f;

    [Header("Generation")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool clearBeforeGenerate = true;

    void Start()
    {
        if (generateOnStart)
            Generate();
    }

    [ContextMenu("Generate Wall")]
    public void Generate()
    {
        if (blockPrefab == null || localLinePoints == null || localLinePoints.Length < 2)
        {
            Debug.LogWarning("WallBlockGenerator: línea inválida", this);
            return;
        }

        if (clearBeforeGenerate)
            Clear();

        for (int i = 0; i < localLinePoints.Length - 1; i++)
        {
            GenerateSegment(
                localLinePoints[i],
                localLinePoints[i + 1]
            );
        }
    }

    void GenerateSegment(Vector3 start, Vector3 end)
    {
        Vector3 direction = (end - start);
        float length = direction.magnitude;
        direction.Normalize();

        float step = blockSize.x + spacing;
        int count = Mathf.FloorToInt(length / step);

        Quaternion rotation = Quaternion.LookRotation(direction);

        for (int i = 0; i <= count; i++)
        {
            Vector3 basePos = start + direction * i * step;

            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    Vector3 localPos =
                        basePos +
                        Vector3.up * y * (blockSize.y + spacing) +
                        rotation * Vector3.right * z * (blockSize.z + spacing);

                    GameObject block = Instantiate(blockPrefab, transform);
                    block.transform.localPosition = localPos;
                    block.transform.localRotation = rotation;
                    block.transform.localScale = blockSize;

                    // IDENTIFICADOR PARA EL MURO
                    block.tag = "WallBlock";
                }
            }
        }
    }

    [ContextMenu("Clear Wall")]
    public void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                Destroy(transform.GetChild(i).gameObject);
            else
                DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    // ---------- GIZMOS ----------

    void OnDrawGizmos()
    {
        if (localLinePoints == null || localLinePoints.Length < 2)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < localLinePoints.Length - 1; i++)
        {
            Vector3 a = transform.TransformPoint(localLinePoints[i]);
            Vector3 b = transform.TransformPoint(localLinePoints[i + 1]);
            Gizmos.DrawLine(a, b);
        }
    }
}