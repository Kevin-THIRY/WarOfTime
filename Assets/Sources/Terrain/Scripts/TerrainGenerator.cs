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
    [Range(0f, 0.5f)] public float localScale = 0.1f; // Taille fixe des carrés
    [Range(0f, 0.5f)] public float extraLocalScale = 0.1f; // Taille fixe des carrés
}
public class Biome
{
    public string name;                // Nom du biome (ex: "Plaine", "Montagne")
    public float minHeight;  // Hauteur minimale du biome
    public Color color;                // Couleur associée
    public float biomeHeight;
    public float localScale = 0.1f; // Taille fixe des carrés
    public float extraLocalScale = 0.1f; // Taille fixe des carrés
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

    [Header("Textures")]
    public TerrainLayer[] terrainLayers;

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
        ApplyBiomeTextures();
        // ApplyBiomeColors();
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
        ApplyBiomeTextures();
        // ApplyBiomeColors();
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

                // Centre de la cellule actuelle
                float centerX = (gridXOverResolution * (x + 0.5f));
                float centerY = (gridYOverResolution * (y + 0.5f));

                // Remplir le tableau avec la hauteur souhaitée
                for (int i = Mathf.RoundToInt(gridXOverResolution * x); i < Mathf.RoundToInt(gridXOverResolution * (x + 1)); i++)
                {
                    for (int j = Mathf.RoundToInt(gridYOverResolution * y); j < Mathf.RoundToInt(gridYOverResolution * (y + 1)); j++)
                    {
                        // ------------------------ Interpolation avec les cellules voisines ------------------------
                        float tx = (i - gridXOverResolution * x) / gridXOverResolution;
                        float ty = (j - gridYOverResolution * y) / gridYOverResolution;

                        float h00 = biomeCells[y, x].biomeHeight;
                        float h10 = (x + 1 < gridX) ? biomeCells[y, x + 1].biomeHeight : h00;
                        float h01 = (y + 1 < gridY) ? biomeCells[y + 1, x].biomeHeight : h00;
                        float h11 = (x + 1 < gridX && y + 1 < gridY) ? biomeCells[y + 1, x + 1].biomeHeight : h00;

                        // Bilinear interpolation
                        float interpolatedHeight = Mathf.Lerp(
                            Mathf.Lerp(h00, h10, tx),
                            Mathf.Lerp(h01, h11, tx),
                            ty
                        );
                        // -------------------------------------------------------------------------------------------

                        // ------------------------ ajout détail ------------------------
                        float perlinX = (i + offset) * scale * 0.1f;  // Ajustement pour éviter des valeurs trop brusques
                        float perlinY = (j + offset) * scale * 0.1f;
                        float noise = Mathf.PerlinNoise(perlinX, perlinY) * biomeCells[y, x].localScale;  // Modulation par noiseFactor

                        interpolatedHeight += noise;  // Ajout du bruit de Perlin
                        // ---------------------------------------------------------------

                        // ------------------------ ajout extra détail au centre de la case ------------------------
                        // Distance normalisée du point au centre de la cellule (0 au centre, ~1 sur les bords)
                        float distToCenterX = Mathf.Abs(i - centerX) / (gridXOverResolution * 0.5f);
                        float distToCenterY = Mathf.Abs(j - centerY) / (gridYOverResolution * 0.5f);
                        float distToCenter = Mathf.Sqrt(distToCenterX * distToCenterX + distToCenterY * distToCenterY);

                        // Appliquer une courbe pour lisser l'effet (ex: cosinus ou 1 - (x²))
                        float attenuation = Mathf.Clamp01(1f - distToCenter * distToCenter); // Plus fort au centre

                        // Second Perlin Noise pour encore plus de détail
                        float perlinX2 = (i + offset) * scale * 0.2f;
                        float perlinY2 = (j + offset) * scale * 0.2f;
                        float extraDetail = Mathf.PerlinNoise(perlinX2, perlinY2) * biomeCells[y, x].extraLocalScale * attenuation;

                        // Ajouter cet effet au terrain
                        interpolatedHeight += extraDetail;
                        // -----------------------------------------------------------------------------------------

                        heights[i, j] = Mathf.Clamp01(interpolatedHeight);
                        // heights[i, j] = Mathf.Clamp01(biomeCells[y, x].biomeHeight);
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
    void ApplyBiomeTextures()
    {
        if (terrainLayers == null || terrainLayers.Length == 0)
        {
            Debug.LogError("Aucune texture assignée !");
            return;
        }

        TerrainData terrainData = terrain.terrainData;
        terrain.terrainData.terrainLayers = terrainLayers;
        int resolution = terrainData.alphamapResolution;
        float[,,] splatmapData = new float[resolution, resolution, terrainLayers.Length];

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float normX = (float)x / (resolution - 1);
                float normY = (float)y / (resolution - 1);

                float height = terrainData.GetHeight(
                    Mathf.RoundToInt(normY * (terrainData.heightmapResolution - 1)),
                    Mathf.RoundToInt(normX * (terrainData.heightmapResolution - 1))
                ) / depth;

                // Trouver le biome correspondant
                string biomeName = GetBiomeForHeight(height);
                if (biomeName == null) continue;

                // Appliquer la texture du biome
                for (int i = 0; i < terrainLayers.Length; i++)
                {
                    splatmapData[x, y, i] = (biomeName == terrainLayers[i].name) ? 1f : 0f;
                }
            }
        }

        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    // Fonction qui interpole les poids entre les biomes
    float[] GetBiomeWeights(float height)
    {
        float[] weights = new float[terrainLayers.Length];

        for (int i = 0; i < biomes.Length; i++)
        {
            BiomeParam biome = biomes[i];

            // Influence du biome en fonction de la hauteur
            float distance = Mathf.Abs(height - biome.minHeight);
            float influence = Mathf.Clamp01(1f - (distance / 0.1f)); // 0.1 = zone de transition

            weights[i] = influence;
        }

        return weights;
    }

    string GetBiomeForHeight(float height)
    {
        string name = null;
        foreach (BiomeParam biome in biomes)
        {
            if (height >= biome.biomeHeight)
            {
                name = biome.name;
            }
        }
        if (name != null) return name;
        else return null;
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
                biomeCells[biomeCellX, BiomeCellY].localScale = biome.localScale;
                biomeCells[biomeCellX, BiomeCellY].extraLocalScale = biome.extraLocalScale;
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
        if (gridCells == null || terrain == null) return;

        Gizmos.color = gridColor;
        TerrainData terrainData = terrain.terrainData;
        int resolution = terrainData.heightmapResolution - 1;

        int gridX = gridCells.GetLength(0) - 1;
        int gridY = gridCells.GetLength(1) - 1;

        for (int x = 0; x <= gridX; x++)
        {
            for (int y = 0; y <= gridY; y++)
            {
                Vector3 current = GetTerrainPoint(gridCells[x, y].position, terrainData, resolution);

                // Ligne horizontale (vers la droite)
                if (x < gridX)
                {
                    Vector3 right = GetTerrainPoint(gridCells[x + 1, y].position, terrainData, resolution);
                    Gizmos.DrawLine(current, right);
                }

                // Ligne verticale (vers le haut)
                if (y < gridY)
                {
                    Vector3 top = GetTerrainPoint(gridCells[x, y + 1].position, terrainData, resolution);
                    Gizmos.DrawLine(current, top);
                }
            }
        }
    }

    // Récupère un point ajusté à la hauteur du terrain
    Vector3 GetTerrainPoint(Vector3 position, TerrainData terrainData, int resolution)
    {
        float normalizedX = position.x / terrainData.size.x;
        float normalizedZ = position.z / terrainData.size.z;

        float height = terrainData.GetHeight(
            Mathf.RoundToInt(normalizedX * resolution),
            Mathf.RoundToInt(normalizedZ * resolution)
        );

        return new Vector3(position.x, height + gridHeight, position.z);
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