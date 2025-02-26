using UnityEngine;
using UnityEditor;

public enum TerrainSizeType { Petit, Moyen, Grand }

[System.Serializable]
public class Biome
{
    public string name;                // Nom du biome (ex: "Plaine", "Montagne") 
    [Range(0f, 1f)] public float minHeight;  // Hauteur minimale du biome
    [Range(0f, 1f)] public float maxHeight;  // Hauteur maximale du biome
    public Color color;                // Couleur associée
}

[ExecuteInEditMode]
public class TerrainGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    public TerrainSizeType terrainSize = TerrainSizeType.Moyen;
    [Range(1, 100)] public int detailLevel = 50;
    public int depth = 20;
    [Range(1f, 100f)] public float scale = 20f;

    [Header("Biome Settings")]
    public Biome[] biomes;

    [Header("Noise Offsets")]
    public float offsetX = 100f;
    public float offsetY = 100f;

    [Header("Grid Settings")]
    public float cellSize = 5f; // Taille fixe des carrés
    public float gridHeight = 1f;
    public Color gridColor = Color.black;

    private Terrain terrain;
    private GridCell[,] gridCells;
    private int width = 0;

    void OnValidate()
    {
        if (!terrain) terrain = GetComponent<Terrain>();
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
        width = GetWidthFromType(terrainSize);
        terrain.terrainData = GenerateTerrainData(terrain.terrainData);
        GenerateLogicalGrid();
        ApplyBiomeColors();
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

    void ApplyBiomeColors()
    {
        if (gridCells == null) return;

        // Récupérer la résolution complète du terrain
        int widthResolution = terrain.terrainData.heightmapResolution;
        Texture2D texture = new Texture2D(widthResolution, widthResolution);

        // Parcours chaque pixel de la texture
        for (int x = 0; x < widthResolution; x++)
        {
            for (int y = 0; y < widthResolution; y++)
            {
                // Convertir les coordonnées de la texture en coordonnées du terrain
                float worldX = (x / (float)widthResolution) * terrain.terrainData.size.x;
                float worldZ = (y / (float)widthResolution) * terrain.terrainData.size.z;

                // Récupérer la hauteur du terrain à ces coordonnées
                float terrainHeight = terrain.terrainData.GetHeight(
                    Mathf.RoundToInt(worldX),
                    Mathf.RoundToInt(worldZ)
                );

                // Normaliser la hauteur entre 0 et 1
                float normalizedHeight = terrainHeight / terrain.terrainData.size.y;

                // Appliquer la couleur basée sur la hauteur du terrain
                Color biomeColor = GetBiomeColor(normalizedHeight);
                texture.SetPixel(x, y, biomeColor);
            }
        }

        // Appliquer les changements sur la texture
        texture.Apply();

        // Créer un TerrainLayer et assigner la texture à ce terrain
        TerrainLayer layer = new TerrainLayer
        {
            diffuseTexture = texture,
            tileSize = new Vector2(terrain.terrainData.size.x, terrain.terrainData.size.z)
        };

        // Applique le TerrainLayer au terrain
        terrain.terrainData.terrainLayers = new TerrainLayer[] { layer };
    }

    Color GetBiomeColor(float heightValue)
    {
        foreach (Biome biome in biomes)
        {
            if (heightValue >= biome.minHeight && heightValue <= biome.maxHeight)
            {
                return biome.color;
            }
        }
        return Color.magenta; // Si aucun biome n'est trouvé (debug visuel)
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
