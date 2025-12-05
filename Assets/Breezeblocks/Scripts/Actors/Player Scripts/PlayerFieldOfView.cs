using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds a 2D field-of-view mesh using Physics2D.Raycast against an obstacle layer.
/// This object does NOT rotate itself – it uses its own Transform.
/// Put it as a child of the VisionLight so it shares the same rotation.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PlayerFieldOfView : MonoBehaviour
{
    #region Variables and Properties
    [FoldoutGroup("Geometry", expanded: true)]
    [SerializeField] private float viewRadius = 8f;
    [FoldoutGroup("Geometry", expanded: true)]
    [SerializeField] [Range(0f, 360f)] private float viewAngle = 120f;
    [FoldoutGroup("Geometry", expanded: true)]
    [SerializeField] [Range(16, 512)] private int rayCount = 200;
    [FoldoutGroup("Geometry", expanded: true)]
    [SerializeField] private Vector2 originOffset = Vector2.zero;

    [FoldoutGroup("Settings", expanded: true)]
    [SerializeField] private LayerMask obstacleMask;

    private Mesh _mesh;
    #endregion

    // ==============================================================

    private void Awake()
    {
        _mesh = new Mesh();
        _mesh.name = "FOV Mesh";
        GetComponent<MeshFilter>().mesh = _mesh;
    }

    private void LateUpdate()
    {
        DrawFieldOfView();
    }

    // ==============================================================

    private void DrawFieldOfView()
    {
        int count = Mathf.Max(2, rayCount);

        float angle = viewAngle;
        float step = angle / (count - 1);
        float half = angle * 0.5f;

        Vector3 originWorld = transform.position + (Vector3)originOffset;

        List<Vector3> worldPoints = new List<Vector3>(count);

        for (int i = 0; i < count; i++)
        {
            // Local angle around this transform's "right" axis
            float localAngle = -half + step * i;
            Quaternion rot = Quaternion.Euler(0f, 0f, localAngle);
            Vector3 dir = rot * transform.right; // uses this transform's rotation

            RaycastHit2D hit = Physics2D.Raycast(originWorld, dir, viewRadius, obstacleMask);
            Vector3 end = hit ? (Vector3)hit.point : originWorld + dir * viewRadius;

            worldPoints.Add(end);
        }

        int vertexCount = worldPoints.Count + 1; // + origin
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        // local origin
        vertices[0] = Vector3.zero;

        for (int i = 0; i < worldPoints.Count; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(worldPoints[i]);
        }

        int triIndex = 0;
        for (int i = 0; i < vertexCount - 2; i++)
        {
            triangles[triIndex++] = 0;
            triangles[triIndex++] = i + 1;
            triangles[triIndex++] = i + 2;
        }

        _mesh.Clear();
        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        _mesh.RecalculateBounds();
    }

    // ==============================================================
}
