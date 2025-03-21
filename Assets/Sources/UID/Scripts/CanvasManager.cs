using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    // Button selection
    private int maxVerticalIndex;
    private int maxHorizontalIndex;
    private int verticalIndex;
    private int horizontalIndex;
    private List<int> verticalIndexList = new List<int>();
    private List<int> horizontalIndexList = new List<int>();
    private List<Vector2Int> buttonPositions = new List<Vector2Int>();
    private Vector2Int VectorIndex;
	private bool keyDown;
	private AudioSource audioSource;

    // Canvas transition
    [SerializeField] private Type type;
    [SerializeField] private float animationTransitionTime;
    private int uniqueId;
    private List<ButtonManager> ButtonList = new List<ButtonManager>();
    private bool state;
    private Button button;
    private CanvasGroup group;
    private Animator animator;

    void Start () {
		audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();

        // Switch without unableing the canvas and used to determined how many button there is in the canvas
        state = false;
        
        foreach (Transform enfant in transform){
            ButtonManager button = enfant.GetComponent<Transform>().GetComponent<ButtonManager>();
            if (button != null) {
                ButtonList.Add(button);
                verticalIndexList.Add(button.GetVerticalIndex());
                horizontalIndexList.Add(button.GetHorizontalIndex());
                buttonPositions.Add(new Vector2Int(button.GetVerticalIndex(), button.GetHorizontalIndex()));
            }
        }
        try{
            maxVerticalIndex = verticalIndexList.Max();
            maxHorizontalIndex = horizontalIndexList.Max();
        }
        finally{StartCoroutine(SCAnimation(false));}
	}
    private void Awake () {
        button = GetComponent<Button>();
        group = GetComponent<CanvasGroup>();
    }

    void Update () {
        if (state)
        {
            if(Input.GetAxis ("Vertical") != 0){
                if(!keyDown){
                    if (Input.GetAxis ("Vertical") < 0) {
                        if(verticalIndex < maxVerticalIndex){
                            verticalIndex++;
                        }else{
                            verticalIndex = 0;
                        }
                    } else if(Input.GetAxis ("Vertical") > 0){
                        if(verticalIndex > 0){
                            verticalIndex --; 
                        }else{
                            verticalIndex = maxVerticalIndex;
                        }
                    }
                    keyDown = true;
                }
            }else if (Input.GetAxis ("Horizontal") != 0) {
                if(!keyDown){
                    if (Input.GetAxis ("Horizontal") > 0){
                        if(horizontalIndex < maxHorizontalIndex){
                            horizontalIndex ++;
                        }else{
                            horizontalIndex = 0;
                        }
                    } else if (Input.GetAxis ("Horizontal") < 0){
                        if(horizontalIndex > 0){
                            horizontalIndex --; 
                        }else{
                            horizontalIndex = maxHorizontalIndex;
                        }
                    }
                    keyDown = true;
                }
            } else{
                keyDown = false;
            }
        }
        VectorIndex = GetClosestButtonPosition(new Vector2Int(verticalIndex, horizontalIndex));
        verticalIndex   = VectorIndex.x;
        horizontalIndex = VectorIndex.y;
	}

    private void UpdateState (bool _animate) {
        StopAllCoroutines();

        // Switch without unableing the canvas
        button.gameObject.SetActive(state);
        verticalIndex   = 0;
        horizontalIndex = 0;
        if (animator != null && animator.HasParameter("selected"))
        {
            animator.SetBool("selected", state);
        }
        if (button.gameObject.activeSelf) StartCoroutine(SCAnimation(state));
        

        // Unable the canvas
        // button.gameObject.SetActive(true);
        // if (_animate) StartCoroutine(UCAnimation(state));
        // else button.gameObject.SetActive(state);
    }

    private IEnumerator SCAnimation(bool _state){
        foreach (ButtonManager button in ButtonList){
            button.SetInteractability(_state);
        }

        yield return null;
    }

    private IEnumerator UCAnimation(bool _state){
        float _t      = _state ? 0 : 1;
        float _target = _state ? 1 : 0;
        int _factor   = _state ? 1 : -1;

        Animation animation = FindAnyObjectByType<Animation>();
        if (animation != null) {
            animation.Play();
        }
        

        while (true){
            yield return null;

            _t += Time.deltaTime * _factor / animationTransitionTime;
            group.alpha = _t;

            if ((state && _t >= _target) || (!state && _t <= _target)){
                group.alpha = _target;
                break;
            }
        }

        button.gameObject.SetActive(_state);
    }

    public void ChangeState (bool _animate, bool _changePanel) {
        state = !state;
        if (button.gameObject.activeSelf) StartCoroutine(SCAnimation(state));
        if (_changePanel) UpdateState(_animate);
    }

    public void ChangeState (bool _animate, bool _state, bool _changePanel) {
        state = _state;
        if (button.gameObject.activeSelf) StartCoroutine(SCAnimation(state));
        if (_changePanel) UpdateState(_animate);
    }

    private Vector2Int GetClosestButtonPosition(Vector2Int position)
    {
    // Si la position existe déjà dans la liste, retourne-la directement
    if (buttonPositions.Contains(position))
        return position;

    // Sinon, cherche la position la plus proche
    return buttonPositions
        .OrderBy(pos => Vector2Int.Distance(position, pos)) // Trie par distance
        .FirstOrDefault(); // Retourne la position la plus proche
    }

    #region Getter

    public Type GetButtonType(){return type;}
    public int GetVerticalIndex() {return VectorIndex.x; }
    public int GetHorizontalIndex() {return VectorIndex.y; }
    public AudioSource GetAudioSource() {return audioSource; }
    public int GetUniqueId() {return uniqueId; }

    #endregion

    #region Setter

    public void SetUniqueId(int Id) { uniqueId = Id; }

    #endregion
}

public static class AnimatorExtensions
{
    public static bool HasParameter(this Animator animator, string paramName)
    {
        if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null)
        return false; // Évite d'accéder à `parameters`

        foreach (var param in animator.parameters)
        {
            if (param.name == paramName) return true;
        }

        return false;
    }
}