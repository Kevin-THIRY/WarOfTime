using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FogOfWar))]
public class FogOfWarEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();  // Affiche l'inspecteur standard pour le script

        FogOfWar myTarget = (FogOfWar)target;

        // Ajouter un bouton pour ajouter des arbres
        if (GUILayout.Button("Update fog of war"))
        {
            myTarget.UpdateFogOfWar();
        }
    }
}