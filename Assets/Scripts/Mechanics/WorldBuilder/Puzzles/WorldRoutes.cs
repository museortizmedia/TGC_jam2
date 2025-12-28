using UnityEngine;

[ExecuteAlways]
public class WorldRoutes : MonoBehaviour
{
    private readonly float[] yRotations = { 0f, 90f, 180f, 270f };

    private void OnValidate()
    {
        if (transform.childCount < 4)
        {
            Debug.LogWarning("WorldRoutes requiere exactamente 4 hijos.");
            return;
        }

        for (int i = 0; i < 4; i++)
        {
            Transform child = transform.GetChild(i);
            Vector3 euler = child.localEulerAngles;
            child.localRotation = Quaternion.Euler(euler.x, yRotations[i], euler.z);
        }
    }
}