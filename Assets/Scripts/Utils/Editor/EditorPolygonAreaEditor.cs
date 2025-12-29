using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class EditorPolygonAreaGizmos
{
    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
    static void DrawPolygonGizmo(EditorPolygonArea area, GizmoType gizmoType)
    {
        if (area.points == null || area.points.Count < 3)
            return;

        Transform t = area.transform;
        int count = area.points.Count;

        Vector3[] worldPoints = new Vector3[count];
        for (int i = 0; i < count; i++)
            worldPoints[i] = t.TransformPoint(area.points[i]);

        // ===== RELLENO (triangulación cóncava) =====
        Handles.color = area.fillColor;

        List<int> indices = Triangulate(worldPoints);

        for (int i = 0; i < indices.Count; i += 3)
        {
            Handles.DrawAAConvexPolygon(
                worldPoints[indices[i]],
                worldPoints[indices[i + 1]],
                worldPoints[indices[i + 2]]
            );
        }

        // ===== CONTORNO =====
        Handles.color = area.outlineColor;
        Handles.DrawAAPolyLine(3f, ClosePolygon(worldPoints));
    }

    static Vector3[] ClosePolygon(Vector3[] points)
    {
        Vector3[] closed = new Vector3[points.Length + 1];
        points.CopyTo(closed, 0);
        closed[points.Length] = points[0];
        return closed;
    }

    // ======================================================
    // TRIANGULACIÓN EAR CLIPPING
    // ======================================================

    static List<int> Triangulate(Vector3[] vertices)
    {
        List<int> indices = new List<int>();

        int n = vertices.Length;
        if (n < 3)
            return indices;

        int[] V = new int[n];

        if (Area(vertices) > 0)
        {
            for (int i = 0; i < n; i++)
                V[i] = i;
        }
        else
        {
            for (int i = 0; i < n; i++)
                V[i] = (n - 1) - i;
        }

        int nv = n;
        int count = 2 * nv;
        int m = 0;
        int v = nv - 1;

        while (nv > 2)
        {
            if ((count--) <= 0)
                break;

            int u = v;
            if (nv <= u) u = 0;
            v = u + 1;
            if (nv <= v) v = 0;
            int w = v + 1;
            if (nv <= w) w = 0;

            if (Snip(vertices, u, v, w, nv, V))
            {
                int a = V[u];
                int b = V[v];
                int c = V[w];

                indices.Add(a);
                indices.Add(b);
                indices.Add(c);
                m++;

                for (int s = v, t = v + 1; t < nv; s++, t++)
                    V[s] = V[t];

                nv--;
                count = 2 * nv;
            }
        }

        return indices;
    }

    static float Area(Vector3[] vertices)
    {
        int n = vertices.Length;
        float A = 0.0f;

        for (int p = n - 1, q = 0; q < n; p = q++)
        {
            Vector3 pval = vertices[p];
            Vector3 qval = vertices[q];
            A += pval.x * qval.z - qval.x * pval.z;
        }

        return A * 0.5f;
    }

    static bool Snip(Vector3[] vertices, int u, int v, int w, int n, int[] V)
    {
        Vector3 A = vertices[V[u]];
        Vector3 B = vertices[V[v]];
        Vector3 C = vertices[V[w]];

        if (Mathf.Epsilon > (((B.x - A.x) * (C.z - A.z)) - ((B.z - A.z) * (C.x - A.x))))
            return false;

        for (int p = 0; p < n; p++)
        {
            if (p == u || p == v || p == w)
                continue;

            Vector3 P = vertices[V[p]];
            if (PointInTriangle(A, B, C, P))
                return false;
        }

        return true;
    }

    static bool PointInTriangle(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
    {
        float ax = C.x - B.x;
        float az = C.z - B.z;
        float bx = A.x - C.x;
        float bz = A.z - C.z;
        float cx = B.x - A.x;
        float cz = B.z - A.z;

        float apx = P.x - A.x;
        float apz = P.z - A.z;
        float bpx = P.x - B.x;
        float bpz = P.z - B.z;
        float cpx = P.x - C.x;
        float cpz = P.z - C.z;

        float aCROSSbp = ax * bpz - az * bpx;
        float cCROSSap = cx * apz - cz * apx;
        float bCROSScp = bx * cpz - bz * cpx;

        return (aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f);
    }
}