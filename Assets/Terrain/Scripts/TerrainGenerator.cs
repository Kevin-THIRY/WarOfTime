using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class TerrainGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    public int width = 256;
    public int height = 256;
    public int depth = 20;
    [Range(1f, 100f)] public float scale = 20f;

    [Header("Noise Offsets")]
    public float offsetX = 100f;
    public float offsetY = 100f;

    [Header("Thickness Settings")]
    public float thickness = 10f;
    public Material thicknessMaterial;

    [Header("Grid Settings")]
    public int gridSize = 10;
    public float gridHeight = 1f;
    public Color gridColor = Color.black;

    private Terrain terrain;
    private GameObject thicknessMeshObj;
    private GridCell[,] gridCells;

    void OnValidate()
    {
        if (!terrain) terrain = GetComponent<Terrain>();
    }

    public void GenerateTerrain()
    {
        if (!terrain) terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrainData(terrain.terrainData);
        GenerateThicknessMesh();
        GenerateLogicalGrid();
    }

    TerrainData GenerateTerrainData(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, depth, height);
        terrainData.SetHeights(0, 0, GenerateHeights());
        return terrainData;
    }

    float[,] GenerateHeights()
    {
        float[,] heights = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xCoord = (float)x / width * scale + offsetX;
                float yCoord = (float)y / height * scale + offsetY;
                if (xCoord == 0) xCoord = 1;
                if (yCoord == 0) yCoord = 1;
                heights[x, y] = Mathf.PerlinNoise(xCoord, yCoord);
            }
        }
        return heights;
    }

    void GenerateThicknessMesh()
    {
        if (thicknessMeshObj) DestroyImmediate(thicknessMeshObj);

        thicknessMeshObj = new GameObject("TerrainThickness");
        thicknessMeshObj.transform.parent = transform;
        thicknessMeshObj.transform.localPosition = Vector3.zero;

        MeshFilter mf = thicknessMeshObj.AddComponent<MeshFilter>();
        MeshRenderer mr = thicknessMeshObj.AddComponent<MeshRenderer>();

        mr.material = thicknessMaterial ? thicknessMaterial : new Material(Shader.Find("Standard"));
        mf.mesh = CreateThicknessMesh();
    }

    Mesh CreateThicknessMesh()
    {
        float w = width;
        float h = thickness;
        float d = height;

        Vector3[] vertices = {
            new Vector3(0, -h, 0), new Vector3(w, -h, 0), new Vector3(w, -h, d), new Vector3(0, -h, d), // bottom
            new Vector3(0, 0, 0), new Vector3(w, 0, 0), new Vector3(w, 0, d), new Vector3(0, 0, d)      // top
        };

        int[] triangles = {
            0, 1, 2, 0, 2, 3, // bottom
            0, 4, 5, 0, 5, 1, // left
            1, 5, 6, 1, 6, 2, // front
            2, 6, 7, 2, 7, 3, // right
            3, 7, 4, 3, 4, 0, // back
            4, 5, 6, 4, 6, 7  // top
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    void GenerateLogicalGrid()
    {
        gridCells = new GridCell[gridSize, gridSize];
        float cellWidth = width / (float)gridSize;
        float cellHeight = height / (float)gridSize;
        TerrainData terrainData = terrain.terrainData;

        for (int x = 0; x <= gridSize; x++)
        {
            for (int y = 0; y <= gridSize; y++)
            {
                float worldX = x * cellWidth;
                float worldZ = y * cellHeight;

                float normalizedX = Mathf.Clamp01(worldX / width);
                float normalizedZ = Mathf.Clamp01(worldZ / height);
                float terrainHeight = terrainData.GetHeight(
                    Mathf.RoundToInt(normalizedX * (terrainData.heightmapResolution - 1)),
                    Mathf.RoundToInt(normalizedZ * (terrainData.heightmapResolution - 1))
                );

                Vector3 worldPosition = new Vector3(worldX, terrainHeight, worldZ);
                gridCells[Mathf.Clamp(x, 0, gridSize - 1), Mathf.Clamp(y, 0, gridSize - 1)] = new GridCell(worldPosition);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (gridCells == null) return;

        Gizmos.color = gridColor;
        float cellWidth = width / (float)gridSize;
        float cellHeight = height / (float)gridSize;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector3 bottomLeft = gridCells[x, y].position + new Vector3(0, gridHeight, 0);
                Vector3 bottomRight = gridCells[Mathf.Min(x + 1, gridSize - 1), y].position + new Vector3(0, gridHeight, 0);
                Vector3 topLeft = gridCells[x, Mathf.Min(y + 1, gridSize - 1)].position + new Vector3(0, gridHeight, 0);
                Vector3 topRight = gridCells[Mathf.Min(x + 1, gridSize - 1), Mathf.Min(y + 1, gridSize - 1)].position + new Vector3(0, gridHeight, 0);

                // Lignes suivant la courbure du terrain
                Gizmos.DrawLine(bottomLeft, bottomRight);
                Gizmos.DrawLine(bottomLeft, topLeft);

                if (x == gridSize - 1) Gizmos.DrawLine(bottomRight, topRight); // Bord droit
                if (y == gridSize - 1) Gizmos.DrawLine(topLeft, topRight);     // Bord supérieur
            }
        }
    }

    public class GridCell
    {
        public Vector3 position;

        public GridCell(Vector3 position)
        {
            this.position = position;
        }
    }
}

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TerrainGenerator generator = (TerrainGenerator)target;
        if (GUILayout.Button("Générer la map"))
        {
            generator.GenerateTerrain();
        }
    }
}
