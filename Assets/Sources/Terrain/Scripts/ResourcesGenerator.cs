using UnityEngine;
using System;

public enum ResourcesType { Farm, Wood, Mines }

public class ResourcesGenerator : MonoBehaviour
{
    [SerializeField] private TerrainGenerator terrainGenerator; // Référence au script TerrainGenerator
    [SerializeField] private Transform resourceContainer; // Parent des ressources

    [SerializeField] private GameObject treePrefab;
    [SerializeField] private GameObject farmPrefab;
    [SerializeField] private GameObject minePrefab;

    [Range(0f, 1f)] public float treeSpawnRate = 0.5f;
    [Range(0f, 1f)] public float farmSpawnRate = 0.3f;
    [Range(0f, 1f)] public float mineSpawnRate = 0.2f;

    private void Start()
    {
        if (terrainGenerator == null)
        {
            Debug.LogError("TerrainGenerator non assigné !");
            return;
        }

        GenerateResources();
    }

    public void GenerateResources()
    {
        TerrainGenerator.GridCell[,] gridCells = terrainGenerator.GetGridCells();
        if (gridCells == null)
        {
            Debug.LogError("GridCells non initialisé !");
            return;
        }

        int gridX = gridCells.GetLength(0);
        int gridY = gridCells.GetLength(1);

        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++)
            {
                string biome = gridCells[x, y].biomeName;
                Vector3 position = gridCells[x, y].position;

                if (biome == "Plaine" && UnityEngine.Random.value < farmSpawnRate)
                {
                    Instantiate(farmPrefab, position, Quaternion.identity, resourceContainer);
                }
                else if (biome == "Forêt" && UnityEngine.Random.value < treeSpawnRate)
                {
                    Instantiate(treePrefab, position, Quaternion.identity, resourceContainer);
                }
                else if (biome == "Montagne" && UnityEngine.Random.value < mineSpawnRate)
                {
                    Instantiate(minePrefab, position, Quaternion.identity, resourceContainer);
                }
            }
        }
        Debug.Log("Génération des ressources terminée !");
    }
}