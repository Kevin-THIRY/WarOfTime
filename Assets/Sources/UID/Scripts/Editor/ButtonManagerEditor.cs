using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
	
[CustomEditor(typeof(ButtonManager), true)]
public class ButtonManagerEditor : Editor
{
    // Start is called before the first frame update
    ButtonManager buttonManager;
    private SerializedProperty verticalIndex;
    private SerializedProperty horizontalIndex;
    private SerializedProperty actionsPropList;
    private SerializedProperty monoBehaviourScripts;
    private SerializedProperty targetPanel;
    private SerializedProperty animationsList;
    private SerializedProperty panelToCreateNextToMe;
    private SerializedProperty panelToDelete;
    private SerializedProperty positionPanel;
    private SerializedProperty blockCanvas;
    private List<ButtonFunction> actionsToPerformList;
 
    private void OnEnable()
    {
        verticalIndex = serializedObject.FindProperty("thisVerticalIndex");
        horizontalIndex = serializedObject.FindProperty("thisHorizontalIndex");
        actionsPropList = serializedObject.FindProperty("actionsToPerform");
        monoBehaviourScripts = serializedObject.FindProperty("scripts");
        targetPanel = serializedObject.FindProperty("targetCanvas");
        animationsList = serializedObject.FindProperty("animations");
        panelToCreateNextToMe = serializedObject.FindProperty("panelToCreateNextToMe");
        positionPanel = serializedObject.FindProperty("positionPanel");
        panelToDelete = serializedObject.FindProperty("panelToDelete");
        blockCanvas = serializedObject.FindProperty("blockCanvas");

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(verticalIndex, true);
        EditorGUILayout.PropertyField(horizontalIndex, true);

        EditorGUILayout.PropertyField(actionsPropList, true);
 
        buttonManager = (ButtonManager) target;

        actionsToPerformList = buttonManager.GetActions();
        foreach (var action in actionsToPerformList)
        {
            if (action == ButtonFunction.ChangeScene)
            {
                base.serializedObject.FindProperty("scene").stringValue = EditorGUILayout.TextField("Scene :", buttonManager.GetScene());
            }
            if (action == ButtonFunction.ChangePanel){
                EditorGUILayout.PropertyField(targetPanel);
            }
            if (action == ButtonFunction.ClosePanel){
                EditorGUILayout.PropertyField(targetPanel);
            }
            if (action == ButtonFunction.OpenPanel){
                EditorGUILayout.PropertyField(targetPanel);
            }
            if (action == ButtonFunction.CreatePanelAndOpenNextToMe){
                EditorGUILayout.PropertyField(panelToCreateNextToMe);
                EditorGUILayout.PropertyField(positionPanel);
            }
            if (action == ButtonFunction.DeletePanel){
                EditorGUILayout.PropertyField(panelToDelete);
            }
            if (action == ButtonFunction.De_ActivateScript){
                EditorGUILayout.PropertyField(monoBehaviourScripts);
            }
            if (action == ButtonFunction.LaunchAnimation){
                EditorGUILayout.PropertyField(animationsList);
            }
            if (action == ButtonFunction.ConnectClient){
                base.serializedObject.FindProperty("ipAddress").stringValue = EditorGUILayout.TextField("Addresse IP :", buttonManager.GetIpAddress());
            }
            if (action == ButtonFunction.LaunchNewSceneFromHost){
                base.serializedObject.FindProperty("sceneFromHost").stringValue = EditorGUILayout.TextField("Scene générée par l'hote :", buttonManager.GetSceneLoadedFromHost());
            }
            if (action == ButtonFunction.BlockCanvas){
                EditorGUILayout.PropertyField(blockCanvas);
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}