using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Circle : MonoBehaviour
{
    [SerializeField] private float radius = 3.0f;
    [SerializeField] private int segments = 60;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh sectorMesh;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
        {
            meshRenderer.sortingLayerName = "Foreground";
            meshRenderer.sortingOrder = 20;
        }

        sectorMesh = new Mesh();
        meshFilter.sharedMesh = sectorMesh;
    }

    public void GenerateSectorBetweenVectors(Vector2 v1, Vector2 v2, Color col)
    {
        if (v1 == Vector2.zero || v2 == Vector2.zero) return;

        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        if (sectorMesh == null)
        {
            sectorMesh = new Mesh();
            meshFilter.sharedMesh = sectorMesh;
        }

        float angle1 = Mathf.Atan2(v1.y, v1.x) * Mathf.Rad2Deg;
        float angle2 = Mathf.Atan2(v2.y, v2.x) * Mathf.Rad2Deg;
        float deltaAngle = Mathf.DeltaAngle(angle1, angle2);

        float startRad = angle1 * Mathf.Deg2Rad;
        float endRad = (angle1 + deltaAngle) * Mathf.Deg2Rad;

        Vector3[] vertices = new Vector3[segments + 2];
        Color[] colors = new Color[segments + 2];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero;
        colors[0] = col;

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = Mathf.Lerp(startRad, endRad, t);

            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            vertices[i + 1] = new Vector3(x, y, 0f);
            colors[i + 1] = col;
        }

        bool isClockwise = deltaAngle < 0;

        for (int i = 0; i < segments; i++)
        {
            int current = i + 1;
            int next = i + 2;

            if (isClockwise)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = next;
                triangles[i * 3 + 2] = current;
            }
            else
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = current;
                triangles[i * 3 + 2] = next;
            }
        }

        sectorMesh.Clear();
        sectorMesh.vertices = vertices;
        sectorMesh.colors = colors;
        sectorMesh.triangles = triangles;
        sectorMesh.RecalculateNormals();
    }

    private void OnDestroy()
    {
        if (sectorMesh != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(sectorMesh);
#else
            Destroy(sectorMesh);
#endif
        }
    }
}