using UnityEngine;

public class MouseShaderController : MonoBehaviour
{
    public Material material;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (material == null || cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 adjustedPosition = hit.point + hit.normal; // Décale légèrement
            material.SetVector("_MousePosition", adjustedPosition);
        }
        else
        {
            material.SetVector("_MousePosition", new Vector4(-1000, -1000, -1000, -1));
        }
    }
}