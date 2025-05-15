using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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

    public string GetPlayerName(string searchValue, int columnIndex = 0)
    {
        foreach (Transform row in tableParent)
        {
            Text[] columns = row.GetComponentsInChildren<Text>();

            // Vérifie si la colonne est valide
            if (columns.Length > columnIndex && columns[columnIndex].text == searchValue)
            {
                return columns[0].text; // Renvoie le nom du joueur (colonne 0)
            }
        }

        Debug.LogWarning($"Aucun joueur trouvé avec la valeur '{searchValue}' dans la colonne {columnIndex}.");
        return string.Empty;
    }

    public List<string> GetBotList()
    {
        List<string> botNames = new List<string>();

        foreach (Transform row in tableParent)
        {
            Text[] columns = row.GetComponentsInChildren<Text>();

            // Vérifie si la ligne a assez de colonnes (au moins 4)
            if (columns.Length >= 4 && columns[3].text == "True") botNames.Add(columns[0].text);
        }

        return botNames;
    }

    public List<string> GetPlayerList()
    {
        List<string> playerNames = new List<string>();

        foreach (Transform row in tableParent)
        {
            Text[] columns = row.GetComponentsInChildren<Text>();
            // Vérifie si la ligne a au moins 1 colonne (le nom du joueur)
            if (columns.Length > 0) playerNames.Add(columns[0].text);
        }
        return playerNames;
    }
}