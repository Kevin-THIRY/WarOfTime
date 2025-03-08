using UnityEngine;
using UnityEditor;
using System;

public enum ResourcesType { Farm, Wood, Mines }

[System.Serializable]
[ExecuteInEditMode]
public class ResourcesGenerator : MonoBehaviour
{
    [SerializeField] private GameObject treePrefab;
    [SerializeField] private GameObject farmPrefab;
    [SerializeField] private GameObject minePrefab;

    [SerializeField] private float treeSize = 1f;   // Taille des arbres
    [SerializeField] private int treeCount = 10;     // Nombre d'arbres par cellule
    [SerializeField] private float treeRotationMin = 0f;  // Rotation minimale des arbres
    [SerializeField] private float treeRotationMax = 360f; // Rotation maximale des arbres

    private TerrainGenerator terrainGenerator;
    private Terrain terrain;
    private bool treesInstantiated = false;
    private GameObject[] instantiatedTrees;  // Stocke les arbres instanciés pour les supprimer

    void OnValidate()
    {
        terrainGenerator = GetComponent<TerrainGenerator>();
        terrain = GetComponent<Terrain>();
        AddTreePrototype(treePrefab);
    }

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

        // Supprimer les arbres existants avant de rajouter les nouveaux
        if (treesInstantiated)
        {
            DeleteAllTrees();
        }

        int gridX = gridCells.GetLength(0);
        int gridY = gridCells.GetLength(1);
        instantiatedTrees = new GameObject[gridX * gridY * treeCount];  // Ajuste la taille du tableau en fonction du nombre d'arbres

        int treeIndex = 0;

        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++)
            {
                Vector3 position = gridCells[x, y].position;

                // On place des arbres uniquement dans les biomes appropriés
                if (gridCells[x, y].biomeName == BiomeName.Plains && UnityEngine.Random.value < 0.5f)  // Exemple de condition (à ajuster selon ton besoin)
                {
                    for (int i = 0; i < treeCount; i++)
                    {
                        // Calcul de la position aléatoire autour du point central
                        Vector3 randomPosition = position + new Vector3(
                            UnityEngine.Random.Range(-5f, 5f),  // Écart aléatoire sur X
                            0,  // Hauteur constante
                            UnityEngine.Random.Range(-5f, 5f)); // Écart aléatoire sur Z

                        // Instancier l'arbre
                        GameObject tree = Instantiate(treePrefab, randomPosition, Quaternion.identity, transform);

                        // Modifications de taille et rotation
                        tree.transform.localScale = Vector3.one * treeSize; // Taille
                        tree.transform.Rotate(0, UnityEngine.Random.Range(treeRotationMin, treeRotationMax), 0); // Rotation

                        instantiatedTrees[treeIndex] = tree;  // Enregistrer l'arbre dans le tableau
                        treeIndex++;
                    }
                }
            }
        }

        treesInstantiated = true;
    }

    // Fonction pour supprimer tous les arbres instanciés
    public void DeleteAllTrees()
    {
        if (instantiatedTrees != null)
        {
            foreach (GameObject tree in instantiatedTrees)
            {
                if (tree != null)
                {
                    DestroyImmediate(tree);  // Détruire l'arbre immédiatement dans l'éditeur
                }
            }
        }

        treesInstantiated = false;
    }

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

    // Fonction pour supprimer tous les arbres avec un bouton dans l'éditeur Unity
    [ContextMenu("Delete All Trees")]
    private void DeleteAllTreesContextMenu()
    {
        DeleteAllTrees();
    }
}