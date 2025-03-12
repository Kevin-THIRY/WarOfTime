using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;

public class FogLayerManager : MonoBehaviour
{
    [SerializeField] private GameObject forOfWarPlane;
    [SerializeField] private LayerMask fogLayer;
    [SerializeField] private float radius = 2f;
    
    private Transform player;
    private float radiusSqr { get { return radius * radius; }}
    private Mesh mesh;
    private Vector3[] vertices;
    private Color[] colors;

    void Start() {
        Initialize();
    }

    void Update()
    {
        Ray r = new Ray(transform.position, player.position - transform.position);
        RaycastHit hit;
        if (Physics.Raycast(r, out hit, 1000, fogLayer, QueryTriggerInteraction.Collide))
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = forOfWarPlane.transform.TransformPoint(vertices[i]);
                float dist = Vector3.SqrMagnitude(v - hit.point);
                if (dist < radiusSqr)
                {
                    float alpha = Mathf.Lerp(colors[i].a, 0f, 1f - (dist / radiusSqr));
                    colors[i].a = alpha;
                }
            }
            UpdateColor();
        }
    }

    private void Initialize()
    {
        player = GetComponentInParent<CameraFollow>().GetCameraFollowObj().transform.parent;
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