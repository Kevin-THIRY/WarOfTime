using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Type{
    None,
    MainScreen,
    FluppyPlanet,
    MainMenuReception,
    Inventaire,
    LoadGame,
    NewGame,
    Map,
    Options,
    Credits,
    Quit,
    BaseUID,
    SlotControl
}

public class MenuController : MonoBehaviour
{

    [Header("Buttons (Il faut obligatoirement BaseUID en position 1 et Inventaire en position 2)")]

    [SerializeField] private List<CanvasManager> buttonList = new List<CanvasManager>();
    // [SerializeField] private CanvasCharacterManager canvasCharacterManager;

    private GameManager manager;
    private CanvasManager activeCanvas;
    private List<int> pileCanvas = new List<int>();
    private int maxId = 0;

    private void Start() {
        manager = GameManager.instance;
        AssignUniqueId();
        ChangePanel(buttonList[0].GetButtonType(), 0, type => type == Type.None, (type, button) => button.GetButtonType() == type);
    }

    // To call once and for all at the beginning of the script
    private void AssignUniqueId()
    {
        foreach (CanvasManager canvasManager in buttonList)
        {
            canvasManager.SetUniqueId(maxId);
            maxId++;
        }
    }

    private void OpenOnePanel<T>(T target, int animationTime, bool animate, bool changePanel, System.Func<T, bool> isTargetNone, System.Func<T, CanvasManager, bool> isMatchingButton)
    {
        StopAllCoroutines();
        StartCoroutine(ChangeStateButton(target, animationTime, animate, changePanel, isTargetNone, isMatchingButton));
    }

    public void ChangePanel<T>(T target, int animationTime, System.Func<T, bool> isTargetNone, System.Func<T, CanvasManager, bool> isMatchingButton)
    {
        OpenOnePanel(target, animationTime, true, true, isTargetNone, isMatchingButton);
    }

    public void OpenPanel<T>(T target, int canvasOrigin, System.Func<T, bool> isTargetNone, System.Func<T, int, bool> isMatchingButton)
    {
        pileCanvas.Add(canvasOrigin);
        StopAllCoroutines();
        StartCoroutine(OpenStateButton(target, 0, false, true, canvasOrigin, isTargetNone, isMatchingButton));
        
    }

    public void ClosePanel<T>(T target, System.Func<T, bool> isTargetNone, System.Func<T, int, bool> isMatchingButton)
    {
        StopAllCoroutines();
        StartCoroutine(CloseStateButton(target, 0, false, true, isTargetNone, isMatchingButton));
    }

    public void DeletePanel(GameObject target)
    {
        ClosePanel(target.GetComponent<CanvasManager>(), canvas => canvas.GetButtonType() == Type.None, (canvas, button) => button == canvas.GetUniqueId());
        if (buttonList.Contains(target.GetComponent<CanvasManager>())) buttonList.Remove(target.GetComponent<CanvasManager>());
        Destroy(target);
    }

    public void CreatePanelAndOpenNextToMe(GameObject canvasObject, int canvasOrigin, Vector2Int positionPanel)
    {
        if (canvasObject == null)
		{
			Debug.LogError("La prefab passée en paramètre est nulle !");
			return;
		}

		// Instancier la prefab
		GameObject instance = Instantiate(canvasObject);
        instance.transform.SetParent(transform, false); // false pour conserver les transformations locales
        instance.transform.localPosition = new Vector3(positionPanel.x, positionPanel.y, 0);
        instance.GetComponent<CanvasManager>().SetUniqueId(maxId++);
        // try {canvasCharacterManager.SetOpenedCanvas(instance);}
        // finally{}
        buttonList.Add(instance.GetComponent<CanvasManager>());
        OpenPanel(instance.GetComponent<CanvasManager>(), canvasOrigin, canvas => canvas.GetButtonType() == Type.None, (canvas, button) => button == canvas.GetUniqueId());
    }

    public void ChangeScene (string _sceneName) {
        manager.ChangeScene(_sceneName);
    }

    public void Quit () {
        manager.Quit();
        Application.Quit();
    }

    private IEnumerator CloseStateButton<T>(T target, int second, bool animate, bool changePanel, System.Func<T, bool> isTargetNone, System.Func<T, int, bool> isMatchingButton)
    {
        yield return new WaitForSeconds(second);

        if (!isTargetNone(target))
        {
            foreach (var button in buttonList)
            {
                // Désactiver si c'est le bouton cible
                if (isMatchingButton(target, button.GetUniqueId())){
                    button.ChangeState(animate, false, changePanel);
                }
                // Réactiver le bouton correspondant au sommet de pileCanvas
                if (button.GetUniqueId() == pileCanvas[pileCanvas.Count - 1]){
                    button.ChangeState(animate, true, !changePanel);
                }
            }
        }

        pileCanvas.RemoveAt(pileCanvas.Count - 1);
    }

    private IEnumerator OpenStateButton<T>(T target, int second, bool animate, bool changePanel, int canvasOrigin, System.Func<T, bool> isTargetNone, System.Func<T, int, bool> isMatchingButton)
    {
        yield return new WaitForSeconds(second);

        if (!isTargetNone(target))
        {
            foreach (var button in buttonList)
            {
                // Activer l'état pour le bouton cible
                if (isMatchingButton(target, button.GetUniqueId())){
                    button.ChangeState(animate, true, changePanel);
                }

                // Désactiver l'état pour le bouton d'origine
                if (button.GetUniqueId() == canvasOrigin) {
                    button.ChangeState(animate, false, !changePanel);
                }
            }
        }
    }

    private IEnumerator ChangeStateButton<T>(T target, int second, bool animate, bool changePanel, System.Func<T, bool> isTargetNone, System.Func<T, CanvasManager, bool> isMatchingButton)
    {
        yield return new WaitForSeconds(second);

        if (!isTargetNone(target))
        {
            foreach (var button in buttonList) button.ChangeState(animate, false, changePanel);

            foreach (var button in buttonList)
            {
                if (isMatchingButton(target, button))
                {
                    button.ChangeState(animate, true, changePanel);
                }
            }
        }

        if (typeof(T) == typeof(Type))
        {
            Type enumValue = (Type)(object)target;
            if (enumValue == Type.BaseUID)activeCanvas = buttonList[0];
            if (enumValue == Type.Inventaire)activeCanvas = buttonList[1];
            // activeCanvas = (Type)(object)target; // Cast générique vers Type
        }
        else if (typeof(T) == typeof(CanvasManager))
        {
            activeCanvas = (CanvasManager)(object) target; //((CanvasManager)(object)target).GetButtonType();
        }
    }

    #region Getter

    public CanvasManager GetActiveCanvas() { return activeCanvas; }

    #endregion
}
