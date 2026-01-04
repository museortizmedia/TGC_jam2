using UnityEngine;

public class LocalMovingPlatform : MonoBehaviour
{
    ZonePlatformSpawner zone;

    Vector3 velocity;
    float speed;

    LocalMovingPlatform[] allPlatforms;

    [Header("Vertical Motion")]
    [SerializeField] float heightAmplitude = 0.25f;
    [SerializeField] float heightSpeed = 0.8f;
    [SerializeField] float platformRadius = 0.6f;

    Vector3 startLocalPos;
    bool initialized;

    public void Initialize(
        Vector3 startPos,
        float moveSpeed,
        LocalMovingPlatform[] platformsInZone
    )
    {
        zone = GetComponentInParent<ZonePlatformSpawner>();
        if (zone == null) return;

        transform.localPosition = startPos;
        startLocalPos = startPos;

        speed = moveSpeed;
        allPlatforms = platformsInZone;

        // Dirección inicial PREDECIBLE
        Vector3[] dirs =
        {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right
        };

        velocity = dirs[Random.Range(0, dirs.Length)].normalized * speed;

        initialized = true;
    }

    void Update()
    {
        if (!initialized) return;

        Vector3 nextPos =
            transform.localPosition +
            velocity * Time.deltaTime;

        // Rebote con bordes del polígono
        if (!IsInsidePolygon(nextPos, zone.GetLocalAreaPoints()))
        {
            Vector3 normal = GetClosestEdgeNormal(
                transform.localPosition,
                zone.GetLocalAreaPoints()
            );

            velocity =
                Vector3.Reflect(velocity, normal).normalized * speed;

            nextPos =
                transform.localPosition +
                velocity * Time.deltaTime;
        }

        // Rebote con otras plataformas
        CheckPlatformCollision(ref nextPos);

        float yOffset =
            Mathf.Sin(Time.time * heightSpeed) * heightAmplitude;

        transform.localPosition =
            new Vector3(
                nextPos.x,
                startLocalPos.y + yOffset,
                nextPos.z
            );
    }

    bool CheckPlatformCollision(ref Vector3 nextPos)
    {
        foreach (var other in allPlatforms)
        {
            if (other == this) continue;

            Vector3 otherPos = other.transform.localPosition;
            Vector3 toOther = nextPos - otherPos;
            float dist = toOther.magnitude;

            float minDist = platformRadius * 2f;

            if (dist < minDist && dist > 0.0001f)
            {
                Vector3 normal = toOther.normalized;

                // reflejamos la velocidad
                velocity =
                    Vector3.Reflect(velocity, normal).normalized * speed;

                // empujamos fuera para evitar solape
                float penetration = minDist - dist;
                nextPos += normal * penetration;

                return true;
            }
        }
        return false;
    }

    bool IsInsidePolygon(Vector3 point, Vector3[] poly)
    {
        bool inside = false;
        int j = poly.Length - 1;

        for (int i = 0; i < poly.Length; i++)
        {
            if (((poly[i].z > point.z) != (poly[j].z > point.z)) &&
                (point.x < (poly[j].x - poly[i].x) *
                 (point.z - poly[i].z) /
                 (poly[j].z - poly[i].z) + poly[i].x))
            {
                inside = !inside;
            }
            j = i;
        }
        return inside;
    }

    Vector3 GetClosestEdgeNormal(Vector3 pos, Vector3[] poly)
    {
        float minDist = float.MaxValue;
        Vector3 bestNormal = Vector3.zero;

        for (int i = 0; i < poly.Length; i++)
        {
            Vector3 a = poly[i];
            Vector3 b = poly[(i + 1) % poly.Length];

            Vector3 edge = b - a;
            Vector3 normal = new Vector3(-edge.z, 0, edge.x).normalized;

            float dist =
                Mathf.Abs(Vector3.Cross(edge, pos - a).magnitude /
                          edge.magnitude);

            if (dist < minDist)
            {
                minDist = dist;
                bestNormal = normal;
            }
        }

        return bestNormal;
    }
}