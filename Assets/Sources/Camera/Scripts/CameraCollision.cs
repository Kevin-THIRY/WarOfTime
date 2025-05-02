using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCollision : MonoBehaviour
{
    [SerializeField] private float minDistance = 1.0f;
    [SerializeField] private float maxDistance = 4.0f;
    [SerializeField] private float smooth = 10.0f;
    Vector3 dollyDir;
    [SerializeField] private Vector3 dollyDirAdjusted;
    [SerializeField] private float distance;
    
    void Awake()
    {
        dollyDir = transform.localPosition.normalized;
        distance = transform.localPosition.magnitude;
    }

    private void FixedUpdate()
    {
        Vector3 desiredCameraPos = transform.parent.TransformPoint(dollyDir * maxDistance);
        RaycastHit hit;

        // int layerMask = ~LayerMask.GetMask("MouseDetection");
        // int layerMask = ~LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer));
        int layerMask = ~LayerMask.GetMask("Player1") & ~LayerMask.GetMask("Player2") & ~LayerMask.GetMask("Player3") & ~LayerMask.GetMask("Player4")
            & ~LayerMask.GetMask("VisibleToPlayer") & ~LayerMask.GetMask("HiddenFromPlayer");
            
        if (Physics.Linecast(transform.parent.position, desiredCameraPos, out hit, layerMask)){
            distance = Mathf.Clamp((hit.distance * 0.87f), minDistance, maxDistance);
        }
        else{
            distance = maxDistance;
        }
        transform.localPosition = Vector3.Lerp(transform.localPosition, dollyDir * distance, Time.fixedDeltaTime * smooth);
    }
}
