using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public enum TerrainSizeType { Petit, Moyen, Grand }
public enum BiomeName { Water, Plains, Mountains, Snow, Sand }

[System.Serializable]
public class BiomeParam
{
    public BiomeName name;                // Nom du biome (ex: "Plaine", "Montagne")
    [Range(0f, 1f)] public float minHeight;  // Hauteur minimale du biome
    [Range(0f, 1f)] public float biomeHeight;
    public Color color;                // Couleur associée
    public TerrainLayer terrainLayer; // Texture associé
    [Range(0f, 0.5f)] public float localScale = 0.1f; // Taille fixe des carrés
    [Range(0f, 0.5f)] public float extraLocalScale = 0.1f; // Taille fixe des carrés
}
public class Biome
{
    public BiomeName name;                // Nom du biome (ex: "Plaine", "Montagne")
    public float minHeight;  // Hauteur minimale du biome
    public Color color;                // Couleur associée
    public TerrainLayer terrainLayer; // Texture associé
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
    public Color gridColor = Color.black;

    [Header("Noise Settings")]
    [Range(1f, 7f)] public float scale = 2f; // Taille fixe des carrés
    [Range(-100, 100)] public int offset = 0;

    [Header("Textures")]
    [Range(0f, 0.5f)] public float borderSize = 0.1f;
    [Range(0.01f, 5f)] public float logBase = 2.5f;

    [Header("Water Material")]
    public Material waterMaterial = null;

    [Header("Fog Of War")]
    [SerializeField] private GameObject fogPrefab;
    [SerializeField] private bool displayFogOfWar = false;

    private Terrain terrain;
    private GridCell[,] gridCells;
    private int width = 0;
    private Biome[,] biomeCells;
    public static GameObject fogInstance;

    void OnValidate()
    {
        if (!terrain) terrain = GetComponent<Terrain>();
        width = GetWidthFromType(terrainSize);
        terrain.terrainData = GenerateTerrainData(terrain.terrainData);
        GenerateLogicalGrid();
        // ApplyBiomeTextures();
        ApplyBiomeColors();

        if (fogPrefab != null)
        {
            fogPrefab.SetActive(displayFogOfWar);
            EditorUtility.SetDirty(fogPrefab);
        }
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

    public void GenerateFogOfWar()
    {
        if (fogInstance == null) fogInstance = Instantiate(fogPrefab, transform.position, Quaternion.identity, transform);
        fogInstance.GetComponentInChildren<FogOfWar>().SetResolution(width);
        fogInstance.GetComponentInChildren<FogOfWar>().SetHeight(depth * biomes[biomes.Length - 1].biomeHeight);
        fogInstance.GetComponentInChildren<FogOfWar>().CreateFogBlock();
        // fogInstance.GetComponent<FogOfWar>().RevealArea(player.transform.position, 10f);
    }

    public void GenerateTerrain()
    {
        if (!terrain) terrain = GetComponent<Terrain>();
        width = GetWidthFromType(terrainSize);
        terrain.terrainData = GenerateTerrainData(terrain.terrainData);
        GenerateLogicalGrid();
        // ApplyBiomeTextures();
        ApplyBiomeColors();
    }

    public void ApplyTextures()
    {
        ApplyBiomeTextures();
        CreateGroundBlock();
        
    }

    TerrainData GenerateTerrainData(TerrainData terrainData)
    {
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, depth, width);
        terrainData.SetHeights(0, 0, GenerateHeights(terrainData));
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

                float cyx   = biomeCells[y, x].biomeHeight;
                float cyxm  = (x - 1 > 0) ? biomeCells[y, x - 1].biomeHeight : cyx;
                float cymx  = (y - 1 > 0) ? biomeCells[y - 1, x].biomeHeight : cyx;
                float cymxm = (x - 1 > 0 && y - 1 > 0) ? biomeCells[y - 1, x - 1].biomeHeight : cyx;
                float cyxp  = (x + 1 < gridX) ? biomeCells[y, x + 1].biomeHeight : cyx;
                float cypx  = (y + 1 < gridY) ? biomeCells[y + 1, x].biomeHeight : cyx;
                float cypxp = (x + 1 < gridX && y + 1 < gridY) ? biomeCells[y + 1, x + 1].biomeHeight : cyx;
                float cymxp = (x + 1 < gridX && y - 1 > 0) ? biomeCells[y - 1, x + 1].biomeHeight : cyx;
                float cypxm = (x - 1 > 0 && y + 1 < gridY) ? biomeCells[y + 1, x - 1].biomeHeight : cyx;

                float h00 = (cyx + cymxm + cymx + cyxm) / 4;
                float h01 = (cyx + cypxm + cypx + cyxm) / 4;
                float h10 = (cyx + cyxp + cymx + cymxp) / 4;
                float h11 = (cyx + cyxp + cypx + cypxp) / 4;

                // Remplir le tableau avec la hauteur souhaitée
                for (int i = Mathf.RoundToInt(gridXOverResolution * x); i < Mathf.RoundToInt(gridXOverResolution * (x + 1)); i++)
                {
                    for (int j = Mathf.RoundToInt(gridYOverResolution * y); j < Mathf.RoundToInt(gridYOverResolution * (y + 1)); j++)
                    {
                        // ------------------------ Interpolation avec les cellules voisines ------------------------
                        float tx = (i - gridXOverResolution * x) / gridXOverResolution;
                        float ty = (j - gridYOverResolution * y) / gridYOverResolution;

                        // Bilinear interpolation
                        float avgH = (h00 + h10 + h01 + h11) * 0.25f;
                        float interpolatedHeight = Mathf.Lerp(
                            Mathf.Lerp(h00, h10, tx),
                            Mathf.Lerp(h01, h11, tx),
                            ty
                        ) * 0.75f + avgH * 0.25f; // Ajoute un lissage pour homogénéiser
                        interpolatedHeight = (interpolatedHeight * 0.9f) + (avgH * 0.1f);
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
        terrain.terrainData.terrainLayers = new TerrainLayer[0];

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

    float LogarithmicBlend(float distance, float maxDistance, float baseLog = 2f)
    {
        float normalizedDist = Mathf.Clamp01(distance / maxDistance);
        return 1f - (Mathf.Log(normalizedDist * (baseLog - 1) + 1, baseLog));
    }

    void ApplyBiomeTextures()
    {
        // Transformer en collection 1D avant Distinct()
        HashSet<TerrainLayer> uniqueTerrainLayers = new HashSet<TerrainLayer>();
        int gridX = Mathf.RoundToInt(width / cellSize);
        int gridY = Mathf.RoundToInt(width / cellSize);
        for (int y = 0; y < gridY; y++)
        {
            for (int x = 0; x < gridX; x++)
            {
                uniqueTerrainLayers.Add(biomeCells[y,x].terrainLayer); // HashSet supprime les doublons automatiquement
            }
        }
        // Convertir en tableau
        TerrainLayer[] terrainLayers = uniqueTerrainLayers.ToArray();
                                                 
        if (terrainLayers == null || terrainLayers.Length == 0)
        {
            Debug.LogError("Aucune texture assignée !");
            return;
        }

        TerrainData terrainData = terrain.terrainData;
        terrain.terrainData.terrainLayers = terrainLayers;
        int textureResolution = terrainData.alphamapResolution;
        float[,,] splatmapData = new float[textureResolution, textureResolution, terrainLayers.Length];

        float gridXOverResolution = textureResolution / (float)gridX;
        float gridYOverResolution = textureResolution / (float)gridY;

        for (int y = 0; y < gridY; y++)
        {
            for (int x = 0; x < gridX; x++)
            {
                // Récupérer la couleur du biome correspondant dans biomeCells
                TerrainLayer terrainLayer = biomeCells[y, x].terrainLayer;

                // Récupérer les biomes des cellules adjacentes
                TerrainLayer biomeLeft = (x > 0) ? biomeCells[y, x - 1].terrainLayer : terrainLayer;
                TerrainLayer biomeRight = (x < gridX - 1) ? biomeCells[y, x + 1].terrainLayer : terrainLayer;
                TerrainLayer biomeTop = (y < gridY - 1) ? biomeCells[y + 1, x].terrainLayer : terrainLayer;
                TerrainLayer biomeBottom = (y > 0) ? biomeCells[y - 1, x].terrainLayer : terrainLayer;

                TerrainLayer biomeTopLeft = (x > 0 && y < gridY - 1) ? biomeCells[y + 1, x - 1].terrainLayer : terrainLayer;
                TerrainLayer biomeTopRight = (x < gridX - 1 && y < gridY - 1) ? biomeCells[y + 1, x + 1].terrainLayer : terrainLayer;
                TerrainLayer biomeBottomLeft = (x > 0 && y > 0) ? biomeCells[y - 1, x - 1].terrainLayer : terrainLayer;
                TerrainLayer biomeBottomRight = (x < gridX - 1 && y > 0) ? biomeCells[y - 1, x + 1].terrainLayer : terrainLayer;

                // Remplir le tableau avec la hauteur souhaitée
                for (int i = Mathf.RoundToInt(gridXOverResolution * x); i < Mathf.RoundToInt(gridXOverResolution * (x + 1)); i++)
                {
                    for (int j = Mathf.RoundToInt(gridYOverResolution * y); j < Mathf.RoundToInt(gridYOverResolution * (y + 1)); j++)
                    {
                        if (borderSize != 0)
                        {
                            float relativeX = (i - Mathf.RoundToInt(gridXOverResolution * x)) / (float)(Mathf.RoundToInt(gridXOverResolution * (x + 1)) - Mathf.RoundToInt(gridXOverResolution * x));
                            float relativeY = (j - Mathf.RoundToInt(gridYOverResolution * y)) / (float)(Mathf.RoundToInt(gridYOverResolution * (y + 1)) - Mathf.RoundToInt(gridYOverResolution * y));

                            // Trouver la distance la plus proche à un bord
                            float distLeft = relativeX;
                            float distRight = 1f - relativeX;
                            float distTop = 1f - relativeY;
                            float distBottom = relativeY;

                            // Distance à chaque coin (normée entre 0 et 1)
                            float distTopLeft = Mathf.Sqrt(distLeft * distLeft + distTop * distTop);
                            float distTopRight = Mathf.Sqrt(distRight * distRight + distTop * distTop);
                            float distBottomLeft = Mathf.Sqrt(distLeft * distLeft + distBottom * distBottom);
                            float distBottomRight = Mathf.Sqrt(distRight * distRight + distBottom * distBottom);

                            // Application de la transition logarithmique
                            float weightCenter = 1f;
                            float weightLeft = LogarithmicBlend(distLeft, borderSize, logBase);
                            float weightRight = LogarithmicBlend(distRight, borderSize, logBase);
                            float weightTop = LogarithmicBlend(distTop, borderSize, logBase);
                            float weightBottom = LogarithmicBlend(distBottom, borderSize, logBase);

                            float weightTopLeft = LogarithmicBlend(distTopLeft, borderSize, logBase);
                            float weightTopRight = LogarithmicBlend(distTopRight, borderSize, logBase);
                            float weightBottomLeft = LogarithmicBlend(distBottomLeft, borderSize, logBase);
                            float weightBottomRight = LogarithmicBlend(distBottomRight, borderSize, logBase);

                            // Appliquer les poids aux textures
                            for (int k = 0; k < terrainLayers.Length; k++)
                            {
                                if (terrainLayers[k] == terrainLayer)
                                    splatmapData[i, j, k] += weightCenter;
                                if (terrainLayers[k] == biomeLeft)
                                    splatmapData[i, j, k] += weightLeft;
                                if (terrainLayers[k] == biomeRight)
                                    splatmapData[i, j, k] += 1 - weightRight;
                                if (terrainLayers[k] == biomeTop)
                                    splatmapData[i, j, k] += 1 - weightTop;
                                if (terrainLayers[k] == biomeBottom)
                                    splatmapData[i, j, k] += weightBottom;
                                if (terrainLayers[k] == biomeTopLeft)
                                    splatmapData[i, j, k] += weightTopLeft;
                                if (terrainLayers[k] == biomeTopRight)
                                    splatmapData[i, j, k] += weightTopRight;
                                if (terrainLayers[k] == biomeBottomLeft)
                                    splatmapData[i, j, k] += weightBottomLeft;
                                if (terrainLayers[k] == biomeBottomRight)
                                    splatmapData[i, j, k] += weightBottomRight;
                            }
                        }
                        else
                        {
                            // Appliquer la texture du biome
                            for (int k = 0; k < terrainLayers.Length; k++)
                            {
                                splatmapData[i, j, k] = (terrainLayer == terrainLayers[k]) ? 1f : 0f;
                            }
                        }
                    }
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    void ConstructBiomeCells(int biomeCellX, int biomeCellY, float heightValue)
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
                biomeCells[biomeCellX, biomeCellY].name = biome.name;
                biomeCells[biomeCellX, biomeCellY].minHeight = biome.minHeight;
                biomeCells[biomeCellX, biomeCellY].color = biome.color;
                biomeCells[biomeCellX, biomeCellY].terrainLayer = biome.terrainLayer;
                biomeCells[biomeCellX, biomeCellY].biomeHeight = biome.biomeHeight;
                biomeCells[biomeCellX, biomeCellY].localScale = biome.localScale;
                biomeCells[biomeCellX, biomeCellY].extraLocalScale = biome.extraLocalScale;
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

                gridCells[x, y] = new GridCell(new Vector3(worldX, terrainHeight, worldZ), biomeCells[x, y].name);
            }
        }
    }
    void OnDrawGizmos()
    {
        if (gridCells == null || terrain == null) return;

        Gizmos.color = gridColor;
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPosition = terrain.transform.position;
        
        int gridX = gridCells.GetLength(0) - 1;
        int gridY = gridCells.GetLength(1) - 1;
        
        int segmentCount = 15; // Nombre de segments pour lisser les lignes
        float minWaterHeight = depth * (biomes[0].biomeHeight + biomes[1].biomeHeight) / 2;

        for (int x = 0; x <= gridX; x++)
        {
            for (int y = 0; y <= gridY; y++)
            {
                Vector3 start = gridCells[x, y].position;
                start.y = terrain.SampleHeight(start) + terrainPosition.y + 0.05f;

                // Vérifier si le point est sous l'eau
                if (start.y < minWaterHeight) continue;

                // Ligne horizontale (vers la droite)
                if (x < gridX)
                {
                    Vector3 end = gridCells[x + 1, y].position;
                    end.y = terrain.SampleHeight(end) + terrainPosition.y + 0.05f;

                    if (end.y >= minWaterHeight)
                        DrawCurvedLine(start, end, segmentCount, minWaterHeight);
                    // DrawCurvedLine(start, end, segmentCount);
                }

                // Ligne verticale (vers le haut)
                if (y < gridY)
                {
                    Vector3 end = gridCells[x, y + 1].position;
                    end.y = terrain.SampleHeight(end) + terrainPosition.y + 0.05f;
                    
                    if (end.y >= minWaterHeight)
                        DrawCurvedLine(start, end, segmentCount, minWaterHeight);
                    // DrawCurvedLine(start, end, segmentCount);
                }
            }
        }
    }

    /// <summary>
    /// Trace une ligne courbée entre deux points en échantillonnant le terrain
    /// </summary>
    void DrawCurvedLine(Vector3 start, Vector3 end, int segments, float minWaterHeight)
    {
        Vector3 previousPoint = start;

        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments; // Interpolation entre 0 et 1
            Vector3 point = Vector3.Lerp(start, end, t);

            // Ajuster la hauteur pour suivre la forme du terrain
            point.y = terrain.SampleHeight(point) + terrain.transform.position.y + 0.05f;

            // Vérifier si le point est sous l'eau, ne pas tracer la ligne
            if (point.y < minWaterHeight) return;

            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }
    }

    void CreateGroundBlock()
    {
        // Création du GameObject
        string waterObjectName = "Water";
        float minWaterHeight = depth * (biomes[0].biomeHeight + biomes[1].biomeHeight) / 2;

        // Vérifier si l'objet existe déjà
        Transform existingBlock = transform.Find(waterObjectName);
        if (existingBlock != null)
        {
            existingBlock.position = new Vector3(0, minWaterHeight, 0);
            return;
        }
        // Création du GameObject
        GameObject groundBlock = new GameObject(waterObjectName);
        groundBlock.transform.parent = transform; // Assigner le parent
        groundBlock.transform.localPosition = new Vector3(0, minWaterHeight, 0);
        
        // Ajout du MeshFilter et du MeshRenderer
        MeshFilter meshFilter = groundBlock.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = groundBlock.AddComponent<MeshRenderer>();
        
        // Définition des sommets du bloc (un simple plan avec 4 sommets)
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(0, minWaterHeight, 0),
            new Vector3(width, minWaterHeight, 0),
            new Vector3(0, minWaterHeight, width),
            new Vector3(width, minWaterHeight, width)
        };

        // Définition des triangles (2 triangles pour un quad)
        int[] triangles = new int[]
        {
            0, 2, 1, // Premier triangle
            2, 3, 1  // Deuxième triangle
        };

        // Définition des UVs pour le mapping de texture
        Vector2[] uvs = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };

        // Création du mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        // Application du mesh
        meshFilter.mesh = mesh;
        
        // Application du Material
        meshRenderer.material = waterMaterial;
    }

    public class GridCell
    {
        public readonly Vector3 position;
        public ResourcesType resourceType;
        public readonly BiomeName biomeName;
        public FogState fogState = FogState.Hidden;

        public GridCell(Vector3 position, BiomeName biomeName)
        {
            this.position = position;
            this.biomeName = biomeName;
        }

        public void ResetResource()
        {
            resourceType = ResourcesType.Null;
        }
    }

    #region Getter
    public GridCell[,] GetGridCells() { return gridCells; }
    #endregion
}