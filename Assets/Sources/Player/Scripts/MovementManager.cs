using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

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
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputActionAsset inputActions;

    // Inventaire
    private bool is_in_inventory;

    // UID
    [SerializeField] private MenuController menuController;

    #endregion
    
    // Start is called before the first frame update
    void Start()
    {
        // Composants du personnage
        rb = gameObject.GetComponent<Rigidbody>();

        movement_state = MovementState.Idle;
        menuController = FindAnyObjectByType<MenuController>();
    }

    private void Update()
    {
        // if (!IsOwner) return;
        // if (IsOwner)
        // {
        Vector2 move = inputActions["Move"].ReadValue<Vector2>();
        bool openOption = inputActions["Option"].ReadValue<bool>();
        if (!is_in_inventory){
            horizontal = move.x;
            vertical = move.y;
        }
        else
        {
            StopAllCoroutines();
            horizontal = 0;
            vertical = 0;
        }
        if (!is_in_inventory && openOption)
        {
            menuController.ChangePanel(Type.Options, 0, type => type == Type.None, (type, button) => button.GetButtonType() == type);
            is_in_inventory = true;
        }
    }

    private void FixedUpdate()
    {
        // if (!IsOwner) return;
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
        if (horizontal == 0 && vertical == 0)
        {
            movement_state = MovementState.Idle;
        }
        else
        {
            movement_state = MovementState.Run;
        }
    }

    #region Getter

    public MovementState GetMovementState() {return movement_state; }
    public bool GetInOutInventory() { return is_in_inventory; }

    #endregion

    #region Setter
    public void SetInOutInventory(bool in_out_inventory) { is_in_inventory = in_out_inventory; }
    public void SetInputSystem(InputActionAsset _inputActions) { inputActions = _inputActions; }

    #endregion
}