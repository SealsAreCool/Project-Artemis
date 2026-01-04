using UnityEngine;

public class VisionCircle : MonoBehaviour
{
    public float radius = 6f;
    public int rayCount = 180;
    public LayerMask wallMask;

    Mesh mesh;

    void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void LateUpdate()
    {
        GenerateMesh();
    }

    void GenerateMesh()
    {
        Vector3[] vertices = new Vector3[rayCount + 1];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = Vector3.zero;

        float angleStep = 360f / rayCount;

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = angleStep * i;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad),
                                      Mathf.Sin(angle * Mathf.Deg2Rad));

            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, radius, wallMask);

            Vector3 point = hit.collider
                ? transform.InverseTransformPoint(hit.point)
                : (Vector3)(dir * radius);

            vertices[i] = point;
        }

        int triIndex = 0;
        for (int i = 1; i < rayCount; i++)
        {
            triangles[triIndex++] = 0;
            triangles[triIndex++] = i;
            triangles[triIndex++] = i + 1;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }
}
