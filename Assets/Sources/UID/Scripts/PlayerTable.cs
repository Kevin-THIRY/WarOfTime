using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerTable : MonoBehaviour
{
    public static PlayerTable Instance;

    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private Transform tableParent;

    public List<PlayerInfos> players;
    public List<BotOption> bots;
    public List<PlayerInfos> playersAndBots;

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
        players = new List<PlayerInfos>();
        bots = new List<BotOption>();
        playersAndBots = new List<PlayerInfos>();
    }

    public void AddPlayerRow(string playerName, Color playerColor, int playerTeam, bool isBot)
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

        if (!isBot) players.Add(new PlayerInfos { Name = playerName, Color = playerColor, Team = playerTeam, isBot = isBot });
        else bots.Add(new BotOption { Name = playerName, Color = playerColor, Team = playerTeam, isBot = isBot, botDifficulty = BotDifficulty.Easy });
        playersAndBots.Add(new PlayerInfos { Name = playerName, Color = playerColor, Team = playerTeam, isBot = isBot });
        
        if (columns.Length >= 4)
        {
            columns[0].text = playerName;
            columns[1].text = playerColor.ToString();
            columns[2].text = playerTeam.ToString();
            columns[3].text = isBot.ToString();
        }
        else
        {
            Debug.LogError("Le prefab de ligne doit contenir au moins trois éléments Text !");
        }
    }

    public void UpdatePlayerRow(int index, string playerName, Color playerColor, int playerTeam, bool isBot)
    {
        // Vérification de base
        if (index < 0 || index >= tableParent.childCount)
        {
            Debug.LogError("Index hors limites !");
            return;
        }

        // Récupère la ligne existante
        Transform row = tableParent.GetChild(index);
        Text[] columns = row.GetComponentsInChildren<Text>();

        // MAJ UI
        if (columns.Length >= 4)
        {
            columns[0].text = playerName;
            columns[1].text = playerColor.ToString();
            columns[2].text = playerTeam.ToString();
            columns[3].text = isBot.ToString();
        }
        else
        {
            Debug.LogError("Le prefab de ligne doit contenir au moins 4 Text !");
        }

        if (!isBot) players.Add(new PlayerInfos { Name = playerName, Color = playerColor, Team = playerTeam, isBot = isBot });
        else bots.Add(new BotOption { Name = playerName, Color = playerColor, Team = playerTeam, isBot = isBot, botDifficulty = BotDifficulty.Easy });
        playersAndBots.Add(new PlayerInfos { Name = playerName, Color = playerColor, Team = playerTeam, isBot = isBot });
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
        playersAndBots.Clear();
        players.Clear();
        bots.Clear();
    }

    public void ClearLists()
    {
        playersAndBots.Clear();
        players.Clear();
        bots.Clear();
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
}