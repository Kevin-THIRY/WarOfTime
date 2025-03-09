using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ResourcesGenerator))]
public class ResourcesGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();  // Affiche l'inspecteur standard pour le script

        ResourcesGenerator myTarget = (ResourcesGenerator)target;

        // Ajouter un bouton pour ajouter des arbres
        if (GUILayout.Button("Add Trees to Grid"))
        {
            myTarget.GenerateResources();
        }
    }
}