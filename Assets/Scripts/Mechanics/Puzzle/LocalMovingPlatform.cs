using UnityEngine;

public class LocalMovingPlatform : MonoBehaviour
{
    [Header("Local Movement")]
    [SerializeField] Vector3 localDirection = Vector3.right;
    [SerializeField] float distance = 2f;
    [SerializeField] float speed = 1f;

    [Header("Vertical Motion")]
    [SerializeField] float heightAmplitude = 0.3f;
    [SerializeField] float heightSpeed = 1f;

    Vector3 startLocalPos;

    void Awake()
    {
        startLocalPos = transform.localPosition;
    }

    void Update()
    {
        float t = Mathf.PingPong(Time.time * speed, 1f);

        Vector3 horizontal =
            startLocalPos +
            localDirection.normalized * distance * (t - 0.5f);

        float yOffset =
            Mathf.Sin(Time.time * heightSpeed) * heightAmplitude;

        transform.localPosition = new Vector3(
            horizontal.x,
            startLocalPos.y + yOffset,
            horizontal.z
        );
    }
}