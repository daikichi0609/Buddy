# if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class PathGizmoDrawer
{
    private static readonly Vector3 OFFSET = new Vector3(0f, 1f, 0f);

    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected, typeof(CharaAi))]
    private static void DrawGizmo(CharaAi ai, GizmoType gizmoType)
    {
        var path = ai.Path;

        // ギズモの描画処理
        Gizmos.color = Color.red;

        for (int i = 0; i < path.Length - 1; i++)
        {
            var startNode = path[i];
            var start = new Vector3(startNode.X, 0f, startNode.Y) + OFFSET;
            var endNode = path[i + 1];
            var end = new Vector3(endNode.X, 0f, endNode.Y) + OFFSET;

            Gizmos.DrawLine(start, end);
            Gizmos.DrawLine(end, (Vector3)end + Quaternion.Euler(0f, 45f, 0f) * (start - end).normalized * 0.5f);
            Gizmos.DrawLine(end, (Vector3)end + Quaternion.Euler(0f, -45f, 0f) * (start - end).normalized * 0.5f);
        }
    }
}
#endif