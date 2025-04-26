using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class CanvasCharacterManager : MonoBehaviour
{
    [SerializeField] private MenuController menuController;
    // [SerializeField] private NearEnvironmentManager nearEnvironmentManager;

    [Header("Add a canvas to open when an object is detected")]
    [SerializeField] private GameObject canvasTarget = null;
    [SerializeField] private Vector2Int positionPanel = new Vector2Int(0, 0);
    private GameObject overCanvasOpened = null;
    // private MovementManager move_character;
    private CanvasManager activeCanvas;
    private bool isCanvasOpen = false;
    
    // Start is called before the first frame update
    void Start()
    {
        menuController.ChangePanel(Type.BaseUID, 0, type => type == Type.None, (type, button) => button.GetButtonType() == type);
        // move_character = GetComponent<MovementManager>();
    }

    private void Update()
    {
        activeCanvas = menuController.GetActiveCanvas();
        ManageInventory();
    }

    private void ManageInventory()
    {
        if (Input.GetKeyDown(KeyCode.E)) OpenInventory();
        // if (nearEnvironmentManager.GetObjectDetected()) OpenActionMenuOnObject();
        else if(isCanvasOpen) CloseCanvas();

        if (activeCanvas.GetButtonType() != Type.BaseUID) MovementManager.instance.SetInOutInventory(true);
        else MovementManager.instance.SetInOutInventory(false);
    }

    private void OpenActionMenuOnObject()
    {
        if (canvasTarget && !isCanvasOpen){
            isCanvasOpen = true;
            // menuController.CreatePanelAndOpenNextToMe(canvasTarget, activeCanvas.GetUniqueId(), positionPanel);
        }
    }

    private void CloseCanvas()
    {
        menuController.DeletePanel(overCanvasOpened);
        isCanvasOpen = false;
    }

    private void OpenInventory()
    {
        menuController.ChangePanel(Type.Inventaire, 0, type => type == Type.None, (type, button) => button.GetButtonType() == type);
    }

    #region Setter

    public void SetOpenedCanvas(GameObject canvas) { overCanvasOpened = canvas; }

    #endregion
}