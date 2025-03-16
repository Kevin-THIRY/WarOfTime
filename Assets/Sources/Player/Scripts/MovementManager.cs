using UnityEngine;

public enum MovementState
{
    Idle,
    Run,
}

public class MovementManager : MonoBehaviour
{
    #region Variables
    // Composants du personnage
    private Rigidbody rb;

    // Vitesses associées aux mouvements
    [SerializeField] private float walk_speed = 1f;

    // Variables
    private MovementState movement_state;
    private float horizontal;
    private float vertical;

    // Inventaire
    private bool is_in_inventory;

    #endregion
    
    // Start is called before the first frame update
    void Start()
    {
        // Composants du personnage
        rb = gameObject.GetComponent<Rigidbody>();

        movement_state = MovementState.Idle;
    }

    private void Update()
    {
        if (!is_in_inventory){
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");
        }
        else
        {
            StopAllCoroutines();
            horizontal = 0;
            vertical = 0;
        }
    }

    private void FixedUpdate()
    {
        UpdateState();
        ProcessMovement();
    }

    private void ProcessMovement()
    {
        if (movement_state == MovementState.Run){
            float angle = Mathf.Atan2(horizontal, vertical) * Mathf.Rad2Deg;
        
            transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);
            rb.MovePosition(rb.position + walk_speed * Time.fixedDeltaTime * transform.forward.normalized);
        }
    }

    private void UpdateState(){
        // Movement state manager
        if (horizontal == 0 && vertical == 0){
            movement_state = MovementState.Idle;
        }
        else{
            movement_state = MovementState.Run;
        }
    }

    #region Getter

    public MovementState GetMovementState() {return movement_state; }
    public bool GetInOutInventory() { return is_in_inventory; }

    #endregion

    #region Setter
    public void SetInOutInventory(bool in_out_inventory) { is_in_inventory = in_out_inventory; }

    #endregion
}