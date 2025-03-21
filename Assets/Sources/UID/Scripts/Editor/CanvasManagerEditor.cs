using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
	
[CustomEditor(typeof(CanvasManager), true)]
public class CanvasManagerEditor : Editor
{
    // Start is called before the first frame update
    CanvasManager canvasManager;
    private SerializedProperty type;
    private SerializedProperty animationTransitionTime;
 
    private void OnEnable()
    {
        type = serializedObject.FindProperty("type");
        animationTransitionTime = serializedObject.FindProperty("animationTransitionTime");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(type, true);
        EditorGUILayout.PropertyField(animationTransitionTime, true);
 
        serializedObject.ApplyModifiedProperties();
    }
}