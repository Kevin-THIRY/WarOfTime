using UnityEngine;
using Unity.Netcode;

public class FogLayerManager : MonoBehaviour
{
    [SerializeField] private GameObject forOfWarPlane;
    // [SerializeField] private LayerMask fogLayer;
    [SerializeField] private float radius = 2f;
    
    private float radiusSqr { get { return radius * radius; }}
    private Mesh mesh;
    private Vector3[] vertices;
    private Color[] colors;

    private NetworkObject netObj;

    void Start() {
        netObj = GetComponent<NetworkObject>();

        // Ne faire le setup QUE si on est owner (sinon → hors de ma juridiction)
        if (!netObj.IsOwner) return;

        forOfWarPlane = FindAnyObjectByType<FogOfWar>().transform.Find("Fog of War").gameObject;

        if (forOfWarPlane != null)
            Initialize();
    }

    void Update()
    {
        if (forOfWarPlane == null) return;

        Ray r = new Ray(transform.position + Vector3.up * forOfWarPlane.transform.position.y * 5f, Vector3.down * 10f);
        // RaycastHit hit;
        RaycastHit[] hits = Physics.RaycastAll(r, Mathf.Infinity);
        // Debug.DrawRay(transform.position + Vector3.up * forOfWarPlane.transform.position.y * 4f, Vector3.down * 5f, Color.red, 2);
        // if (Physics.Raycast(r, out hit, 10, fogLayer, QueryTriggerInteraction.Collide))
        // if (Physics.Raycast(r, out hit, 10, LayerMask.GetMask(LayerMask.LayerToName(forOfWarPlane.layer)), QueryTriggerInteraction.Collide))
        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("FogPlane"))
            {
                (int centerX, int centerY) = ElementaryBasics.GetGridPositionFromWorldPosition(hit.point);
                for (int x = -GetComponent<Unit>().visibility; x <= GetComponent<Unit>().visibility; x++)
                {
                    for (int y = -GetComponent<Unit>().visibility; y <= GetComponent<Unit>().visibility; y++)
                    {
                        if (Mathf.Abs(x) + Mathf.Abs(y) <= GetComponent<Unit>().visibility)
                        {
                            ElementaryBasics.revealedCells.Add((centerX + x, centerY + y));
                        }
                    }
                }

                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 worldV = forOfWarPlane.transform.TransformPoint(vertices[i]);
                    (int vx, int vy) = ElementaryBasics.GetGridPositionFromWorldPosition(worldV);

                    if (ElementaryBasics.revealedCells.Contains((vx, vy)))
                    {
                        colors[i].a = 0f;
                    }
                }

                UpdateColor();
            }
        }
    }

    private void Initialize()
    {
        mesh = forOfWarPlane.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        colors = new Color[vertices.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color(0, 0, 0, 1);
        }
        UpdateColor();
    }

    private void UpdateColor()
    {
        forOfWarPlane.GetComponent<MeshRenderer>().material.enableInstancing = true;
        mesh.colors = colors;
    }
}