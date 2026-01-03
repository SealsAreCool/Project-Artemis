using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Vision360 : MonoBehaviour
{
    [Header("Vision Settings")]
    public float viewRadius = 8f;
    public int rayCount = 360;
    public LayerMask wallMask;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    private MeshRenderer meshRenderer;

    void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Unlit/Transparent"));
        meshRenderer.material.color = new Color(1f, 1f, 1f, 0f); 
        meshRenderer.material.renderQueue = 3000; 
    }

    void LateUpdate()
    {
        UpdateVisionMesh();
    }

    void UpdateVisionMesh()
    {
        float angleStep = 360f / rayCount;

        vertices = new Vector3[rayCount + 1];
        triangles = new int[rayCount * 3];

        vertices[0] = Vector3.zero; 

        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * angleStep;
            Vector3 dir = DirFromAngle(angle);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, viewRadius, wallMask);
            Vector3 vertex = hit ? transform.InverseTransformPoint(hit.point) : dir * viewRadius;
            vertices[i + 1] = vertex;
        }

        for (int i = 0; i < rayCount; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 2 > rayCount) ? 1 : i + 2;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    Vector3 DirFromAngle(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);
    }
}
