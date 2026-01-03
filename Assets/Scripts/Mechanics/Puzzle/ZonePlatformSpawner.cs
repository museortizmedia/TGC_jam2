using UnityEngine;

public class ZonePlatformSpawner : MonoBehaviour
{
    [Header("Platform Setup")]
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private int platformCount = 3;
    [SerializeField] private float yHeight = 0f;

    [Header("Zone Shape (LOCAL SPACE)")]
    [Tooltip("Define el área cerrada donde pueden aparecer plataformas")]
    [SerializeField] private Vector3[] localAreaPoints;

    void Start()
    {
        if (localAreaPoints == null || localAreaPoints.Length < 3)
        {
            Debug.LogWarning("Zona inválida: se necesitan al menos 3 puntos");
            return;
        }

        for (int i = 0; i < platformCount; i++)
        {
            Vector3 localPos = GetRandomPointInPolygon();
            localPos.y = yHeight;

            GameObject platform = Instantiate(platformPrefab, transform);
            platform.transform.localPosition = localPos;
            platform.transform.localRotation = platformPrefab.transform.localRotation;
        }
    }


    void OnDrawGizmos()
    {
        if (localAreaPoints == null || localAreaPoints.Length < 3)
            return;

        Gizmos.color = new Color(0f, 1f, 1f, 0.9f);

        for (int i = 0; i < localAreaPoints.Length; i++)
        {
            Vector3 a = transform.TransformPoint(localAreaPoints[i]);
            Vector3 b = transform.TransformPoint(
                localAreaPoints[(i + 1) % localAreaPoints.Length]
            );

            Gizmos.DrawLine(a, b);
        }
    }

    Vector3 GetRandomPointInPolygon()
    {
        // Método simple y seguro para JAM:
        // triangulamos en abanico desde el punto 0

        int triangleIndex = Random.Range(1, localAreaPoints.Length - 1);

        Vector3 a = localAreaPoints[0];
        Vector3 b = localAreaPoints[triangleIndex];
        Vector3 c = localAreaPoints[triangleIndex + 1];

        return GetRandomPointInTriangle(a, b, c);
    }

    Vector3 GetRandomPointInTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        float r1 = Mathf.Sqrt(Random.value);
        float r2 = Random.value;

        return
            (1 - r1) * a +
            r1 * (1 - r2) * b +
            r1 * r2 * c;
    }
}