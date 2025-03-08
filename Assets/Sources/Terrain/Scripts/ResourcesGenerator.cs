using UnityEngine;
using System;
using System.Collections.Generic;

public enum ResourcesType { Farm, Wood, Mines }

[System.Serializable]
[ExecuteInEditMode]
public class ResourcesGenerator : MonoBehaviour
{
    [SerializeField] private GameObject treePrefab;
    [SerializeField] private GameObject farmPrefab;
    [SerializeField] private GameObject minePrefab;

    [Range(0.05f, 0.3f)] [SerializeField] private float minTreeSize = 0.1f;   // Taille min des arbres
    [Range(0.1f, 0.5f)] [SerializeField] private float maxTreeSize = 0.3f;   // Taille max des arbres
    [Range(1, 30)] [SerializeField] private int treesPerCell = 10;     // Nombre d'arbres par cellule
    [Range(0f, 1f)] [SerializeField] private float centerBias = 0.5f;     // Nombre d'arbres par cellule

    private TerrainGenerator terrainGenerator;
    private Terrain terrain;

    void OnValidate()
    {
        terrainGenerator = GetComponent<TerrainGenerator>();
        terrain = GetComponent<Terrain>();
        AddTreePrototype(treePrefab);
        AddTreesToGrid();
    }

    // public void AddTreesToGrid()
    // {
    //     if (terrainGenerator == null)
    //     {
    //         Debug.LogError("TerrainGenerator manquant !");
    //         return;
    //     }

    //     TerrainGenerator.GridCell[,] gridCells = terrainGenerator.GetGridCells();
    //     if (gridCells == null)
    //     {
    //         Debug.LogError("GridCells non initialisé !");
    //         return;
    //     }

    //     terrain.terrainData.treeInstances = new TreeInstance[0]; // Supprime les anciens arbres
    //     TerrainData terrainData = terrain.terrainData;
    //     List<TreeInstance> trees = new List<TreeInstance>();

    //     int gridX = gridCells.GetLength(0);
    //     int gridY = gridCells.GetLength(1);

    //     float terrainWidth = terrainData.size.x;
    //     float terrainHeight = terrainData.size.z;
    //     int heightmapResolution = terrainData.heightmapResolution; // Résolution de la heightmap

    //     float cellWidth = terrainWidth / gridX;
    //     float cellHeight = terrainHeight / gridY;

    //     int resolution = terrainData.heightmapResolution;

    //     float gridXOverResolution = resolution / (float)gridX;
    //     float gridYOverResolution = resolution / (float)gridY;

    //     Debug.Log(resolution);
    //     Debug.Log(gridX);
    //     Debug.Log(gridY);
    //     Debug.Log(gridXOverResolution);
    //     Debug.Log(gridYOverResolution);

    //     for (int x = 0; x < gridX; x++)
    //     {
    //         for (int y = 0; y < gridY; y++)
    //         {
    //             Debug.Log("X : " + x);
    //             Debug.Log("Y : " + y);
    //             float worldX = Mathf.RoundToInt(gridXOverResolution * (x + 0.5f));
    //             float worldZ = Mathf.RoundToInt(gridYOverResolution * (y + 0.5f));
    //             Debug.Log("Center X : " + worldX);
    //             Debug.Log("Center Y : " + worldZ);
    //             // Position réelle du centre de la cellule en unités monde
    //             // float worldX = (x + 0.5f) * cellWidth;
    //             // float worldZ = (y + 0.5f) * cellHeight;

    //             // Convertir en indices heightmap
    //             int heightX = Mathf.RoundToInt((worldX / terrainWidth) * heightmapResolution);
    //             int heightZ = Mathf.RoundToInt((worldZ / terrainHeight) * heightmapResolution);

    //             // Récupérer la hauteur du terrain (assurer qu'on ne dépasse pas les limites)
    //             heightX = Mathf.Clamp(heightX, 0, heightmapResolution - 1);
    //             heightZ = Mathf.Clamp(heightZ, 0, heightmapResolution - 1);
    //             float worldY = terrainData.GetHeight(heightX, heightZ);

    //             // Normaliser les positions pour TreeInstance
    //             float normX = Mathf.InverseLerp(0, resolution, worldX);
    //             float normZ = Mathf.InverseLerp(0, resolution, worldZ);
    //             // float normX = worldX / terrainWidth;
    //             // float normZ = worldZ / terrainHeight;
    //             float normY = worldY / terrainData.size.y;

    //             TreeInstance tree = new TreeInstance();
    //             tree.position = new Vector3(normX, normY, normZ);
    //             tree.prototypeIndex = UnityEngine.Random.Range(0, terrainData.treePrototypes.Length);
    //             tree.widthScale = 0.2f; // Taille fixe
    //             tree.heightScale = 0.2f;
    //             tree.color = Color.white;
    //             tree.lightmapColor = Color.white;

    //             trees.Add(tree);
    //         }
    //     }

    //     terrainData.treeInstances = trees.ToArray();
    // }


    public void AddTreesToGrid()
    {
        if (terrainGenerator == null)
        {
            Debug.LogError("TerrainGenerator manquant !");
            return;
        }

        TerrainGenerator.GridCell[,] gridCells = terrainGenerator.GetGridCells();
        if (gridCells == null)
        {
            Debug.LogError("GridCells non initialisé !");
            return;
        }

        terrain.terrainData.treeInstances = new TreeInstance[0];

        int gridX = gridCells.GetLength(0);
        int gridY = gridCells.GetLength(1);
        TerrainData terrainData = terrain.terrainData;
        List<TreeInstance> trees = new List<TreeInstance>();

        int resolution = terrainData.heightmapResolution;

        float gridXOverResolution = resolution / ((float)gridX + 1);
        float gridYOverResolution = resolution / ((float)gridY + 1);

        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++)
            {
                // Centre de la cellule
                float centerX = (gridXOverResolution * (x + 0.5f)) / resolution;
                float centerZ = (gridYOverResolution * (y + 0.5f)) / resolution;

                for (int t = 0; t < treesPerCell; t++)
                {
                    // Position aléatoire dans la cellule avec un biais vers le centre
                    // float randomX = (gridXOverResolution * x + UnityEngine.Random.Range(0f, gridXOverResolution)) / resolution;
                    // float randomZ = (gridYOverResolution * y + UnityEngine.Random.Range(0f, gridYOverResolution)) / resolution;
                    float randomX = (x * gridXOverResolution + (gridXOverResolution / 2)) / resolution;
                    float randomZ = (y * gridYOverResolution + (gridYOverResolution / 2)) / resolution;

                    // // Vérifie la distance par rapport au centre
                    // float distanceToCenter = Vector2.Distance(new Vector2(randomX, randomZ), new Vector2(centerX, centerZ));
                    // if (distanceToCenter > centerBias) continue; // Ignore si trop loin du centre

                    // Calcul de la hauteur du terrain
                    float normY = terrainData.GetHeight((int)(randomX * resolution), (int)(randomZ * resolution)) / terrainData.size.y;

                    TreeInstance tree = new TreeInstance();
                    tree.position = new Vector3(randomX, normY, randomZ);
                    tree.prototypeIndex = UnityEngine.Random.Range(0, terrainData.treePrototypes.Length);
                    // float scale = UnityEngine.Random.Range(minTreeSize, maxTreeSize);
                    float scale = 0.2f;
                    tree.widthScale = scale;
                    tree.heightScale = scale;
                    tree.color = Color.white;
                    tree.lightmapColor = Color.white;

                    trees.Add(tree);
                }
            }
        }

        terrainData.treeInstances = trees.ToArray();
    }

    // Fonction pour supprimer tous les arbres instanciés
    private void AddTreePrototype(GameObject newTreePrefab)
    {
        // Récupère les prototypes existants
        TreePrototype[] currentPrototypes = terrain.terrainData.treePrototypes;

        // Vérifie si le prototype existe déjà
        foreach (var prototype in currentPrototypes)
        {
            if (prototype.prefab == newTreePrefab) return;
        }

        // Crée un nouveau tableau avec un prototype supplémentaire
        TreePrototype[] newPrototypes = new TreePrototype[currentPrototypes.Length + 1];

        // Copie les anciens prototypes dans le nouveau tableau
        for (int i = 0; i < currentPrototypes.Length; i++)
        {
            newPrototypes[i] = currentPrototypes[i];
        }

        // Ajoute le nouveau prototype d'arbre
        newPrototypes[currentPrototypes.Length] = new TreePrototype();
        newPrototypes[currentPrototypes.Length].prefab = newTreePrefab;

        // Assigne le nouveau tableau à terrainData.treePrototypes
        terrain.terrainData.treePrototypes = newPrototypes;
    }

    // Fonction pour ajouter des arbres au terrain avec un bouton dans l'éditeur Unity
    [ContextMenu("Add Trees to Grid")]
    private void AddTreesContextMenu()
    {
        AddTreesToGrid();
    }
}