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
    public static MovementManager instance {private set; get;}

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

    #endregion

    private void Awake() {
        if(instance != null){
            Destroy(this);
            return;
        }

        instance = this;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        // Composants du personnage
        rb = gameObject.GetComponent<Rigidbody>();
        movement_state = MovementState.Idle;
    }

    private void Update()
    {
        Vector2 move = inputActions["Move"].ReadValue<Vector2>();
        float openOption = inputActions["Option"].ReadValue<float>();
        if (!is_in_inventory){ // && turn.IsMyTurn()){
            horizontal = move.x;
            vertical = move.y;
        }
        else
        {
            StopAllCoroutines();
            horizontal = 0;
            vertical = 0;
        }
        if (!is_in_inventory && (openOption != 0))
        {
            MenuController.instance.ChangePanel(Type.Options, 0, type => type == Type.None, (type, button) => button.GetButtonType() == type);
            MenuController.instance.SetBlockingCanvas(true);
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