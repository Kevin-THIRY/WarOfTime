using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTable : MonoBehaviour
{
    public static PlayerTable Instance;

    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private Transform tableParent;

    private void Awake()
    {
        // Singleton pattern pour un accès global
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddPlayerRow(string playerName, string playerColor, int playerTeam, bool isBot)
    {
        // Vérifie que les éléments UI sont bien assignés
        if (rowPrefab == null || tableParent == null)
        {
            Debug.LogError("RowPrefab ou TableParent n'est pas assigné !");
            return;
        }

        // Crée une nouvelle ligne dans le tableau
        GameObject newRow = Instantiate(rowPrefab, tableParent);

        // Récupère tous les éléments Text de la ligne
        Text[] columns = newRow.GetComponentsInChildren<Text>();
        
        if (columns.Length >= 4)
        {
            columns[0].text = playerName;
            columns[1].text = playerColor;
            columns[2].text = playerTeam.ToString();
            columns[3].text = isBot.ToString();
        }
        else
        {
            Debug.LogError("Le prefab de ligne doit contenir au moins trois éléments Text !");
        }
    }

    public void RemovePlayerRow(string playerName)
    {
        foreach (Transform row in tableParent)
        {
            Text[] columns = row.GetComponentsInChildren<Text>();
            if (columns.Length > 0 && columns[0].text == playerName)
            {
                Destroy(row.gameObject);
                return;
            }
        }
        Debug.LogWarning($"Joueur '{playerName}' non trouvé dans la table.");
    }

    public void ClearTable()
    {
        // Supprime toutes les lignes existantes
        foreach (Transform child in tableParent)
        {
            Destroy(child.gameObject);
        }
    }
}