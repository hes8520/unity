using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[ExecuteInEditMode] // ★ 에디터에서 바로 보이게 하는 마법
public class SectorIndicator : MonoBehaviour
{
    [Range(1, 360)] public float angle = 40f; // 부채꼴 각도
    public float radius = 0.5f;               // 반지름 (기본 0.5로 하면 Scale 1일때 지름 1이 됨)
    public int segments = 50;                 // 곡선 부드러움 정도
    public Color color = new Color(1, 0, 0, 0.3f); // 기본 색상 (빨강 반투명)

    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    void OnEnable()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        CreateMesh();
        UpdateMaterial();
    }

    void OnValidate() // 인스펙터 값 바꿀 때마다 실행
    {
        CreateMesh();
        UpdateMaterial();
    }

    void CreateMesh()
    {
        if (mesh == null) mesh = new Mesh();
        
        // 정점(Vertex) 계산
        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero; // 중심점

        // 각도를 라디안으로 변환 (좌우 대칭을 위해 -angle/2 부터 시작)
        float startAngle = -angle / 2;
        float currentAngle = startAngle;
        float deltaAngle = angle / segments;

        for (int i = 0; i <= segments; i++)
        {
            float rad = Mathf.Deg2Rad * (currentAngle + transform.eulerAngles.y); // 로컬 회전 반영 안함 (메쉬 자체는 정면 기준)
            // 그냥 수학적 계산 (Z축이 앞, X축이 옆)
            float x = Mathf.Sin(Mathf.Deg2Rad * currentAngle) * radius;
            float z = Mathf.Cos(Mathf.Deg2Rad * currentAngle) * radius;

            vertices[i + 1] = new Vector3(x, 0, z);
            currentAngle += deltaAngle;
        }

        // 삼각형(Triangle) 연결
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        if (meshFilter != null) meshFilter.mesh = mesh;
    }

    void UpdateMaterial()
    {
        if (meshRenderer == null) return;
        
        // 머티리얼이 없으면 임시 생성 (쉐이더 문제 방지)
        if (meshRenderer.sharedMaterial == null)
        {
            Material mat = new Material(Shader.Find("Sprites/Default")); // 투명 가능한 쉐이더
            mat.color = color;
            meshRenderer.sharedMaterial = mat;
        }
    }
}