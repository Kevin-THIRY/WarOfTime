using UnityEngine;
using UnityEditor;
using System;

public enum TerrainSizeType { Petit, Moyen, Grand }

[System.Serializable]
public class BiomeParam
{
    public string name;                // Nom du biome (ex: "Plaine", "Montagne") 
    [Range(0f, 1f)] public float minHeight;  // Hauteur minimale du biome
    [Range(0f, 1f)] public float biomeHeight;
    public Color color;                // Couleur associée
    [Range(0f, 1f)] public float noiseFactor;
}
public class Biome
{
    public string name;                // Nom du biome (ex: "Plaine", "Montagne") 
    [Range(0f, 1f)] public float minHeight;  // Hauteur minimale du biome
    public Color color;                // Couleur associée
    [Range(0f, 1f)] public float noiseFactor;
    public float biomeHeight;
}

[ExecuteInEditMode]
public class TerrainGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    public TerrainSizeType terrainSize = TerrainSizeType.Moyen;
    [Range(1, 100)] public int detailLevel = 50;
    public int depth = 20;

    [Header("Biome Settings")]
    public BiomeParam[] biomes;

    [Header("Grid Settings")]
    public float cellSize = 5f; // Taille fixe des carrés
    public float gridHeight = 1f;
    public Color gridColor = Color.black;

    [Header("Noise Settings")]
    [Range(1f, 7f)] public float scale = 2f; // Taille fixe des carrés
    [Range(-100, 100)] public int offset = 0;

    private Terrain terrain;
    private GridCell[,] gridCells;
    private int width = 0;
    private Biome[,] biomeCells;

    void OnValidate()
    {
        if (!terrain) terrain = GetComponent<Terrain>();
        width = GetWidthFromType(terrainSize);
        terrain.terrainData = GenerateTerrainData(terrain.terrainData);
        GenerateLogicalGrid();
        ApplyBiomeColors();
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
        terrainData.SetHeights(0, 0, GenerateHeights(terrainData));
        // GenerateHeights(terrainData);
        return terrainData;
    }
    
    float[,] GenerateHeights(TerrainData terrainData)
    {
        int resolution = terrain.terrainData.heightmapResolution;
        float[,] heights = new float[resolution, resolution];

        // Calcule le nombre de cellules pour remplir la map au mieux
        int gridX = Mathf.RoundToInt(width / cellSize);
        int gridY = Mathf.RoundToInt(width / cellSize);

        // float adjustedCellSizeX = width / (float)gridX;
        // float adjustedCellSizeY = width / (float)gridY;

        // int cellSizeX = Mathf.RoundToInt(adjustedCellSizeX);
        // int cellSizeY = Mathf.RoundToInt(adjustedCellSizeY);

        float gridXOverResolution = resolution / (float)gridX;
        float gridYOverResolution = resolution / (float)gridY;

        float[,] biomeMap = new float[gridY, gridX];

        // float scale = UnityEngine.Random.Range(1f, 3f);
        // int offset = UnityEngine.Random.Range(-100, 100);

        // Génération du bruit de Perlin pour les biomes
        for (int y = 0; y < gridY; y++)
        {
            for (int x = 0; x < gridX; x++)
            {
                float xCoord = (float)x / gridX * scale + offset;
                float yCoord = (float)y / gridY * scale + offset;
                biomeMap[y, x] = Mathf.PerlinNoise(xCoord, yCoord);
                float biomeValue = biomeMap[y, x];
                ConstructBiomeCells(y, x, biomeValue);

                // Remplir le tableau avec la hauteur souhaitée
                for (int i = Mathf.RoundToInt(gridXOverResolution * x); i < Mathf.RoundToInt(gridXOverResolution * (x + 1)); i++)
                {
                    for (int j = Mathf.RoundToInt(gridYOverResolution * y); j < Mathf.RoundToInt(gridYOverResolution * (y + 1)); j++)
                    {
                        heights[i, j] = Mathf.Clamp01(biomeCells[y, x].biomeHeight);
                    }
                }
            }
        }
        return heights;
    }

    void ApplyBiomeColors()
    {
        if (gridCells == null || biomeCells == null) return;

        // Récupérer la résolution complète du terrain
        int resolution = terrain.terrainData.heightmapResolution;
        Texture2D texture = new Texture2D(resolution, resolution);

        // Calcule le nombre de cellules pour remplir la map au mieux
        int gridX = Mathf.RoundToInt(width / cellSize);
        int gridY = Mathf.RoundToInt(width / cellSize);

        float gridXOverResolution = resolution / (float)gridX;
        float gridYOverResolution = resolution / (float)gridY;

        for (int y = 0; y < gridY; y++)
        {
            for (int x = 0; x < gridX; x++)
            {
                // Récupérer la couleur du biome correspondant dans biomeCells
                Color biomeColor = biomeCells[x, y].color;
                // Remplir le tableau avec la hauteur souhaitée
                for (int i = Mathf.RoundToInt(gridXOverResolution * x); i < Mathf.RoundToInt(gridXOverResolution * (x + 1)); i++)
                {
                    for (int j = Mathf.RoundToInt(gridYOverResolution * y); j < Mathf.RoundToInt(gridYOverResolution * (y + 1)); j++)
                    {
                        texture.SetPixel(i, j, biomeColor);
                    }
                }
            }
        }
        // Appliquer les changements sur la texture
        texture.Apply();

        // Créer un TerrainLayer et assigner la texture au terrain
        TerrainLayer layer = new TerrainLayer
        {
            diffuseTexture = texture,
            tileSize = new Vector2(terrain.terrainData.size.x, terrain.terrainData.size.z)
        };

        // Appliquer le TerrainLayer au terrain
        terrain.terrainData.terrainLayers = new TerrainLayer[] { layer };
    }

    void ConstructBiomeCells(int biomeCellX, int BiomeCellY, float heightValue)
    {
        if (biomeCells == null)
        {
            biomeCells = new Biome[width, width];
            // Initialiser chaque cellule avec un biome par défaut
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    biomeCells[x, y] = new Biome(); // Crée une nouvelle instance par défaut
                }
            }
        }
        foreach (BiomeParam biome in biomes)
        {
            if (heightValue >= biome.minHeight)
            {
                biomeCells[biomeCellX, BiomeCellY].name = biome.name;
                biomeCells[biomeCellX, BiomeCellY].minHeight = biome.minHeight;
                biomeCells[biomeCellX, BiomeCellY].color = biome.color;
                biomeCells[biomeCellX, BiomeCellY].biomeHeight = biome.biomeHeight;
                biomeCells[biomeCellX, BiomeCellY].noiseFactor = biome.noiseFactor;
            }
        }
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