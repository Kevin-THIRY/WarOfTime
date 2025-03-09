using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public enum ResourcesType { Null, Farm, Wood, GoldMines, IronMines }


public class BiomeResourceMapping
{
    private static readonly Dictionary<BiomeName, List<ResourcesType>> biomeResources = new Dictionary<BiomeName, List<ResourcesType>>()
    {
        { BiomeName.Water, new List<ResourcesType> { } },
        { BiomeName.Plains, new List<ResourcesType> { ResourcesType.Wood, ResourcesType.Farm, ResourcesType.GoldMines } },
        { BiomeName.Mountains, new List<ResourcesType> { ResourcesType.IronMines } },
        { BiomeName.Snow, new List<ResourcesType> { } }
    };

    public static bool IsResourceInBiome(ResourcesType resource, BiomeName biome)
    {
        return biomeResources.ContainsKey(biome) && biomeResources[biome].Contains(resource);
    }
    public static List<BiomeName> GetBiomesForResource(ResourcesType resource)
    {
        return biomeResources
            .Where(kvp => kvp.Value.Contains(resource))
            .Select(kvp => kvp.Key)
            .ToList();
    }
}

[System.Serializable]
public class ResourcesClass
{
    public ResourcesType resourcesType;
    public GameObject prefab;
    [Range(0f, 1f)] public float density = 0.1f; // Taille fixe des carrés
    [SerializeField] public float minSize = 0.1f;   // Taille min des arbres
    [SerializeField] public float maxSize = 0.3f;   // Taille max des arbres
    [Range(1, 100)] [SerializeField] public int elementPerCell = 10;     // Nombre d'arbres par cellule
    [Range(0f, 1f)] [SerializeField] public float radius = 0.5f;     // Rayon autour du centre
    [NonSerialized] public int index;
}

[ExecuteInEditMode]
public class ResourcesGenerator : MonoBehaviour
{
    [SerializeField] private ResourcesClass[] resources;

    private TerrainGenerator terrainGenerator;
    private Terrain terrain;
    private TerrainGenerator.GridCell[,] gridCells;
    private List<TreeInstance> trees;

    void OnValidate()
    {
        terrainGenerator = GetComponent<TerrainGenerator>();
        terrain = GetComponent<Terrain>();
        gridCells = terrainGenerator.GetGridCells();
        ResetResourcesFromGrid(gridCells);
        if (terrain) terrain.terrainData.treePrototypes = new TreePrototype[0];
        foreach (var resource in resources) AddTreePrototype(resource);
        foreach (var resource in resources) AddResourcesToGrid(resource);
    }

    public void GenerateResources()
    {
        terrainGenerator = GetComponent<TerrainGenerator>();
        terrain = GetComponent<Terrain>();
        gridCells = terrainGenerator.GetGridCells();
        ResetResourcesFromGrid(gridCells);
        if (terrain) terrain.terrainData.treePrototypes = new TreePrototype[0];
        foreach (var resource in resources) AddTreePrototype(resource);
        foreach (var resource in resources) AddResourcesToGrid(resource);
    }

    private void ResetResourcesFromGrid(TerrainGenerator.GridCell[,] gridCells)
    {
        foreach (var cell in gridCells) cell.ResetResource();
        terrain.terrainData.treeInstances = new TreeInstance[0];
        trees = new List<TreeInstance>();
    }

    private void AddResourcesToGrid(ResourcesClass resource)
    {
        if (terrainGenerator == null)
        {
            Debug.LogError("TerrainGenerator manquant !");
            return;
        }

        if (gridCells == null)
        {
            Debug.LogError("GridCells non initialisé !");
            return;
        }

        List<BiomeName> biomes = BiomeResourceMapping.GetBiomesForResource(resource.resourcesType);

        int gridX = gridCells.GetLength(0) - 1;
        int gridY = gridCells.GetLength(1) - 1;
        TerrainData terrainData = terrain.terrainData;

        int resolution = terrainData.heightmapResolution;

        float gridXOverResolution = resolution / ((float)gridX);
        float gridYOverResolution = resolution / ((float)gridY);

        float maxRadius = Mathf.Min(gridXOverResolution, gridYOverResolution) * resource.radius / (resolution * 2);

        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++)
            {
                foreach (BiomeName biome in biomes)
                {
                    if (gridCells[x, y].biomeName == biome && UnityEngine.Random.Range(0f, 1f) > 1 - resource.density && gridCells[x, y].resourceType == ResourcesType.Null)
                    {
                        gridCells[x, y].resourceType = resource.resourcesType;
                        // Centre de la cellule
                        float centerX = (x * gridXOverResolution + (gridXOverResolution / 2)) / resolution;
                        float centerZ = (y * gridYOverResolution + (gridYOverResolution / 2)) / resolution;

                        for (int t = 0; t < resource.elementPerCell; t++)
                        {
                            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2);
                            float radius = UnityEngine.Random.Range(0f, maxRadius); // Rayon limité par radius
                            float offsetX = Mathf.Cos(angle) * radius;
                            float offsetZ = Mathf.Sin(angle) * radius;

                            // Position dans la cellule avec une zone autour du centre
                            float randomX = centerX + offsetX;
                            float randomZ = centerZ + offsetZ;

                            // Calcul de la hauteur du terrain
                            float normY = terrainData.GetHeight((int)(randomX * resolution), (int)(randomZ * resolution)) / terrainData.size.y;

                            TreeInstance tree = new TreeInstance();
                            tree.position = new Vector3(randomX, normY, randomZ);
                            tree.prototypeIndex = resource.index;
                            float scale = UnityEngine.Random.Range(resource.minSize, resource.maxSize);
                            tree.widthScale = scale;
                            tree.heightScale = scale;
                            tree.color = Color.white;
                            tree.lightmapColor = Color.white;

                            trees.Add(tree);
                        }
                    }
                }
            }
        }

        terrainData.treeInstances = trees.ToArray();
    }

    // Fonction pour supprimer tous les arbres instanciés
    private void AddTreePrototype(ResourcesClass resource)
    {
        // Récupère les prototypes existants
        TreePrototype[] currentPrototypes = terrain.terrainData.treePrototypes;

        // Vérifie si le prototype existe déjà
        foreach (var prototype in currentPrototypes)
        {
            if (prototype.prefab == resource.prefab) return;
        }

        resource.index = Array.FindIndex(resources, r => r.resourcesType == resource.resourcesType);

        // Crée un nouveau tableau avec un prototype supplémentaire
        TreePrototype[] newPrototypes = new TreePrototype[currentPrototypes.Length + 1];

        // Copie les anciens prototypes dans le nouveau tableau
        for (int i = 0; i < currentPrototypes.Length; i++)
        {
            newPrototypes[i] = currentPrototypes[i];
        }

        // Ajoute le nouveau prototype
        newPrototypes[currentPrototypes.Length] = new TreePrototype();
        newPrototypes[currentPrototypes.Length].prefab = resource.prefab;

        // Assigne le nouveau tableau à terrainData.treePrototypes
        terrain.terrainData.treePrototypes = newPrototypes;
    }
}