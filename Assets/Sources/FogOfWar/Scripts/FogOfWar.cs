using UnityEngine;
using System;
using System.Collections.Generic;

public enum FogState { Hidden, Revealed, Visible }

[ExecuteInEditMode]
public class FogOfWar : MonoBehaviour
{
    [SerializeField] private Material fogMaterial;
    private int resolution;
    private Dictionary<(int, int), List<int>> vertexGridMap = new();
    private Color[] vertexColors;
    private Mesh mesh;
    private Color[] colors;

    void Update()
    {
        DiscoverFog();
    }

    private void DiscoverFog()
    {
        if (mesh == null || colors == null || vertexGridMap == null || UnitList.MyUnitsList.Count == 0 || UnitList.MyUnitsList.Count == 0) return;
        
        // 1. Récupère les cases visibles de TOUTES les unités du joueur
        foreach (Unit unit in UnitList.MyUnitsList)
        {
            if (unit == null) break;
            Vector3 worldPos = unit.transform.position;
            (int centerX, int centerY) = ElementaryBasics.GetGridPositionFromWorldPosition(worldPos);

            for (int x = -unit.visibility; x <= unit.visibility; x++)
            {
                for (int y = -unit.visibility; y <= unit.visibility; y++)
                {
                    if (Mathf.Abs(x) + Mathf.Abs(y) <= unit.visibility)
                    {
                        ElementaryBasics.revealedCells.Add((centerX + x, centerY + y));
                    }
                }
            }
        }

        // 3. Mets à jour les vertices affectés
        foreach ((int gx, int gy) in ElementaryBasics.revealedCells)
        {
            if (vertexGridMap.TryGetValue((gx, gy), out var indices))
            {
                foreach (int i in indices)
                {
                    colors[i].a = 0f;
                }
            }
        }

        mesh.colors = colors;
    }

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

        BuildVertexGridMap(mesh, groundBlock.transform);
        colors = mesh.colors;
    }

    private void BuildVertexGridMap(Mesh mesh, Transform meshTransform)
    {
        Vector3[] vertices = mesh.vertices;
        vertexColors = new Color[vertices.Length];

        vertexGridMap.Clear();

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 worldPos = meshTransform.TransformPoint(vertices[i]);
            (int vx, int vy) = ElementaryBasics.GetGridPositionFromWorldPosition(worldPos);
            var key = (vx, vy);

            if (!vertexGridMap.ContainsKey(key))
                vertexGridMap[key] = new List<int>();

            vertexGridMap[key].Add(i);
            vertexColors[i] = new Color(0, 0, 0, 1); // alpha de base
        }

        mesh.colors = vertexColors;
        this.mesh = mesh;
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

    #region Getter
    public Dictionary<(int, int), List<int>> GetVertexGridMap() => vertexGridMap;
    #endregion
}