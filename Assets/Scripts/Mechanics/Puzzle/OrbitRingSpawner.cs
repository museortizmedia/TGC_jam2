using UnityEngine;

public class OrbitRingSpawner : MonoBehaviour
{
    [Header("Platform Setup")]
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private int platformCount = 6;
    [SerializeField] private float radius = 4f;

    [Header("Movement")]
    [SerializeField] private float orbitSpeed = 1f;
    [SerializeField] private bool clockwise = true;

    [Header("Vertical Motion")]
    [SerializeField] private float heightAmplitude = 0.5f;
    [SerializeField] private float heightSpeed = 1f;

    [SerializeField] private Transform center;

    void Start()
    {
        SpawnPlatforms();
    }

    void SpawnPlatforms()
    {
        float angleStep = Mathf.PI * 2f / platformCount;

        for (int i = 0; i < platformCount; i++)
        {
            float angle = angleStep * i;

            Vector3 pos = center.position + new Vector3(
                Mathf.Cos(angle) * radius,
                transform.position.y,
                Mathf.Sin(angle) * radius
            );

            GameObject platform = Instantiate(platformPrefab, pos, platformPrefab.transform.rotation, transform);

            OrbitingPlatform orbit = platform.GetComponent<OrbitingPlatform>();
            orbit.Initialize(
                center,
                radius,
                orbitSpeed,
                clockwise,
                heightAmplitude,
                heightSpeed,
                angle
            );
        }
    }
}