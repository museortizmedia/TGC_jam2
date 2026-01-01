using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[ExecuteAlways]
#endif
public class EditorPolygonArea : MonoBehaviour
{
    [Header("Polygon Settings")]
    public Color fillColor = new Color(1f, 0f, 0f, 0.25f);
    public Color outlineColor = Color.red;

    [Min(3)]
    public int initialPoints = 4;

    [Header("Polygon Points (Local Space)")]
    public List<Vector3> points = new List<Vector3>();

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (points == null)
            points = new List<Vector3>();

        if (points.Count < 3)
        {
            GenerateDefaultPolygon();
        }
    }

    private void GenerateDefaultPolygon()
    {
        points.Clear();

        float radius = 1f;
        for (int i = 0; i < initialPoints; i++)
        {
            float angle = i * Mathf.PI * 2f / initialPoints;
            points.Add(new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            ));
        }
    }
#endif
}


#if UNITY_EDITOR
[CustomEditor(typeof(EditorPolygonArea))]
public class EditorPolygonAreaEditor : Editor
{
    private EditorPolygonArea area;

    private void OnEnable()
    {
        area = (EditorPolygonArea)target;
    }

    private void OnSceneGUI()
    {
        if (!area.enabled)
        return;
        
        if (area.points == null || area.points.Count < 3)
            return;

        Transform t = area.transform;

        EditorGUI.BeginChangeCheck();

        for (int i = 0; i < area.points.Count; i++)
        {
            Vector3 worldPos = t.TransformPoint(area.points[i]);

            // Dial de posición
            Vector3 newWorldPos = Handles.PositionHandle(
                worldPos,
                Quaternion.identity
            );

            if (newWorldPos != worldPos)
            {
                Undo.RecordObject(area, "Move Polygon Point");
                area.points[i] = t.InverseTransformPoint(newWorldPos);
                EditorUtility.SetDirty(area);
            }
        }

        EditorGUI.EndChangeCheck();
    }
}
#endif