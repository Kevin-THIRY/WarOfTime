using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEditor;

public enum ButtonFunction{
    ChangePanel,
	ChangeScene,
	De_ActivateScript,
	LaunchAnimation,
	ClosePanel,
	OpenPanel,
	CreatePanelAndOpenNextToMe,
	DeletePanel
}

public class ButtonManager : MonoBehaviour
{
    // Button animation
    private CanvasManager canvasManager;
	private bool disableOnce;
	private bool keyDown = false;

    // Button selection
	private Animator animator;
	[SerializeField] private int thisVerticalIndex;
	[SerializeField] private int thisHorizontalIndex;

    // Transition variables
    [SerializeField] private int animationTime;
    private MenuController controller;

	private Dictionary<ButtonFunction, Delegate> actionDictionary;

	[SerializeField] private List<ButtonFunction> actionsToPerform;

	[SerializeField] private string scene;
    [SerializeField] private List<MonoBehaviour> scripts;
	[SerializeField] private CanvasManager targetCanvas;
	[SerializeField] private List<Animation> animations;
	[SerializeField] private GameObject panelToCreateNextToMe;
	[SerializeField] private Vector2Int positionPanel;
	[SerializeField] private GameObject panelToDelete;
	
    void Start()
    {
        controller = FindAnyObjectByType<MenuController>();
		try{canvasManager = GetComponentInParent<CanvasManager>();} finally{}
        try{animator = GetComponent<Animator>();} finally{}

		InitializeActionDictionary();
    }

	private void InitializeActionDictionary()
    {
        actionDictionary = new Dictionary<ButtonFunction, Delegate>
        {
            { ButtonFunction.ChangePanel, new Action(ChangePanel) },
            { ButtonFunction.ChangeScene, new Action<string>(ChangeScene) },
            { ButtonFunction.De_ActivateScript, new Action<MonoBehaviour>(ReactivateScript) },
			{ ButtonFunction.LaunchAnimation, new Action<Animation>(LaunchAnimation) },
			{ ButtonFunction.ClosePanel, new Action(ClosePanel) },
			{ ButtonFunction.OpenPanel, new Action(OpenPanel) },
			{ ButtonFunction.CreatePanelAndOpenNextToMe, new Action<GameObject, Vector2Int>(CreatePanelAndOpenNextToMe) },
			{ ButtonFunction.DeletePanel, new Action<GameObject>(DeletePanel) }
        };
    }

    void FixedUpdate()
    {
		if (canvasManager != null){
			if(canvasManager.GetVerticalIndex() == thisVerticalIndex && canvasManager.GetHorizontalIndex() == thisHorizontalIndex)
			{
				animator.SetBool ("selected", true);
				if((Input.GetAxis ("Submit") == 1) && GetComponent<Button>().interactable){
					if (!keyDown){
						PressedButton();
						// PlaySound();
					}
					keyDown = true;
				}
				else {
					if (animator.GetBool ("pressed")){
						animator.SetBool ("pressed", false);
						disableOnce = true;
					}
					else {keyDown = false;}
				}
			}else{
				animator.SetBool ("selected", false);
			}
		}
    }

    void PlaySound(AudioClip whichSound){
		if(!disableOnce && whichSound != null){
			canvasManager.GetAudioSource().PlayOneShot (whichSound);
		}else{
			disableOnce = false;
		}
	}

	public void PressedButton(){
		try
		{
			animator.SetBool ("selected", true);
			animator.SetBool ("pressed", true);
		}
		finally{StartCoroutine(PressButton());}
	}

	private IEnumerator PressButton()
    {
		try{
        	AnimatorClipInfo[] CurrentClipInfo = animator.GetCurrentAnimatorClipInfo(0);
			yield return new WaitForSeconds(CurrentClipInfo[0].clip.length);
		}
		finally{ExecuteActions();}
    }

	public void ExecuteActions()
    {
        foreach (var action in actionsToPerform)
        {
			switch (action)
			{
				case ButtonFunction.ChangeScene:
					ExecuteAction(action, scene);
					break;
				case ButtonFunction.De_ActivateScript:
					foreach (var script in scripts)
					{
						if (script != null)
						{
							ExecuteAction(action, script);
						}
					}
					break;
				case ButtonFunction.LaunchAnimation:
					foreach (var animation in animations)
					{
						if (animation != null)
						{
							ExecuteAction(action, animation);
						}
					}
					break;
				case ButtonFunction.CreatePanelAndOpenNextToMe:
					ExecuteAction(action, panelToCreateNextToMe, positionPanel);
					break;
				case ButtonFunction.DeletePanel:
					ExecuteAction(action, panelToDelete);
					break;
				default:
					ExecuteAction(action);
					break;
			}
        }
    }

	private void ExecuteAction(ButtonFunction action)
	{
		if (actionDictionary.TryGetValue(action, out Delegate actionToPerform))
        {
            if (actionToPerform is Action simpleAction)
            {
                simpleAction();
            }
            else
            {
                Debug.LogError("Action does not match expected signature.");
            }
        }
        else
        {
            Debug.LogError("Action not found in dictionary");
        }
	}

	private void ExecuteAction<T>(ButtonFunction action, T parameter)
    {
        if (actionDictionary.TryGetValue(action, out Delegate actionToPerform))
        {
            if (actionToPerform is Action<T> paramAction)
            {
                paramAction(parameter);
            }
            else
            {
                Debug.LogError("Action does not match expected signature.");
            }
        }
        else
        {
            Debug.LogError("Action not found in dictionary");
        }
    }

	private void ExecuteAction<T1, T2>(ButtonFunction action, T1 parameter1, T2 parameter2)
    {
        if (actionDictionary.TryGetValue(action, out Delegate actionToPerform))
        {
            if (actionToPerform is Action<T1, T2> paramAction)
            {
                paramAction(parameter1, parameter2);
            }
            else
            {
                Debug.LogError("Action does not match expected signature.");
            }
        }
        else
        {
            Debug.LogError("Action not found in dictionary");
        }
    }

	private void ChangePanel()
	{
		if (targetCanvas.GetButtonType() == Type.Quit){
			controller.Quit();
		}
		else{controller.ChangePanel(targetCanvas, animationTime, canvas => canvas.GetButtonType() == Type.None,(canvas, button) => button.GetUniqueId() == canvas.GetUniqueId());}
	}

	private void ChangeScene(string sceneName)
	{
		controller.ChangeScene(sceneName);
	}

	private void ReactivateScript(MonoBehaviour script)
	{
		script.enabled = !script.enabled;
	}

	private void LaunchAnimation(Animation anim)
	{
		anim.Play();
	}

	private void ClosePanel()
	{
		if (targetCanvas.GetButtonType() == Type.Quit){
			controller.Quit();
		}
		else{controller.ClosePanel(targetCanvas, canvas => canvas.GetButtonType() == Type.None, (canvas, button) => button == canvas.GetUniqueId());}
	}

	private void OpenPanel()
	{
		if (targetCanvas.GetButtonType() == Type.Quit){
			controller.Quit();
		}
		else{controller.OpenPanel(targetCanvas, canvasManager.GetUniqueId(), canvas => canvas.GetButtonType() == Type.None, (canvas, button) => button == canvas.GetUniqueId());}
	}

	private void CreatePanelAndOpenNextToMe(GameObject canvasObject, Vector2Int positionPanel)
	{
		controller.CreatePanelAndOpenNextToMe(canvasObject, canvasManager.GetUniqueId(), positionPanel);
	}

	private void DeletePanel(GameObject canvasObject)
	{
		controller.DeletePanel(canvasObject);
	}

	#region Getter
	
	public string GetScene(){ return scene; }
	public List<ButtonFunction> GetActions() { return actionsToPerform; }
	public List<MonoBehaviour> GetScripts() { return scripts;}
	public CanvasManager GetTargetPanel() { return targetCanvas; }
	public List<Animation> GetAnimations() { return animations; }
	public int GetVerticalIndex() { return thisVerticalIndex; }
	public int GetHorizontalIndex() { return thisHorizontalIndex; }

	#endregion

	#region Setter

	public void SetInteractability(bool state){
		GetComponent<Button>().interactable = state;
	}

	#endregion
}