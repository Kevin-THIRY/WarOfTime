using UnityEngine;
using UnityEditor;

public enum TerrainSizeType { Petit, Moyen, Grand }

[ExecuteInEditMode]
public class TerrainGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    public TerrainSizeType terrainSize = TerrainSizeType.Moyen;
    [Range(1, 100)] public int detailLevel = 50;
    public int depth = 20;
    [Range(1f, 100f)] public float scale = 20f;

    [Header("Noise Offsets")]
    public float offsetX = 100f;
    public float offsetY = 100f;

    [Header("Thickness Settings")]
    public float thickness = 10f;
    public Material thicknessMaterial;

    [Header("Grid Settings")]
    public float cellSize = 5f; // Taille fixe des carrés
    public float gridHeight = 1f;
    public Color gridColor = Color.black;

    private Terrain terrain;
    private GameObject thicknessMeshObj;
    private GridCell[,] gridCells;
    private int width = 0;

    void OnValidate()
    {
        if (!terrain) terrain = GetComponent<Terrain>();
        width = GetWidthFromType(terrainSize);
    }

    private int GetWidthFromType(TerrainSizeType type)
    {
        int min, max;
        switch (type)
        {
            case TerrainSizeType.Petit:
                min = 23; max = 83; break;
            case TerrainSizeType.Moyen:
                min = 96; max = 151; break;
            case TerrainSizeType.Grand:
                min = 192; max = 287; break;
            default:
                return 128;
        }
        // Interpolation en fonction du detailLevel (0% → min, 100% → max)
        return Mathf.RoundToInt(Mathf.Lerp(min, max, detailLevel / 100f));
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
        terrainData.size = new Vector3(width, depth, width);
        terrainData.SetHeights(0, 0, GenerateHeights());
        return terrainData;
    }

    float[,] GenerateHeights()
    {
        int resolution = Mathf.Max(33, Mathf.ClosestPowerOfTwo(width) + 1);
        float[,] heights = new float[resolution, resolution];
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                float xCoord = (float)x / width * scale + offsetX;
                float yCoord = (float)y / width * scale + offsetY;
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
        float d = width;
        float h = thickness;

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
        // Calcule le nombre de cellules pour remplir la map au mieux
        int gridX = Mathf.RoundToInt(width / cellSize);
        int gridY = Mathf.RoundToInt(width / cellSize);

        // Ajuste la taille des cellules pour que ça match pile avec la map
        float adjustedCellSizeX = width / (float)gridX;
        float adjustedCellSizeY = width / (float)gridY;

        gridCells = new GridCell[gridX + 1, gridY + 1];
        TerrainData terrainData = terrain.terrainData;

        for (int x = 0; x <= gridX; x++)
        {
            for (int y = 0; y <= gridY; y++)
            {
                float worldX = x * adjustedCellSizeX;
                float worldZ = y * adjustedCellSizeY;

                float normalizedX = worldX / width;
                float normalizedZ = worldZ / width;

                float terrainHeight = terrainData.GetHeight(
                    Mathf.RoundToInt(normalizedX * (terrainData.heightmapResolution - 1)),
                    Mathf.RoundToInt(normalizedZ * (terrainData.heightmapResolution - 1))
                );

                gridCells[x, y] = new GridCell(new Vector3(worldX, terrainHeight, worldZ));
            }
        }
    }

    void OnDrawGizmos()
    {
        if (gridCells == null) return;

        Gizmos.color = gridColor;

        int gridX = gridCells.GetLength(0) - 1;
        int gridY = gridCells.GetLength(1) - 1;

        for (int x = 0; x <= gridX; x++)
        {
            for (int y = 0; y <= gridY; y++)
            {
                Vector3 current = gridCells[x, y].position + Vector3.up * gridHeight;

                // Ligne horizontale (vers la droite)
                if (x < gridX)
                {
                    Vector3 right = gridCells[x + 1, y].position + Vector3.up * gridHeight;
                    Gizmos.DrawLine(current, right);
                }

                // Ligne verticale (vers le haut)
                if (y < gridY)
                {
                    Vector3 top = gridCells[x, y + 1].position + Vector3.up * gridHeight;
                    Gizmos.DrawLine(current, top);
                }

                // Bordure droite
                if (x == gridX && y < gridY)
                {
                    Vector3 top = gridCells[x, y + 1].position + Vector3.up * gridHeight;
                    Gizmos.DrawLine(current, top);
                }

                // Bordure supérieure
                if (y == gridY && x < gridX)
                {
                    Vector3 right = gridCells[x + 1, y].position + Vector3.up * gridHeight;
                    Gizmos.DrawLine(current, right);
                }
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
