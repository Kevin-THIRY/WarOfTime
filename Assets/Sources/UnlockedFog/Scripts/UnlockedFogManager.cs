using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class UnlockedFogManager : MonoBehaviour
{
    [SerializeField] private GameObject unlockedFogBlock;
    // [SerializeField] private LayerMask fogLayer;
    [SerializeField] private float radius = 2f;

    private HashSet<(int, int)> myRevealedCells = new HashSet<(int, int)>();
    
    private float radiusSqr { get { return radius * radius; }}
    private Mesh mesh;
    private Vector3[] vertices;
    private Color[] colors;

    private NetworkObject netObj;

    void Start() {
        netObj = GetComponent<NetworkObject>();

        // Ne faire le setup QUE si on est owner (sinon → hors de ma juridiction)
        if (!netObj.IsOwner) return;

        unlockedFogBlock = FindAnyObjectByType<UnlockedFog>().transform.Find("UnlockedFog").gameObject;

        if (unlockedFogBlock != null)
            Initialize();
    }

    void Update()
    {
        if (unlockedFogBlock == null) return;
        // Ray r = new Ray(transform.position, Vector3.up);
        Ray r = new Ray(transform.position + Vector3.up * unlockedFogBlock.transform.position.y * 4f, Vector3.down * 10f);
        RaycastHit[] hits = Physics.RaycastAll(r, Mathf.Infinity);
        // RaycastHit hit;
        // Debug.DrawRay(transform.position + Vector3.up * unlockedFogBlock.transform.position.y * 4f, Vector3.down * 10f, Color.red, 2);
        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("UnlockedFog"))
            {
                (int centerX, int centerY) = ElementaryBasics.GetGridPositionFromWorldPosition(hit.point);
                HashSet<(int, int)> currentVisibleCells = new HashSet<(int, int)>();
                for (int x = -GetComponent<Unit>().visibility; x <= GetComponent<Unit>().visibility; x++)
                {
                    for (int y = -GetComponent<Unit>().visibility; y <= GetComponent<Unit>().visibility; y++)
                    {
                        if (Mathf.Abs(x) + Mathf.Abs(y) <= GetComponent<Unit>().visibility)
                        {
                            currentVisibleCells.Add((centerX + x, centerY + y));
                        }
                    }
                }

                // Ajoute nouvelles cases visibles globalement
                foreach (var cell in currentVisibleCells)
                {
                    ElementaryBasics.visibleCells.Add(cell);
                }

                // Retire les anciennes qui ne sont plus dans le champ vision
                foreach (var cell in myRevealedCells)
                {
                    if (!currentVisibleCells.Contains(cell))
                    {
                        ElementaryBasics.visibleCells.Remove(cell);
                    }
                }

                // Mets à jour ton cache
                myRevealedCells = currentVisibleCells;

                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 worldV = unlockedFogBlock.transform.TransformPoint(vertices[i]);
                    (int vx, int vy) = ElementaryBasics.GetGridPositionFromWorldPosition(worldV);

                    if (ElementaryBasics.visibleCells.Contains((vx, vy)))
                    {
                        colors[i].a = 0f;
                    }
                    else
                    {
                        colors[i].a = Mathf.MoveTowards(colors[i].a, 0.5f, Time.deltaTime * 2f); // 0.1f = vitesse du "re-fog"
                    }
                }

                UpdateColor();
            }
        }
        foreach (Unit enemy in UnitList.AllUnits)
        {
            if (!UnitList.MyUnitsList.Contains(enemy))
            {
                bool isVisible = ElementaryBasics.visibleCells.Contains(((int)enemy.gridPosition.x, (int)enemy.gridPosition.y));
                SetLayerRecursively(enemy.gameObject, isVisible ? LayerMask.NameToLayer("VisibleToPlayer") : LayerMask.NameToLayer("HiddenFromPlayer"));
            }
        }
    }

    private void Initialize()
    {
        mesh = unlockedFogBlock.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        colors = new Color[vertices.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color(0, 0, 0, 0.5f);
        }
        UpdateColor();
    }

    private void UpdateColor()
    {
        unlockedFogBlock.GetComponent<MeshRenderer>().material.enableInstancing = true;
        mesh.colors = colors;
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}