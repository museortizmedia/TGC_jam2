using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    [Header("Cinemachine")]
    [SerializeField]
    private CinemachineVirtualCameraBase freeLookCamera;
    public Transform cameraTransform;

    public CinemachineVirtualCameraBase FreeLookCamera => freeLookCamera;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}
