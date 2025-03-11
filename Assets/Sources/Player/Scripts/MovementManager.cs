using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.Mathematics;
using UnityEngine.Rendering;

public enum MovementState
{
    Idle,
    Run,
}

public enum JumpState
{
    Jump,
    Fall,
    NoJump,
}

public enum DashState
{
    DashOn,
    DashOff,
}

public enum AttackState
{
    None,
    Charge,
    Nab,
    Attack,
    Block,
}

public enum InOutSpaceState
{
    FlyInSpace,
    NotFlyInSpace,
}

public class AttractorInfo
{
    public String AttractorName { get; set; }
    public Vector3 CenterPositon { get; set; }

    // Constructeur
    public AttractorInfo(String name = "", Vector3 position = default)
    {
        AttractorName = name;
        CenterPositon = position;
    }
}

public class MovementManager : MonoBehaviour
{
    #region Variables
    // Composants du personnage
    private Rigidbody rb;

    // Planete sur laquelle est le personnage
    public AttractorInfo attractor_info = new();

    // Vitesses associées aux mouvements
    [SerializeField] private float walk_speed = 1f;
    [SerializeField] private float dash_speed = 2f;
    [SerializeField] private float dash_time = 0.5f;
    [SerializeField] private float jump_speed = 10f;
    [SerializeField] public float gravity_on_base_plan = 1f;
    [SerializeField] private float coolDown_dash = 2.0f;

    // Camera associée
    [SerializeField] private Transform tf_cam;

    // Variables
    private MovementState movement_state;
    private AttackState attack_state;
    private JumpState jump_state;
    private DashState dash_state;
    [SerializeField] private InOutSpaceState in_out_space_state;
    private float turnSmooth = 0.1f;
    private float turnSmoothVelocity;
    private Vector3 vector_direction = Vector3.zero;
    private float horizontal;
    private float vertical;
    private bool canDash;
    private bool is_attracted_by_a_planet = false;
    private bool is_catch_by_a_planet = false;

    // Inventaire
    private bool is_in_inventory;

    #endregion
    
    // Start is called before the first frame update
    void Start()
    {
        // Composants du personnage
        rb = gameObject.GetComponent<Rigidbody>();

        movement_state = MovementState.Idle;
        attack_state = AttackState.None;
        jump_state = JumpState.NoJump;
        dash_state = DashState.DashOff;
        canDash = true;

        // gravity_target = new AttractorInfo();
    }

    private void Update()
    {
        UpdateState();
        if (!is_in_inventory){
            if (dash_state == DashState.DashOn)return;

            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");

            if (Input.GetButtonDown("Jump") && isGrounded())
            {
                StartCoroutine(Jump());
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
            {
                StartCoroutine(Dash());
            }
        }
        else
        {
            StopAllCoroutines();
            horizontal = 0;
            vertical = 0;
        }

        if (is_catch_by_a_planet){
            walk_speed = 0.01f;
        }

        if (in_out_space_state == InOutSpaceState.NotFlyInSpace) is_attracted_by_a_planet = true;
    }

    private void FixedUpdate()
    {
        if (dash_state == DashState.DashOn)
        {
            return;
        }

        vector_direction = new Vector3(horizontal * walk_speed, 0.0f, vertical * walk_speed);
        ProcessMovement();
    }

    private void ProcessMovement(){
        if (in_out_space_state == InOutSpaceState.FlyInSpace){
            if (is_attracted_by_a_planet) {
                Vector3 DirVect_Cam = tf_cam.forward.normalized * vertical + tf_cam.right.normalized * horizontal;
                float angle_to_reach = Vector3.SignedAngle(transform.forward.normalized, DirVect_Cam.normalized, transform.up);

                if (movement_state == MovementState.Run){
                    transform.rotation *= Quaternion.Euler(0f, angle_to_reach * turnSmooth, 0f);
                    rb.MovePosition(rb.position + walk_speed * Time.fixedDeltaTime * transform.forward.normalized);
                }
            }
            else{
                Vector3 DirVect_Cam = tf_cam.forward.normalized * vertical + tf_cam.right.normalized * horizontal;
                float angle_to_reach = Vector3.SignedAngle(transform.forward.normalized, DirVect_Cam.normalized, transform.up);

                Vector3 DirVect_Cam_vert = tf_cam.up.normalized * math.abs(vertical);
                float angle_to_reach_vert = Vector3.SignedAngle(transform.up.normalized, DirVect_Cam_vert.normalized, transform.right);

                if (movement_state == MovementState.Run){
                    transform.rotation *= Quaternion.Euler(angle_to_reach_vert * turnSmooth, angle_to_reach * turnSmooth, 0f);
                    rb.MovePosition(rb.position + walk_speed * Time.fixedDeltaTime * transform.forward.normalized);
                }
            }
        }
        else{
            // Angle
            // Vector3 DirVect_Cam = tf_cam.forward.normalized * vertical + tf_cam.right.normalized * horizontal;
            // float angle_to_reach = Vector3.SignedAngle(transform.forward.normalized, DirVect_Cam.normalized, transform.up);
            float angle = Mathf.Atan2(horizontal, vertical) * Mathf.Rad2Deg + tf_cam.eulerAngles.y;
            angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, angle, ref turnSmoothVelocity, turnSmooth);
            if (movement_state == MovementState.Run){
                transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);
                rb.MovePosition(rb.position + walk_speed * Time.fixedDeltaTime * transform.forward.normalized);
            }
            rb.AddForce(new Vector3(0.0f, rb.linearVelocity.y - (gravity_on_base_plan * Time.fixedDeltaTime), 0.0f));
        }
    }

    private bool isGrounded()
    {
        // Vector3 ray = transform.TransformDirection(Vector3.down) * 3f;
        // Debug.DrawRay(transform.position, ray, Color.green, 600);

        if (Physics.Raycast(transform.position, Vector3.up, out RaycastHit Hitup, 3f))
        {
            // Debug.Log(Hitup.collider);
            if (Hitup.collider.tag == "Environment") return true;
            else return false;
        }
        else if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit Hitdown, 3f))
        {
            if (Hitdown.collider.tag == "Environment") return true;
            else return false;
        }
        else if (Physics.Raycast(transform.position, Vector3.left, out RaycastHit Hitleft, 3f))
        {
            if (Hitleft.collider.tag == "Environment") return true;
            else return false;
        }
        else if (Physics.Raycast(transform.position, Vector3.right, out RaycastHit Hitright, 3f))
        {
            if (Hitright.collider.tag == "Environment") return true;
            else return false;
        }
        else
        {
            return false;
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        dash_state = DashState.DashOn;
        rb.useGravity = false;        
        vector_direction = rb.position + dash_speed * transform.forward;
        rb.AddForce(vector_direction * Time.fixedDeltaTime);
        // tr.emitting = true;
        yield return new WaitForSeconds(dash_time);
        // tr.emitting = false;
        rb.useGravity = true;
        dash_state = DashState.DashOff;
        yield return new WaitForSeconds(coolDown_dash);
        canDash = true;
    }

    private IEnumerator Jump()
    {
        jump_state = JumpState.Jump;
        yield return new WaitForSeconds(0.1f);
        rb.AddForce(transform.up * jump_speed * rb.mass);
    }

    private void UpdateState(){
        // Movement state manager
        if (horizontal == 0 && vertical == 0){
            movement_state = MovementState.Idle;
        }
        else{
            movement_state = MovementState.Run;
        }

        // Jump state manager
        if (!isGrounded()){
            if (rb.linearVelocity.y > 0){
                jump_state = JumpState.Jump;
            }
            else if (rb.linearVelocity.y <= 0){
                jump_state = JumpState.Fall;
            }
        }
        else{
            jump_state = JumpState.NoJump;
        }
    }

    #region Getter

    public MovementState GetMovementState() {return movement_state; }

    public JumpState GetJumpState() {return jump_state; }

    public DashState GetDashState() {return dash_state; }

    public AttackState GetAttackState() {return attack_state; }

    public bool GetGravityStatus() { return is_attracted_by_a_planet; }
    public bool GetCatchByAPlanetStatus() { return is_catch_by_a_planet; }
    public InOutSpaceState GetInOutSpaceState() { return in_out_space_state; }
    public bool GetInOutInventory() { return is_in_inventory; }

    #endregion

    #region Setter

    public void SetGravityState(bool gravity_status, AttractorInfo attractorInfo) { is_attracted_by_a_planet = gravity_status; attractor_info = attractorInfo; }
    public void SetCatchByMainPlanetState(bool catch_by_a_planet_status) { is_catch_by_a_planet = catch_by_a_planet_status; }
    public void SetInOutSpaceState(InOutSpaceState in_space_status) { in_out_space_state = in_space_status; }
    public void SetInOutInventory(bool in_out_inventory) { is_in_inventory = in_out_inventory; }

    #endregion
}