using System.Linq;
using UnityEngine;

public class MouseShaderController : MonoBehaviour
{
    [SerializeField] private Material highlightMaterial;
    private float cellSize;
    private Camera cam;
    private int resolution;
    private TerrainGenerator.GridCell[,] gridCells;
    private PlayerManager playerManager;
    private bool clickedOnCell;

    void Start()
    {
        clickedOnCell = false;
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) clickedOnCell = true;
        else clickedOnCell = false;
    }

    void FixedUpdate()
    {
        if (highlightMaterial == null || cam == null) return;

        highlightMaterial.SetFloat("_GridLenght", cellSize);

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        // if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer))))
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);

        // Trier les hits par distance si besoin
        // Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));

        foreach (var hit in hits)
        {
            // Vérifie si c'est un plan et si c'est celui que tu cherches
            if (hit.collider.CompareTag("MouseDetection"))
            {
                Vector2 mousePos2D = new Vector2(hit.point.x + hit.normal.x, hit.point.z + hit.normal.z);
                TerrainGenerator.GridCell cell = new TerrainGenerator.GridCell(new Vector3(0, 0, 0), new Vector2(0, 0), BiomeName.Water, 0);
                float minDist = float.MaxValue;

                foreach (TerrainGenerator.GridCell currentCell in gridCells)
                {
                    float dist = Vector2.Distance(mousePos2D, currentCell.center);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        cell = currentCell;
                    }
                }
                // Vector3 adjustedPosition = hit.point + hit.normal; // Décale légèrement
                highlightMaterial.SetVector("_MousePosition", new Vector3(cell.center.x, hit.point.y + hit.normal.y, cell.center.y));
                if (playerManager != null && clickedOnCell)
                {
                    playerManager.SetSelectedCell(cell);
                }
                break; // Si tu veux sortir dès que tu trouves le premier plan
            }
        }
        if (hits.Length == 0) highlightMaterial.SetVector("_MousePosition", new Vector4(-1000, -1000, -1000, -1));

        // if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        // {
        //     if (hit.collider.CompareTag("MouseDetection"))
        //     {
        //         Vector2 mousePos2D = new Vector2(hit.point.x + hit.normal.x, hit.point.z + hit.normal.z);
        //         TerrainGenerator.GridCell cell = new TerrainGenerator.GridCell(new Vector3(0, 0, 0), new Vector2(0, 0), BiomeName.Water, 0);
        //         float minDist = float.MaxValue;

        //         foreach (TerrainGenerator.GridCell currentCell in gridCells)
        //         {
        //             float dist = Vector2.Distance(mousePos2D, currentCell.center);
        //             if (dist < minDist)
        //             {
        //                 minDist = dist;
        //                 cell = currentCell;
        //             }
        //         }
        //         // Vector3 adjustedPosition = hit.point + hit.normal; // Décale légèrement
        //         highlightMaterial.SetVector("_MousePosition", new Vector3(cell.center.x, hit.point.y + hit.normal.y, cell.center.y));
        //         if (playerManager != null && clickedOnCell)
        //         {
        //             playerManager.SetSelectedCell(cell);
        //         }
        //     }
        // }
        // else
        // {
        //     highlightMaterial.SetVector("_MousePosition", new Vector4(-1000, -1000, -1000, -1));
        // }
    }

    public void CreateHighlightBlock(Vector3 sizeTerrain, float[,] heights)
    {
        // Création du GameObject
        string highlightObjectName = "Highlight";

        // Vérifier si l'objet existe déjà
        Transform existingBlock = transform.Find(highlightObjectName);
        if (existingBlock != null)
        {
            MeshFilter existingMeshFilter = existingBlock.GetComponent<MeshFilter>();
            if (existingMeshFilter != null)
            {
                existingMeshFilter.mesh = GenerateMesh(sizeTerrain, heights);
            }
            existingBlock.localPosition = new Vector3(0, 0.2f, 0);
            return;
        }
        // Création du GameObject
        GameObject groundBlock = new GameObject(highlightObjectName);
        groundBlock.transform.parent = transform; // Assigner le parent
        groundBlock.transform.localPosition = new Vector3(0, 0.2f, 0);
        groundBlock.tag = "MouseDetection";
        
        // Ajout du MeshFilter et du MeshRenderer
        MeshFilter meshFilter = groundBlock.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = groundBlock.AddComponent<MeshRenderer>();

        // Génération et assignation du mesh
        Mesh mesh = GenerateMesh(sizeTerrain, heights);
        meshFilter.mesh = mesh;
        meshRenderer.material = highlightMaterial;

        // Ajout du MeshCollider pour interactivité (optionnel)
        MeshCollider meshCollider = groundBlock.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        // Désactive les ombres pour éviter les artefacts
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    public Mesh GenerateMesh(Vector3 sizeTerrain, float[,] heights)
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
    public void SetCellSize(float _cellSize) { cellSize = _cellSize; }
    public void SetGridCell(TerrainGenerator.GridCell[,] _gridCells) { gridCells = _gridCells; }
    public void SetPlayerManager(PlayerManager _playerManager) { playerManager = _playerManager; }
    #endregion
}