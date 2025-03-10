using UnityEngine;
using System;

public enum FogState { Hidden, Revealed, Visible }

[System.Serializable]
[ExecuteInEditMode]
public class FogOfWar : MonoBehaviour
{
    [SerializeField] private Material fogMaterial;
    private TerrainGenerator.GridCell[,] gridCells;
    private int resolution;
    private float height;

    public void UpdateFogOfWar()
    {
        gridCells = GetComponentInParent<TerrainGenerator>().GetGridCells();
    }

    public void CreateFogBlock()
    {
        // Création du GameObject
        string fogObjectName = "Fog of War";

        // Vérifier si l'objet existe déjà
        Transform existingBlock = transform.Find(fogObjectName);
        if (existingBlock != null)
        {
            existingBlock.position = new Vector3(0, height, 0);
            return;
        }
        // Création du GameObject
        GameObject groundBlock = new GameObject(fogObjectName);
        groundBlock.transform.parent = transform; // Assigner le parent
        groundBlock.transform.localPosition = new Vector3(0, height, 0);
        
        // Ajout du MeshFilter et du MeshRenderer
        MeshFilter meshFilter = groundBlock.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = groundBlock.AddComponent<MeshRenderer>();
        
        // Définition des sommets du bloc (un simple plan avec 4 sommets)
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(0, height, 0),
            new Vector3(resolution, height, 0),
            new Vector3(0, height, resolution),
            new Vector3(resolution, height, resolution)
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
        meshRenderer.material = fogMaterial;

        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    // public void RevealArea(Vector3 worldPos, float radius)
    // {
    //     int centerX = (int)((worldPos.x / resolution) * mapSize);
    //     int centerY = (int)((worldPos.z / resolution) * mapSize);
    //     int pixelRadius = (int)((radius / resolution) * mapSize);

    //     for (int x = -pixelRadius; x <= pixelRadius; x++)
    //     {
    //         for (int y = -pixelRadius; y <= pixelRadius; y++)
    //         {
    //             int px = Mathf.Clamp(centerX + x, 0, mapSize - 1);
    //             int py = Mathf.Clamp(centerY + y, 0, mapSize - 1);
    //             fogData[py * mapSize + px] = Color.white; // Visible
    //         }
    //     }

    //     fogTexture.SetPixels(fogData);
    //     fogTexture.Apply();
    // }
    
    #region Setter
    public void SetResolution(int res) { resolution = res; }
    public void SetHeight(float fogHeight) { height = fogHeight; }
    #endregion
}