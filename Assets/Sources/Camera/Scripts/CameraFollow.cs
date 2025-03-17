using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float CameraMoveSpeed = 120.0f;
    [SerializeField] private GameObject CameraFollowObj;
    [SerializeField] private float clampAngle = 80.0f;
    [SerializeField] private float inputSensitivity = 150.0f;
    [SerializeField] private GameObject CameraObj;
    [SerializeField] private GameObject PlayerObj;
    [SerializeField] private float camDistanceXToPlayer;
    [SerializeField] private float camDistanceYToPlayer;
    [SerializeField] private float camDistanceZToPlayer;
    [SerializeField] private float smoothX;
    [SerializeField] private float smoothY;
    [SerializeField] private float transition_camera_smooth_vertical = 1f;
    private float mouseX;
    private float mouseY;
    private float finalInputX;
    private float finalInputZ;
    private float rotX = 0.0f;
    private float rotY = 0.0f;

    
    // Start is called before the first frame update
    void Start()
    {
        transform.position = CameraFollowObj.GetComponentInParent<Transform>().position;
        Vector3 rot = transform.rotation.eulerAngles;
        rotX = rot.x;
        rotY = rot.y;
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    private void Update()
    {
        DynamicFollow();
    }

    private void DynamicFollow()
    {
        float inputX = Input.GetAxis("RightStickHorizontal");
        float inputZ = Input.GetAxis("RightStickVertical");
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
        finalInputX = inputX + mouseX;
        finalInputZ = inputZ + mouseY;
        
        // rotY = finalInputX * inputSensitivity * Time.fixedDeltaTime;
        rotX -= finalInputZ * inputSensitivity * Time.fixedDeltaTime;

        rotX = Mathf.Clamp(rotX, clampAngle, clampAngle);
        // rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);
        
        Quaternion quat_local_target = Quaternion.FromToRotation(transform.up, CameraFollowObj.GetComponentInParent<Rigidbody>().transform.up) * transform.rotation;

        Quaternion localRotation = quat_local_target * Quaternion.Euler(rotX, rotY, 0.0f);
        transform.localRotation = localRotation;
    }

    private void LateUpdate(){
        CameraUpdater();
    }

    private void CameraUpdater(){
        Transform target = CameraFollowObj.transform;

        float step = CameraMoveSpeed * Time.fixedDeltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);
    }

    #region Getter
    public GameObject GetCameraFollowObj() { return CameraFollowObj; }
    #endregion
}