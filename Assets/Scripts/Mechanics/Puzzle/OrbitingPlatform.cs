using UnityEngine;

public class OrbitingPlatform : MonoBehaviour
{
    private Transform center;
    private float radius;
    private float orbitSpeed;
    private bool clockwise;

    private float heightAmplitude;
    private float heightSpeed;

    private float angle;
    private float baseHeight;

    public void Initialize(
        Transform center,
        float radius,
        float orbitSpeed,
        bool clockwise,
        float heightAmplitude,
        float heightSpeed,
        float startAngle
    )
    {
        this.center = center;
        this.radius = radius;
        this.orbitSpeed = orbitSpeed;
        this.clockwise = clockwise;
        this.heightAmplitude = heightAmplitude;
        this.heightSpeed = heightSpeed;
        this.angle = startAngle;

        baseHeight = transform.position.y;
    }

    void Update()
    {
        if (!center) return;

        float direction = clockwise ? -1f : 1f;
        angle += orbitSpeed * direction * Time.deltaTime;

        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;
        float yOffset = Mathf.Sin(Time.time * heightSpeed) * heightAmplitude;

        transform.position = center.position + new Vector3(x, baseHeight + yOffset, z);
    }
}