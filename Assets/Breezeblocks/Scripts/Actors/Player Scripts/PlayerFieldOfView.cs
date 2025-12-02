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
    [Header("Geometry")]
    [Tooltip("How far the player can see (for occlusion).")]
    public float viewRadius = 8f;

    [Tooltip("Angle of the vision cone in degrees.")]
    [Range(0f, 360f)]
    public float viewAngle = 120f;

    [Tooltip("Number of rays. Higher = smoother, more expensive.")]
    [Range(16, 512)]
    public int rayCount = 200;

    [Header("Obstacles")]
    [Tooltip("Layers that block line of sight.")]
    public LayerMask obstacleMask;

    [Tooltip("Offset from this transform's position, e.g. if you want origin at feet/head.")]
    public Vector2 originOffset = Vector2.zero;

    private Mesh _mesh;

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
}
