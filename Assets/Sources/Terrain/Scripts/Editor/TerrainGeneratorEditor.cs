using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TerrainGenerator generator = (TerrainGenerator)target;
        if (GUILayout.Button("Générer la map"))
        {
            generator.GenerateTerrain();
        }
        if (GUILayout.Button("Générer les textures"))
        {
            generator.ApplyTextures();
        }
        // Ajouter un bouton pour ajouter des arbres
        if (GUILayout.Button("Add Fog of War"))
        {
            generator.GenerateFogOfWar();
        }
        if (GUILayout.Button("Add Highlight Map"))
        {
            generator.GenerateHighlightMap();
        }
    }
}