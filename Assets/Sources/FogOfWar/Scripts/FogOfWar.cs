using UnityEngine;
using System;

public enum FogState { Hidden, Revealed, Visible }

[ExecuteInEditMode]
public class FogOfWar : MonoBehaviour
{
    [SerializeField] private Material fogMaterial;
    private int resolution;
    // private float height;

    // public void CreateFogBlock()
    // {
    //     // Création du GameObject
    //     string fogObjectName = "Fog of War";

    //     // Vérifier si l'objet existe déjà
    //     Transform existingBlock = transform.Find(fogObjectName);
    //     if (existingBlock != null)
    //     {
    //         existingBlock.position = new Vector3(0, 1.1f, 0); //new Vector3(0, height, 0);
    //         return;
    //     }
    //     // Création du GameObject
    //     GameObject groundBlock = new GameObject(fogObjectName);
    //     groundBlock.transform.parent = transform; // Assigner le parent
    //     groundBlock.transform.localPosition = new Vector3(0, 1.1f, 0); //new Vector3(0, height, 0);
    //     groundBlock.tag = "FogPlane";
        
    //     // Ajout du MeshFilter et du MeshRenderer
    //     MeshFilter meshFilter = groundBlock.AddComponent<MeshFilter>();
    //     MeshRenderer meshRenderer = groundBlock.AddComponent<MeshRenderer>();

    //     int size = resolution + 1; // Nombre de sommets par ligne/colonne
    //     Vector3[] vertices = new Vector3[size * size];
    //     int[] triangles = new int[resolution * resolution * 6];

    //     // Générer les sommets
    //     for (int z = 0; z < size; z++)
    //     {
    //         for (int x = 0; x < size; x++)
    //         {
    //             vertices[z * size + x] = new Vector3(x, height, z);
    //         }
    //     }

    //     // Générer les triangles
    //     int triIndex = 0;
    //     for (int z = 0; z < resolution; z++)
    //     {
    //         for (int x = 0; x < resolution; x++)
    //         {
    //             int start = z * size + x;
    //             triangles[triIndex++] = start;
    //             triangles[triIndex++] = start + size;
    //             triangles[triIndex++] = start + 1;

    //             triangles[triIndex++] = start + 1;
    //             triangles[triIndex++] = start + size;
    //             triangles[triIndex++] = start + size + 1;
    //         }
    //     }

    //     // Création du mesh
    //     Mesh mesh = new Mesh();
    //     mesh.vertices = vertices;
    //     mesh.triangles = triangles;
    //     // mesh.uv = uvs;
    //     mesh.RecalculateNormals();

    //     // Application du mesh
    //     meshFilter.mesh = mesh;
        
    //     // Application du Material
    //     meshRenderer.material = fogMaterial;
    //     MeshCollider mc = GetComponent<MeshCollider>();
    //     if (mc == null) mc = groundBlock.AddComponent<MeshCollider>();
    //     mc.sharedMesh = mesh;

    //     meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    // }

    public void CreateFogBlock(Vector3 sizeTerrain, float[,] heights)
    {
        // Création du GameObject
        string highlightObjectName = "Fog of War";

        // Vérifier si l'objet existe déjà
        Transform existingBlock = transform.Find(highlightObjectName);
        if (existingBlock != null)
        {
            MeshFilter existingMeshFilter = existingBlock.GetComponent<MeshFilter>();
            if (existingMeshFilter != null)
            {
                existingMeshFilter.mesh = GenerateMesh(sizeTerrain, heights);
            }
            existingBlock.localPosition = new Vector3(0, 3f, 0);
            return;
        }
        // Création du GameObject
        GameObject groundBlock = new GameObject(highlightObjectName);
        groundBlock.transform.parent = transform; // Assigner le parent
        groundBlock.transform.localPosition = new Vector3(0, 3f, 0);
        groundBlock.tag = "FogPlane";
        
        // Ajout du MeshFilter et du MeshRenderer
        MeshFilter meshFilter = groundBlock.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = groundBlock.AddComponent<MeshRenderer>();

        // Génération et assignation du mesh
        Mesh mesh = GenerateMesh(sizeTerrain, heights);
        meshFilter.mesh = mesh;
        meshRenderer.material = fogMaterial;

        // Ajout du MeshCollider pour interactivité (optionnel)
        MeshCollider meshCollider = groundBlock.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        // Désactive les ombres pour éviter les artefacts
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    private Mesh GenerateMesh(Vector3 sizeTerrain, float[,] heights)
    {
        Vector3[] vertices = new Vector3[resolution * resolution];
        Vector2[] uvs = new Vector2[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];

        float scaleX = sizeTerrain.x / (resolution - 1);
        float scaleZ = sizeTerrain.z / (resolution - 1);
        float scaleY = sizeTerrain.y;

        // Génération des vertices et UVs
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int index = z * resolution + x;
                float y = heights[z, x] * scaleY;
                vertices[index] = new Vector3(x * scaleX, y, z * scaleZ);
                uvs[index] = new Vector2((float)x / resolution, (float)z / resolution);
            }
        }

        // Génération des triangles
        int triIndex = 0;
        for (int z = 0; z < resolution - 1; z++)
        {
            for (int x = 0; x < resolution - 1; x++)
            {
                int index = z * resolution + x;

                triangles[triIndex++] = index;
                triangles[triIndex++] = index + resolution;
                triangles[triIndex++] = index + 1;

                triangles[triIndex++] = index + 1;
                triangles[triIndex++] = index + resolution;
                triangles[triIndex++] = index + resolution + 1;
            }
        }

        // Création du mesh
        Mesh mesh = new Mesh
        {
            vertices = vertices,
            triangles = triangles,
            uv = uvs
        };
        // mesh.Clear(); // Nettoyer avant assignation
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
    
    #region Setter
    public void SetResolution(int res) { resolution = res; }
    // public void SetHeight(float fogHeight) { height = fogHeight; }
    #endregion
}